using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using Kantab.Classes.Messages.Common;
using Kantab.Classes.Router;
using Kantab.WinAPI.Structs;

namespace Kantab.Classes; 

public class KantabServer {

    public short Port = 8337;
    private string _prefix => $"http://localhost:{Port}/";

    private HttpListener _listener;

    private KantabHttpHandler _httpHandler;
    private List<KantabClient> _clients = new();

    public KantabServer(KantabServerConfig? config) {
        Port = config?.Port ?? Port;
    }

    internal async void BeginHttpServer() {
        _listener = new HttpListener();
        _httpHandler = new KantabHttpHandler();
        _httpHandler.Init();
        _listener.Prefixes.Add(_prefix);
        _listener.Start();
        Console.WriteLine("kantab server started");
        while (true) {
            HttpListenerContext httpCtx = await _listener.GetContextAsync();
            if (httpCtx.Request.IsWebSocketRequest) {
                UpgradeToWebsocket(httpCtx);
            }
            else {
                _httpHandler.RunHandler(httpCtx);
            }
        }
        
        // ReSharper disable once FunctionNeverReturns
    }

    internal async void HttpStub(HttpListenerContext ctx) {

    }

    internal async void UpgradeToWebsocket(HttpListenerContext ctx) {

        Console.WriteLine("upgrading new connection to websocket");
        try {
            WebSocketContext wsCtx = await ctx.AcceptWebSocketAsync("kantab-v1");
            KantabClient newClient = new KantabClient(this, wsCtx.WebSocket);
            newClient.OnClientDisconnect += (s, graceful) => { _clients.Remove(newClient); };
            _clients.Add(newClient);
            newClient.SendMessage(new HelloMessage());
        }
        catch (Exception e) {
            ctx.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/json";
            StreamWriter sw = new StreamWriter(ctx.Response.OutputStream);
            await sw.WriteAsync("{\"error\": \"black hole\"}");
            ctx.Response.Close();
            return;
        }
    }

    // NEXT: Do basic webserver and websocket upgrade


}