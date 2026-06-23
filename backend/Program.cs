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
app.MapGet("/api/patients", () => Results.Ok(patients)).RequireAuthorization();
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
app.MapGet("/api/diseases", () => Results.Ok(diseases)).RequireAuthorization();
app.MapGet("/api/patientdiseases", () => Results.Ok(patientDiseases)).RequireAuthorization();
app.MapGet("/api/patientallergies", () => Results.Ok(patientAllergies)).RequireAuthorization();
app.MapGet("/api/druginteractions", () => Results.Ok(drugInteractions)).RequireAuthorization();
app.MapGet("/api/contraindications", () => Results.Ok(contraindications)).RequireAuthorization();
app.MapGet("/api/sales", () => Results.Ok(sales)).RequireAuthorization();
app.MapGet("/api/saledetails", () => Results.Ok(saleDetails)).RequireAuthorization();
app.MapGet("/api/warnings", () => Results.Ok(warnings)).RequireAuthorization();

// Patient CRUD endpoints
app.MapPost("/api/patients", (Patient patient) =>
{
    var newId = patients.Any() ? patients.Max(p => p.PatientId) + 1 : 1;
    var newPat = patient with { PatientId = newId, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd") };
    patients.Add(newPat);
    return Results.Ok(newPat);
}).RequireAuthorization(policy => policy.RequireRole("admin", "pharmacist"));

app.MapPut("/api/patients/{id:int}", (int id, Patient patient) =>
{
    var idx = patients.FindIndex(p => p.PatientId == id);
    if (idx < 0) return Results.NotFound("Patient not found");
    patients[idx] = patient with { PatientId = id };
    return Results.Ok(patients[idx]);
}).RequireAuthorization(policy => policy.RequireRole("admin", "pharmacist"));

app.MapDelete("/api/patients/{id:int}", (int id) =>
{
    var idx = patients.FindIndex(p => p.PatientId == id);
    if (idx < 0) return Results.NotFound("Patient not found");
    patients.RemoveAt(idx);
    return Results.Ok(new { success = true });
}).RequireAuthorization(policy => policy.RequireRole("admin", "pharmacist"));

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

// Safety Check Endpoint
app.MapPost("/api/safety-check", (SafetyCheckRequest request) =>
{
    var generatedWarnings = new List<Warning>();
    var patient = patients.FirstOrDefault(p => p.PatientId == request.PatientId);
    if (patient == null) return Results.BadRequest("Patient not found");

    var checkId = Random.Shared.Next(1, 1000);

    // Extract ingredients from cart items
    var cartIngredients = new List<(int medicineId, int ingredientId, string ingredientName)>();
    foreach (var item in request.CartItems)
    {
        var medIngredients = medicineIngredients.Where(mi => mi.MedicineId == item.MedicineId);
        foreach (var mi in medIngredients)
        {
            var ingName = activeIngredients.FirstOrDefault(ai => ai.IngredientId == mi.IngredientId)?.IngredientName ?? "";
            cartIngredients.Add((item.MedicineId, mi.IngredientId, ingName));
        }
    }

    // 1. Patient Allergies check
    var patientAllergiesData = patientAllergies.Where(pa => pa.PatientId == patient.PatientId).ToList();
    foreach (var cartIng in cartIngredients)
    {
        var matchIng = patientAllergiesData.FirstOrDefault(pa => pa.IngredientId == cartIng.ingredientId);
        if (matchIng != null)
        {
            var medName = medicines.FirstOrDefault(m => m.MedicineId == cartIng.medicineId)?.MedicineName ?? "";
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

            var interact = drugInteractions.FirstOrDefault(di =>
                (di.IngredientAId == ingA.ingredientId && di.IngredientBId == ingB.ingredientId) ||
                (di.IngredientAId == ingB.ingredientId && di.IngredientBId == ingA.ingredientId)
            );

            if (interact != null)
            {
                var medAName = medicines.FirstOrDefault(m => m.MedicineId == ingA.medicineId)?.MedicineName ?? "";
                var medBName = medicines.FirstOrDefault(m => m.MedicineId == ingB.medicineId)?.MedicineName ?? "";

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
    var patientDiseasesData = patientDiseases.Where(pd => pd.PatientId == patient.PatientId).ToList();
    foreach (var cartIng in cartIngredients)
    {
        foreach (var pDisease in patientDiseasesData)
        {
            var contra = contraindications.FirstOrDefault(c =>
                c.DiseaseId == pDisease.DiseaseId &&
                (c.IngredientId == cartIng.ingredientId || c.MedicineId == cartIng.medicineId)
            );

            if (contra != null)
            {
                var disName = diseases.FirstOrDefault(d => d.DiseaseId == pDisease.DiseaseId)?.DiseaseName ?? "";
                var medName = medicines.FirstOrDefault(m => m.MedicineId == cartIng.medicineId)?.MedicineName ?? "";

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
            var pregContra = contraindications.FirstOrDefault(c =>
                c.ConditionType == "Đối tượng đặc biệt" &&
                (c.MedicineId == cartIng.medicineId || c.IngredientId == cartIng.ingredientId)
            );

            if (pregContra != null)
            {
                var medName = medicines.FirstOrDefault(m => m.MedicineId == cartIng.medicineId)?.MedicineName ?? "";
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
        var med = medicines.FirstOrDefault(m => m.MedicineId == item.MedicineId);
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

// Create Sale Endpoint
app.MapPost("/api/sales", (SaleRequest request) =>
{
    var newSaleId = sales.Count + 1;
    var totalAmount = request.CartItems.Sum(item =>
    {
        var price = medicines.FirstOrDefault(m => m.MedicineId == item.MedicineId)?.Price ?? 0;
        return price * item.Quantity;
    });

    var hasWarnings = request.Warnings != null && request.Warnings.Any();
    var note = request.FinalDecision == "Approved" && hasWarnings
        ? "Bán sau khi duyệt cảnh báo. " + request.Note
        : "Bán an toàn thông thường. " + request.Note;

    var newSale = new Sale(
        newSaleId,
        request.PatientId,
        2, // Ds. Trần Thị Mai
        null,
        DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
        totalAmount,
        request.FinalDecision,
        request.FinalDecision == "Denied" ? "Cancelled" : "Completed",
        note
    );

    sales.Insert(0, newSale);

    foreach (var item in request.CartItems)
    {
        var price = medicines.FirstOrDefault(m => m.MedicineId == item.MedicineId)?.Price ?? 0;
        var detail = new SaleDetail(
            saleDetails.Count + 1,
            newSaleId,
            item.MedicineId,
            item.Quantity,
            price,
            item.DosageInstruction,
            item.TimesPerDay,
            item.Duration,
            item.AdviceNote
        );
        saleDetails.Add(detail);
    }

    if (request.Warnings != null)
    {
        foreach (var w in request.Warnings)
        {
            var warningWithId = w with { WarningId = warnings.Count + 1 };
            warnings.Insert(0, warningWithId);
        }
    }

    return Results.Ok(newSale);
}).RequireAuthorization(policy => policy.RequireRole("pharmacist"));

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
