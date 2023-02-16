using Auxiliary;
using Auxiliary.Configuration;
using Discord.Webhook;
using System.Timers;
using Terraqord.Configuration;
using Terraqord.Entities;
using Terraqord.Extensions;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Timer = System.Timers.Timer;

namespace Terraqord
{
    public class GameManager
    {
        private readonly DateTime?[] _joinedAt;
        private readonly Timer _timer;

        private readonly DiscordSocketClient _client;

        private readonly DiscordWebhookClient _loggingHook;
        private readonly DiscordWebhookClient _mainHook;
        private readonly DiscordWebhookClient _staffHook;

        public GameManager(DiscordSocketClient client)
        {
            _joinedAt = new DateTime?[256];

            for (int i = 0; i < _joinedAt.Length; i++)
                _joinedAt[i] = null;

            _timer = new Timer(30000)
            {
                Enabled = true,
                AutoReset = true
            };
            _timer.Elapsed += async (_, x) 
                => await OnElapsedAsync(x);
            _timer.Start();

            _client = client;

            _mainHook = new(Configuration<TerraqordSettings>.Settings.Webhooks.Main);
            _staffHook = new(Configuration<TerraqordSettings>.Settings.Webhooks.Staff);
            _loggingHook = new(Configuration<TerraqordSettings>.Settings.Webhooks.Logging);

            Terraqord.ChatSent += ChatSent;
            Terraqord.CommandSent += CommandSent;

            Terraqord.Join += Join;
            Terraqord.Leave += Leave;

            Terraqord.ServerStarted += ServerStarted;
        }

        private async Task OnElapsedAsync(ElapsedEventArgs _)
            => await _client.SetGameAsync($"on TBC ({TShock.Utils.GetActivePlayerCount()}/{TShock.Config.Settings.MaxSlots})");

        private async Task ServerStarted()
        {
            var eb = new EmbedBuilder()
                .WithTitle("Main started!")
                .AddField("Map:", $"`{Main.worldName}`")
                .AddField("Max players:", $"`{TShock.Config.Settings.MaxSlots}`")
                .AddField("Difficulty:", $"`{Enum.GetName(typeof(GameMode), GameMode.All)}`")
                .WithColor(Color.Blue);

            await _mainHook.SendMessageAsync(
                embeds: new[] { eb.Build() });
        }

        public async Task StartAsync()
        {
            var eb = new EmbedBuilder()
                .WithTitle("Main starting!")
                .WithColor(Color.Blue);

            await _mainHook.SendMessageAsync(
                embeds: new[] { eb.Build() });
        }

        private async Task Leave(LeaveEventArgs arg)
        {
            if (!Configuration<TerraqordSettings>.Settings.Retention.SendLeaves)
                return;

            var player = TShock.Players[arg.Who];

            if (player != null && player.Active && player.RealPlayer)
            {
                var span = DateTime.UtcNow - _joinedAt[arg.Who]!.Value;

                var eb = new EmbedBuilder()
                    .WithTitle($"{player.Name} has left!")
                    .AddField("Playtime:", $"`{span.ToReadable()}`")
                    .AddField("Playercount:", $"`{TShock.Utils.GetActivePlayerCount() - 1}/{TShock.Config.Settings.MaxSlots}`")
                    .WithColor(Color.Red);

                await _mainHook.SendMessageAsync(
                    embeds: new[] { eb.Build() });

                var lb = new EmbedBuilder()
                    .WithTitle($"{player.Name} has left!")
                    .AddField("IP:", $"`{player.IP}`")
                    .WithColor(Color.Red);

                await _loggingHook.SendMessageAsync(
                    embeds: new[] { lb.Build() });
            }
        }

        private async Task Join(GreetPlayerEventArgs arg)
        {
            if (!Configuration<TerraqordSettings>.Settings.Retention.SendJoins)
                return;

            _joinedAt[arg.Who] = DateTime.UtcNow;

            var player = TShock.Players[arg.Who];

            if (player != null && player.Active && player.RealPlayer)
            {
                var eb = new EmbedBuilder()
                    .WithTitle($"{player.Name} has joined!")
                    .AddField("Playercount:", $"`{TShock.Utils.GetActivePlayerCount()}/{TShock.Config.Settings.MaxSlots}`")
                    .WithColor(Color.Green);

                await _mainHook.SendMessageAsync(
                    username: "Main",
                    embeds: new[] { eb.Build() });

                var lb = new EmbedBuilder()
                    .WithTitle($"{player.Name} has joined!")
                    .AddField("IP:", $"`{player.IP}`")
                    .WithColor(Color.Green);

                await _loggingHook.SendMessageAsync(
                    username: "Main",
                    embeds: new[] { lb.Build() });
            }
        }

        private async Task CommandSent(PlayerCommandEventArgs arg)
        {
            var player = arg.Player;

            if (arg.CommandName is "login" or "password" or "user" or "register")
                return;

            if (arg.CommandName is "staffchat" or "sc" && !arg.Handled)
            {
                if (arg.Player.Name != TSPlayer.Server.Name)
                {
                    var stringify = arg.CommandText[arg.CommandName.Length..].StripTags().Trim();

                    if (!string.IsNullOrEmpty(stringify))
                    {

                        string? avatarUrl = null;
                        if (player.Account != null)
                        {
                            var user = await IModel.GetAsync(GetRequest.Bson<TerraqordUser>(x => x.TShockId == player.Account.ID));

                            avatarUrl = user?.AuthorUrl ?? null;
                        }

                        await _staffHook.SendMessageAsync(
                            text: stringify,
                            username: $"{player.Group.Prefix}{player.Name}".StripTags(),
                            avatarUrl: avatarUrl);
                    }
                }
            }

            var lb = new EmbedBuilder()
                .WithTitle($"{player.Name} has executed a command!")
                .AddField("Command:", $"/{arg.CommandText}")
                .WithColor(Color.Blue);

            await _loggingHook.SendMessageAsync(
                username: "Main",
                embeds: new[] { lb.Build() });
        }

        private async Task ChatSent(PlayerChatEventArgs arg)
        {
            var player = arg.Player;

            if (!arg.Handled)
            {
                var stringify = arg.RawText.StripTags().Trim();

                if (string.IsNullOrEmpty(stringify))
                    return;

                string? avatarUrl = null;
                if (player.Account != null)
                {
                    var user = await IModel.GetAsync(GetRequest.Bson<TerraqordUser>(x => x.TShockId == player.Account.ID));

                    avatarUrl = user?.AuthorUrl ?? null;
                }

                try
                {
                    await _mainHook.SendMessageAsync(
                        text: stringify,
                        username: $"{player.Group.Prefix}{player.Name}".StripTags(),
                        avatarUrl: avatarUrl);
                }
                catch (TimeoutException)
                {
                    return;
                }
            }
        }
    }
}
