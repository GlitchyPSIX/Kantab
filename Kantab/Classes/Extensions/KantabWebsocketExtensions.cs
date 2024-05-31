using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Kantab.Classes.Messages;
using Kantab.Classes.Messages.Common;

namespace Kantab.Classes.Extensions; 

public static class KantabWebsocketExtensions {
    public static async Task SendKantabMessage(this WebSocket ws, KantabMessage message) {
        await ws.SendAsync(new ArraySegment<byte>(message.ToBytes()), WebSocketMessageType.Binary, true, CancellationToken.None);
    }
}