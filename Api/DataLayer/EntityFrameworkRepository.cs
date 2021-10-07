#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Api.DataLayer
{
    public class EntityFrameworkRepository<T> : IRepository<T>
        where T : class
    {
        private readonly OrderDbContext _dbContext;
        private readonly ILogger<EntityFrameworkRepository<T>> _logger;

        public EntityFrameworkRepository(OrderDbContext dbContext, ILogger<EntityFrameworkRepository<T>> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IQueryable<T> AllRecords
        {
            get
            {
                _logger.LogInformation("AllRecords called to query the result");
                return _dbContext.Set<T>();
            }
        }

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

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public Task UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            return _dbContext.SaveChangesAsync();
        }
    }
}