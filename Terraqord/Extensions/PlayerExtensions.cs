using Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraqord.Entities;
using TShockAPI;

namespace Terraqord.Extensions
{
    public static class PlayerExtensions
    {
        public static bool TryGetPlayer(this int index, out TSPlayer player)
        {
            player = TShock.Players[index];

            return player != null && player.Active && player.RealPlayer;
        }

        public static bool TryGetMessage(this SocketUserMessage message, out SocketGuildUser author)
        {
            author = default!;

            if (message is null)
                return false;

            if (message.Author is not SocketGuildUser user)
                return false;

            author = user;

            if (author.IsBot || author.IsWebhook)
                return false;

            if (message.Content is null || message.CleanContent.Length <= 0)
                return false;

            if (message.Content.StartsWith('.'))
                return false;

            return true;
        }

        public static async Task HandleDefaultAsync(this SocketUserMessage message)
        {
            if (message.TryGetMessage(out var user))
                return;

            var member = await IModel.GetAsync(GetRequest.Bson<TerraqordUser>(x => x.DiscordId == user.Id));

            if (member is not null)
            {
                var account = TShock.UserAccounts.GetUserAccountByID(member.TShockId);
                if (account is null)
                    return;
                var group = TShock.Groups.GetGroupByName(account.Group);
                if (group is null)
                    return;

                TShock.Utils.Broadcast($"[c/28D2B9:Discord] ⇒ {group.Prefix}{user.DisplayName}: {message.CleanContent}", group.R, group.G, group.B);
            }
            else
                TShock.Utils.Broadcast($"[c/28D2B9:Discord] ⇒ {user.DisplayName}: {message.CleanContent}", Microsoft.Xna.Framework.Color.LightGray);
        }

        public static async Task HandleStaffAsync(this SocketUserMessage message)
        {
            if (message.TryGetMessage(out var user))
                return;

            Commands.HandleCommand(TSPlayer.Server, $"/sc [Discord] ⇒ {user.DisplayName}: {message.CleanContent}");

            await Task.CompletedTask;
        }
    }
}
