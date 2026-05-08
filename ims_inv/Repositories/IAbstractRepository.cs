using System.Linq.Expressions;

namespace ims_inv.Repositories
{
    /// <summary>
    /// Generic repository interface providing common CRUD operations for all entities.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IAbstractRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its primary key.
        /// </summary>
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets entities matching the specified predicate.
        /// </summary>
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching the specified predicate.
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity matches the specified predicate.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts entities matching the specified predicate.
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple entities.
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Updates multiple entities.
        /// </summary>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>
        /// Deletes an entity by its primary key.
        /// </summary>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Deletes multiple entities.
        /// </summary>
        void DeleteRange(IEnumerable<T> entities);

        /// <summary>
        /// Saves all changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
