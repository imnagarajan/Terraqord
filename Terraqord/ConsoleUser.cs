using TShockAPI;

namespace Terraqord
{
    public class ConsoleUser : TSPlayer, IDisposable
    {
        private readonly List<string> _output;

        public ConsoleUser(string name) : base(name)
            => _output = new List<string>();

        public override void SendErrorMessage(string msg)
            => _output.Add(msg);

        public override void SendInfoMessage(string msg)
            => _output.Add(msg);

        public override void SendMessage(string msg, byte red, byte green, byte blue)
            => _output.Add(msg);

        public override void SendMessage(string msg, Microsoft.Xna.Framework.Color color)
            => _output.Add(msg);

        public override void SendSuccessMessage(string msg)
            => _output.Add(msg);

        public override void SendWarningMessage(string msg)
            => _output.Add(msg);

        public List<string> GetOutput()
        {
            return _output;
        }

        void IDisposable.Dispose() { }
    }
}
