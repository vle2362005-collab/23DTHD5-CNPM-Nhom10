using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Register DB Context
builder.Services.AddDbContext<PharmacySafetyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JSON serialization options to handle reference cycles & preserve casing matching frontend fields
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.PropertyNamingPolicy = null;
});

// Configure CORS to authorize Vite dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ==========================================
// API ENDPOINTS
// ==========================================

// --- Patients CRUD ---
app.MapGet("/api/patients", async (PharmacySafetyContext context) =>
    await context.Patients.ToListAsync());

app.MapPost("/api/patients", async (Patient patient, PharmacySafetyContext context) =>
{
    patient.CreatedAt = DateTime.Now;
    context.Patients.Add(patient);
    await context.SaveChangesAsync();
    return Results.Created($"/api/patients/{patient.PatientId}", patient);
});

app.MapPut("/api/patients/{id:int}", async (int id, Patient updatedPatient, PharmacySafetyContext context) =>
{
    var patient = await context.Patients.FindAsync(id);
    if (patient == null) return Results.NotFound();

    patient.FullName = updatedPatient.FullName;
    patient.Phone = updatedPatient.Phone;
    patient.Gender = updatedPatient.Gender;
    patient.DateOfBirth = updatedPatient.DateOfBirth;
    patient.WeightKg = updatedPatient.WeightKg;
    patient.Address = updatedPatient.Address;
    patient.IsPregnant = updatedPatient.IsPregnant;
    patient.IsBreastfeeding = updatedPatient.IsBreastfeeding;
    patient.Note = updatedPatient.Note;

    await context.SaveChangesAsync();
    return Results.Ok(patient);
});

app.MapDelete("/api/patients/{id:int}", async (int id, PharmacySafetyContext context) =>
{
    var patient = await context.Patients.FindAsync(id);
    if (patient == null) return Results.NotFound();

    context.Patients.Remove(patient);
    await context.SaveChangesAsync();
    return Results.Ok(true);
});

// --- Medicines CRUD ---
app.MapGet("/api/medicines", async (PharmacySafetyContext context) =>
    await context.Medicines.ToListAsync());

app.MapPost("/api/medicines", async (Medicine medicine, PharmacySafetyContext context) =>
{
    medicine.CreatedAt = DateTime.Now;
    context.Medicines.Add(medicine);
    await context.SaveChangesAsync();
    return Results.Created($"/api/medicines/{medicine.MedicineId}", medicine);
});

app.MapPut("/api/medicines/{id:int}", async (int id, Medicine updatedMedicine, PharmacySafetyContext context) =>
{
    var medicine = await context.Medicines.FindAsync(id);
    if (medicine == null) return Results.NotFound();

    medicine.DrugGroupId = updatedMedicine.DrugGroupId;
    medicine.MedicineName = updatedMedicine.MedicineName;
    medicine.Strength = updatedMedicine.Strength;
    medicine.DosageForm = updatedMedicine.DosageForm;
    medicine.Unit = updatedMedicine.Unit;
    medicine.Price = updatedMedicine.Price;
    medicine.RequiresPrescription = updatedMedicine.RequiresPrescription;
    medicine.IsActive = updatedMedicine.IsActive;
    medicine.SideEffects = updatedMedicine.SideEffects;
    medicine.Note = updatedMedicine.Note;

    await context.SaveChangesAsync();
    return Results.Ok(medicine);
});

app.MapDelete("/api/medicines/{id:int}", async (int id, PharmacySafetyContext context) =>
{
    var medicine = await context.Medicines.FindAsync(id);
    if (medicine == null) return Results.NotFound();

    context.Medicines.Remove(medicine);
    await context.SaveChangesAsync();
    return Results.Ok(true);
});

// --- Users (Read-only) ---
app.MapGet("/api/users", async (PharmacySafetyContext context) =>
    await context.Users.ToListAsync());

// --- DrugGroups CRUD ---
app.MapGet("/api/druggroups", async (PharmacySafetyContext context) =>
    await context.DrugGroups.ToListAsync());

app.MapPost("/api/druggroups", async (DrugGroup group, PharmacySafetyContext context) =>
{
    context.DrugGroups.Add(group);
    await context.SaveChangesAsync();
    return Results.Created($"/api/druggroups/{group.DrugGroupId}", group);
});

