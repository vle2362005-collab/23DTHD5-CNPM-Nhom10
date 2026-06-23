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
}
