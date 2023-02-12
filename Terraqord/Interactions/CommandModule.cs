using Auxiliary;
using Terraqord.Entities;
using Terraqord.Extensions;
using Terraqord.Interactions.Models;
using Terraria;
using Terraria.Utilities;
using TShockAPI;

namespace Terraqord.Interactions
{
    [EnabledInDm(false)]
    public class CommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("who", "Gets ingame players.")]
        public async Task WhoAsync()
        {
            var eb = new EmbedBuilder()
                .WithTitle($"Playing: ({TShock.Utils.GetActivePlayerCount()}/{TShock.Config.Settings.MaxSlots})")
                .WithDescription($"```\n{string.Join(", ", TShock.Players.Where(e => e != null && e.Active).Select(e => e.Name))}\n```");

            await RespondAsync(
                text: ":white_check_mark: **A list of all online players:**",
                embed: eb.Build(),
                ephemeral: false);
        }

        [SlashCommand("command", "Executes an ingame command.")]
        public async Task CommandAsync(string query, bool visible = false)
        {
            await DeferAsync(!visible);

            var user = await IModel.GetAsync(GetRequest.Bson<TerraqordUser>(x => x.DiscordId == Context.User.Id));

            if (user is null)
            {
                await FollowupAsync(
                    text: ":x: **You can't execute commands if you're not logged in!** Execute `/login` to log in.");
                return;
            }

            var account = TShock.UserAccounts.GetUserAccountByID(user.TShockId);

            if (account is null)
            {
                await FollowupAsync(
                    text: ":x: **Your account does not exist. Please log out and log back in again.**");
                return;
            }

            var group = TShock.Groups.GetGroupByName(account.Group);

            var param = query.ParseParameters();

            var commands = Commands.ChatCommands.Where(c => c.HasAlias(param[0].ToLower()));

            if (!commands.Any())
            {
                await FollowupAsync(
                    text: ":x: **Command not found!**");

                return;
            }

            var plr = new ConsoleUser(account.Name)
            {
                Account = account,
                Group = group
            };

            if (Main.rand == null)
                Main.rand = new UnifiedRandom();

            foreach (var command in commands)
            {
                if (!command.AllowServer)
                {
                    await FollowupAsync(
                        text: ":x: **This command is only available ingame!");
                    return;
                }

                plr.IsLoggedIn = true;
                command.Run(query, plr, param.GetRange(1, param.Count - 1));
                if (!plr.GetOutput().Any())
                {
                    await FollowupAsync(
                        text: ":white_check_mark: **Command executed but did not return any results.**");
                    return;
                }

                var eb = new EmbedBuilder()
                    .WithDescription($"```\n{string.Join("\n", plr.GetOutput()).StripTags()}\n```");
                await FollowupAsync(
                    text: ":white_check_mark: **Succesfully executed command:**",
                    embed: eb.Build());
            }
        }

        [SlashCommand("login", "Logs in to your ingame account.")]
        public async Task LoginAsync()
            => await RespondWithModalAsync<LoginModel>("loggedin");

        [ModalInteraction("loggedin")]
        public async Task LoginResolveAsync(LoginModel model)
        {
            var account = TShock.UserAccounts.GetUserAccountByName(model.Username);

            if (account is null)
            {
                await RespondAsync(
                    text: $":x: **Failed to find an account with name {model.Username}.**",
                    ephemeral: true);
                return;
            }

            var user = await IModel.GetAsync(GetRequest.Bson<TerraqordUser>(x => x.TShockId == account.ID));

            if (user is not null)
            {
                await RespondAsync(
                    text: ":x: **You're already logged in!** Execute `/logout` to log out.",
                    ephemeral: true);
                return;
            }

            if (!account.VerifyPassword(model.Password))
            {
                await RespondAsync(
                    text: ":x: **Invalid password! Please try again.**",
                    ephemeral: true);
                return;
            }

            if (Context.User is not SocketGuildUser member)
                return;

            await IModel.CreateAsync(CreateRequest.Bson<TerraqordUser>(x =>
            {
                x.DiscordId = Context.User.Id;
                x.TShockId = account.ID;
                x.AuthorUrl = member.GetAvatarUrl();
            }));

            await RespondAsync(
                text: ":white_check_mark: **Successfully logged in.**",
                ephemeral: true);
        }

        [SlashCommand("logout", "Logs out of your ingame account.")]
        public async Task LogoutAsync()
        {
            var user = await IModel.GetAsync(GetRequest.Bson<TerraqordUser>(x => x.DiscordId == Context.User.Id));

            if (user is not null)
            {
                await user.DeleteAsync();

                await RespondAsync(
                    text: ":white_check_mark: **Succesfully logged out.**",
                    ephemeral: true);

                return;
            }

            await RespondAsync(
                text: ":x: **You aren't logged in!**",
                ephemeral: true);
        }
    }
}