app.MapPut("/api/druggroups/{id:int}", async (int id, DrugGroup updatedGroup, PharmacySafetyContext context) =>
{
    var group = await context.DrugGroups.FindAsync(id);
    if (group == null) return Results.NotFound();

    group.GroupName = updatedGroup.GroupName;
    group.Description = updatedGroup.Description;

    await context.SaveChangesAsync();
    return Results.Ok(group);
});

app.MapDelete("/api/druggroups/{id:int}", async (int id, PharmacySafetyContext context) =>
{
    var group = await context.DrugGroups.FindAsync(id);
    if (group == null) return Results.NotFound();

    context.DrugGroups.Remove(group);
    await context.SaveChangesAsync();
    return Results.Ok(true);
});

// --- Ingredients (ActiveIngredients) CRUD ---
app.MapGet("/api/ingredients", async (PharmacySafetyContext context) =>
    await context.ActiveIngredients.ToListAsync());

app.MapPost("/api/ingredients", async (ActiveIngredient ingredient, PharmacySafetyContext context) =>
{
    context.ActiveIngredients.Add(ingredient);
    await context.SaveChangesAsync();
    return Results.Created($"/api/ingredients/{ingredient.IngredientId}", ingredient);
});

app.MapPut("/api/ingredients/{id:int}", async (int id, ActiveIngredient updatedIngredient, PharmacySafetyContext context) =>
{
    var ingredient = await context.ActiveIngredients.FindAsync(id);
    if (ingredient == null) return Results.NotFound();

    ingredient.IngredientName = updatedIngredient.IngredientName;
    ingredient.Description = updatedIngredient.Description;

    await context.SaveChangesAsync();
    return Results.Ok(ingredient);
});

app.MapDelete("/api/ingredients/{id:int}", async (int id, PharmacySafetyContext context) =>
{
    var ingredient = await context.ActiveIngredients.FindAsync(id);
    if (ingredient == null) return Results.NotFound();

    context.ActiveIngredients.Remove(ingredient);
    await context.SaveChangesAsync();
    return Results.Ok(true);
});

// --- MedicineIngredients ---
app.MapGet("/api/medicineingredients", async (PharmacySafetyContext context) =>
    await context.MedicineIngredients.ToListAsync());

// --- Diseases ---
app.MapGet("/api/diseases", async (PharmacySafetyContext context) =>
    await context.Diseases.ToListAsync());

// --- PatientDiseases ---
app.MapGet("/api/patientdiseases", async (PharmacySafetyContext context) =>
    await context.PatientDiseases.ToListAsync());

// --- PatientAllergies ---
app.MapGet("/api/patientallergies", async (PharmacySafetyContext context) =>
    await context.PatientAllergies.ToListAsync());

// --- DrugInteractions ---
app.MapGet("/api/druginteractions", async (PharmacySafetyContext context) =>
    await context.DrugInteractions.ToListAsync());

// --- Contraindications CRUD ---
app.MapGet("/api/contraindications", async (PharmacySafetyContext context) =>
    await context.Contraindications.ToListAsync());

app.MapPost("/api/contraindications", async (Contraindication contra, PharmacySafetyContext context) =>
{
    context.Contraindications.Add(contra);
    await context.SaveChangesAsync();
    return Results.Created($"/api/contraindications/{contra.ContraindicationId}", contra);
});

app.MapPut("/api/contraindications/{id:int}", async (int id, Contraindication updatedContra, PharmacySafetyContext context) =>
{
    var contra = await context.Contraindications.FindAsync(id);
    if (contra == null) return Results.NotFound();

    contra.MedicineId = updatedContra.MedicineId;
    contra.IngredientId = updatedContra.IngredientId;
    contra.DiseaseId = updatedContra.DiseaseId;
    contra.ConditionType = updatedContra.ConditionType;
    contra.Severity = updatedContra.Severity;
    contra.Description = updatedContra.Description;
    contra.Recommendation = updatedContra.Recommendation;

    await context.SaveChangesAsync();
    return Results.Ok(contra);
});

app.MapDelete("/api/contraindications/{id:int}", async (int id, PharmacySafetyContext context) =>
{
    var contra = await context.Contraindications.FindAsync(id);
    if (contra == null) return Results.NotFound();

    context.Contraindications.Remove(contra);
    await context.SaveChangesAsync();
    return Results.Ok(true);
});

