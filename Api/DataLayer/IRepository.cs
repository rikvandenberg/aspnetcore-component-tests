using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DataLayer
{
    public interface IRepository<T>
        where T : class

    {
        IQueryable<T> AllRecords { get; }

        Task<T?> GetByIdAsync(Guid id);

        Task CreateAsync(T entity);

        Task DeleteAsync(Guid id);

        Task UpdateAsync(T entity);
    }
}