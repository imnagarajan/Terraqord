using Auxiliary.Events;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;

namespace Terraqord
{
    [ApiVersion(2, 1)]
    public class Terraqord : TerrariaPlugin
    {
        public override string Author
            => "Armano den Boef (Rozen4334)";

        public override string Description
            => "A plugin that manages the connection between Discord and Terraria.";

        public override string Name
            => "Terraqord";

        public override Version Version
            => new(1, 0);

        public Terraqord(Main game)
            : base(game)
        {
            Order = 2;
        }

        private static readonly AsyncEvent<Func<PlayerChatEventArgs, Task>> _chatSent = new();
        /// <summary>
        ///     Invoked when a chat message is sent to the server.
        /// </summary>
        public static event Func<PlayerChatEventArgs, Task> ChatSent
        {
            add
                => _chatSent.Add(value);
            remove
                => _chatSent.Remove(value);
        }

        private static readonly AsyncEvent<Func<PlayerCommandEventArgs, Task>> _commandSent = new();
        /// <summary>
        ///     Invoked when a command is sent to the server.
        /// </summary>
        public static event Func<PlayerCommandEventArgs, Task> CommandSent
        {
            add
                => _commandSent.Add(value);
            remove
                => _commandSent.Remove(value);
        }

        private static readonly AsyncEvent<Func<ReloadEventArgs, Task>> _reload = new();
        /// <summary>
        ///     Invokes when a reload is executed.
        /// </summary>
        public static event Func<ReloadEventArgs, Task> Reload
        {
            add
                => _reload.Add(value);
            remove
                => _reload.Remove(value);
        }

        private static readonly AsyncEvent<Func<GreetPlayerEventArgs, Task>> _join = new();
        /// <summary>
        ///     Invokes when a player joins the server.
        /// </summary>
        public static event Func<GreetPlayerEventArgs, Task> Join
        {
            add
                => _join.Add(value);
            remove
                => _join.Remove(value);
        }

        private static readonly AsyncEvent<Func<LeaveEventArgs, Task>> _leave = new();
        /// <summary>
        ///     Invokes when a player leaves the server.
        /// </summary>
        public static event Func<LeaveEventArgs, Task> Leave
        {
            add
                => _leave.Add(value);
            remove
                => _leave.Remove(value);
        }

        public override void Initialize()
        {
            PlayerHooks.PlayerChat += async (x) =>
            {
                await _chatSent.InvokeAsync(x);
            };

            PlayerHooks.PlayerCommand += async (x) =>
            {
                await _commandSent.InvokeAsync(x);
            };

            GeneralHooks.ReloadEvent += async (x) =>
            {
                await _reload.InvokeAsync(x);
            };

            ServerApi.Hooks.NetGreetPlayer.Register(this, async (x) =>
            {
                await _join.InvokeAsync(x);
            });

            ServerApi.Hooks.ServerLeave.Register(this, async (x) =>
            {
                await _leave.InvokeAsync(x);
            });

            Application.Start();
        }
    }
}