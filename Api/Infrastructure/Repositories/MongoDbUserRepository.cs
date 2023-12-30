using Domain.AggregatesModel.UserAggregate;
using Infrastructure.Seedwork;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class MongoDbUserRepository : MongoDbRepository<User>, IUserRepository
    {
        public MongoDbUserRepository(IMongoContext context, string collectionName)
            : base(context, collectionName)
        {
        }

        public async Task<User> GetUser(
            string userId = null)
        {
            var filter = Builders<User>.Filter.Empty;

            if (!string.IsNullOrEmpty(userId))
                filter &= Builders<User>.Filter.Eq(x => x.UserId, userId);

            var property = await QueryOne(filter, null);

            return property;
        }

        public async Task<List<User>> GetUsers(
            int limit,
            int offset,
            string sortBy,
            int sortOrder)
        {
            var filter = Builders<User>.Filter.Empty;

            sortBy = !string.IsNullOrEmpty(sortBy) ? sortBy : "_id";

            var sort = sortOrder == -1 ? Builders<User>.Sort.Descending(sortBy) : Builders<User>.Sort.Ascending(sortBy);

            var option = new FindOptions<User, BsonDocument>
            {
                Limit = limit,
                Skip = offset,
                Sort = sort
            };

            var products = await Query<User>(filter, option);

            return products.ToList();
        }

        public async Task<long> GetUserCount()
        {
            var filter = Builders<User>.Filter.Empty;

            var count = await DbSet.CountDocumentsAsync(filter);

            return count;
        }

        public async Task<bool> UpdateUser(User user, (string id, string name) currentUser, string userId)
        {
            user.SetModified(currentUser);

            var filter = Builders<User>.Filter.Empty;

            if (!string.IsNullOrEmpty(userId))
                filter &= Builders<User>.Filter.Eq(x => x.UserId, userId);

            filter &= Builders<User>.Filter.Eq(x => x.ModifiedUTCDateTime, user.OriginalModifiedUTCDateTime);

            var update = Builders<User>.Update
                .Set(a => a.FullName, user.FullName)
                .Set(a => a.Email, user.Email)
                .Set(a => a.ProfilePictures, user.ProfilePictures)
                .Set(a => a.ModifiedBy, user.ModifiedBy)
                .Set(a => a.ModifiedByName, user.ModifiedByName)
                .Set(a => a.ModifiedUTCDateTime, user.ModifiedUTCDateTime);

            var options = new UpdateOptions { IsUpsert = false };

            var updateResult = await DbSet.UpdateOneAsync(filter, update, options);

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
    }
}
