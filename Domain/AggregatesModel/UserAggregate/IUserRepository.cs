using Domain.Seedwork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.AggregatesModel.UserAggregate
{
    public interface IUserRepository : IRepository<User>
    {
        Task<List<User>> GetUsers(
            int limit,
            int offset,
            string sortBy,
            int sortOrder);

        Task<long> GetUserCount();

        Task<User> GetUser(
            string userId = null);

        Task<bool> UpdateUser(
            User user,
            (string id, string name) currentUser,
            string userId);
    }
}
