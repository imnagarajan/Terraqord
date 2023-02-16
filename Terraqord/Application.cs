using Auxiliary;
using Auxiliary.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Terraqord.Configuration;
using Terraqord.Entities;
using TShockAPI;

namespace Terraqord
{
    public class Application
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _service;

        private bool _ready;

        public Application()
        {
            _provider = BuildServiceProvider();
            _client = _provider.GetRequiredService<DiscordSocketClient>();
            _service = _provider.GetRequiredService<InteractionService>();
        }

        /// <summary>
        ///     Starts the application and immediately exits the handler to not hold execution back.
        /// </summary>
        public static void Start()
        {
            _ = new Application().RunAsync();
        }

        private async Task RunAsync()
        {
            var gameClient = _provider.GetRequiredService<GameManager>()
                .StartAsync();

            await _client.LoginAsync(TokenType.Bot, Configuration<TerraqordSettings>.Settings.Bot.Token);

            _client.Ready += ReadyAsync;
            _client.Log += LogAsync;

            _client.InteractionCreated += InteractionReceived;
            _client.MessageReceived += MessageReceived;

            await _service.AddModulesAsync(typeof(Application).Assembly, _provider);

            _service.Log += LogAsync;
            _service.InteractionExecuted += InteractionExecuted;

            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private async Task ReadyAsync()
        {
            if (!_ready)
                _ready = true;

            if (Configuration<TerraqordSettings>.Settings.Bot.AllowRegistration)
                await _service.RegisterCommandsGloballyAsync();
        }


        private readonly object _lock = new();

        private async Task LogAsync(LogMessage message)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                switch (message.Severity)
                {
                    case LogSeverity.Error:
                    case LogSeverity.Critical:
                        TShock.Log.ConsoleError(message.ToString());
                        break;
                    case LogSeverity.Warning:
                    case LogSeverity.Info:
                        TShock.Log.ConsoleInfo(message.ToString());
                        break;
                    case LogSeverity.Debug:
                    case LogSeverity.Verbose:
                        TShock.Log.ConsoleDebug(message.ToString());
                        break;
                }
            }
        }

        private async Task InteractionReceived(SocketInteraction interaction)
        {
            if (!_ready)
                return;

            var context = new SocketInteractionContext(_client, interaction);

            await _service.ExecuteCommandAsync(context, _provider);
        }

        private async Task InteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                if (context.Interaction.HasResponded)
                    await context.Interaction.FollowupAsync(
                        text: ":x: **Command execution failed. Please report this to the developer.**");
                else
                    await context.Interaction.RespondAsync(
                        text: ":x: **Command execution failed. Please report this to the developer.**",
                        ephemeral: true);
            }
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (!_ready)
                return;

            if (message is not SocketUserMessage userMessage)
                return;

            if (userMessage.Channel.Id == Configuration<TerraqordSettings>.Settings.Channels.Main)
                await HandleDefaultAsync(userMessage);

            else if (userMessage.Channel.Id == Configuration<TerraqordSettings>.Settings.Channels.Staff)
                await HandleStaffAsync(userMessage);
        }

        private async Task HandleDefaultAsync(SocketUserMessage message)
        {
            if (message is null)
                return;

            if (message.Author is not SocketGuildUser user)
                return;

            if (user.IsBot || user.IsWebhook)
                return;

            if (message.Content is null || message.CleanContent.Length <= 0)
                return;

            if (message.Content.StartsWith('.'))
                return;

            var member = await IModel.GetAsync(GetRequest.Bson<TerraqordUser>(x => x.DiscordId == message.Author.Id));

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

        private async Task HandleStaffAsync(SocketUserMessage message)
        {
            await Task.CompletedTask;

            if (message.Author is not SocketGuildUser user)
                return;

            if (user.IsBot || user.IsWebhook)
                return;

            var cmd = $"/sc [Discord] ⇒ {user.DisplayName}: {message.CleanContent}";

            Commands.HandleCommand(TSPlayer.Server, cmd);
        }

        private static IServiceProvider BuildServiceProvider()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
                FormatUsersInBidirectionalUnicode = false,
                LogLevel = LogSeverity.Warning,
                LogGatewayIntentWarnings = false
            });
            collection.AddSingleton<DiscordSocketClient>();

            collection.AddSingleton(new InteractionServiceConfig()
            {
                UseCompiledLambda = true,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Info
            });
            collection.AddSingleton<InteractionService>();

            collection.AddSingleton<GameManager>();

            return collection.BuildServiceProvider();
        }
    }
}
