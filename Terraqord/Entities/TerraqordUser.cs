using Auxiliary;

namespace Terraqord.Entities
{
    public class TerraqordUser : BsonModel
    {
        private ulong _discordId;
        public ulong DiscordId
        {
            get
                => _discordId;
            set
            {
                _ = this.SaveAsync(x => x.DiscordId, value);
                _discordId = value;
            }
        }

        private string? _authorUrl;
        public string? AuthorUrl
        {
            get
                => _authorUrl;
            set
            {
                _ = this.SaveAsync(x => x.AuthorUrl, value);
                _authorUrl = value;
            }
        }

        private int _tshockId;
        public int TShockId
        {
            get
                => _tshockId;
            set
            {
                _ = this.SaveAsync(x => x.TShockId, value);
                _tshockId = value;
            }
        }

        public void Dispose()
        {

        }
    }
}
