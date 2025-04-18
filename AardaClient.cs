using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AardaLibrary
{
    public class AardaClient
    {
        private readonly IApiClient _apiClient;
        private readonly ISocketClient _socketClient;

        public string ApiKey { get; private set; }
        public List<Character> Characters { get; private set; }

        public event Action<InitializeResponse> OnInitializeReceived;
        public event Action<TranscriptResponse> OnTranscriptReceived;
        public event Action<MessageResponse> OnMessageReceived;
        public event Action<byte[]> OnAudioMessageReceived;
        public event Action OnWebSocketOpen;
        public event Action<string> OnWebSocketError;
        public event Action OnWebSocketClose;

        public AardaClient(IApiClient apiClient, ISocketClient socketClient)
        {
            _apiClient = apiClient;
            _socketClient = socketClient;

            // Hook socket events
            _socketClient.OnOpen += () => OnWebSocketOpen?.Invoke();
            _socketClient.OnError += (err) => OnWebSocketError?.Invoke(err);
            _socketClient.OnClose += () => OnWebSocketClose?.Invoke();
            _socketClient.OnTextMessage += (message) => ProcessSocketMessage(message);
            _socketClient.OnAudioMessage += (audio) => OnAudioMessageReceived?.Invoke(audio);
        }

        public async Task FetchApiKeyAsync(string username, string password)
        {
            ApiKey = await _apiClient.FetchApiKeyAsync(username, password);
        }

        public async Task FetchCharactersAsync()
        {
            Characters = await _apiClient.FetchCharactersAsync(ApiKey);
        }

        public Character GetCharacterByName(string characterName)
        {
            if (Characters == null)
                throw new InvalidOperationException("Characters not loaded");
            return Characters.Find(c => c.Name == characterName);
        }

        public async Task ConnectWebSocketAsync(string wsUrl, int sessionId = 0)
        {
            Console.WriteLine($"Connecting to WebSocket at {wsUrl}...");
            string url = (sessionId != 0) ? $"{wsUrl}?session_id={sessionId}" : wsUrl;

            // This line will NOT proceed until the socket is actually open
            try
            {
                await _socketClient.ConnectAsync(url);
                
                // Now you can safely send the API key
                var apiObj = new { api_token = ApiKey };
                var jsonString = JsonConvert.SerializeObject(apiObj);
                await _socketClient.SendTextAsync(jsonString);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to connect: {e}");
            }

        }

        public async Task InitializeChatSessionAsync(InitSessionArgs args)
        {
            string initPayload = GetInitializationString(args);
            await _socketClient.SendTextAsync(initPayload);
        }

        public async Task CloseAsync() {
            await _socketClient.CloseAsync();
        }

        public async Task SendChatMessageAsync(string message, string userUuid, int sessionId)
        {
            var payload = new
            {
                type = "message",
                message = message,
                user_uuid = userUuid,
                session_id = sessionId
            };
            string stringPayload = JsonConvert.SerializeObject(payload);
            await _socketClient.SendTextAsync(stringPayload);
        }

        public async Task SendChatAudioAsync(byte[] audioBytes, string userUuid, int sessionId, string encoding = "audio/pcm")
        {
            string base64Audio = Convert.ToBase64String(audioBytes);
            string stringPayload = $"{{\"type\":\"audio\",\"data\":\"{base64Audio}\",\"encoding\":\"{encoding}\",\"user_uuid\":\"{userUuid}\",\"session_id\":{sessionId}}}";
            await _socketClient.SendTextAsync(stringPayload);
        }

        private void ProcessSocketMessage(string message)
        {
            try
            {
                Console.WriteLine($"Processing message: {message}");

                // Deserialize only to grab the 'source' (discriminator)
                var baseMessage = JsonConvert.DeserializeObject<BaseMessage>(message);
                Console.WriteLine($"Base message: {baseMessage}");

                if (string.IsNullOrEmpty(baseMessage?.Source))
                {
                    Console.WriteLine("Message does not contain a 'source' property");
                    OnWebSocketError?.Invoke("Message does not contain a 'source' property");
                    return;
                }
                Console.WriteLine($"Base message source: {baseMessage.Source}");

                switch (baseMessage.Source)
                {
                    case "initialize_response":
                        var initializeResponse = JsonConvert.DeserializeObject<InitializeResponse>(message);
                        Console.WriteLine($"Initialize response: {initializeResponse}");
                        OnInitializeReceived?.Invoke(initializeResponse);
                        break;
                    case "transcription":
                        var transcriptResponse = JsonConvert.DeserializeObject<TranscriptResponse>(message);
                        Console.WriteLine($"Transcript response: {transcriptResponse}");
                        OnTranscriptReceived?.Invoke(transcriptResponse);
                        break;
                    case "text_response":
                        var messageResponse = JsonConvert.DeserializeObject<MessageResponse>(message);
                        Console.WriteLine($"Message response: {messageResponse}");
                        OnMessageReceived?.Invoke(messageResponse);
                        break;
                    default:
                        OnWebSocketError?.Invoke($"Unknown message source: {baseMessage.Source}");
                        break;
                }
            }
            catch (JsonReaderException jex)
            {
                OnWebSocketError?.Invoke($"Message is not valid JSON: {jex.Message}");
            }
            catch (Exception ex)
            {
                OnWebSocketError?.Invoke($"Message processing error: {ex.Message}");
            }
        }


        private bool IsJson(string data)
        {
            data = data.Trim();
            return (data.StartsWith("{") && data.EndsWith("}")) ||
                   (data.StartsWith("[") && data.EndsWith("]"));
        }

        private string GetInitializationString(InitSessionArgs args)
        {
            var initializeObject = new
            {
                type = "initialize",
                user_uuid = args.userUuid,
                mood = args.mood,
                characterId = args.characterId,
                playerId = args.playerId,
                sceneId = args.sceneId,
                audioSupport = args.audioSupport,
                language = args.language
            };
            return JsonConvert.SerializeObject(initializeObject);
        }
    }
}
