using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AardaLibrary
{
    public class WebSocketClient : ISocketClient
    {
        public event Action OnOpen;
        public event Action<string> OnError;
        public event Action OnClose;
        public event Action<string> OnTextMessage;
        public event Action<byte[]> OnAudioMessage;

        private ClientWebSocket _client;

        public async Task ConnectAsync(string url)
        {
            _client = new ClientWebSocket();
            try
            {
                await _client.ConnectAsync(new Uri(url), CancellationToken.None);
                OnOpen?.Invoke();
                _ = ReceiveLoop(); // Start listening without blocking
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
            }
        }

        public async Task SendTextAsync(string message)
        {
            if (_client.State == WebSocketState.Open)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                await _client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                OnError?.Invoke("WebSocket is not open.");
            }
        }

        public async Task CloseAsync()
        {
            if (_client != null && _client.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await _client.CloseAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                    "Client requested close",
                    System.Threading.CancellationToken.None
                );
            }
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (_client.State == WebSocketState.Open)
                {
                    var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        OnClose?.Invoke();
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var msgBytes = new byte[result.Count];
                        Array.Copy(buffer, msgBytes, result.Count);
                        OnTextMessage?.Invoke(System.Text.Encoding.UTF8.GetString(msgBytes));
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        var msgBytes = new byte[result.Count];
                        Array.Copy(buffer, msgBytes, result.Count);
                        OnAudioMessage?.Invoke(msgBytes);
                    }
                    else
                    {
                        OnError?.Invoke("Unknown message type");
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
        }
    }
}
