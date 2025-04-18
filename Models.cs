using System.Collections.Generic;
using Newtonsoft.Json;

namespace AardaLibrary
{
    public class Character
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        [JsonProperty("life_period")]
        public string LifePeriod { get; set; }
        public string Origin { get; set; }
        public string Role { get; set; }
        [JsonProperty("core_description")]
        public string CoreDescription { get; set; }
        public string Motivation { get; set; }
        public string Flaws { get; set; }
        public string Backstory { get; set; }
        public string Species { get; set; }
        public string Hobbies { get; set; }
        public string SpeechAdjectives { get; set; }
        public string Traits { get; set; }
        public string Appearance { get; set; }
        public string Voice { get; set; }
        public int Id { get; set; }
        public List<KnowledgeBrick> KnowledgeBricks { get; set; }
        public List<object> Groups { get; set; }
    }

    public class KnowledgeBrick
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Permission { get; set; }
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public List<object> Children { get; set; }
    }

    public class InitSessionArgs
    {
        public string userUuid;
        public int characterId;
        public int playerId;
        public int sceneId;
        public string mood;
        public string language;
        public bool audioSupport;
    }

    public class OptionsResponse
    {
        public List<string> knowledge_blocks { get; set; }
        public List<string> characters { get; set; }
        public List<string> players { get; set; }
    }

    public class TokenResponse
    {
        public string access_token;
        public string token_type;
    }

    public class UserData 
    {
        public string username;
        public string password;
    }

    public class BaseMessage
    {
        [JsonProperty("source")]
        public string Source { get; set; }
    }

    public class InitializeResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("user_uuid")]
        public string UserUuid { get; set; }

        [JsonProperty("session_id")]
        public int SessionId { get; set; }
    }

    public class TranscriptResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("response")]
        public string Response { get; set; }
    }

    public class EmotionState
    {
        [JsonProperty("joy_sadness")]
        public float JoySadness { get; set; }

        [JsonProperty("trust_disgust")]
        public float TrustDisgust { get; set; }

        [JsonProperty("fear_anger")]
        public float FearAnger { get; set; }

        [JsonProperty("surprise_anticipation")]
        public float SurpriseAnticipation { get; set; }
    }

    public class MessageResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("response")]
        public string Response { get; set; }

        [JsonProperty("flags_player")]
        public List<string> FlagsPlayer { get; set; }

        [JsonProperty("flags_character")]
        public List<string> FlagsCharacter { get; set; }

        [JsonProperty("tokens_spent")]
        public int TokensSpent { get; set; }

        [JsonProperty("immediate_emotion")]
        public EmotionState ImmediateEmotion { get; set; }

        [JsonProperty("accumulated_emotion")]
        public EmotionState AccumulatedEmotion { get; set; }
    }

}