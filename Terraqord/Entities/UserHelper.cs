using Auxiliary;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraqord.Entities
{
    internal class UserHelper
    {
        private static readonly Collection<UserEntity> _client = new("Terraqord");

        public static async Task<bool> ModifyAsync(UserEntity user, UpdateDefinition<UserEntity> update)
        {
            if (user.State is EntityState.Deserializing)
                return false;

            if (user.State is EntityState.Deleted)
                throw new InvalidOperationException($"{nameof(user)} cannot be modified post-deletion.");

            return await _client.ModifyDocumentAsync(user, update);
        }

        public static async Task<bool> DeleteAsync(UserEntity user)
        {
            user.State = EntityState.Deleted;

            return await _client.DeleteDocumentAsync(user);
        }

        public static async Task<UserEntity> GetAsync(ulong id)
        {
            var document = await _client.FindDocumentAsync(x => x.DiscordId == id);

            if (document is null)
                return null!;

            document.State = EntityState.Initialized;
            return document;
        }

        public static async Task<UserEntity> GetAsync(int id)
        {
            var document = await _client.FindDocumentAsync(x => x.TShockId == id);

            if (document is null)
                return null!;

            document.State = EntityState.Initialized;
            return document;
        }

        public static async Task<UserEntity> CreateAsync(ulong discordId, int tshockId, string avatarUrl)
        {
            var entity = new UserEntity
            {
                DiscordId = discordId,
                TShockId = tshockId,
                AuthorUrl = avatarUrl
            };

            await _client.InsertDocumentAsync(entity);

            entity.State = EntityState.Initialized;
            return entity;
        }

        public static async IAsyncEnumerable<UserEntity> GetAllAsync()
        {
            var documents = _client.GetAllDocumentsAsync();

            await foreach (var document in documents)
            {
                document.State = EntityState.Initialized;
                yield return document;
            }
        }
    }
}
