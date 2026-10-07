using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class UserRepository : AbstractRepository<User>, IUserRepository
    {
        public UserRepository(WebAppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> IsEmailTakenAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            if (excludeUserId.HasValue)
            {
                return await _dbSet.AnyAsync(u => u.Email == email && u.Id != excludeUserId.Value, cancellationToken);
            }
            return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
        }
    }
}
