﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Kantab.Classes.Messages.Common;
using Kantab.Classes.Messages.Server;
using Kantab.Classes.PenStateProviders;
using Kantab.Classes.Router;
using Kantab.Classes.Router.Middleware;
using Kantab.Interfaces;
using Kantab.Structs;
using Kantab.WinAPI.Structs;

namespace Kantab.Classes;

public class KantabServer {

    public short Port = 7329;
    private string _prefix => $"http://localhost:{Port}/";

    private HttpListener _listener;

    private KantabHttpHandler _httpHandler;
    private List<KantabClient> _clients = new();
    public int ConnectedClients => _clients.Count;

    public bool RegionSetupMode { get; set; }
    public bool Running { get; private set; }

    public event EventHandler<KantabClient> ClientConnected;
    public event EventHandler<KantabClient> ClientDisconnected;
    public event EventHandler<PenState> PositionRelayed;

    public event EventHandler<PenState> SetupModePositionReceived;

    public event EventHandler ServerStarted;
    public event EventHandler ServerStopped;
    public event EventHandler<Exception> ServerCatastrophe;

    public Dictionary<string, ConstructMetadata> AvailableConstructs = new();
    public ConstructMetadata? CurrentConstruct = null;

    private KantabSettings _loadedSettings;
    public KantabSettings LoadedSettings => _loadedSettings;
    public IPenStateProvider? PenStateProvider { get; set; }
    private Task _httpServerTask;

    private System.Timers.Timer _positionFetchTimer;

    public KantabServer(KantabSettings config) {
        SetSettings(config);
        _positionFetchTimer = new();

        if (_loadedSettings.ScreenRegion.Empty) _loadedSettings.ScreenRegion = new Rectangle(1920, 0, 1920 * 2, 1080);
        _positionFetchTimer.Elapsed += BroadcastPenState;
        _positionFetchTimer.Interval = _loadedSettings.FetchRate;

        _httpHandler = new KantabHttpHandler();
        SetupRoutes();

        LoadConstructs();
        if (_loadedSettings.ConstructFolder == null) {
            string? firstConstruct = AvailableConstructs.Keys.FirstOrDefault();
            if (firstConstruct == null) {
                CurrentConstruct = null;
                return;
            }
            SelectConstruct(firstConstruct);
        }
    }

    public void Start() {
        _positionFetchTimer.Start();
        Task.Run(BeginHttpServer);
        ServerStarted?.Invoke(this, EventArgs.Empty);
        Running = true;
    }

    public void Stop() {
        _positionFetchTimer?.Stop();
        _listener?.Stop();
        _listener?.Close();
        ServerStopped?.Invoke(this, EventArgs.Empty);
        Running = false;
    }

    public void SetSettings(KantabSettings config) {
        _loadedSettings = config;
        Port = config.Port;
        float fetchRate = config.FetchRate;

        if (_loadedSettings.ScreenRegion.Empty) _loadedSettings.ScreenRegion = new Rectangle(1920, 0, 1920 * 2, 1080);
        if (_positionFetchTimer != null) {
            _positionFetchTimer.Interval = fetchRate;
        }

        if (_loadedSettings.ConstructFolder != null) {
            SelectConstruct(_loadedSettings.ConstructFolder);
        }
    }

    private async Task BeginHttpServer() {
        try {
            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);
            _listener.Start();
        }
        catch (Exception e) {
            Stop();
            ServerCatastrophe?.Invoke(this, e);
            return;
        }

