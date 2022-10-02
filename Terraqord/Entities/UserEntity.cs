using Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Terraqord.Entities
{
    [BsonIgnoreExtraElements]
    public class UserEntity : IEntity, IConcurrentlyAccessible<UserEntity>
    {
        [BsonId]
        public ObjectId ObjectId { get; set; }

        [BsonIgnore]
        public EntityState State { get; set; }

        private ulong _discordId;
        public ulong DiscordId
        {
            get
                => _discordId;
            set
            {
                _ = ModifyAsync(Builders<UserEntity>.Update.Set(x => x.DiscordId, value));
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
                _ = ModifyAsync(Builders<UserEntity>.Update.Set(x => x.AuthorUrl, value));
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
                _ = ModifyAsync(Builders<UserEntity>.Update.Set(x => x.TShockId, value));
                _tshockId = value;
            }
        }

        public async Task<bool> DeleteAsync()
            => await UserHelper.DeleteAsync(this);

        public async Task<bool> ModifyAsync(UpdateDefinition<UserEntity> update)
            => await UserHelper.ModifyAsync(this, update);

        public static async Task<UserEntity> GetAsync(ulong discordid)
            => await UserHelper.GetAsync(discordid);

        public static async Task<UserEntity> GetAsync(int tshockid)
            => await UserHelper.GetAsync(tshockid);

        public static async Task<UserEntity> CreateAsync(ulong discordid, int tshockid, string avatarUrl)
            => await UserHelper.CreateAsync(discordid, tshockid, avatarUrl);

        public void Dispose()
        {
            
        }
    }
}
