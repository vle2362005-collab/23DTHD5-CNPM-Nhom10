using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }

    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public decimal? WeightKg { get; set; }
        public string? Address { get; set; }
        public bool IsPregnant { get; set; } = false;
        public bool IsBreastfeeding { get; set; } = false;
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }

    public class DrugGroup
    {
        [Key]
        public int DrugGroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ActiveIngredient
    {
        [Key]
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class Medicine
    {
        [Key]
        public int MedicineId { get; set; }
        public int? DrugGroupId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string? Strength { get; set; }
        public string? DosageForm { get; set; }
        public string? Unit { get; set; }
        public decimal Price { get; set; } = 0;
        public bool RequiresPrescription { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string? SideEffects { get; set; }
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }

    public class MedicineIngredient
    {
        public int MedicineId { get; set; }
        public int IngredientId { get; set; }
        public string? Amount { get; set; }
    }

    public class Disease
    {
        [Key]
        public int DiseaseId { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class PatientDisease
    {
        [Key]
        public int PatientDiseaseId { get; set; }
        public int PatientId { get; set; }
        public int DiseaseId { get; set; }
        public string? Note { get; set; }
    }

    public class PatientAllergy
    {
        [Key]
        public int AllergyId { get; set; }
        public int PatientId { get; set; }
        public int? IngredientId { get; set; }
        public int? MedicineId { get; set; }
        public string? AllergyNote { get; set; }
        public string? Severity { get; set; }
    }

    public class PatientCurrentMedicine
    {
        [Key]
        public int CurrentMedicineId { get; set; }
        public int PatientId { get; set; }
        public int? MedicineId { get; set; }
        public string? MedicineNameText { get; set; }
        public string? Note { get; set; }
    }

    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }
        public int PatientId { get; set; }
        public string? PrescriptionCode { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? PrescribedDate { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsValid { get; set; } = true;
        public string? Note { get; set; }
    }

    public class Sale
    {
        [Key]
        public int SaleId { get; set; }
        public int PatientId { get; set; }
        public int PharmacistId { get; set; }
        public int? PrescriptionId { get; set; }
        public DateTime? SaleDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; } = 0;
        public string FinalDecision { get; set; } = "Pending";
        public string Status { get; set; } = "Pending";
        public string? Note { get; set; }
    }

    public class SaleDetail
    {
        [Key]
        public int SaleDetailId { get; set; }
        public int SaleId { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? DosageInstruction { get; set; }
        public int? TimesPerDay { get; set; }
        public string? Duration { get; set; }
        public string? AdviceNote { get; set; }
    }

    public class DrugInteraction
    {
        [Key]
        public int InteractionId { get; set; }
        public int IngredientAId { get; set; }
        public int IngredientBId { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Recommendation { get; set; }
    }

    public class Contraindication
    {
        [Key]
        public int ContraindicationId { get; set; }
        public int? MedicineId { get; set; }
        public int? IngredientId { get; set; }
        public int? DiseaseId { get; set; }
        public string? ConditionType { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Recommendation { get; set; }
    }

    public class SafetyCheck
    {
        [Key]
        public int SafetyCheckId { get; set; }
        public int SaleId { get; set; }
        public DateTime? CheckedAt { get; set; } = DateTime.Now;
        public string? HighestSeverity { get; set; }
        public string Result { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
    }

    public class Warning
    {
        [Key]
        public int WarningId { get; set; }
        public int SafetyCheckId { get; set; }
        public int PatientId { get; set; }
        public int? MedicineId { get; set; }
        public string WarningType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
        public bool? IsAcknowledged { get; set; } = false;
        public int? AcknowledgedBy { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? Decision { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}
