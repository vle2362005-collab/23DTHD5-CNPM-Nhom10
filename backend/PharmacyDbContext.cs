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
        public DbSet<DbMedicine> Medicines { get; set; } = null!;
        public DbSet<DbMedicineIngredient> MedicineIngredients { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DbMedicineIngredient>()
                .HasKey(mi => new { mi.MedicineId, mi.IngredientId });
        }
    }
}
