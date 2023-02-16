using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraqord.Webhooks
{
    /// <summary>
    ///     Represents a webhook resolver that can handle messages even when the hooks are not configured.
    /// </summary>
    public class WebhookResolver
    {
        private readonly DiscordWebhookClient? _client;

        public bool IsActive { get; }

        /// <summary>
        ///     Creates a new webhook resolver that can handle messages even when the hooks are not configured.
        /// </summary>
        /// <param name="clientSecret"></param>
        public WebhookResolver(string? clientSecret)
        {
            IsActive = !string.IsNullOrEmpty(clientSecret);

            if (IsActive)
                _client = new(clientSecret);
        }

        public async Task SendAsync(EmbedBuilder? embed)
            => await SendAsync(null, embed, null, null);

        public async Task SendAsync(EmbedBuilder? embed, string? username)
            => await SendAsync(null, embed, username, null);

        public async Task SendAsync(string? text, string? username, string? avatarUrl)
            => await SendAsync(text, null, username, avatarUrl);

        public async Task SendAsync(string? text, EmbedBuilder? embed, string? username, string? avatarUrl)
        {
            if (IsActive)
                await _client!.SendMessageAsync(
                    text: text,
                    embeds: embed is not null 
                        ? new[] { embed.Build() } 
                        : null,
                    username: username,
                    avatarUrl: avatarUrl);
        }
    }
}