// --- Sales history ---
app.MapGet("/api/sales", async (PharmacySafetyContext context) =>
    await context.Sales.OrderByDescending(s => s.SaleId).ToListAsync());

// --- Clinical Safety Check Engine ---
app.MapPost("/api/safety-check", async (SafetyCheckRequest request, PharmacySafetyContext context) =>
{
    var patient = await context.Patients.FindAsync(request.PatientId);
    if (patient == null)
    {
        return Results.NotFound(new { message = "Không tìm thấy bệnh nhân." });
    }

    var warnings = new List<Warning>();
    var cartMedicineIds = request.CartItems.Select(ci => ci.MedicineId).Distinct().ToList();
    var medicineIngredients = await context.MedicineIngredients
        .Where(mi => cartMedicineIds.Contains(mi.MedicineId))
        .ToListAsync();

    var medicines = await context.Medicines
        .Where(m => cartMedicineIds.Contains(m.MedicineId))
        .ToDictionaryAsync(m => m.MedicineId);

    var allIngredientIdsInCart = medicineIngredients.Select(mi => mi.IngredientId).Distinct().ToList();
    var ingredients = await context.ActiveIngredients
        .Where(ai => allIngredientIdsInCart.Contains(ai.IngredientId))
        .ToDictionaryAsync(ai => ai.IngredientId);

    // 1. Allergies Check
    var allergies = await context.PatientAllergies
        .Where(pa => pa.PatientId == request.PatientId)
        .ToListAsync();

    foreach (var cartMedId in cartMedicineIds)
    {
        var medIngredients = medicineIngredients.Where(mi => mi.MedicineId == cartMedId).ToList();
        var med = medicines.GetValueOrDefault(cartMedId);
        if (med == null) continue;

        foreach (var medIng in medIngredients)
        {
            var matchedAllergy = allergies.FirstOrDefault(pa => pa.IngredientId == medIng.IngredientId);
            if (matchedAllergy != null)
            {
                var ingName = ingredients.TryGetValue(medIng.IngredientId, out var ingObj) ? ingObj.IngredientName : "Hoạt chất";
                warnings.Add(new Warning
                {
                    WarningId = Random.Shared.Next(1, 10000),
                    PatientId = request.PatientId,
                    MedicineId = cartMedId,
                    WarningType = "Dị ứng thuốc",
                    Severity = matchedAllergy.Severity ?? "Nghiêm trọng",
                    Message = $"Bệnh nhân dị ứng với hoạt chất [{ingName}]. Thuốc [{med.MedicineName}] có chứa hoạt chất này.",
                    Recommendation = $"Ngay lập tức thay thế thuốc [{med.MedicineName}] bằng một thuốc khác không thuộc cùng nhóm dược lý.",
                    IsAcknowledged = false
                });
            }
        }

        var matchedMedAllergy = allergies.FirstOrDefault(pa => pa.MedicineId == cartMedId);
        if (matchedMedAllergy != null)
        {
            warnings.Add(new Warning
            {
                WarningId = Random.Shared.Next(1, 10000),
                PatientId = request.PatientId,
                MedicineId = cartMedId,
                WarningType = "Dị ứng thuốc",
                Severity = matchedMedAllergy.Severity ?? "Nghiêm trọng",
                Message = $"Bệnh nhân dị ứng với thuốc [{med.MedicineName}].",
                Recommendation = $"Ngay lập tức thay thế thuốc [{med.MedicineName}] bằng một thuốc khác không thuộc cùng nhóm dược lý.",
                IsAcknowledged = false
            });
        }
    }

    // 2. Drug Interactions Check
    var interactions = await context.DrugInteractions.ToListAsync();
    for (int i = 0; i < medicineIngredients.Count; i++)
    {
        for (int j = i + 1; j < medicineIngredients.Count; j++)
        {
            var miA = medicineIngredients[i];
            var miB = medicineIngredients[j];
            if (miA.MedicineId == miB.MedicineId) continue;

            var medA = medicines.GetValueOrDefault(miA.MedicineId);
            var medB = medicines.GetValueOrDefault(miB.MedicineId);
            var ingA = ingredients.GetValueOrDefault(miA.IngredientId);
            var ingB = ingredients.GetValueOrDefault(miB.IngredientId);

            if (medA != null && medB != null && ingA != null && ingB != null)
            {
                var interact = interactions.FirstOrDefault(di =>
                    (di.IngredientAId == miA.IngredientId && di.IngredientBId == miB.IngredientId) ||
                    (di.IngredientAId == miB.IngredientId && di.IngredientBId == miA.IngredientId));

                if (interact != null)
                {
                    warnings.Add(new Warning
                    {
                        WarningId = Random.Shared.Next(1, 10000),
                        PatientId = request.PatientId,
                        MedicineId = miA.MedicineId,
                        WarningType = "Tương tác thuốc",
                        Severity = interact.Severity,
                        Message = $"Tương tác nghiêm trọng giữa [{medA.MedicineName}] ({ingA.IngredientName}) và [{medB.MedicineName}] ({ingB.IngredientName}). {interact.Description}",
                        Recommendation = interact.Recommendation,
                        IsAcknowledged = false
                    });
                }
            }
        }
    }

    // 3. Contraindications Check
    var patientDiseases = await context.PatientDiseases
        .Where(pd => pd.PatientId == request.PatientId)
        .ToListAsync();

    var contraindications = await context.Contraindications.ToListAsync();
    var allDiseases = await context.Diseases.ToDictionaryAsync(d => d.DiseaseId);

    foreach (var cartMedId in cartMedicineIds)
    {
        var med = medicines.GetValueOrDefault(cartMedId);
        if (med == null) continue;

        var medIngredients = medicineIngredients.Where(mi => mi.MedicineId == cartMedId).Select(mi => mi.IngredientId).ToList();

        foreach (var pd in patientDiseases)
        {
            var contra = contraindications.FirstOrDefault(c =>
                c.DiseaseId == pd.DiseaseId &&
                ((c.MedicineId.HasValue && c.MedicineId.Value == cartMedId) ||
                 (c.IngredientId.HasValue && medIngredients.Contains(c.IngredientId.Value))));

            if (contra != null)
            {
                var disName = allDiseases.TryGetValue(pd.DiseaseId, out var dObj) ? dObj.DiseaseName : "Bệnh lý";
                warnings.Add(new Warning
                {
                    WarningId = Random.Shared.Next(1, 10000),
                    PatientId = request.PatientId,
                    MedicineId = cartMedId,
                    WarningType = "Chống chỉ định bệnh nền",
                    Severity = contra.Severity,
                    Message = $"Thuốc [{med.MedicineName}] chống chỉ định ở người có bệnh nền [{disName}]. {contra.Description}",
                    Recommendation = contra.Recommendation,
                    IsAcknowledged = false
                });
            }
        }

        if (patient.IsPregnant)
        {
            var pregContra = contraindications.FirstOrDefault(c =>
                c.ConditionType == "Đối tượng đặc biệt" &&
                ((c.MedicineId.HasValue && c.MedicineId.Value == cartMedId) ||
                 (c.IngredientId.HasValue && medIngredients.Contains(c.IngredientId.Value))));

            if (pregContra != null)
            {
                warnings.Add(new Warning
                {
                    WarningId = Random.Shared.Next(1, 10000),
                    PatientId = request.PatientId,
                    MedicineId = cartMedId,
                    WarningType = "Đối tượng đặc biệt",
                    Severity = pregContra.Severity,
                    Message = $"Thuốc [{med.MedicineName}] chống chỉ định ở phụ nữ mang thai. {pregContra.Description}",
                    Recommendation = pregContra.Recommendation,
                    IsAcknowledged = false
                });
            }
        }
    }

    // 4. Prescription Required Check
    foreach (var item in request.CartItems)
    {
        var med = medicines.GetValueOrDefault(item.MedicineId);
        if (med != null && med.RequiresPrescription)
        {
            warnings.Add(new Warning
            {
                WarningId = Random.Shared.Next(1, 10000),
                PatientId = request.PatientId,
                MedicineId = item.MedicineId,
                WarningType = "PrescriptionRequired",
                Severity = "Trung bình",
                Message = $"Thuốc [{med.MedicineName}] yêu cầu phải có đơn thuốc của bác sĩ.",
                Recommendation = "Yêu cầu bệnh nhân cung cấp đơn thuốc hoặc liên hệ bác sĩ kê toa.",
                IsAcknowledged = false
            });
        }
    }

    var highestSeverity = warnings.Any() ? "Medium" : "None";
    var result = warnings.Any() ? "Warning" : "Approved";

    return Results.Ok(new SafetyCheckResponse
    {
        Warnings = warnings,
        HighestSeverity = highestSeverity,
        Result = result
    });
});

