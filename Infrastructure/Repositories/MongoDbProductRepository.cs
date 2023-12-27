using Domain.AggregatesModel.ProductAggregate;
using Infrastructure.Seedwork;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class MongoDbProductRepository : MongoDbRepository<Product>, IProductRepository
    {
        public MongoDbProductRepository(IMongoContext context, string collectionName)
            : base(context, collectionName)
        {
        }

        public async Task<Product> GetProduct(
            string productId = null)
        {
            var filter = Builders<Product>.Filter.Empty;

            if (!string.IsNullOrEmpty(productId))
                filter &= Builders<Product>.Filter.Eq(x => x.ProductId, productId);

            var property = await QueryOne(filter, null);

            return property;
        }

        public async Task<List<Product>> GetProducts(
            int limit,
            int offset,
            string sortBy,
            int sortOrder)
        {
            var filter = Builders<Product>.Filter.Empty;

            sortBy = !string.IsNullOrEmpty(sortBy) ? sortBy : "_id";

            var sort = sortOrder == -1 ? Builders<Product>.Sort.Descending(sortBy) : Builders<Product>.Sort.Ascending(sortBy);

            var option = new FindOptions<Product, BsonDocument>
            {
                Limit = limit,
                Skip = offset,
                Sort = sort
            };

            var products = await Query<Product>(filter, option);

            return products.ToList();
        }

        public async Task<long> GetProductCount()
        {
            var filter = Builders<Product>.Filter.Empty;

            var count = await DbSet.CountDocumentsAsync(filter);

            return count;
        }

        public async Task<bool> UpdateProduct(Product product, (string id, string name) user, string productId)
        {
            product.SetModified(user);

            var filter = Builders<Product>.Filter.Empty;

            if (!string.IsNullOrEmpty(productId))
                filter &= Builders<Product>.Filter.Eq(x => x.ProductId, productId);

            filter &= Builders<Product>.Filter.Eq(x => x.ModifiedUTCDateTime, product.OriginalModifiedUTCDateTime);

            var update = Builders<Product>.Update
                .Set(a => a.ProductName, product.ProductName)
                .Set(a => a.ModifiedBy, product.ModifiedBy)
                .Set(a => a.ModifiedByName, product.ModifiedByName)
                .Set(a => a.ModifiedUTCDateTime, product.ModifiedUTCDateTime);

            var options = new UpdateOptions { IsUpsert = false };

            var updateResult = await DbSet.UpdateOneAsync(filter, update, options);

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
    }
}
