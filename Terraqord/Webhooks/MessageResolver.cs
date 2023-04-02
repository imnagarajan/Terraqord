using Discord.Webhook;
using Discord.WebSocket;
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
    public class MessageResolver
    {
        private readonly IMessageChannel _channel = null!;

        public bool IsActive { get; }

        /// <summary>
        ///     Creates a new webhook resolver that can handle messages even when the hooks are not configured.
        /// </summary>
        /// <param name="clientSecret"></param>
        public MessageResolver(DiscordSocketClient client, ulong channelId)
        {
            var channel = client.GetChannel(channelId);

            if (channel is IMessageChannel messageChannel)
                _channel = messageChannel;

            IsActive = channel is not null;
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
                await _channel!.SendMessageAsync(
                    text: text,
                    embeds: embed is not null 
                        ? new[] { embed.Build() } 
                        : null,
                    username: username,
                    avatarUrl: avatarUrl);
        }
    }
}
