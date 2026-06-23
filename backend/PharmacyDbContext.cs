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
        public DbSet<DbDrugGroup> DrugGroups { get; set; } = null!;
        public DbSet<DbActiveIngredient> ActiveIngredients { get; set; } = null!;
        public DbSet<DbDisease> Diseases { get; set; } = null!;
        public DbSet<DbDrugInteraction> DrugInteractions { get; set; } = null!;
        public DbSet<DbContraindication> Contraindications { get; set; } = null!;
        public DbSet<DbSale> Sales { get; set; } = null!;
        public DbSet<DbSaleDetail> SaleDetails { get; set; } = null!;
        public DbSet<DbSafetyCheck> SafetyChecks { get; set; } = null!;
        public DbSet<DbWarning> Warnings { get; set; } = null!;
        public DbSet<DbPatient> Patients { get; set; } = null!;
        public DbSet<DbPatientAllergy> PatientAllergies { get; set; } = null!;
        public DbSet<DbPatientDisease> PatientDiseases { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DbMedicineIngredient>()
                .HasKey(mi => new { mi.MedicineId, mi.IngredientId });
        }
    }
}
