using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class PharmacyDbContext : DbContext
    {
        public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options)
        {
        }

        public DbSet<DbUser> Users { get; set; } = null!;
        public DbSet<DbRole> Roles { get; set; } = null!;
    }
}
