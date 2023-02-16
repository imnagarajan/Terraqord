using Auxiliary.Configuration;
using System.Text.Json.Serialization;

namespace Terraqord.Configuration
{
    public class TerraqordSettings : ISettings
    {
        /// <summary>
        ///     Bot config.
        /// </summary>
        [JsonPropertyName("bot")]
        public BotInformation Bot { get; set; } = new();

        /// <summary>
        ///     Server config.
        /// </summary>
        [JsonPropertyName("server")]
        public ServerInformation Server { get; set; } = new();

        /// <summary>
        ///     Webhook config.
        /// </summary>
        [JsonPropertyName("webhooks")]
        public WebhookInformation Webhooks { get; set; } = new();

        /// <summary>
        ///     Main config.
        /// </summary>
        [JsonPropertyName("channels")]
        public ChannelInformation Channels { get; set; } = new();

        /// <summary>
        ///     Join/Leave config.
        /// </summary>
        [JsonPropertyName("retention")]
        public RetentionInformation Retention { get; set; } = new();

        /// <summary>
        ///     Public api config.
        /// </summary>
        [JsonPropertyName("api")]
        public ApiInformation Api { get; set; } = new();
    }

    public class BotInformation
    {
        /// <summary>
        ///     If the bot should register commands.
        /// </summary>
        [JsonPropertyName("register-commands")]
        public bool AllowRegistration { get; set; } = false;

        /// <summary>
        ///     The bot token.
        /// </summary>
        [JsonPropertyName("bot-token")]
        public string Token { get; set; } = string.Empty;
    }

    public class ServerInformation
    {
        /// <summary>
        ///     The server IP.
        /// </summary>
        [JsonPropertyName("ip")]
        public string ServerIp { get; set; } = string.Empty;

        /// <summary>
        ///     The server name.
        /// </summary>
        [JsonPropertyName("server-name")]
        public string ServerName { get; set; } = string.Empty;

        /// <summary>
        ///     The bridge name.
        /// </summary>
        [JsonPropertyName("bridge-name")]
        public string BridgeName { get; set; } = string.Empty;
    }

    public class WebhookInformation
    {
        /// <summary>
        ///     Main channel webhook.
        /// </summary>
        [JsonPropertyName("main")]
        public string Main { get; set; } = string.Empty;

        /// <summary>
        ///     Logging webhook.
        /// </summary>
        [JsonPropertyName("logging")]
        public string Logging { get; set; } = string.Empty;

        /// <summary>
        ///     Staff webhook.
        /// </summary>
        [JsonPropertyName("staff")]
        public string Staff { get; set; } = string.Empty;
    }

    public class ChannelInformation
    {
        /// <summary>
        ///     Main channel.
        /// </summary>
        [JsonPropertyName("main")]
        public ulong Main { get; set; }

        /// <summary>
        ///     Staff channel.
        /// </summary>
        [JsonPropertyName("staff")]
        public ulong Staff { get; set; }
    }

    public class RetentionInformation
    {
        /// <summary>
        ///     If leaves should be sent.
        /// </summary>
        [JsonPropertyName("send-leave")]
        public bool SendLeaves { get; set; } = true;

        /// <summary>
        ///     If joins should be sent.
        /// </summary>
        [JsonPropertyName("send-join")]
        public bool SendJoins { get; set; } = true;
    }

    public class ApiInformation
    {
        /// <summary>
        ///     The api port.
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        ///     The api IP.
        /// </summary>
        [JsonPropertyName("ip")]
        public string Ip { get; set; } = "localhost";
    }
}
