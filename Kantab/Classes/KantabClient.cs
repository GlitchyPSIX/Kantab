using System;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Kantab.Classes.Messages.Common;
using Kantab.Enums;
using Timer = System.Timers.Timer;

namespace Kantab.Classes; 

public class KantabClient {
    private WebSocket _clientSocket;
    public ClientFeatures Features;

    private bool _ready;
    private int _accumulatedWhoops = 0;
    public int MissedHeartbeats { get; private set; } = 0;

    private Timer _heartbeatTimer;

    public EventHandler<bool> OnClientConnectionEstablished;
    public EventHandler<bool> OnClientDisconnect;

    private CancellationTokenSource _tkn;

    public KantabClient(WebSocket clientSocket) {
        _clientSocket = clientSocket;
        _heartbeatTimer = new Timer(0);
        _heartbeatTimer.Stop();
        _heartbeatTimer.Elapsed += OnHeartbeatTick;
        _tkn = new CancellationTokenSource();

        Task.Run(ReceiveLoop, _tkn.Token);
    }

    async void ReceiveLoop()
    {
        var loopToken = _tkn.Token;
        MemoryStream outputStream = null;
        WebSocketReceiveResult receiveResult = null;
        byte[] buffer = new byte[32];
        try
        {
            while (!loopToken.IsCancellationRequested)
            {
                outputStream = new MemoryStream(32);
                do
                {
                    receiveResult = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), loopToken);
                    if (receiveResult.MessageType != WebSocketMessageType.Close)
                        outputStream.Write(buffer, 0, receiveResult.Count);
                } while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                outputStream.Position = 0;
                ProcessKantabMessage(outputStream.ToArray());
            }
        }
        catch (TaskCanceledException) { OnClientDisconnect?.Invoke(this, true); }
        catch (WebSocketException) { _tkn.Cancel(); OnClientDisconnect?.Invoke(this, true); }
        finally
        {
            outputStream?.Dispose();
        }
    }

    private void ProcessKantabMessage(byte[] buf) {
        // NEXT: Processing of client messages
    }

    private async void OnHeartbeatTick(object? sender, ElapsedEventArgs args) {
        if (_clientSocket.State != WebSocketState.Open) return;
        if (_ready) return;

        _accumulatedWhoops = 0;
        await _clientSocket.SendAsync(new ArraySegment<byte>(new PingMessage().ToBytes()), WebSocketMessageType.Binary, true, CancellationToken.None);
    }
}