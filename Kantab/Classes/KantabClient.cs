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
using Kantab.Classes.PenStateProviders;
using Kantab.Enums;
using Kantab.Interfaces;
using Kantab.Structs;
using Timer = System.Timers.Timer;

namespace Kantab.Classes;

public class KantabClient {

    private KantabServer _myServer;
    private WebSocket _clientSocket;
    private SemaphoreSlim _wsSemaphore = new(1, 1);
    public ClientFeatures Features;

    private int _accumulatedWhoops = 0;

    public bool Ready { get; private set; }
    /// <summary>
    /// True if this client serves as RELAY_AUTHORITY
    /// </summary>
    public bool IsRelay => Features.HasFlag(ClientFeatures.RELAY_AUTHORITY);

    public int MissedHeartbeats { get; private set; } = 0;

    private Timer _heartbeatTimer;

    public EventHandler<bool> OnClientConnectionEstablished;
    public EventHandler<bool> OnClientDisconnect;
    /// <summary>
    /// Invoked when the server responds with RELAY_AUTHORITY in C 03 Capabilities
    /// </summary>
    public EventHandler RelayUpgrade;
    public EventHandler<PenState> PositionReceived;


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

    async void ReceiveLoop() {
        var loopToken = _tkn.Token;
        MemoryStream outputStream = new MemoryStream(32);
        byte[] buffer = new byte[MEMORY_BUFFER_LENGTH];
        try {
            while (!loopToken.IsCancellationRequested) {
                WebSocketReceiveResult receiveResult;
                outputStream.Position = 0;
                do {
                    receiveResult = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), loopToken);
                    if (receiveResult.MessageType != WebSocketMessageType.Close)
                        await outputStream.WriteAsync(buffer, 0, receiveResult.Count, loopToken);
                } while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType == WebSocketMessageType.Close) {
                    _tkn.Cancel();
                    break;
                }
                outputStream.Position = 0;
                ProcessKantabMessage(new(outputStream.ToArray(), 0, receiveResult.Count));
                await outputStream.WriteAsync((new byte[MEMORY_BUFFER_LENGTH]).AsMemory(0, MEMORY_BUFFER_LENGTH), loopToken);
            }
        }
        catch (TaskCanceledException) { OnClientDisconnect?.Invoke(this, true); }
        catch (WebSocketException) { _tkn.Cancel(); OnClientDisconnect?.Invoke(this, true); }
        finally {
            outputStream?.Dispose();
        }
    }

    private void ProcessKantabMessage(ArraySegment<byte> buf) {
        KantabMessage msg = KantabMessage.FromBytes(buf);

        switch (msg) {
            case HelloMessage hello: {
                    Ready = true;
                    _heartbeatTimer.Start();
                    break;
                }
            case CapabilitiesMessage caps: {
                    if (caps.Features.HasFlag(ClientFeatures.RELAY_AUTHORITY)) {
                        RelayUpgrade?.Invoke(this, EventArgs.Empty);
                    };
                    Features = caps.Features;
                    break;
                }
            case PenInformationMessage penInfo: {

                    RelayPenState(penInfo.State);
                    break;
                }
        }

        Console.WriteLine("Received: " + string.Join(" ", buf.Select(x => x.ToString("X2")).ToArray()));
    }

    public async void SendMessage(KantabMessage message, bool overrideReady = false) {
        if (_clientSocket.State != WebSocketState.Open) return;
        if (!Ready && !overrideReady) return;

        await _wsSemaphore.WaitAsync();
        try {
            await _clientSocket.SendKantabMessage(message);
        }
        finally {
            _wsSemaphore.Release();
        }
    }

    private void RelayPenState(PenState ps) {
        if (IsRelay)
            PositionReceived?.Invoke(this, ps);
    }

    private async void OnHeartbeatTick(object? sender, ElapsedEventArgs args) {
        if (_clientSocket.State != WebSocketState.Open) return;
        if (!Ready) return;

        _accumulatedWhoops = 0;
        await _wsSemaphore.WaitAsync();
        try {
            await _clientSocket.SendKantabMessage(new PingMessage());
        }
        finally {
            _wsSemaphore.Release();
        }

    }
}