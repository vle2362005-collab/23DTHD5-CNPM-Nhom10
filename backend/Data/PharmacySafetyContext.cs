using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class PharmacySafetyContext : DbContext
    {
        public PharmacySafetyContext(DbContextOptions<PharmacySafetyContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<DrugGroup> DrugGroups { get; set; } = null!;
        public DbSet<ActiveIngredient> ActiveIngredients { get; set; } = null!;
        public DbSet<Medicine> Medicines { get; set; } = null!;
        public DbSet<MedicineIngredient> MedicineIngredients { get; set; } = null!;
        public DbSet<Disease> Diseases { get; set; } = null!;
        public DbSet<PatientDisease> PatientDiseases { get; set; } = null!;
        public DbSet<PatientAllergy> PatientAllergies { get; set; } = null!;
        public DbSet<PatientCurrentMedicine> PatientCurrentMedicines { get; set; } = null!;
        public DbSet<Prescription> Prescriptions { get; set; } = null!;
        public DbSet<Sale> Sales { get; set; } = null!;
        public DbSet<SaleDetail> SaleDetails { get; set; } = null!;
        public DbSet<DrugInteraction> DrugInteractions { get; set; } = null!;
        public DbSet<Contraindication> Contraindications { get; set; } = null!;
        public DbSet<SafetyCheck> SafetyChecks { get; set; } = null!;
        public DbSet<Warning> Warnings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for MedicineIngredient
            modelBuilder.Entity<MedicineIngredient>()
                .HasKey(mi => new { mi.MedicineId, mi.IngredientId });

            // Explicitly configure table names
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Patient>().ToTable("Patients");
            modelBuilder.Entity<DrugGroup>().ToTable("DrugGroups");
            modelBuilder.Entity<ActiveIngredient>().ToTable("ActiveIngredients");
            modelBuilder.Entity<Medicine>().ToTable("Medicines");
            modelBuilder.Entity<MedicineIngredient>().ToTable("MedicineIngredients");
            modelBuilder.Entity<Disease>().ToTable("Diseases");
            modelBuilder.Entity<PatientDisease>().ToTable("PatientDiseases");
            modelBuilder.Entity<PatientAllergy>().ToTable("PatientAllergies");
            modelBuilder.Entity<PatientCurrentMedicine>().ToTable("PatientCurrentMedicines");
            modelBuilder.Entity<Prescription>().ToTable("Prescriptions");
            modelBuilder.Entity<Sale>().ToTable("Sales");
            modelBuilder.Entity<SaleDetail>().ToTable("SaleDetails");
            modelBuilder.Entity<DrugInteraction>().ToTable("DrugInteractions");
            modelBuilder.Entity<Contraindication>().ToTable("Contraindications");
            modelBuilder.Entity<SafetyCheck>().ToTable("SafetyChecks");
            modelBuilder.Entity<Warning>().ToTable("Warnings");
        }
    }
}
