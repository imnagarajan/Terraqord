using Auxiliary;
using Auxiliary.Configuration;
using Discord.Webhook;
using System.Timers;
using Terraqord.Configuration;
using Terraqord.Entities;
using Terraqord.Extensions;
using Terraqord.Webhooks;
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

        private readonly MessageResolver _mainHook;
        private readonly MessageResolver _staffHook;
        private readonly MessageResolver _loggingHook;

        public GameManager(DiscordSocketClient client)
        {
            _joinedAt = new DateTime?[256];

            _timer = new Timer(30000)
            {
                Enabled = true,
                AutoReset = true
            };
            _timer.Elapsed += async (_, x) 
                => await OnElapsedAsync(x);
            _timer.Start();

            _client = client;

            _mainHook = new(_client, Configuration<TerraqordSettings>.Settings.Channels.Main);
            _staffHook = new(_client, Configuration<TerraqordSettings>.Settings.Channels.Staff);
            _loggingHook = new(_client, Configuration<TerraqordSettings>.Settings.Channels.Logging);

            Terraqord.ChatSent += ChatSent;
            Terraqord.CommandSent += CommandSent;

            Terraqord.Join += Join;
            Terraqord.Leave += Leave;

            Terraqord.ServerStarted += ServerStarted;
        }

        private async Task OnElapsedAsync(ElapsedEventArgs _)
            => await _client.SetGameAsync($"on {Configuration<TerraqordSettings>.Settings.Server.ServerName} ({TShock.Utils.GetActivePlayerCount()}/{TShock.Config.Settings.MaxSlots})");

        private async Task ServerStarted()
        {
            var eb = new EmbedBuilder()
                .WithTitle($"{Configuration<TerraqordSettings>.Settings.Server.BridgeName} started!")
                .AddField("Map:", $"`{Main.worldName}`")
                .AddField("Max players:", $"`{TShock.Config.Settings.MaxSlots}`")
                .AddField("Difficulty:", $"`{Enum.GetName(typeof(GameMode), GameMode.All)}`")
                .WithColor(Color.Blue);

            await _mainHook.SendAsync(eb, Configuration<TerraqordSettings>.Settings.Server.BridgeName);
        }

        public async Task StartAsync()
        {
            var eb = new EmbedBuilder()
                .WithTitle($"{Configuration<TerraqordSettings>.Settings.Server.BridgeName} starting!")
                .WithColor(Color.Blue);

            await _mainHook.SendAsync(eb, Configuration<TerraqordSettings>.Settings.Server.BridgeName);
        }

        private async Task Leave(LeaveEventArgs arg)
        {
            if (!Configuration<TerraqordSettings>.Settings.Retention.SendLeaves)
                return;

            if (!arg.Who.TryGetPlayer(out var player))
                return;

            var span = DateTime.UtcNow - _joinedAt[arg.Who]!.Value;

            var eb = new EmbedBuilder()
                .WithTitle($"{player.Name} has left!")
                .AddField("Playtime:", $"`{span.ToReadable()}`")
                .AddField("Playercount:", $"`{TShock.Utils.GetActivePlayerCount() - 1}/{TShock.Config.Settings.MaxSlots}`")
                .WithColor(Color.Red);

            await _mainHook.SendAsync(eb, Configuration<TerraqordSettings>.Settings.Server.BridgeName);

            var lb = new EmbedBuilder()
                .WithTitle($"{player.Name} has left!")
                .AddField("IP:", $"`{player.IP}`")
                .WithColor(Color.Red);

            await _loggingHook.SendAsync(lb);
        }

        private async Task Join(GreetPlayerEventArgs arg)
        {
            if (!Configuration<TerraqordSettings>.Settings.Retention.SendJoins)
                return;

            _joinedAt[arg.Who] = DateTime.UtcNow;

            if (!arg.Who.TryGetPlayer(out var player))
                return;
            
            var eb = new EmbedBuilder()
                .WithTitle($"{player.Name} has joined!")
                .AddField("Playercount:", $"`{TShock.Utils.GetActivePlayerCount()}/{TShock.Config.Settings.MaxSlots}`")
                .WithColor(Color.Green);

            await _mainHook.SendAsync(eb, Configuration<TerraqordSettings>.Settings.Server.BridgeName);

            var lb = new EmbedBuilder()
                .WithTitle($"{player.Name} has joined!")
                .AddField("IP:", $"`{player.IP}`")
                .WithColor(Color.Green);

            await _loggingHook.SendAsync(lb);
        }

        private async Task CommandSent(PlayerCommandEventArgs arg)
        {
            var player = arg.Player;

            if (arg.CommandName is "login" or "password" or "user" or "register")
                return;

            if (arg.CommandName is "staffchat" or "sc" && !arg.Handled)
            {
                if (arg.Player.Name == TSPlayer.Server.Name)
                    return;

                var stringify = arg.CommandText[arg.CommandName.Length..].StripTags().Trim();

                if (string.IsNullOrEmpty(stringify))
                    return;

                string? avatarUrl = null;
                if (player.Account != null)
                {
                    var user = await IModel.GetAsync(GetRequest.Bson<TerraqordUser>(x => x.TShockId == player.Account.ID));

                    avatarUrl = user?.AuthorUrl ?? null;
                }

                await _staffHook.SendAsync(stringify, $"{player.Group.Prefix}{player.Name}".StripTags(), avatarUrl);
            }

            var lb = new EmbedBuilder()
                .WithTitle($"{player.Name} has executed a command!")
                .AddField("Command:", $"/{arg.CommandText}")
                .WithColor(Color.Blue);

            await _loggingHook.SendAsync(lb);
        }

        private async Task ChatSent(PlayerChatEventArgs arg)
        {
            var player = arg.Player;

            if (arg.Handled)
                return;

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
                await _mainHook.SendAsync(stringify, $"{player.Group.Prefix}{player.Name}".StripTags(), avatarUrl);
            }
            catch (TimeoutException)
            {
                return;
            }
        }
    }
}
