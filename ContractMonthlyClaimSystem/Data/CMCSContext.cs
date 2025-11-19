using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Data
{
    public class CMCSContext : IdentityDbContext<ApplicationUser>
    {
        public CMCSContext(DbContextOptions<CMCSContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
    }
}