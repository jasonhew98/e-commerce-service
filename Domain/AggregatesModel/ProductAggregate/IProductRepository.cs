using Domain.Seedwork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.AggregatesModel.ProductAggregate
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> GetProducts(
            int limit,
            int offset,
            string sortBy,
            int sortOrder);

        Task<long> GetProductCount();

        Task<Product> GetProduct(
            string productId = null);

        Task<bool> UpdateProduct(
            Product product,
            (string id, string name) user,
            string productId);
    }
}
