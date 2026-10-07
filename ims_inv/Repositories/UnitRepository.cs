using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class UnitRepository : AbstractRepository<Unit>, IUnitRepository
    {
        public UnitRepository(WebAppDbContext context) : base(context)
        {
        }
    }
}