// --- Checkout and Save Sale with Warnings ---
app.MapPost("/api/sales", async (CreateSaleRequest request, PharmacySafetyContext context) =>
{
    decimal totalAmount = 0;
    foreach (var item in request.CartItems)
    {
        var medicine = await context.Medicines.FindAsync(item.MedicineId);
        if (medicine != null)
        {
            totalAmount += medicine.Price * item.Quantity;
        }
    }

    var sale = new Sale
    {
        PatientId = request.PatientId,
        PharmacistId = 2,
        PrescriptionId = null,
        SaleDate = DateTime.Now,
        TotalAmount = totalAmount,
        FinalDecision = request.FinalDecision,
        Status = request.FinalDecision == "Denied" ? "Cancelled" : "Completed",
        Note = request.Note
    };

    context.Sales.Add(sale);
    await context.SaveChangesAsync();

    foreach (var item in request.CartItems)
    {
        var medicine = await context.Medicines.FindAsync(item.MedicineId);
        var detail = new SaleDetail
        {
            SaleId = sale.SaleId,
            MedicineId = item.MedicineId,
            Quantity = item.Quantity,
            UnitPrice = medicine?.Price ?? 0,
            DosageInstruction = item.DosageInstruction,
            TimesPerDay = item.TimesPerDay,
            Duration = item.Duration,
            AdviceNote = item.AdviceNote
        };
        context.SaleDetails.Add(detail);
    }
    await context.SaveChangesAsync();

    if (request.Warnings.Any())
    {
        var highestSeverity = request.Warnings.Any(w => w.Severity == "Nghiêm trọng" || w.Severity == "High") ? "High" : "Medium";
        var safetyCheck = new SafetyCheck
        {
            SaleId = sale.SaleId,
            CheckedAt = DateTime.Now,
            HighestSeverity = highestSeverity,
            Result = "Warning",
            Recommendation = "Cần tuân thủ hướng dẫn và theo dõi tác dụng phụ của thuốc."
        };
        context.SafetyChecks.Add(safetyCheck);
        await context.SaveChangesAsync();

        foreach (var wDto in request.Warnings)
        {
            DateTime? ackAt = null;
            if (!string.IsNullOrEmpty(wDto.AcknowledgedAt))
            {
                if (DateTime.TryParse(wDto.AcknowledgedAt, out var dt))
                    ackAt = dt;
                else
                    ackAt = DateTime.Now;
            }

            var warning = new Warning
            {
                SafetyCheckId = safetyCheck.SafetyCheckId,
                PatientId = request.PatientId,
                MedicineId = wDto.MedicineId,
                WarningType = wDto.WarningType,
                Severity = wDto.Severity,
                Message = wDto.Message,
                Recommendation = wDto.Recommendation,
                IsAcknowledged = wDto.IsAcknowledged,
                AcknowledgedBy = wDto.AcknowledgedBy,
                AcknowledgedAt = ackAt,
                Decision = wDto.Decision,
                CreatedAt = DateTime.Now
            };
            context.Warnings.Add(warning);
        }
        await context.SaveChangesAsync();
    }

    return Results.Created($"/api/sales/{sale.SaleId}", sale);
});

app.Run();

// ==========================================
// DTO DEFINITIONS
// ==========================================

public class SafetyCheckRequest
{
    public int PatientId { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new();
}

public class CartItemDto
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public string DosageInstruction { get; set; } = string.Empty;
    public int TimesPerDay { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string AdviceNote { get; set; } = string.Empty;
}

public class SafetyCheckResponse
{
    public List<Warning> Warnings { get; set; } = new();
    public string HighestSeverity { get; set; } = "None";
    public string Result { get; set; } = "Approved";
}

public class CreateSaleRequest
{
    public int PatientId { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new();
    public string FinalDecision { get; set; } = "Pending";
    public List<WarningDto> Warnings { get; set; } = new();
    public string Note { get; set; } = string.Empty;
}

public class WarningDto
{
    public int? MedicineId { get; set; }
    public string WarningType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public bool IsAcknowledged { get; set; }
    public int? AcknowledgedBy { get; set; }
    public string? AcknowledgedAt { get; set; }
    public string? Decision { get; set; }
}
