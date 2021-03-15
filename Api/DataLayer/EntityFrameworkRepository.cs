using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DataLayer
{
    public class EntityFrameworkRepository<T> : IRepository<T>
        where T : class
    {
        private readonly OrderDbContext _dbContext;

        public EntityFrameworkRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<T> AllRecords => _dbContext.Set<T>();

        public Task CreateAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            return _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            T entity = await _dbContext.Set<T>().FindAsync(id);
            if (entity != null)
            {
                _dbContext.Set<T>().Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public Task UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            return _dbContext.SaveChangesAsync();
        }
    }
}