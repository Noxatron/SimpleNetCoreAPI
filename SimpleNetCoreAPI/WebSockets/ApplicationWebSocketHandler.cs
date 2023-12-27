using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace SimpleNetCoreAPI.WebSockets
{
    public class ApplicationWebSocketHandler
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public async Task HandleConnectionAsync(WebSocket webSocket)
        {
            string socketId = Guid.NewGuid().ToString();
            _sockets.TryAdd(socketId, webSocket);

            await Receive(webSocket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Handle text messages received from the client here
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await DisconnectAsync(socketId);
                }
            });
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            byte[] buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        public async Task NotifyClientsOfDataChangeAsync(string message)
        {
            foreach (var pair in _sockets)
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await pair.Value.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private async Task DisconnectAsync(string socketId)
        {
            if (_sockets.TryRemove(socketId, out var socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }
    }
}
