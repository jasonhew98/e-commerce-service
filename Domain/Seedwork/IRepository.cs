using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Domain.Seedwork
{
    public interface IRepository<T>
    {
        void Add(T obj);
        void AddMany(IEnumerable<T> documents);
        void Update(T obj, Expression<Func<T, bool>> filter);
        void Remove(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> Query();
        Task<IEnumerable<T>> Query(Expression<Func<T, bool>> filter);
        Task<T> QueryOne(Expression<Func<T, bool>> filter);
    }
}
