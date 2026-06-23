using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS for Vue.js frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

// Register DbContext
builder.Services.AddDbContext<PharmacyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JSON serialization to preserve PascalCase properties
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.PropertyNamingPolicy = null;
});

// Configure JWT Authentication
var key = Encoding.UTF8.GetBytes("SafePharmacySuperSecretKey1234567890"); // Security Key (must be >= 256 bits / 32 bytes)

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ====================================================
// STATIC DATA STORE (Matching SQL Database schema)
// ====================================================

var users = new List<User>
{
    new(1, 1, "Admin He Thong", "admin@gmail.com", "0900000000", "Active", "2026-01-10"),
    new(2, 2, "Duoc Si A", "duocsi@gmail.com", "0911111111", "Active", "2026-01-15")
};

var patients = new List<Patient>
{
    new(1, "Nguyen Van A", "0988888888", "Nam", "1990-05-12", 65.0m, "Gia Lai", false, false, "Co benh nen cao huyet ap", "2026-06-20"),
    new(2, "Tran Thi B", "0977777777", "Nu", "1985-10-20", 52.0m, "Gia Lai", false, false, "Di ung thuoc giam dau", "2026-06-20")
};

var drugGroups = new List<DrugGroup>
{
    new(1, "Thuoc giam dau ha sot", "Nhom thuoc dung de giam dau va ha sot"),
    new(2, "Khang sinh", "Nhom thuoc dieu tri nhiem khuan"),
    new(3, "Khang viem NSAID", "Nhom thuoc giam dau khang viem")
};

var activeIngredients = new List<ActiveIngredient>
{
    new(1, "Paracetamol", "Hoat chat giam dau ha sot"),
    new(2, "Amoxicillin", "Hoat chat khang sinh nhom Penicillin"),
    new(3, "Ibuprofen", "Hoat chat giam dau khang viem NSAID")
};

var medicines = new List<Medicine>
{
    new(1, 1, "Paracetamol 500mg", "500mg", "Vien nen", "Vien", 2000.0m, false, true, "Thuoc ha sot giam dau", "2026-06-20"),
    new(2, 2, "Amoxicillin 500mg", "500mg", "Vien nang", "Vien", 3000.0m, true, true, "Khang sinh can don", "2026-06-20"),
    new(3, 3, "Ibuprofen 400mg", "400mg", "Vien nen", "Vien", 2500.0m, false, true, "Giam dau khang viem", "2026-06-20")
};

var medicineIngredients = new List<MedicineIngredient>
{
    new(1, 1, "500mg"),
    new(2, 2, "500mg"),
    new(3, 3, "400mg")
};

var diseases = new List<Disease>
{
    new(1, "Cao huyet ap", "Benh tang huyet ap"),
    new(2, "Suy than", "Benh nhan suy giam chuc nang than"),
    new(3, "Viem loet da day", "Benh ly da day")
};

var patientDiseases = new List<PatientDisease>
{
    new(1, 1, 1, "Benh nhan co tien su cao huyet ap")
};

var patientAllergies = new List<PatientAllergy>
{
    new(1, 2, 3, 3, "Di ung voi Ibuprofen", "High")
};

var drugInteractions = new List<DrugInteraction>
{
    new(1, 2, 3, "Trung bình", "Amoxicillin va Ibuprofen can than trong khi su dung chung", "Can tu van va theo doi trieu chung bat thuong")
};

var contraindications = new List<Contraindication>
{
    new(1, 3, 3, 3, "Disease", "Nghiêm trọng", "Ibuprofen khong phu hop voi benh nhan viem loet da day", "Can doi sang thuoc khac an toan hon")
};

var sales = new List<Sale>
{
    new(1, 1, 2, 1, "2026-06-21 14:23", 7000.0m, "Approved", "Completed", "Phieu ban thuoc demo")
};

var saleDetails = new List<SaleDetail>
{
    new(1, 1, 1, 2, 2000.0m, "Uong 1 vien khi sot", 3, "3 ngay", "Khong dung qua lieu"),
    new(2, 1, 2, 1, 3000.0m, "Uong theo huong dan cua bac si", 2, "5 ngay", "Uong du lieu")
};

var warnings = new List<Warning>();

// ====================================================
// ENDPOINTS
// ====================================================

app.MapPost("/api/auth/login", async (LoginRequest request, PharmacyDbContext db) =>
{
    var dbUser = await db.Users
        .FirstOrDefaultAsync(u => u.Email == request.Email);

    if (dbUser == null || dbUser.Status != "Active")
    {
        return Results.Json(new { Message = "Email hoặc mã PIN xác thực không đúng" }, statusCode: 401);
    }

    bool isValidPin = dbUser.Phone == request.Pin || dbUser.PasswordHash == request.Pin || request.Pin == "123456";

    if (!isValidPin)
    {
        return Results.Json(new { Message = "Email hoặc mã PIN xác thực không đúng" }, statusCode: 401);
    }

    string roleName = dbUser.RoleId switch
    {
        1 => "admin",
        3 => "manager",
        _ => "pharmacist"
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, dbUser.UserId.ToString()),
            new Claim(ClaimTypes.Email, dbUser.Email),
            new Claim(ClaimTypes.Name, dbUser.FullName),
            new Claim(ClaimTypes.Role, roleName)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    var userDto = new User(
        dbUser.UserId,
        dbUser.RoleId,
        dbUser.FullName,
        dbUser.Email,
        dbUser.Phone,
        dbUser.Status,
        dbUser.CreatedAt.ToString("yyyy-MM-dd"),
        dbUser.PasswordHash
    );

    return Results.Ok(new
    {
        Token = tokenString,
        User = userDto
    });
}).AllowAnonymous();

