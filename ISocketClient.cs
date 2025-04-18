using System;
using System.Threading.Tasks;

namespace AardaLibrary
{
    public interface ISocketClient
    {
        event Action OnOpen;
        event Action<string> OnError;
        event Action OnClose;
        event Action<string> OnTextMessage;
        event Action<byte[]> OnAudioMessage;

        Task ConnectAsync(string url);
        Task SendTextAsync(string message);
        Task CloseAsync();
    }
}
