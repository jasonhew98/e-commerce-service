using Infrastructure.Seedwork;
using MediatR;

namespace Infrastructure.Repositories
{
    public class CoreMongoDbContext : MongoDbContext
    {
        public CoreMongoDbContext(string mongoDbUrl, string database, IMediator mediator)
            : base(mongoDbUrl, database, mediator)
        {
        }
    }
}