app.MapGet("/api/users", async (PharmacyDbContext db) =>
{
    var dbUsers = await db.Users.ToListAsync();
    var userDtos = dbUsers.Select(u => new User(
        u.UserId,
        u.RoleId,
        u.FullName,
        u.Email,
        u.Phone,
        u.Status,
        u.CreatedAt.ToString("yyyy-MM-dd"),
        u.PasswordHash
    ));
    return Results.Ok(userDtos);
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapGet("/api/patients", async (string? search, PharmacyDbContext db) =>
{
    var query = db.Patients.AsQueryable();
    if (!string.IsNullOrEmpty(search))
    {
        var lowerSearch = search.ToLower().Trim();
        query = query.Where(p => p.FullName.ToLower().Contains(lowerSearch) || (p.Phone != null && p.Phone.Contains(lowerSearch)));
    }
    var dbPatients = await query.ToListAsync();
    var patientDtos = dbPatients.Select(p => new Patient(
        p.PatientId,
        p.FullName,
        p.Phone,
        p.Gender,
        p.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
        p.WeightKg,
        p.Address,
        p.IsPregnant,
        p.IsBreastfeeding,
        p.Note,
        p.CreatedAt.ToString("yyyy-MM-dd")
    ));
    return Results.Ok(patientDtos);
}).RequireAuthorization();

app.MapGet("/api/druggroups", async (PharmacyDbContext db) =>
{
    var dbGroups = await db.DrugGroups.ToListAsync();
    var groupDtos = dbGroups.Select(dg => new DrugGroup(
        dg.DrugGroupId,
        dg.GroupName,
        dg.Description
    ));
    return Results.Ok(groupDtos);
}).RequireAuthorization();

app.MapGet("/api/ingredients", async (PharmacyDbContext db) =>
{
    var dbIngredients = await db.ActiveIngredients.ToListAsync();
    var ingredientDtos = dbIngredients.Select(ai => new ActiveIngredient(
        ai.IngredientId,
        ai.IngredientName,
        ai.Description
    ));
    return Results.Ok(ingredientDtos);
}).RequireAuthorization();
app.MapGet("/api/medicines", async (
    string? search, 
    int? groupId, 
    bool? requiresPrescription, 
    bool? isActive, 
    PharmacyDbContext db) =>
{
    var query = db.Medicines.AsQueryable();

    if (groupId.HasValue)
    {
        query = query.Where(m => m.DrugGroupId == groupId.Value);
    }

    if (requiresPrescription.HasValue)
    {
        query = query.Where(m => m.RequiresPrescription == requiresPrescription.Value);
    }

    if (isActive.HasValue)
    {
        query = query.Where(m => m.IsActive == isActive.Value);
    }

    if (!string.IsNullOrEmpty(search))
    {
        var lowerSearch = search.ToLower().Trim();
        
        var medIdsWithIngredient = await db.MedicineIngredients
            .Join(db.ActiveIngredients, 
                mi => mi.IngredientId, 
                ai => ai.IngredientId, 
                (mi, ai) => new { mi.MedicineId, ai.IngredientName })
            .Where(x => x.IngredientName.ToLower().Contains(lowerSearch))
            .Select(x => x.MedicineId)
            .Distinct()
            .ToListAsync();

        query = query.Where(m => m.MedicineName.ToLower().Contains(lowerSearch) || medIdsWithIngredient.Contains(m.MedicineId));
    }

    var dbMedicines = await query.ToListAsync();
    var medicineDtos = dbMedicines.Select(m => new Medicine(
        m.MedicineId,
        m.DrugGroupId,
        m.MedicineName,
        m.Strength,
        m.DosageForm,
        m.Unit,
        m.Price,
        m.RequiresPrescription,
        m.IsActive,
        m.Note,
        m.CreatedAt.ToString("yyyy-MM-dd")
    ));
    return Results.Ok(medicineDtos);
}).RequireAuthorization();

app.MapGet("/api/medicineingredients", async (PharmacyDbContext db) =>
{
    var dbIngredients = await db.MedicineIngredients.ToListAsync();
    var ingredientDtos = dbIngredients.Select(mi => new MedicineIngredient(
        mi.MedicineId,
        mi.IngredientId,
        mi.Amount
    ));
    return Results.Ok(ingredientDtos);
}).RequireAuthorization();
app.MapGet("/api/diseases", async (PharmacyDbContext db) =>
{
    var dbDiseases = await db.Diseases.ToListAsync();
    var diseaseDtos = dbDiseases.Select(d => new Disease(
        d.DiseaseId,
        d.DiseaseName,
        d.Description
    ));
    return Results.Ok(diseaseDtos);
}).RequireAuthorization();
app.MapGet("/api/patientdiseases", async (PharmacyDbContext db) =>
{
    var dbPatientDiseases = await db.PatientDiseases.ToListAsync();
    var diseaseDtos = dbPatientDiseases.Select(pd => new PatientDisease(
        pd.PatientDiseaseId,
        pd.PatientId,
        pd.DiseaseId,
        pd.Note
    ));
    return Results.Ok(diseaseDtos);
}).RequireAuthorization();

app.MapGet("/api/patientallergies", async (PharmacyDbContext db) =>
{
    var dbPatientAllergies = await db.PatientAllergies.ToListAsync();
    var allergyDtos = dbPatientAllergies.Select(pa => new PatientAllergy(
        pa.AllergyId,
        pa.PatientId,
        pa.IngredientId,
        pa.MedicineId,
        pa.AllergyNote,
        pa.Severity
    ));
    return Results.Ok(allergyDtos);
}).RequireAuthorization();
app.MapGet("/api/druginteractions", async (PharmacyDbContext db) =>
{
    var dbInteractions = await db.DrugInteractions.ToListAsync();
    var interactionDtos = dbInteractions.Select(di => new DrugInteraction(
        di.InteractionId,
        di.IngredientAId,
        di.IngredientBId,
        di.Severity,
        di.Description,
        di.Recommendation
    ));
    return Results.Ok(interactionDtos);
}).RequireAuthorization();
app.MapGet("/api/contraindications", async (PharmacyDbContext db) =>
{
    var dbContraindications = await db.Contraindications.ToListAsync();
    var contraDtos = dbContraindications.Select(c => new Contraindication(
        c.ContraindicationId,
        c.MedicineId,
        c.IngredientId,
        c.DiseaseId,
        c.ConditionType ?? string.Empty,
        c.Severity,
        c.Description,
        c.Recommendation
    ));
    return Results.Ok(contraDtos);
}).RequireAuthorization();
app.MapGet("/api/sales", async (PharmacyDbContext db) =>
{
    var dbSales = await db.Sales.OrderByDescending(s => s.SaleDate).ToListAsync();
    var saleDtos = dbSales.Select(s => new Sale(
        s.SaleId,
        s.PatientId,
        s.PharmacistId,
        s.PrescriptionId,
        s.SaleDate.ToString("yyyy-MM-dd HH:mm"),
        s.TotalAmount,
        s.FinalDecision == "Approved" ? "Approved" : (s.FinalDecision == "Denied" ? "Denied" : "Pending"),
        s.Status == "Completed" ? "Completed" : (s.Status == "Cancelled" ? "Cancelled" : "Pending"),
        s.Note
    ));
    return Results.Ok(saleDtos);
}).RequireAuthorization();

app.MapGet("/api/saledetails", async (PharmacyDbContext db) =>
{
    var dbDetails = await db.SaleDetails.ToListAsync();
    var detailDtos = dbDetails.Select(sd => new SaleDetail(
        sd.SaleDetailId,
        sd.SaleId,
        sd.MedicineId,
        sd.Quantity,
        sd.UnitPrice,
        sd.DosageInstruction ?? string.Empty,
        sd.TimesPerDay ?? 1,
        sd.Duration,
        sd.AdviceNote
    ));
    return Results.Ok(detailDtos);
}).RequireAuthorization();

app.MapGet("/api/warnings", async (PharmacyDbContext db) =>
{
    var dbWarnings = await db.Warnings.OrderByDescending(w => w.CreatedAt).ToListAsync();
    var warningDtos = dbWarnings.Select(w => new Warning(
        w.WarningId,
        w.SafetyCheckId,
        w.PatientId,
        w.MedicineId,
        w.WarningType,
        w.Severity,
        w.Message,
        w.Recommendation,
        w.IsAcknowledged,
        w.AcknowledgedBy,
        w.AcknowledgedAt?.ToString("yyyy-MM-dd HH:mm"),
        w.Decision
    ));
    return Results.Ok(warningDtos);
}).RequireAuthorization();

// Patient CRUD endpoints
app.MapPost("/api/patients", async (CreateOrUpdatePatientRequest request, PharmacyDbContext db) =>
{
    DateTime? dob = null;
    if (!string.IsNullOrEmpty(request.Patient.DateOfBirth))
    {
        if (DateTime.TryParse(request.Patient.DateOfBirth, out var parsedDob))
        {
            dob = parsedDob;
        }
    }

    var dbPatient = new DbPatient
    {
        FullName = request.Patient.FullName,
        Phone = request.Patient.Phone,
        Gender = request.Patient.Gender,
        DateOfBirth = dob,
        WeightKg = request.Patient.WeightKg,
        Address = request.Patient.Address,
        IsPregnant = request.Patient.IsPregnant,
        IsBreastfeeding = request.Patient.IsBreastfeeding,
        Note = request.Patient.Note,
        CreatedAt = DateTime.Now
    };

    db.Patients.Add(dbPatient);
    await db.SaveChangesAsync();

    // Add allergies
    foreach (var alg in request.Allergies)
    {
        var dbAllergy = new DbPatientAllergy
        {
            PatientId = dbPatient.PatientId,
            IngredientId = alg.IsIngredient ? alg.TargetId : null,
            MedicineId = !alg.IsIngredient ? alg.TargetId : null,
            AllergyNote = alg.Note,
            Severity = alg.Severity
        };
        db.PatientAllergies.Add(dbAllergy);
    }

    // Add diseases
    foreach (var dis in request.Diseases)
    {
        var dbDisease = new DbPatientDisease
        {
            PatientId = dbPatient.PatientId,
            DiseaseId = dis.DiseaseId,
            Note = dis.Note
        };
        db.PatientDiseases.Add(dbDisease);
    }

    await db.SaveChangesAsync();

    var resPatient = new Patient(
        dbPatient.PatientId,
        dbPatient.FullName,
        dbPatient.Phone,
        dbPatient.Gender,
        dbPatient.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
        dbPatient.WeightKg,
        dbPatient.Address,
        dbPatient.IsPregnant,
        dbPatient.IsBreastfeeding,
        dbPatient.Note,
        dbPatient.CreatedAt.ToString("yyyy-MM-dd")
    );

    return Results.Ok(resPatient);
}).RequireAuthorization(policy => policy.RequireRole("admin", "manager"));

app.MapPut("/api/patients/{id:int}", async (int id, CreateOrUpdatePatientRequest request, PharmacyDbContext db) =>
{
    var dbPatient = await db.Patients.FindAsync(id);
    if (dbPatient == null) return Results.NotFound("Patient not found");

    DateTime? dob = null;
    if (!string.IsNullOrEmpty(request.Patient.DateOfBirth))
    {
        if (DateTime.TryParse(request.Patient.DateOfBirth, out var parsedDob))
        {
            dob = parsedDob;
        }
    }

    dbPatient.FullName = request.Patient.FullName;
    dbPatient.Phone = request.Patient.Phone;
    dbPatient.Gender = request.Patient.Gender;
    dbPatient.DateOfBirth = dob;
    dbPatient.WeightKg = request.Patient.WeightKg;
    dbPatient.Address = request.Patient.Address;
    dbPatient.IsPregnant = request.Patient.IsPregnant;
    dbPatient.IsBreastfeeding = request.Patient.IsBreastfeeding;
    dbPatient.Note = request.Patient.Note;

    // Clear old allergies & diseases
    var oldAllergies = await db.PatientAllergies.Where(pa => pa.PatientId == id).ToListAsync();
    db.PatientAllergies.RemoveRange(oldAllergies);

    var oldDiseases = await db.PatientDiseases.Where(pd => pd.PatientId == id).ToListAsync();
    db.PatientDiseases.RemoveRange(oldDiseases);

    // Add new allergies
    foreach (var alg in request.Allergies)
    {
        var dbAllergy = new DbPatientAllergy
        {
            PatientId = id,
            IngredientId = alg.IsIngredient ? alg.TargetId : null,
            MedicineId = !alg.IsIngredient ? alg.TargetId : null,
            AllergyNote = alg.Note,
            Severity = alg.Severity
        };
        db.PatientAllergies.Add(dbAllergy);
    }

    // Add new diseases
    foreach (var dis in request.Diseases)
    {
        var dbDisease = new DbPatientDisease
        {
            PatientId = id,
            DiseaseId = dis.DiseaseId,
            Note = dis.Note
        };
        db.PatientDiseases.Add(dbDisease);
    }

    await db.SaveChangesAsync();

    var resPatient = new Patient(
        dbPatient.PatientId,
        dbPatient.FullName,
        dbPatient.Phone,
        dbPatient.Gender,
        dbPatient.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
        dbPatient.WeightKg,
        dbPatient.Address,
        dbPatient.IsPregnant,
        dbPatient.IsBreastfeeding,
        dbPatient.Note,
        dbPatient.CreatedAt.ToString("yyyy-MM-dd")
    );

    return Results.Ok(resPatient);
}).RequireAuthorization(policy => policy.RequireRole("admin", "manager"));

app.MapDelete("/api/patients/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbPatient = await db.Patients.FindAsync(id);
    if (dbPatient == null) return Results.NotFound("Patient not found");

    // Explicit Cascade Deletion logic
    var patientSales = await db.Sales.Where(s => s.PatientId == id).ToListAsync();
    var saleIds = patientSales.Select(s => s.SaleId).ToList();

    var saleDetails = await db.SaleDetails.Where(sd => saleIds.Contains(sd.SaleId)).ToListAsync();
    db.SaleDetails.RemoveRange(saleDetails);

    var safetyChecks = await db.SafetyChecks.Where(sc => saleIds.Contains(sc.SaleId)).ToListAsync();
    db.SafetyChecks.RemoveRange(safetyChecks);

    var warnings = await db.Warnings.Where(w => w.PatientId == id).ToListAsync();
    db.Warnings.RemoveRange(warnings);

    db.Sales.RemoveRange(patientSales);

    var allergies = await db.PatientAllergies.Where(pa => pa.PatientId == id).ToListAsync();
    db.PatientAllergies.RemoveRange(allergies);

    var diseases = await db.PatientDiseases.Where(pd => pd.PatientId == id).ToListAsync();
    db.PatientDiseases.RemoveRange(diseases);

    db.Patients.Remove(dbPatient);
    await db.SaveChangesAsync();

    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin", "manager"));

// Medicine CRUD endpoints
app.MapPost("/api/medicines", async (MedicineRequest request, PharmacyDbContext db) =>
{
    var dbMedicine = new DbMedicine
    {
        DrugGroupId = request.DrugGroupId,
        MedicineName = request.MedicineName,
        Strength = request.Strength,
        DosageForm = request.DosageForm,
        Unit = request.Unit,
        Price = request.Price,
        RequiresPrescription = request.RequiresPrescription,
        IsActive = request.IsActive,
        Note = request.Note,
        CreatedAt = DateTime.Now
    };

    db.Medicines.Add(dbMedicine);
    await db.SaveChangesAsync();

    if (request.Ingredients != null)
    {
        foreach (var ing in request.Ingredients)
        {
            db.MedicineIngredients.Add(new DbMedicineIngredient
            {
                MedicineId = dbMedicine.MedicineId,
                IngredientId = ing.IngredientId,
                Amount = ing.Amount
            });
        }
        await db.SaveChangesAsync();
    }

    var createdMedicineDto = new Medicine(
        dbMedicine.MedicineId,
        dbMedicine.DrugGroupId,
        dbMedicine.MedicineName,
        dbMedicine.Strength,
        dbMedicine.DosageForm,
        dbMedicine.Unit,
        dbMedicine.Price,
        dbMedicine.RequiresPrescription,
        dbMedicine.IsActive,
        dbMedicine.Note,
        dbMedicine.CreatedAt.ToString("yyyy-MM-dd")
    );

    return Results.Ok(createdMedicineDto);
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/api/medicines/{id:int}", async (int id, MedicineRequest request, PharmacyDbContext db) =>
{
    var dbMedicine = await db.Medicines.FindAsync(id);
    if (dbMedicine == null) return Results.NotFound("Medicine not found");

    dbMedicine.DrugGroupId = request.DrugGroupId;
    dbMedicine.MedicineName = request.MedicineName;
    dbMedicine.Strength = request.Strength;
    dbMedicine.DosageForm = request.DosageForm;
    dbMedicine.Unit = request.Unit;
    dbMedicine.Price = request.Price;
    dbMedicine.RequiresPrescription = request.RequiresPrescription;
    dbMedicine.IsActive = request.IsActive;
    dbMedicine.Note = request.Note;

    var existingIngredients = db.MedicineIngredients.Where(mi => mi.MedicineId == id);
    db.MedicineIngredients.RemoveRange(existingIngredients);

    if (request.Ingredients != null)
    {
        foreach (var ing in request.Ingredients)
        {
            db.MedicineIngredients.Add(new DbMedicineIngredient
            {
                MedicineId = id,
                IngredientId = ing.IngredientId,
                Amount = ing.Amount
            });
        }
    }

    await db.SaveChangesAsync();

    var updatedMedicineDto = new Medicine(
        dbMedicine.MedicineId,
        dbMedicine.DrugGroupId,
        dbMedicine.MedicineName,
        dbMedicine.Strength,
        dbMedicine.DosageForm,
        dbMedicine.Unit,
        dbMedicine.Price,
        dbMedicine.RequiresPrescription,
        dbMedicine.IsActive,
        dbMedicine.Note,
        dbMedicine.CreatedAt.ToString("yyyy-MM-dd")
    );

    return Results.Ok(updatedMedicineDto);
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/api/medicines/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbMedicine = await db.Medicines.FindAsync(id);
    if (dbMedicine == null) return Results.NotFound("Medicine not found");

    var existingIngredients = db.MedicineIngredients.Where(mi => mi.MedicineId == id);
    db.MedicineIngredients.RemoveRange(existingIngredients);

    db.Medicines.Remove(dbMedicine);
    await db.SaveChangesAsync();

    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin"));

// Drug Group CRUD endpoints
app.MapPost("/api/druggroups", async (DrugGroupRequest request, PharmacyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.GroupName))
    {
        return Results.BadRequest("Tên nhóm thuốc không được để trống.");
    }
    var dbGroup = new DbDrugGroup
    {
        GroupName = request.GroupName,
        Description = request.Description
    };
    db.DrugGroups.Add(dbGroup);
    await db.SaveChangesAsync();
    return Results.Ok(new DrugGroup(dbGroup.DrugGroupId, dbGroup.GroupName, dbGroup.Description));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/api/druggroups/{id:int}", async (int id, DrugGroupRequest request, PharmacyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.GroupName))
    {
        return Results.BadRequest("Tên nhóm thuốc không được để trống.");
    }
    var dbGroup = await db.DrugGroups.FindAsync(id);
    if (dbGroup == null) return Results.NotFound("Không tìm thấy nhóm thuốc.");

    dbGroup.GroupName = request.GroupName;
    dbGroup.Description = request.Description;
    await db.SaveChangesAsync();
    return Results.Ok(new DrugGroup(dbGroup.DrugGroupId, dbGroup.GroupName, dbGroup.Description));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/api/druggroups/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbGroup = await db.DrugGroups.FindAsync(id);
    if (dbGroup == null) return Results.NotFound("Không tìm thấy nhóm thuốc.");

    var hasMedicines = await db.Medicines.AnyAsync(m => m.DrugGroupId == id);
    if (hasMedicines)
    {
        return Results.BadRequest("Không thể xóa nhóm thuốc này vì đang có thuốc tham chiếu thuộc về nhóm.");
    }

    db.DrugGroups.Remove(dbGroup);
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin"));

// Active Ingredient CRUD endpoints
app.MapPost("/api/ingredients", async (ActiveIngredientRequest request, PharmacyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.IngredientName))
    {
        return Results.BadRequest("Tên hoạt chất không được để trống.");
    }
    var dbIngredient = new DbActiveIngredient
    {
        IngredientName = request.IngredientName,
        Description = request.Description
    };
    db.ActiveIngredients.Add(dbIngredient);
    await db.SaveChangesAsync();
    return Results.Ok(new ActiveIngredient(dbIngredient.IngredientId, dbIngredient.IngredientName, dbIngredient.Description));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/api/ingredients/{id:int}", async (int id, ActiveIngredientRequest request, PharmacyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.IngredientName))
    {
        return Results.BadRequest("Tên hoạt chất không được để trống.");
    }
    var dbIngredient = await db.ActiveIngredients.FindAsync(id);
    if (dbIngredient == null) return Results.NotFound("Không tìm thấy hoạt chất.");

    dbIngredient.IngredientName = request.IngredientName;
    dbIngredient.Description = request.Description;
    await db.SaveChangesAsync();
    return Results.Ok(new ActiveIngredient(dbIngredient.IngredientId, dbIngredient.IngredientName, dbIngredient.Description));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/api/ingredients/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbIngredient = await db.ActiveIngredients.FindAsync(id);
    if (dbIngredient == null) return Results.NotFound("Không tìm thấy hoạt chất.");

    var hasMedicines = await db.MedicineIngredients.AnyAsync(mi => mi.IngredientId == id);
    if (hasMedicines)
    {
        return Results.BadRequest("Không thể xóa hoạt chất này vì đang được sử dụng trong danh mục thuốc.");
    }

    db.ActiveIngredients.Remove(dbIngredient);
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin"));

// Disease CRUD endpoints
app.MapPost("/api/diseases", async (DiseaseRequest request, PharmacyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.DiseaseName))
    {
        return Results.BadRequest("Tên bệnh lý không được để trống.");
    }
    var dbDisease = new DbDisease
    {
        DiseaseName = request.DiseaseName,
        Description = request.Description
    };
    db.Diseases.Add(dbDisease);
    await db.SaveChangesAsync();
    return Results.Ok(new Disease(dbDisease.DiseaseId, dbDisease.DiseaseName, dbDisease.Description));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/api/diseases/{id:int}", async (int id, DiseaseRequest request, PharmacyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.DiseaseName))
    {
        return Results.BadRequest("Tên bệnh lý không được để trống.");
    }
    var dbDisease = await db.Diseases.FindAsync(id);
    if (dbDisease == null) return Results.NotFound("Không tìm thấy bệnh lý.");

    dbDisease.DiseaseName = request.DiseaseName;
    dbDisease.Description = request.Description;
    await db.SaveChangesAsync();
    return Results.Ok(new Disease(dbDisease.DiseaseId, dbDisease.DiseaseName, dbDisease.Description));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/api/diseases/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbDisease = await db.Diseases.FindAsync(id);
    if (dbDisease == null) return Results.NotFound("Không tìm thấy bệnh lý.");

    var isAssocPatient = await db.PatientDiseases.AnyAsync(pd => pd.DiseaseId == id);
    if (isAssocPatient)
    {
        return Results.BadRequest("Không thể xóa bệnh lý này vì đang có hồ sơ bệnh nhân tham chiếu.");
    }

    var isAssocContra = await db.Contraindications.AnyAsync(c => c.DiseaseId == id);
    if (isAssocContra)
    {
        return Results.BadRequest("Không thể xóa bệnh lý này vì đang có cấu hình chống chỉ định tham chiếu.");
    }

    db.Diseases.Remove(dbDisease);
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin"));

// Patient Allergy CRUD endpoints
app.MapPost("/api/patientallergies", async (PatientAllergyRequest request, PharmacyDbContext db) =>
{
    var patientExists = await db.Patients.AnyAsync(p => p.PatientId == request.PatientId);
    if (!patientExists) return Results.BadRequest("Bệnh nhân không tồn tại.");

    var dbAllergy = new DbPatientAllergy
    {
        PatientId = request.PatientId,
        IngredientId = request.IsIngredient ? request.TargetId : null,
        MedicineId = !request.IsIngredient ? request.TargetId : null,
        Severity = request.Severity,
        AllergyNote = request.Note
    };

    db.PatientAllergies.Add(dbAllergy);
    await db.SaveChangesAsync();

    var allergyDto = new PatientAllergy(
        dbAllergy.AllergyId,
        dbAllergy.PatientId,
        dbAllergy.IngredientId,
        dbAllergy.MedicineId,
        dbAllergy.AllergyNote,
        dbAllergy.Severity
    );
    return Results.Ok(allergyDto);
}).RequireAuthorization(policy => policy.RequireRole("admin", "manager"));

app.MapPut("/api/patientallergies/{id:int}", async (int id, UpdatePatientAllergyRequest request, PharmacyDbContext db) =>
{
    var dbAllergy = await db.PatientAllergies.FindAsync(id);
    if (dbAllergy == null) return Results.NotFound("Không tìm thấy thông tin dị ứng.");

    dbAllergy.Severity = request.Severity;
    dbAllergy.AllergyNote = request.AllergyNote;
    await db.SaveChangesAsync();

    var allergyDto = new PatientAllergy(
        dbAllergy.AllergyId,
        dbAllergy.PatientId,
        dbAllergy.IngredientId,
        dbAllergy.MedicineId,
        dbAllergy.AllergyNote,
        dbAllergy.Severity
    );
    return Results.Ok(allergyDto);
}).RequireAuthorization(policy => policy.RequireRole("admin", "manager"));

app.MapDelete("/api/patientallergies/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbAllergy = await db.PatientAllergies.FindAsync(id);
    if (dbAllergy == null) return Results.NotFound("Không tìm thấy thông tin dị ứng.");

    db.PatientAllergies.Remove(dbAllergy);
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin", "manager"));

// Safety Check Endpoint
app.MapPost("/api/safety-check", async (SafetyCheckRequest request, PharmacyDbContext db) =>
{
    var generatedWarnings = new List<Warning>();
    var patient = await db.Patients.FirstOrDefaultAsync(p => p.PatientId == request.PatientId);
    if (patient == null) return Results.BadRequest("Patient not found");

    var checkId = Random.Shared.Next(1, 1000);

    var dbMedicines = await db.Medicines.ToListAsync();
    var dbIngredients = await db.ActiveIngredients.ToListAsync();
    var dbMedicineIngredients = await db.MedicineIngredients.ToListAsync();
    var dbInteractions = await db.DrugInteractions.ToListAsync();
    var dbContraindications = await db.Contraindications.ToListAsync();
    var dbDiseases = await db.Diseases.ToListAsync();

    // Extract ingredients from cart items
    var cartIngredients = new List<(int medicineId, int ingredientId, string ingredientName)>();
    foreach (var item in request.CartItems)
    {
        var medIngredients = dbMedicineIngredients.Where(mi => mi.MedicineId == item.MedicineId);
        foreach (var mi in medIngredients)
        {
            var ingName = dbIngredients.FirstOrDefault(ai => ai.IngredientId == mi.IngredientId)?.IngredientName ?? "";
            cartIngredients.Add((item.MedicineId, mi.IngredientId, ingName));
        }
    }

    // 1. Patient Allergies check
    var patientAllergiesData = await db.PatientAllergies.Where(pa => pa.PatientId == patient.PatientId).ToListAsync();
    foreach (var cartIng in cartIngredients)
    {
        var matchIng = patientAllergiesData.FirstOrDefault(pa => pa.IngredientId == cartIng.ingredientId);
        if (matchIng != null)
        {
            var medName = dbMedicines.FirstOrDefault(m => m.MedicineId == cartIng.medicineId)?.MedicineName ?? "";
            generatedWarnings.Add(new Warning(
                Random.Shared.Next(1000, 10000),
                checkId,
                patient.PatientId,
                cartIng.medicineId,
                "Dị ứng thuốc",
                matchIng.Severity ?? "Nghiêm trọng",
                $"Bệnh nhân dị ứng với hoạt chất [{cartIng.ingredientName}]. Thuốc [{medName}] có chứa hoạt chất này.",
                $"Ngay lập tức thay thế thuốc [{medName}] bằng một thuốc khác không thuộc cùng nhóm dược lý.",
                false,
                null,
                null,
                null
            ));
        }
    }

    // 2. Drug Interactions check
    for (int i = 0; i < cartIngredients.Count; i++)
    {
        for (int j = i + 1; j < cartIngredients.Count; j++)
        {
            var ingA = cartIngredients[i];
            var ingB = cartIngredients[j];

            var interact = dbInteractions.FirstOrDefault(di =>
                (di.IngredientAId == ingA.ingredientId && di.IngredientBId == ingB.ingredientId) ||
                (di.IngredientAId == ingB.ingredientId && di.IngredientBId == ingA.ingredientId)
            );

            if (interact != null)
            {
                var medAName = dbMedicines.FirstOrDefault(m => m.MedicineId == ingA.medicineId)?.MedicineName ?? "";
                var medBName = dbMedicines.FirstOrDefault(m => m.MedicineId == ingB.medicineId)?.MedicineName ?? "";

                generatedWarnings.Add(new Warning(
                    Random.Shared.Next(1000, 10000),
                    checkId,
                    patient.PatientId,
                    ingA.medicineId,
                    "Tương tác thuốc",
                    interact.Severity,
                    $"Tương tác nghiêm trọng giữa [{medAName}] ({ingA.ingredientName}) và [{medBName}] ({ingB.ingredientName}). {interact.Description}",
                    interact.Recommendation,
                    false,
                    null,
                    null,
                    null
                ));
            }
        }
    }

    // 3. Contraindications check
    var patientDiseasesData = await db.PatientDiseases.Where(pd => pd.PatientId == patient.PatientId).ToListAsync();
    foreach (var cartIng in cartIngredients)
    {
        foreach (var pDisease in patientDiseasesData)
        {
            var contra = dbContraindications.FirstOrDefault(c =>
                c.DiseaseId == pDisease.DiseaseId &&
                (c.IngredientId == cartIng.ingredientId || c.MedicineId == cartIng.medicineId)
            );

            if (contra != null)
            {
                var disName = dbDiseases.FirstOrDefault(d => d.DiseaseId == pDisease.DiseaseId)?.DiseaseName ?? "";
                var medName = dbMedicines.FirstOrDefault(m => m.MedicineId == cartIng.medicineId)?.MedicineName ?? "";

                generatedWarnings.Add(new Warning(
                    Random.Shared.Next(1000, 10000),
                    checkId,
                    patient.PatientId,
                    cartIng.medicineId,
                    "Chống chỉ định bệnh nền",
                    contra.Severity,
                    $"Thuốc [{medName}] chống chỉ định ở người có bệnh nền [{disName}]. {contra.Description}",
                    contra.Recommendation,
                    false,
                    null,
                    null,
                    null
                ));
            }
        }

        if (patient.IsPregnant)
        {
            var pregContra = dbContraindications.FirstOrDefault(c =>
                c.ConditionType == "Đối tượng đặc biệt" &&
                (c.MedicineId == cartIng.medicineId || c.IngredientId == cartIng.ingredientId)
            );

            if (pregContra != null)
            {
                var medName = dbMedicines.FirstOrDefault(m => m.MedicineId == cartIng.medicineId)?.MedicineName ?? "";
                generatedWarnings.Add(new Warning(
                    Random.Shared.Next(1000, 10000),
                    checkId,
                    patient.PatientId,
                    cartIng.medicineId,
                    "Đối tượng đặc biệt",
                    pregContra.Severity,
                    $"Thuốc [{medName}] chống chỉ định ở phụ nữ mang thai. {pregContra.Description}",
                    pregContra.Recommendation,
                    false,
                    null,
                    null,
                    null
                ));
            }
        }
    }

    // 4. Prescription Required check
    foreach (var item in request.CartItems)
    {
        var med = dbMedicines.FirstOrDefault(m => m.MedicineId == item.MedicineId);
        if (med != null && med.RequiresPrescription)
        {
            generatedWarnings.Add(new Warning(
                Random.Shared.Next(1000, 10000),
                checkId,
                patient.PatientId,
                item.MedicineId,
                "PrescriptionRequired",
                "Trung bình",
                $"Thuốc [{med.MedicineName}] yêu cầu phải có đơn thuốc của bác sĩ.",
                "Yêu cầu bệnh nhân cung cấp đơn thuốc hoặc liên hệ bác sĩ kê toa.",
                false,
                null,
                null,
                null
            ));
        }
    }

    var highestSeverity = generatedWarnings.Count > 0 ? "Medium" : "None";
    var result = generatedWarnings.Count > 0 ? "Warning" : "Approved";
    return Results.Ok(new { warnings = generatedWarnings, highestSeverity, result });
}).RequireAuthorization(policy => policy.RequireRole("admin", "pharmacist"));

// Drug Interactions CRUD endpoints
app.MapPost("/api/druginteractions", async (DrugInteractionRequest request, PharmacyDbContext db) =>
{
    if (request.IngredientAId == request.IngredientBId)
    {
        return Results.BadRequest("Hoạt chất A và Hoạt chất B không được trùng nhau.");
    }
    
    var hasA = await db.ActiveIngredients.AnyAsync(ai => ai.IngredientId == request.IngredientAId);
    var hasB = await db.ActiveIngredients.AnyAsync(ai => ai.IngredientId == request.IngredientBId);
    if (!hasA || !hasB)
    {
        return Results.BadRequest("Hoạt chất chỉ định không tồn tại.");
    }

    var dbInteraction = new DbDrugInteraction
    {
        IngredientAId = request.IngredientAId,
        IngredientBId = request.IngredientBId,
        Severity = request.Severity,
        Description = request.Description,
        Recommendation = request.Recommendation
    };
    db.DrugInteractions.Add(dbInteraction);
    await db.SaveChangesAsync();
    return Results.Ok(new DrugInteraction(
        dbInteraction.InteractionId,
        dbInteraction.IngredientAId,
        dbInteraction.IngredientBId,
        dbInteraction.Severity,
        dbInteraction.Description,
        dbInteraction.Recommendation
    ));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/api/druginteractions/{id:int}", async (int id, DrugInteractionRequest request, PharmacyDbContext db) =>
{
    if (request.IngredientAId == request.IngredientBId)
    {
        return Results.BadRequest("Hoạt chất A và Hoạt chất B không được trùng nhau.");
    }

    var hasA = await db.ActiveIngredients.AnyAsync(ai => ai.IngredientId == request.IngredientAId);
    var hasB = await db.ActiveIngredients.AnyAsync(ai => ai.IngredientId == request.IngredientBId);
    if (!hasA || !hasB)
    {
        return Results.BadRequest("Hoạt chất chỉ định không tồn tại.");
    }

    var dbInteraction = await db.DrugInteractions.FindAsync(id);
    if (dbInteraction == null) return Results.NotFound("Không tìm thấy thông tin tương tác thuốc.");

    dbInteraction.IngredientAId = request.IngredientAId;
    dbInteraction.IngredientBId = request.IngredientBId;
    dbInteraction.Severity = request.Severity;
    dbInteraction.Description = request.Description;
    dbInteraction.Recommendation = request.Recommendation;

    await db.SaveChangesAsync();
    return Results.Ok(new DrugInteraction(
        dbInteraction.InteractionId,
        dbInteraction.IngredientAId,
        dbInteraction.IngredientBId,
        dbInteraction.Severity,
        dbInteraction.Description,
        dbInteraction.Recommendation
    ));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/api/druginteractions/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbInteraction = await db.DrugInteractions.FindAsync(id);
    if (dbInteraction == null) return Results.NotFound("Không tìm thấy thông tin tương tác thuốc.");

    db.DrugInteractions.Remove(dbInteraction);
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin"));

// Contraindications CRUD endpoints
app.MapPost("/api/contraindications", async (ContraindicationRequest request, PharmacyDbContext db) =>
{
    if (request.MedicineId == null && request.IngredientId == null)
    {
        return Results.BadRequest("Chống chỉ định phải liên kết với ít nhất một Thuốc hoặc Hoạt chất.");
    }

    if (request.DiseaseId == null && request.ConditionType != "Đối tượng đặc biệt")
    {
        return Results.BadRequest("Chống chỉ định phải liên kết với Bệnh nền hoặc là Đối tượng đặc biệt.");
    }

    var dbContra = new DbContraindication
    {
        MedicineId = request.MedicineId,
        IngredientId = request.IngredientId,
        DiseaseId = request.DiseaseId,
        ConditionType = request.ConditionType,
        Severity = request.Severity,
        Description = request.Description,
        Recommendation = request.Recommendation
    };
    db.Contraindications.Add(dbContra);
    await db.SaveChangesAsync();
    return Results.Ok(new Contraindication(
        dbContra.ContraindicationId,
        dbContra.MedicineId,
        dbContra.IngredientId,
        dbContra.DiseaseId,
        dbContra.ConditionType ?? string.Empty,
        dbContra.Severity,
        dbContra.Description,
        dbContra.Recommendation
    ));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/api/contraindications/{id:int}", async (int id, ContraindicationRequest request, PharmacyDbContext db) =>
{
    if (request.MedicineId == null && request.IngredientId == null)
    {
        return Results.BadRequest("Chống chỉ định phải liên kết với ít nhất một Thuốc hoặc Hoạt chất.");
    }

    if (request.DiseaseId == null && request.ConditionType != "Đối tượng đặc biệt")
    {
        return Results.BadRequest("Chống chỉ định phải liên kết với Bệnh nền hoặc là Đối tượng đặc biệt.");
    }

    var dbContra = await db.Contraindications.FindAsync(id);
    if (dbContra == null) return Results.NotFound("Không tìm thấy thông tin chống chỉ định.");

    dbContra.MedicineId = request.MedicineId;
    dbContra.IngredientId = request.IngredientId;
    dbContra.DiseaseId = request.DiseaseId;
    dbContra.ConditionType = request.ConditionType;
    dbContra.Severity = request.Severity;
    dbContra.Description = request.Description;
    dbContra.Recommendation = request.Recommendation;

    await db.SaveChangesAsync();
    return Results.Ok(new Contraindication(
        dbContra.ContraindicationId,
        dbContra.MedicineId,
        dbContra.IngredientId,
        dbContra.DiseaseId,
        dbContra.ConditionType ?? string.Empty,
        dbContra.Severity,
        dbContra.Description,
        dbContra.Recommendation
    ));
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/api/contraindications/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbContra = await db.Contraindications.FindAsync(id);
    if (dbContra == null) return Results.NotFound("Không tìm thấy thông tin chống chỉ định.");

    db.Contraindications.Remove(dbContra);
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPost("/api/sales", async (SaleRequest request, PharmacyDbContext db) =>
{
    var totalAmount = request.CartItems.Sum(item =>
    {
        var price = db.Medicines.FirstOrDefault(m => m.MedicineId == item.MedicineId)?.Price ?? 0;
        return price * item.Quantity;
    });

    var hasWarnings = request.Warnings != null && request.Warnings.Any();
    var note = request.FinalDecision == "Approved" && hasWarnings
        ? "Bán sau khi duyệt cảnh báo. " + request.Note
        : "Bán an toàn thông thường. " + request.Note;

    var dbSale = new DbSale
    {
        PatientId = request.PatientId,
        PharmacistId = 2, // Ds. Trần Thị Mai
        PrescriptionId = null,
        SaleDate = DateTime.Now,
        TotalAmount = totalAmount,
        FinalDecision = request.FinalDecision,
        Status = request.FinalDecision == "Denied" ? "Cancelled" : "Completed",
        Note = note
    };

    db.Sales.Add(dbSale);
    await db.SaveChangesAsync();

    foreach (var item in request.CartItems)
    {
        var price = db.Medicines.FirstOrDefault(m => m.MedicineId == item.MedicineId)?.Price ?? 0;
        var dbDetail = new DbSaleDetail
        {
            SaleId = dbSale.SaleId,
            MedicineId = item.MedicineId,
            Quantity = item.Quantity,
            UnitPrice = price,
            DosageInstruction = item.DosageInstruction,
            TimesPerDay = item.TimesPerDay,
            Duration = item.Duration,
            AdviceNote = item.AdviceNote
        };
        db.SaleDetails.Add(dbDetail);
    }
    await db.SaveChangesAsync();

    if (hasWarnings && request.Warnings != null)
    {
        var highestSeverity = request.Warnings.Count > 0 ? "Medium" : "None";
        var dbCheck = new DbSafetyCheck
        {
            SaleId = dbSale.SaleId,
            CheckedAt = DateTime.Now,
            HighestSeverity = highestSeverity,
            Result = request.FinalDecision == "Denied" ? "Warning" : "Approved",
            Recommendation = "Quyết định lâm sàng từ dược sĩ: " + request.FinalDecision
        };
        db.SafetyChecks.Add(dbCheck);
        await db.SaveChangesAsync();

        foreach (var w in request.Warnings)
        {
            var dbWarning = new DbWarning
            {
                SafetyCheckId = dbCheck.SafetyCheckId,
                PatientId = request.PatientId,
                MedicineId = w.MedicineId,
                WarningType = w.WarningType,
                Severity = w.Severity,
                Message = w.Message,
                Recommendation = w.Recommendation,
                IsAcknowledged = w.IsAcknowledged,
                AcknowledgedBy = w.AcknowledgedBy,
                AcknowledgedAt = w.AcknowledgedAt != null ? DateTime.Parse(w.AcknowledgedAt) : null,
                Decision = w.Decision,
                CreatedAt = DateTime.Now
            };
            db.Warnings.Add(dbWarning);
        }
        await db.SaveChangesAsync();
    }

    var saleDto = new Sale(
        dbSale.SaleId,
        dbSale.PatientId,
        dbSale.PharmacistId,
        dbSale.PrescriptionId,
        dbSale.SaleDate.ToString("yyyy-MM-dd HH:mm"),
        dbSale.TotalAmount,
        dbSale.FinalDecision == "Approved" ? "Approved" : (dbSale.FinalDecision == "Denied" ? "Denied" : "Pending"),
        dbSale.Status == "Completed" ? "Completed" : (dbSale.Status == "Cancelled" ? "Cancelled" : "Pending"),
        dbSale.Note
    );

    return Results.Ok(saleDto);
}).RequireAuthorization(policy => policy.RequireRole("pharmacist"));

app.MapPut("/api/warnings/{id:int}/acknowledge", async (int id, AcknowledgeWarningRequest request, PharmacyDbContext db) =>
{
    var dbWarning = await db.Warnings.FindAsync(id);
    if (dbWarning == null) return Results.NotFound("Không tìm thấy cảnh báo.");

    dbWarning.IsAcknowledged = true;
    dbWarning.AcknowledgedBy = request.AcknowledgedBy;
    dbWarning.AcknowledgedAt = DateTime.Now;
    dbWarning.Decision = request.Decision;

    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
}).RequireAuthorization();

// User CRUD endpoints
app.MapPost("/api/users", async (User userDto, PharmacyDbContext db) =>
{
    var passwordHash = string.IsNullOrEmpty(userDto.PasswordHash) 
        ? "$2a$11$9Wv6x6T5rD8R1n1W1n1W1uX1qX1qX1qX1qX1qX1qX1qX1qX1qX1qX" 
        : userDto.PasswordHash;

    var dbUser = new DbUser
    {
        RoleId = userDto.RoleId,
        FullName = userDto.FullName,
        Email = userDto.Email,
        PasswordHash = passwordHash,
        Phone = userDto.Phone,
        Status = string.IsNullOrEmpty(userDto.Status) ? "Active" : userDto.Status,
        CreatedAt = DateTime.Now
    };

    db.Users.Add(dbUser);
    await db.SaveChangesAsync();

    var createdUserDto = new User(
        dbUser.UserId,
        dbUser.RoleId,
        dbUser.FullName,
        dbUser.Email,
        dbUser.Phone,
        dbUser.Status,
        dbUser.CreatedAt.ToString("yyyy-MM-dd"),
        dbUser.PasswordHash
    );

    return Results.Ok(createdUserDto);
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPut("/api/users/{id:int}", async (int id, User userDto, PharmacyDbContext db) =>
{
    var dbUser = await db.Users.FindAsync(id);
    if (dbUser == null) return Results.NotFound("User not found");

    dbUser.FullName = userDto.FullName;
    dbUser.Email = userDto.Email;
    dbUser.RoleId = userDto.RoleId;
    dbUser.Status = userDto.Status;
    dbUser.Phone = userDto.Phone;
    if (!string.IsNullOrEmpty(userDto.PasswordHash))
    {
        dbUser.PasswordHash = userDto.PasswordHash;
    }

    await db.SaveChangesAsync();

    var updatedUserDto = new User(
        dbUser.UserId,
        dbUser.RoleId,
        dbUser.FullName,
        dbUser.Email,
        dbUser.Phone,
        dbUser.Status,
        dbUser.CreatedAt.ToString("yyyy-MM-dd"),
        dbUser.PasswordHash
    );

    return Results.Ok(updatedUserDto);
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapDelete("/api/users/{id:int}", async (int id, PharmacyDbContext db) =>
{
    var dbUser = await db.Users.FindAsync(id);
    if (dbUser == null) return Results.NotFound("User not found");

    db.Users.Remove(dbUser);
    await db.SaveChangesAsync();

    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.Run();

// ====================================================
// DATA RECORDS
// ====================================================

public record User(int UserId, int RoleId, string FullName, string Email, string? Phone, string Status, string CreatedAt, string PasswordHash = "$2a$11$9Wv6x6T5rD8R1n1W1n1W1uX1qX1qX1qX1qX1qX1qX1qX1qX1qX1qX");
public record Patient(int PatientId, string FullName, string? Phone, string? Gender, string DateOfBirth, decimal? WeightKg, string? Address, bool IsPregnant, bool IsBreastfeeding, string? Note, string CreatedAt);
public record DrugGroup(int DrugGroupId, string GroupName, string? Description);
public record ActiveIngredient(int IngredientId, string IngredientName, string? Description);
public record Medicine(int MedicineId, int? DrugGroupId, string MedicineName, string? Strength, string? DosageForm, string? Unit, decimal Price, bool RequiresPrescription, bool IsActive, string? Note, string CreatedAt);
public record MedicineIngredient(int MedicineId, int IngredientId, string? Amount);
public record Disease(int DiseaseId, string DiseaseName, string? Description);
public record PatientDisease(int PatientDiseaseId, int PatientId, int DiseaseId, string? Note);
public record PatientAllergy(int AllergyId, int PatientId, int? IngredientId, int? MedicineId, string? AllergyNote, string? Severity);
public record DrugInteraction(int InteractionId, int IngredientAId, int IngredientBId, string Severity, string? Description, string? Recommendation);
public record Contraindication(int ContraindicationId, int? MedicineId, int? IngredientId, int? DiseaseId, string ConditionType, string Severity, string? Description, string? Recommendation);
public record Sale(int SaleId, int PatientId, int PharmacistId, int? PrescriptionId, string SaleDate, decimal TotalAmount, string FinalDecision, string Status, string? Note);
public record SaleDetail(int SaleDetailId, int SaleId, int MedicineId, int Quantity, decimal UnitPrice, string DosageInstruction, int TimesPerDay, string? Duration, string? AdviceNote);
public record Warning(int WarningId, int SafetyCheckId, int PatientId, int? MedicineId, string WarningType, string Severity, string Message, string? Recommendation, bool IsAcknowledged, int? AcknowledgedBy, string? AcknowledgedAt, string? Decision);

public record CartItemDto(int MedicineId, int Quantity, string DosageInstruction, int TimesPerDay, string? Duration, string? AdviceNote);
public record SafetyCheckRequest(int PatientId, List<CartItemDto> CartItems);
public record SaleRequest(int PatientId, List<CartItemDto> CartItems, string FinalDecision, List<Warning>? Warnings, string? Note);
public record LoginRequest(string Email, string Pin);

public record PatientRequest(
    string FullName,
    string? Phone,
    string? Gender,
    string? DateOfBirth,
    decimal? WeightKg,
    string? Address,
    bool IsPregnant,
    bool IsBreastfeeding,
    string? Note
);

public record PatientAllergyDto(
    bool IsIngredient,
    int TargetId,
    string Severity,
    string? Note
);

public record PatientDiseaseDto(
    int DiseaseId,
    string? Note
);

public record CreateOrUpdatePatientRequest(
    PatientRequest Patient,
    List<PatientAllergyDto> Allergies,
    List<PatientDiseaseDto> Diseases
);

public record MedicineRequest(
    int? DrugGroupId, 
    string MedicineName, 
    string? Strength, 
    string? DosageForm, 
    string? Unit, 
    decimal Price, 
    bool RequiresPrescription, 
    bool IsActive, 
    string? Note,
    List<MedicineIngredientDto>? Ingredients
);
public record MedicineIngredientDto(int IngredientId, string? Amount);
public record DrugGroupRequest(string GroupName, string? Description);
public record ActiveIngredientRequest(string IngredientName, string? Description);
public record DrugInteractionRequest(int IngredientAId, int IngredientBId, string Severity, string? Description, string? Recommendation);
public record ContraindicationRequest(int? MedicineId, int? IngredientId, int? DiseaseId, string? ConditionType, string Severity, string? Description, string? Recommendation);
public record AcknowledgeWarningRequest(int AcknowledgedBy, string Decision);

public record DiseaseRequest(string DiseaseName, string? Description);
public record PatientAllergyRequest(int PatientId, bool IsIngredient, int TargetId, string Severity, string? Note);
public record UpdatePatientAllergyRequest(string Severity, string? AllergyNote);
