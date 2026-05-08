using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class UserRepository : AbstractRepository<User>
    {
        public UserRepository(WebAppDbContext context) : base(context)
        {
        }


    }
}
