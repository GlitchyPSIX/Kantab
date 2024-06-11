using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Kantab.Classes.Extensions;
using Kantab.Classes.Messages;
using Kantab.Classes.Messages.Common;
using Kantab.Enums;
using Timer = System.Timers.Timer;

namespace Kantab.Classes; 

public class KantabClient {

    private KantabServer _myServer;
    private WebSocket _clientSocket;
    public ClientFeatures Features;

    private bool _ready;
    private int _accumulatedWhoops = 0;
    public int MissedHeartbeats { get; private set; } = 0;

    private Timer _heartbeatTimer;

    public EventHandler<bool> OnClientConnectionEstablished;
    public EventHandler<bool> OnClientDisconnect;

    private CancellationTokenSource _tkn;

    public KantabClient(KantabServer server, WebSocket clientSocket) {
        _myServer = server;
        _clientSocket = clientSocket;
        _heartbeatTimer = new Timer(1000);
        _heartbeatTimer.Stop();
        _heartbeatTimer.Elapsed += OnHeartbeatTick;
        _tkn = new CancellationTokenSource();

        Task.Run(ReceiveLoop, _tkn.Token);
    }

    const int MEMORY_BUFFER_LENGTH = 32;

    async void ReceiveLoop()
    {
        var loopToken = _tkn.Token;
        MemoryStream outputStream = new MemoryStream(32);
        byte[] buffer = new byte[MEMORY_BUFFER_LENGTH];
        try
        {
            while (!loopToken.IsCancellationRequested)
            {
                WebSocketReceiveResult receiveResult;
                outputStream.Position = 0;
                do
                {
                    receiveResult = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), loopToken);
                    if (receiveResult.MessageType != WebSocketMessageType.Close)
                        await outputStream.WriteAsync(buffer, 0, receiveResult.Count, loopToken);
                } while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType == WebSocketMessageType.Close) {
                    _tkn.Cancel();
                    break;
                }
                outputStream.Position = 0;
                ProcessKantabMessage(new (outputStream.ToArray(), 0, receiveResult.Count));
                await outputStream.WriteAsync((new byte[MEMORY_BUFFER_LENGTH]).AsMemory(0, MEMORY_BUFFER_LENGTH), loopToken);
            }
        }
        catch (TaskCanceledException) { OnClientDisconnect?.Invoke(this, true); }
        catch (WebSocketException) { _tkn.Cancel(); OnClientDisconnect?.Invoke(this, true); }
        finally
        {
            outputStream?.Dispose();
        }
    }

    private void ProcessKantabMessage(ArraySegment<byte> buf) {
        KantabMessage msg = KantabMessage.FromBytes(buf);

        switch (msg) {
            case HelloMessage hello:
            {
                _ready = true;
                _heartbeatTimer.Start();
                break;
            }
        }

        Console.WriteLine("Received: " + string.Join(" ", buf.Select(x => x.ToString("X2")).ToArray()));
    }
    
    public async void SendMessage(KantabMessage message) {
        if (_clientSocket.State != WebSocketState.Open) return;
        if (_ready) return;

        await _clientSocket.SendKantabMessage(message);

    }

    private async void OnHeartbeatTick(object? sender, ElapsedEventArgs args) {
        if (_clientSocket.State != WebSocketState.Open) return;
        if (!_ready) return;

        _accumulatedWhoops = 0;
        await _clientSocket.SendKantabMessage(new PingMessage());
    }
}