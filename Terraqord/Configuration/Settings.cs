using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Terraqord.Configuration
{
    public class Settings
    {
        [JsonPropertyName("bot-token")]
        public string BotToken { get; set; } = string.Empty;

        [JsonPropertyName("db-token")]
        public string DatabaseToken { get; set; } = string.Empty;

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

        public static Settings Read()
        {
            var path = Path.Combine(TShock.SavePath, Config.FilePath);

            if (!File.Exists(path))
            {
                var obj = new Settings();
                var content = JsonSerializer.Serialize(obj, Config.JsonConfig);

                File.WriteAllText(path, content);

                return obj;
            }

            return JsonSerializer.Deserialize<Settings>(path, Config.JsonConfig) ?? new Settings();
        }
    }
}
