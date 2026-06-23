using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Roles")]
    public class DbRole
    {
        [Key]
        public int RoleId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;
    }

    [Table("Users")]
    public class DbUser
    {
        [Key]
        public int UserId { get; set; }
        
        public int RoleId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("RoleId")]
        public DbRole? Role { get; set; }
    }

    [Table("Medicines")]
    public class DbMedicine
    {
        [Key]
        public int MedicineId { get; set; }

        public int? DrugGroupId { get; set; }

        [Required]
        [StringLength(150)]
        public string MedicineName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Strength { get; set; }

        [StringLength(100)]
        public string? DosageForm { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;

        public bool RequiresPrescription { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("MedicineIngredients")]
    public class DbMedicineIngredient
    {
        public int MedicineId { get; set; }

        public int IngredientId { get; set; }

        [StringLength(100)]
        public string? Amount { get; set; }
    }

    [Table("DrugGroups")]
    public class DbDrugGroup
    {
        [Key]
        public int DrugGroupId { get; set; }

        [Required]
        [StringLength(150)]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    [Table("ActiveIngredients")]
    public class DbActiveIngredient
    {
        [Key]
        public int IngredientId { get; set; }

        [Required]
        [StringLength(150)]
        public string IngredientName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    [Table("Diseases")]
    public class DbDisease
    {
        [Key]
        public int DiseaseId { get; set; }

        [Required]
        [StringLength(150)]
        public string DiseaseName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    [Table("DrugInteractions")]
    public class DbDrugInteraction
    {
        [Key]
        public int InteractionId { get; set; }

        public int IngredientAId { get; set; }

        public int IngredientBId { get; set; }

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Recommendation { get; set; }
    }

    [Table("Contraindications")]
    public class DbContraindication
    {
        [Key]
        public int ContraindicationId { get; set; }

        public int? MedicineId { get; set; }

        public int? IngredientId { get; set; }

        public int? DiseaseId { get; set; }

        [StringLength(100)]
        public string? ConditionType { get; set; }

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Recommendation { get; set; }
    }

    [Table("Sales")]
    public class DbSale
    {
        [Key]
        public int SaleId { get; set; }

        public int PatientId { get; set; }

        public int PharmacistId { get; set; }

        public int? PrescriptionId { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string FinalDecision { get; set; } = "Pending";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? Note { get; set; }
    }

    [Table("SaleDetails")]
    public class DbSaleDetail
    {
        [Key]
        public int SaleDetailId { get; set; }

        public int SaleId { get; set; }

        public int MedicineId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [StringLength(500)]
        public string? DosageInstruction { get; set; }

        public int? TimesPerDay { get; set; }

        [StringLength(100)]
        public string? Duration { get; set; }

        [StringLength(500)]
        public string? AdviceNote { get; set; }
    }

    [Table("SafetyChecks")]
    public class DbSafetyCheck
    {
        [Key]
        public int SafetyCheckId { get; set; }

        public int SaleId { get; set; }

        public DateTime CheckedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? HighestSeverity { get; set; }

        [Required]
        [StringLength(50)]
        public string Result { get; set; } = "Approved";

        [StringLength(500)]
        public string? Recommendation { get; set; }
    }

    [Table("Warnings")]
    public class DbWarning
    {
        [Key]
        public int WarningId { get; set; }

        public int SafetyCheckId { get; set; }

        public int PatientId { get; set; }

        public int? MedicineId { get; set; }

        [Required]
        [StringLength(100)]
        public string WarningType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Recommendation { get; set; }

        public bool IsAcknowledged { get; set; } = false;

        public int? AcknowledgedBy { get; set; }

        public DateTime? AcknowledgedAt { get; set; }

        [StringLength(50)]
        public string? Decision { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("Patients")]
    public class DbPatient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? WeightKg { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public bool IsPregnant { get; set; } = false;

        public bool IsBreastfeeding { get; set; } = false;

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("PatientAllergies")]
    public class DbPatientAllergy
    {
        [Key]
        public int AllergyId { get; set; }

        public int PatientId { get; set; }

        public int? IngredientId { get; set; }

        public int? MedicineId { get; set; }

        [StringLength(500)]
        public string? AllergyNote { get; set; }

        [StringLength(50)]
        public string? Severity { get; set; }
    }

    [Table("PatientDiseases")]
    public class DbPatientDisease
    {
        [Key]
        public int PatientDiseaseId { get; set; }

        public int PatientId { get; set; }

        public int DiseaseId { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }
}
