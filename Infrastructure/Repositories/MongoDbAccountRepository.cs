using Domain.AggregatesModel.AccountAggregate;
using Infrastructure.Seedwork;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class MongoDbAccountRepository : MongoDbRepository<Account>, IAccountRepository
    {
        public MongoDbAccountRepository(IMongoContext context, string collectionName)
            : base(context, collectionName)
        {
        }

        public async Task<Account> GetAccount(
            string accountId = null)
        {
            var filter = Builders<Account>.Filter.Empty;

            if (!string.IsNullOrEmpty(accountId))
                filter &= Builders<Account>.Filter.Eq(x => x.AccountId, accountId);

            var property = await QueryOne(filter, null);

            return property;
        }

        public async Task<List<Account>> GetAccounts(
            int limit,
            int offset,
            string sortBy,
            int sortOrder)
        {
            var filter = Builders<Account>.Filter.Empty;

            sortBy = !string.IsNullOrEmpty(sortBy) ? sortBy : "_id";

            var sort = sortOrder == -1 ? Builders<Account>.Sort.Descending(sortBy) : Builders<Account>.Sort.Ascending(sortBy);

            var option = new FindOptions<Account, BsonDocument>
            {
                Limit = limit,
                Skip = offset,
                Sort = sort
            };

            var products = await Query<Account>(filter, option);

            return products.ToList();
        }

        public async Task<long> GetAccountCount()
        {
            var filter = Builders<Account>.Filter.Empty;

            var count = await DbSet.CountDocumentsAsync(filter);

            return count;
        }

        public async Task<bool> UpdateAccount(Account account, (string id, string name) user, string accountId)
        {
            account.SetModified(user);

            var filter = Builders<Account>.Filter.Empty;

            if (!string.IsNullOrEmpty(accountId))
                filter &= Builders<Account>.Filter.Eq(x => x.AccountId, accountId);

            filter &= Builders<Account>.Filter.Eq(x => x.ModifiedUTCDateTime, account.OriginalModifiedUTCDateTime);

            var update = Builders<Account>.Update
                .Set(a => a.FullName, account.FullName)
                .Set(a => a.Email, account.Email)
                .Set(a => a.ProfilePictures, account.ProfilePictures)
                .Set(a => a.ModifiedBy, account.ModifiedBy)
                .Set(a => a.ModifiedByName, account.ModifiedByName)
                .Set(a => a.ModifiedUTCDateTime, account.ModifiedUTCDateTime);

            var options = new UpdateOptions { IsUpsert = false };

            var updateResult = await DbSet.UpdateOneAsync(filter, update, options);

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
    }
}
