using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ims_inv.Data;

namespace ims_inv.Repositories
{
    /// <summary>
    /// Generic repository base class providing common CRUD operations for all entities.
    /// Implements the IAbstractRepository interface with Entity Framework Core.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class AbstractRepository<T> : IAbstractRepository<T> where T : class
    {
        protected readonly WebAppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public AbstractRepository(WebAppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Gets an entity by its primary key.
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets entities matching the specified predicate.
        /// </summary>
        public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a single entity matching the specified predicate.
        /// </summary>
        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Checks if any entity matches the specified predicate.
        /// </summary>
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Counts entities matching the specified predicate.
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            return predicate == null
                ? await _dbSet.CountAsync(cancellationToken)
                : await _dbSet.CountAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Adds multiple entities.
        /// </summary>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Updates multiple entities.
        /// </summary>
        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        /// <summary>
        /// Deletes an entity by its primary key.
        /// </summary>
        public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Deletes multiple entities.
        /// </summary>
        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// Saves all changes to the database.
        /// </summary>
        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
