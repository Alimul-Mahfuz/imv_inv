using ims_inv.Models;

namespace ims_inv.Repositories
{
    public interface IUserRepository : IAbstractRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsEmailTakenAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default);
    }
}
