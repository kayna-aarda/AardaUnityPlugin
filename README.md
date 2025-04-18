# AardaLibrary

The AardaLibrary is a platform-agnostic .NET library that provides logic and functionality for connecting to a chat service via HTTP and WebSocket. It is designed to integrate seamlessly into Unity projects or other .NET environments without including platform-specific dependencies or UI logic.

## Overview

**AardaLibrary** encapsulates all business logic and data models, leaving the Unity side responsible only for view and minimal configuration. This approach promotes clean architecture, improved testability, and easier maintenance.

**Core features:**
- Retrieve API keys securely via HTTP.
- Fetch a list of characters and their associated data from a backend.
- Connect to a WebSocket endpoint for bi-directional chat communication.
- Initialize chat sessions with various parameters.
- Send text and audio messages through the connected WebSocket.
- Exposes a simple event model for handling incoming messages and connection states.

## Key Components

### AardaClient
`AardaClient` is the main entry point for using the library. It provides asynchronous methods for:
- `FetchApiKeyAsync(username, password)`
- `FetchCharactersAsync()`
- `ConnectWebSocketAsync(wsUrl, sessionId = 0)`
- `InitializeChatSessionAsync(args)`
- `SendChatMessageAsync(message)`
- `SendChatAudioAsync(audioBytes)`
- `CloseAsync()`

It also exposes events:
- `OnWebSocketOpen`
- `OnWebSocketClose`
- `OnWebSocketError`
- `OnTextMessageReceived`
- `OnAudioMessageReceived`

### HttpApiClient & ISocketClient Implementations
- **HttpApiClient**: Uses `HttpClient` to handle HTTP requests for API key retrieval and fetching characters.
- **WebSocketClient**: Uses `System.Net.WebSockets` for establishing and maintaining WebSocket connections.

Both `IApiClient` and `ISocketClient` are interfaces, making it easy to replace or mock these components if needed.

### Models
The library includes strongly-typed model classes (`Character`, `InitSessionArgs`, `MessageResponse`, etc.) to handle data responses from the server.

## Using the Library in Unity

**Aarda_Service.cs** acts as the Unity-side adapter for `AardaClient`. It is a `MonoBehaviour` that you add to a GameObject in your Unity scene. You configure it by setting the fields in the Unity Inspector:

```csharp
[Header("Configuration")]
public Config config; // Contains HttpApiUrl, WsApiUrl, ApiUsername, ApiPassword
```

# Aarda_Service Unity Integration Guide

## Overview
Once you've set these values, `Aarda_Service` automatically:

- Fetches the API key on startup.
- Provides simple methods and coroutines to call library functionality from Unity scripts.
- Logs events via `Debug.Log` for quick verification.

---

## Example Unity Setup Steps

### 1. Add the Library to Unity
- Include the compiled `AardaLibrary.dll` in your Unity project's `Assets/Plugins` (or another suitable folder).

### 2. Attach Aarda_Service to a GameObject
- Create an empty GameObject in your scene (e.g., "AardaManager").
- Add `Aarda_Service.cs` to it.
- In the **Inspector**, set:
  - `HttpApiUrl`
  - `WsApiUrl`
  - `ApiUsername`
  - `ApiPassword`

### 3. Play Mode
- Press **Play** in the Unity Editor.
- `Aarda_Service` fetches the API key automatically and logs successes or errors.

---

## Interacting with AardaClient from Unity

Use `Aarda_Service.Instance` to access methods:

```csharp
// Connect to WebSocket
Aarda_Service.Instance.ConnectWebSocket(onConnected: () => Debug.Log("Connected to WebSocket"));

// Initialize a chat session
Aarda_Service.Instance.InitializeSession(new InitSessionArgs
{
    userUuid = "123e4567-e89b-12d3-a456-426614174000",
    characterId = 1,
    playerId = 42,
    sceneId = 1001,
    mood = "happy",
    language = "en",
    audioSupport = true
},
onInitialized: () => Debug.Log("Session Initialized"));

// Fetch characters
Aarda_Service.Instance.FetchCharacters(chars =>
{
    Debug.Log("Fetched characters: " + chars.Count);
});

// Send a text message
Aarda_Service.Instance.SendChatMessage("Hello!", onSent: () => Debug.Log("Message sent"));

// Close the connection
Aarda_Service.Instance.CloseConnection(onClosed: () => Debug.Log("WebSocket closed"));
```