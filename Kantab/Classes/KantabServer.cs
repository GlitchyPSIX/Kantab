using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Kantab.Classes.Messages.Common;
using Kantab.Classes.PenStateProviders;
using Kantab.Classes.Router;
using Kantab.Interfaces;
using Kantab.Structs;
using Kantab.WinAPI.Structs;

namespace Kantab.Classes;

public class KantabServer
{

    public short Port = 8337;
    private string _prefix => $"http://localhost:{Port}/";

    private HttpListener _listener;

    private KantabHttpHandler _httpHandler;
    private List<KantabClient> _clients = new();
    public int ConnectedClients => _clients.Count;

    public event EventHandler<KantabClient> ClientConnected;
    public event EventHandler<KantabClient> ClientDisconnected;

    private KantabSettings _loadedSettings;
    public IPenStateProvider PenStateProvider = new MousePenStateProvider();
    private Task _httpServerTask;

    private System.Timers.Timer _positionFetchTimer;

    public KantabServer(KantabSettings? config) {
        float fetchRate;
        if (config.HasValue) _loadedSettings = config.Value;
        Port = config?.Port ?? Port;
        fetchRate = config?.FetchRate ?? 12;
        _positionFetchTimer = new();

        if (_loadedSettings.ScreenRegion.Empty) _loadedSettings.ScreenRegion = new Rectangle(1920, 0, 1920*2, 1080);
        _positionFetchTimer.Elapsed += BroadcastPenState;
        _positionFetchTimer.Interval = fetchRate;
    }

    public void Start() {
        _positionFetchTimer.Start();
        Task.Run(BeginHttpServer);
    }

    private async Task BeginHttpServer()
    {
        _listener = new HttpListener();
        _httpHandler = new KantabHttpHandler();
        _httpHandler.Init();
        _listener.Prefixes.Add(_prefix);
        _listener.Start();

        while (true)
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
    }

    private void BroadcastPenState(object? sender, ElapsedEventArgs args) {
        foreach (KantabClient client in _clients) {
            if (!client.Ready) continue;
            client.SendMessage(new PenInformationMessage(false, PenStateProvider.CurrentPenState, _loadedSettings.ScreenRegion));
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

}