        while (true) {
            try {
                HttpListenerContext httpCtx = await _listener.GetContextAsync();
                if (httpCtx.Request.IsWebSocketRequest) {
                    UpgradeToWebsocket(httpCtx);
                }
                else {
                    _ = _httpHandler.RunHandler(httpCtx);
                }
            }
            catch (Exception e) {
                Stop();
                ServerCatastrophe?.Invoke(this, e);
                break;
            }
        }
    }

    private void BroadcastPenState(object? sender, ElapsedEventArgs args) {
        if (!RegionSetupMode) {
            foreach (KantabClient client in _clients) {
                if (!client.Ready || client.IsRelay) continue;
                // sends pen position, normalized to relative (0, 1), input is absolute
                client.SendMessage(new PenInformationMessage(PenStateProvider.Extended, PenStateProvider?.CurrentPenState ?? new(), _loadedSettings.ScreenRegion));
            }
        }
        else {
            // scissor setup mode, notify no clients
            SetupModePositionReceived?.Invoke(this, PenStateProvider?.CurrentPenState ?? new());
        }

    }

    private void ReceivePenState(object? sender, PenState recvState) {
        // assume default absolute
        bool absolute = sender == null || ((KantabClient)sender).Features.HasFlag(Enums.ClientFeatures.ABSOLUTE_POSITION);

        if (!absolute) {
            recvState.Position = recvState.DenormalizePosition(_loadedSettings.ScreenRegion);
        }

        PositionRelayed?.Invoke(this, recvState);
    }

    internal async void UpgradeToWebsocket(HttpListenerContext ctx) {
        try {
            WebSocketContext wsCtx = await ctx.AcceptWebSocketAsync("kantab-v1");
            KantabClient newClient = new KantabClient(this, wsCtx.WebSocket);

            newClient.OnClientDisconnect += (s, graceful) => {
                _clients.Remove(newClient);
                if (newClient.PositionReceived != null) {
                    newClient.PositionReceived -= ReceivePenState;
                }
            };

            newClient.RelayUpgrade += (s, o) => {

                Console.Write("damn OK look at that RELAY_AUTHORITY go, ight buddy won't bother you");
            };

            newClient.PositionReceived += ReceivePenState;

            _clients.Add(newClient);
            newClient.SendMessage(new HelloMessage(), true);
        }
        catch (Exception e) {
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/json";
            StreamWriter sw = new StreamWriter(ctx.Response.OutputStream);
            await sw.WriteAsync("{\"error\": \"black hole\"}");
            ctx.Response.Close();
            return;
        }
    }

    public void LoadConstructs() {
        AvailableConstructs.Clear();
        string[] filePaths = Directory.GetFiles(
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Constructs"),
            "kantab.json", SearchOption.AllDirectories);

        foreach (string filePath in filePaths) {
            string jsonText = File.ReadAllText(filePath);
            ConstructMetadata? meta;
            try {
                meta = JsonSerializer.Deserialize<ConstructMetadata>(jsonText);
            }
            catch {
                meta = null;
            }
            if (!meta.HasValue) continue;

            string folderName = Path.GetDirectoryName(filePath).Replace("/", "\\").Split('\\')[^1];
            ConstructMetadata actualMeta = meta.Value;
            actualMeta.FilesystemBase = $"/constructs/{folderName}/";
            actualMeta.Id = folderName;

            AvailableConstructs.Add(folderName, actualMeta);
        }

        SelectConstruct(_loadedSettings.ConstructFolder);
    }

    public void SelectConstruct(string? id) {
        if (id == null || !AvailableConstructs.ContainsKey(id)) return;
        CurrentConstruct = AvailableConstructs[id];
        ConsiderRefreshMessage msg = new ConsiderRefreshMessage();

        foreach (KantabClient kantabClient in _clients) {
            kantabClient.SendMessage(msg);
        }
    }

    private void SetupRoutes() {
        _httpHandler.Get(new Regex(@"^constructs/.*$"),
            async (kCtx) => {
                await KantabHttpFileServer.ServeDirectory(kCtx,
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Constructs"),
                    1);
            }
        );

        _httpHandler.Get(new Regex(@"^settings/?$"),
            async (kCtx) => {
                ConstructMetadata? constr = CurrentConstruct;
                await KantabHttpFileServer.ServeText(kCtx, JsonSerializer.Serialize(_loadedSettings),
                    "application/json");
            }
        );

        _httpHandler.Get(new Regex(@"^settings/construct/?"),
            async (kCtx) => {
                ConstructMetadata? constr = CurrentConstruct;
                await KantabHttpFileServer.ServeText(kCtx, JsonSerializer.Serialize(constr),
                    "application/json");
            }
        );

        _httpHandler.Get(new Regex(@"^views/.*/?$"),
            async (kCtx) => {
                await KantabHttpFileServer.ServeDirectory(kCtx,
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Views"),
                    1);
            }
        );

        _httpHandler.Get(new Regex(@"^img/.*/?$"),
            async (kCtx) => {
                await KantabHttpFileServer.ServeDirectory(kCtx,
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Views/img"),
                    1);
            }
        );
    }

}