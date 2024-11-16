using System;
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
using Kantab.Classes.PenStateProviders;
using Kantab.Classes.Router;
using Kantab.Classes.Router.Middleware;
using Kantab.Interfaces;
using Kantab.Structs;
using Kantab.WinAPI.Structs;

namespace Kantab.Classes;

public class KantabServer
{

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

    public event EventHandler<PenState> SetupModePositionReceived;

    public event EventHandler ServerStarted;
    public event EventHandler ServerStopped;
    public event EventHandler<Exception> ServerCatastrophe;

    public Dictionary<string, ConstructMetadata> AvailableConstructs = new();
    public ConstructMetadata? CurrentConstruct = null;

    private KantabSettings _loadedSettings;
    public KantabSettings LoadedSettings => _loadedSettings;
    public IPenStateProvider PenStateProvider { get; set; } = new MousePenStateProvider();
    private Task _httpServerTask;

    private System.Timers.Timer _positionFetchTimer;

    public KantabServer(KantabSettings config)
    {
        SetSettings(config);
        _positionFetchTimer = new();

        if (_loadedSettings.ScreenRegion.Empty) _loadedSettings.ScreenRegion = new Rectangle(1920, 0, 1920 * 2, 1080);
        _positionFetchTimer.Elapsed += BroadcastPenState;
        _positionFetchTimer.Interval = _loadedSettings.FetchRate;

        _httpHandler = new KantabHttpHandler();
        SetupRoutes();

        LoadConstructs();
        if (_loadedSettings.ConstructFolder == null)
        {
            string? firstConstruct = AvailableConstructs.Keys.FirstOrDefault();
            if (firstConstruct == null)
            {
                CurrentConstruct = null;
                return;
            }
            SelectConstruct(firstConstruct);
        }
    }

    public void Start()
    {
        _positionFetchTimer.Start();
        Task.Run(BeginHttpServer);
        ServerStarted?.Invoke(this, EventArgs.Empty);
        Running = true;
    }

    public void Stop()
    {
        _positionFetchTimer?.Stop();
        _listener?.Stop();
        _listener?.Close();
        ServerStopped?.Invoke(this, EventArgs.Empty);
        Running = false;
    }

    public void SetSettings(KantabSettings config)
    {
        _loadedSettings = config;
        Port = config.Port;
        float fetchRate = config.FetchRate;

        if (_loadedSettings.ScreenRegion.Empty) _loadedSettings.ScreenRegion = new Rectangle(1920, 0, 1920 * 2, 1080);
        if (_positionFetchTimer != null)
        {
            _positionFetchTimer.Interval = fetchRate;
        }

        if (_loadedSettings.ConstructFolder != null)
        {
            SelectConstruct(_loadedSettings.ConstructFolder);
        }
    }

    private async Task BeginHttpServer()
    {
        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);
            _listener.Start();
        }
        catch (Exception e)
        {
            Stop();
            ServerCatastrophe?.Invoke(this, e);
            return;
        }

        while (true)
        {
            try
            {
                HttpListenerContext httpCtx = await _listener.GetContextAsync();
                if (httpCtx.Request.IsWebSocketRequest)
                {
                    UpgradeToWebsocket(httpCtx);
                }
                else
                {
                    _ = _httpHandler.RunHandler(httpCtx);
                }
            }
            catch (Exception e)
            {
                Stop();
                ServerCatastrophe?.Invoke(this, e);
                break;
            }
        }
    }

    private void BroadcastPenState(object? sender, ElapsedEventArgs args)
    {
        if (!RegionSetupMode)
        {
            foreach (KantabClient client in _clients)
            {
                if (!client.Ready) continue;
                client.SendMessage(new PenInformationMessage(false, PenStateProvider.CurrentPenState, _loadedSettings.ScreenRegion));
            }
        }
        else
        {
            SetupModePositionReceived?.Invoke(this, PenStateProvider.CurrentPenState);
        }

    }

    internal async void UpgradeToWebsocket(HttpListenerContext ctx)
    {
        try
        {
            WebSocketContext wsCtx = await ctx.AcceptWebSocketAsync("kantab-v1");
            KantabClient newClient = new KantabClient(this, wsCtx.WebSocket);

            newClient.OnClientDisconnect += (s, graceful) =>
            {
                _clients.Remove(newClient);
            };

            _clients.Add(newClient);
            newClient.SendMessage(new HelloMessage(), true);
        }
        catch (Exception e)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/json";
            StreamWriter sw = new StreamWriter(ctx.Response.OutputStream);
            await sw.WriteAsync("{\"error\": \"black hole\"}");
            ctx.Response.Close();
            return;
        }
    }

    public void LoadConstructs()
    {
        AvailableConstructs.Clear();
        string[] filePaths = Directory.GetFiles(
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Constructs"),
            "kantab.json", SearchOption.AllDirectories);

        foreach (string filePath in filePaths)
        {
            string jsonText = File.ReadAllText(filePath);
            ConstructMetadata? meta;
            try
            {
                meta = JsonSerializer.Deserialize<ConstructMetadata>(jsonText);
            }
            catch
            {
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

    public void SelectConstruct(string? id)
    {
        if (id == null || !AvailableConstructs.ContainsKey(id)) return;
        CurrentConstruct = AvailableConstructs[id];
    }

    private void SetupRoutes()
    {
        _httpHandler.Get(new Regex(@"^constructs/.*$"),
            async (kCtx) =>
            {
                await KantabHttpFileServer.ServeDirectory(kCtx,
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Constructs"),
                    1);
            }
        );

        _httpHandler.Get(new Regex(@"^settings/?$"),
            async (kCtx) =>
            {
                ConstructMetadata? constr = CurrentConstruct;
                await KantabHttpFileServer.ServeText(kCtx, JsonSerializer.Serialize(_loadedSettings),
                    "application/json");
            }
        );

        _httpHandler.Get(new Regex(@"^settings/construct/?"),
            async (kCtx) =>
            {
                ConstructMetadata? constr = CurrentConstruct;
                await KantabHttpFileServer.ServeText(kCtx, JsonSerializer.Serialize(constr),
                    "application/json");
            }
        );

        _httpHandler.Get(new Regex(@"^views/.*$/?$"),
            async (kCtx) =>
            {
                await KantabHttpFileServer.ServeDirectory(kCtx,
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Views"),
                    1);
            }
        );
    }

}