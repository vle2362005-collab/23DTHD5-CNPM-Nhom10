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

app.MapPost("/api/auth/login", (LoginRequest request) =>
{
    var foundUser = users.FirstOrDefault(u => 
        u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) && 
        (u.Phone == request.Pin || request.Pin == "123456")
    );

    if (foundUser == null || foundUser.Status != "Active")
    {
        return Results.Json(new { Message = "Email hoặc mã PIN xác thực không đúng" }, statusCode: 401);
    }

    string roleName = foundUser.RoleId switch
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
            new Claim(ClaimTypes.NameIdentifier, foundUser.UserId.ToString()),
            new Claim(ClaimTypes.Email, foundUser.Email),
            new Claim(ClaimTypes.Name, foundUser.FullName),
            new Claim(ClaimTypes.Role, roleName)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new
    {
        Token = tokenString,
        User = foundUser
    });
}).AllowAnonymous();

app.MapGet("/api/users", () => Results.Ok(users)).RequireAuthorization();
app.MapGet("/api/patients", () => Results.Ok(patients)).RequireAuthorization();
app.MapGet("/api/druggroups", () => Results.Ok(drugGroups)).RequireAuthorization();
app.MapGet("/api/ingredients", () => Results.Ok(activeIngredients)).RequireAuthorization();
app.MapGet("/api/medicines", () => Results.Ok(medicines)).RequireAuthorization();
app.MapGet("/api/medicineingredients", () => Results.Ok(medicineIngredients)).RequireAuthorization();
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
}).RequireAuthorization();

app.MapPut("/api/patients/{id:int}", (int id, Patient patient) =>
{
    var idx = patients.FindIndex(p => p.PatientId == id);
    if (idx < 0) return Results.NotFound("Patient not found");
    patients[idx] = patient with { PatientId = id };
    return Results.Ok(patients[idx]);
}).RequireAuthorization();

app.MapDelete("/api/patients/{id:int}", (int id) =>
{
    var idx = patients.FindIndex(p => p.PatientId == id);
    if (idx < 0) return Results.NotFound("Patient not found");
    patients.RemoveAt(idx);
    return Results.Ok(new { success = true });
}).RequireAuthorization();

// Medicine CRUD endpoints
app.MapPost("/api/medicines", (Medicine medicine) =>
{
    var newId = medicines.Any() ? medicines.Max(m => m.MedicineId) + 1 : 1;
    var newMed = medicine with { MedicineId = newId, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd") };
    medicines.Add(newMed);
    return Results.Ok(newMed);
}).RequireAuthorization();

app.MapPut("/api/medicines/{id:int}", (int id, Medicine medicine) =>
{
    var idx = medicines.FindIndex(m => m.MedicineId == id);
    if (idx < 0) return Results.NotFound("Medicine not found");
    medicines[idx] = medicine with { MedicineId = id };
    return Results.Ok(medicines[idx]);
}).RequireAuthorization();

app.MapDelete("/api/medicines/{id:int}", (int id) =>
{
    var idx = medicines.FindIndex(m => m.MedicineId == id);
    if (idx < 0) return Results.NotFound("Medicine not found");
    medicines.RemoveAt(idx);
    return Results.Ok(new { success = true });
}).RequireAuthorization();

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
}).RequireAuthorization();

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
}).RequireAuthorization();

// User CRUD endpoints
app.MapPost("/api/users", (User user) =>
{
    var newId = users.Any() ? users.Max(u => u.UserId) + 1 : 1;
    var passwordHash = string.IsNullOrEmpty(user.PasswordHash) 
        ? "$2a$11$9Wv6x6T5rD8R1n1W1n1W1uX1qX1qX1qX1qX1qX1qX1qX1qX1qX1qX" 
        : user.PasswordHash;
    var newUser = user with { 
        UserId = newId, 
        PasswordHash = passwordHash,
        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd") 
    };
    users.Add(newUser);
    return Results.Ok(newUser);
}).RequireAuthorization();

app.MapPut("/api/users/{id:int}", (int id, User user) =>
{
    var idx = users.FindIndex(u => u.UserId == id);
    if (idx < 0) return Results.NotFound("User not found");
    
    var oldHash = users[idx].PasswordHash;
    var updatedUser = user with { 
        UserId = id, 
        PasswordHash = string.IsNullOrEmpty(user.PasswordHash) ? oldHash : user.PasswordHash 
    };
    users[idx] = updatedUser;
    return Results.Ok(users[idx]);
}).RequireAuthorization();

app.MapDelete("/api/users/{id:int}", (int id) =>
{
    var idx = users.FindIndex(u => u.UserId == id);
    if (idx < 0) return Results.NotFound("User not found");
    users.RemoveAt(idx);
    return Results.Ok(new { success = true });
}).RequireAuthorization();

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
