using Auxiliary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Terraqord.Configuration
{
    public class TerraqordSettings : ISettings
    {
        [JsonPropertyName("bot-token")]
        public string BotToken { get; set; } = string.Empty;

        [JsonPropertyName("chat-webhook")]
        public string ServerHook { get; set; } = string.Empty;

        [JsonPropertyName("log-webhook")]
        public string LoggingHook { get; set; } = string.Empty;

        [JsonPropertyName("server-ip")]
        public string JoinIp { get; set; } = string.Empty;

        [JsonPropertyName("listen-channel")]
        public ulong Channel { get; set; }

        [JsonPropertyName("register-commands")]
        public bool AllowRegistration { get; set; }
    }
}
