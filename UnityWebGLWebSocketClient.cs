using AardaLibrary;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class UnityWebGLWebSocketClient : ISocketClient
{
    public event Action OnOpen;
    public event Action<string> OnError;
    public event Action OnClose;
    public event Action<byte[]> OnAudioMessage;
    public event Action<string> OnTextMessage;

    // We store the socket ID returned from JavaScript side
    private int socketId = -1;

    // In a static dictionary, map socketId -> client instance,
    // so callback functions can look up the correct instance.
    private static readonly Dictionary<int, UnityWebGLWebSocketClient> clients =
        new Dictionary<int, UnityWebGLWebSocketClient>();

    // Flag to ensure we only register callbacks once
    private static bool isCallbacksRegistered = false;

    // ----------------------------------------------------------------------
    // 1) Declare delegates for the callback signatures we expect from JS
    // ----------------------------------------------------------------------
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnOpenCallback(int socketId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnTextMessageCallback(int socketId, IntPtr messagePtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnAudioMessageCallback(int socketId, IntPtr dataPtr, int length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnErrorCallback(int socketId, IntPtr errorPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OnCloseCallback(int socketId);

    // ----------------------------------------------------------------------
    // 2) Mark them with AOT.MonoPInvokeCallback to avoid GC in IL2CPP
    // ----------------------------------------------------------------------
    [AOT.MonoPInvokeCallback(typeof(OnOpenCallback))]
    private static void HandleOnOpen(int socketId)
    {
        if (clients.TryGetValue(socketId, out var client))
        {
            client.OnOpen?.Invoke();
        }
    }

    [AOT.MonoPInvokeCallback(typeof(OnAudioMessageCallback))]
    private static void HandleOnAudioMessage(int socketId, IntPtr dataPtr, int length)
    {
        if (clients.TryGetValue(socketId, out var client))
        {
            // Copy raw bytes from dataPtr into a managed byte array
            byte[] audioBytes = new byte[length];
            Marshal.Copy(dataPtr, audioBytes, 0, length);

            // Now invoke whatever handler the client has for audio
            client.OnAudioMessage?.Invoke(audioBytes);
        }
    }

    [AOT.MonoPInvokeCallback(typeof(OnTextMessageCallback))]
    private static void HandleOnTextMessage(int socketId, IntPtr audioPtr)
    {
        if (clients.TryGetValue(socketId, out var client))
        {
            string error = Marshal.PtrToStringAnsi(audioPtr);
            client.OnTextMessage?.Invoke(error);
        }
    }

    [AOT.MonoPInvokeCallback(typeof(OnErrorCallback))]
    private static void HandleOnError(int socketId, IntPtr errorPtr)
    {
        if (clients.TryGetValue(socketId, out var client))
        {
            string error = Marshal.PtrToStringAnsi(errorPtr);
            client.OnError?.Invoke(error);
        }
    }

    [AOT.MonoPInvokeCallback(typeof(OnCloseCallback))]
    private static void HandleOnClose(int socketId)
    {
        if (clients.TryGetValue(socketId, out var client))
        {
            client.OnClose?.Invoke();
        }
        clients.Remove(socketId);
    }

    // ----------------------------------------------------------------------
    // 3) Create a function to register these callbacks on the JS side
    // ----------------------------------------------------------------------
    [DllImport("__Internal")]
    private static extern void RegisterSocketCallbacks(
        OnOpenCallback onOpen,
        OnAudioMessageCallback onAudioMessage,
        OnTextMessageCallback onTextMessage,
        OnErrorCallback onError,
        OnCloseCallback onClose
    );

    // ----------------------------------------------------------------------
    // 4) The actual JS functions for connect, send, and close
    // ----------------------------------------------------------------------
    [DllImport("__Internal")]
    private static extern int WebSocketConnect(string url);

    [DllImport("__Internal")]
    private static extern void WebSocketSend(int socketId, string message);

    [DllImport("__Internal")]
    private static extern void WebSocketClose(int socketId);

    // ----------------------------------------------------------------------
    // Initialize & Register Callbacks Once
    // ----------------------------------------------------------------------
    private void EnsureCallbacksAreRegistered()
    {
        if (!isCallbacksRegistered)
        {
            RegisterSocketCallbacks(
                HandleOnOpen,
                HandleOnAudioMessage,
                HandleOnTextMessage,
                HandleOnError,
                HandleOnClose
            );
            isCallbacksRegistered = true;
        }
    }

    // ----------------------------------------------------------------------
    // Connect
    // ----------------------------------------------------------------------
    public async Task ConnectAsync(string url)
    {
        EnsureCallbacksAreRegistered();
        socketId = WebSocketConnect(url);
        clients[socketId] = this;

        var tcs = new TaskCompletionSource<bool>();

        Action onOpenHandler = null;
        Action<string> onErrorHandler = null;
        Action onCloseHandler = null;

        onOpenHandler = () =>
        {
            OnOpen -= onOpenHandler;
            OnError -= onErrorHandler;  // unsubscribe
            OnClose -= onCloseHandler;
            tcs.TrySetResult(true);
        };

        onErrorHandler = (errorMsg) =>
        {
            OnOpen -= onOpenHandler;
            OnError -= onErrorHandler;  
            OnClose -= onCloseHandler;
            tcs.TrySetException(new Exception("WebSocket error: " + errorMsg));
        };

        onCloseHandler = () =>
        {
            OnOpen -= onOpenHandler;
            OnError -= onErrorHandler;
            OnClose -= onCloseHandler;
            tcs.TrySetException(new Exception("WebSocket closed before OnOpen."));
        };

        OnOpen += onOpenHandler;
        OnError += onErrorHandler;
        OnClose += onCloseHandler;

        await tcs.Task;
    }



    // ----------------------------------------------------------------------
    // Send (Text)
    // ----------------------------------------------------------------------
    public async Task SendTextAsync(string message)
    {
        if (socketId >= 0)
        {
            WebSocketSend(socketId, message);
        }
        else
        {
            Debug.LogWarning("SendTextAsync: Socket not connected or invalid socketId.");
        }
        await Task.CompletedTask;
    }

    // ----------------------------------------------------------------------
    // Close
    // ----------------------------------------------------------------------
    public async Task CloseAsync()
    {
        if (socketId >= 0)
        {
            WebSocketClose(socketId);
        }
        await Task.CompletedTask;
    }
}
