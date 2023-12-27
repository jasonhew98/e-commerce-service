using Domain.Seedwork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.AggregatesModel.AccountAggregate
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<List<Account>> GetAccounts(
            int limit,
            int offset,
            string sortBy,
            int sortOrder);

        Task<long> GetAccountCount();

        Task<Account> GetAccount(
            string accountId = null);

        Task<bool> UpdateAccount(
            Account account,
            (string id, string name) user,
            string accountId);
    }
}
