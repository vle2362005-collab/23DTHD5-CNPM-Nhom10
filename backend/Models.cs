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
}
