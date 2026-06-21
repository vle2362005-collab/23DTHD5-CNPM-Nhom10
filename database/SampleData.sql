USE PharmacySafetyDB;
GO

INSERT INTO Roles (RoleName)
VALUES 
(N'Admin'),
(N'DuocSi'),
(N'QuanLy');

INSERT INTO Users (RoleId, FullName, Email, PasswordHash, Phone)
VALUES
(1, N'Admin He Thong', N'admin@gmail.com', N'123456', N'0900000000'),
(2, N'Duoc Si A', N'duocsi@gmail.com', N'123456', N'0911111111');

INSERT INTO DrugGroups (GroupName, Description)
VALUES
(N'Thuoc giam dau ha sot', N'Nhom thuoc dung de giam dau va ha sot'),
(N'Khang sinh', N'Nhom thuoc dieu tri nhiem khuan'),
(N'Khang viem NSAID', N'Nhom thuoc giam dau khang viem');

INSERT INTO ActiveIngredients (IngredientName, Description)
VALUES
(N'Paracetamol', N'Hoat chat giam dau ha sot'),
(N'Amoxicillin', N'Hoat chat khang sinh nhom Penicillin'),
(N'Ibuprofen', N'Hoat chat giam dau khang viem NSAID');

INSERT INTO Medicines
(DrugGroupId, MedicineName, Strength, DosageForm, Unit, Price, RequiresPrescription, IsActive, Note)
VALUES
(1, N'Paracetamol 500mg', N'500mg', N'Vien nen', N'Vien', 2000, 0, 1, N'Thuoc ha sot giam dau'),
(2, N'Amoxicillin 500mg', N'500mg', N'Vien nang', N'Vien', 3000, 1, 1, N'Khang sinh can don'),
(3, N'Ibuprofen 400mg', N'400mg', N'Vien nen', N'Vien', 2500, 0, 1, N'Giam dau khang viem');

INSERT INTO MedicineIngredients (MedicineId, IngredientId, Amount)
VALUES
(1, 1, N'500mg'),
(2, 2, N'500mg'),
(3, 3, N'400mg');

INSERT INTO Patients
(FullName, Phone, Gender, DateOfBirth, WeightKg, Address, IsPregnant, IsBreastfeeding, Note)
VALUES
(N'Nguyen Van A', N'0988888888', N'Nam', '1990-05-12', 65, N'Gia Lai', 0, 0, N'Co benh nen cao huyet ap'),
(N'Tran Thi B', N'0977777777', N'Nu', '1985-10-20', 52, N'Gia Lai', 0, 0, N'Di ung thuoc giam dau');

INSERT INTO Diseases (DiseaseName, Description)
VALUES
(N'Cao huyet ap', N'Benh tang huyet ap'),
(N'Suy than', N'Benh nhan suy giam chuc nang than'),
(N'Viem loet da day', N'Benh ly da day');

INSERT INTO PatientDiseases (PatientId, DiseaseId, Note)
VALUES
(1, 1, N'Benh nhan co tien su cao huyet ap');

INSERT INTO PatientAllergies (PatientId, IngredientId, MedicineId, AllergyNote, Severity)
VALUES
(2, 3, 3, N'Di ung voi Ibuprofen', N'High');

INSERT INTO PatientCurrentMedicines (PatientId, MedicineId, MedicineNameText, Note)
VALUES
(1, NULL, N'Thuoc huyet ap dang dung', N'Can hoi ky truoc khi ban thuoc cam hoac thuoc giam dau');

INSERT INTO Prescriptions
(PatientId, PrescriptionCode, DoctorName, PrescribedDate, IsValid, Note)
VALUES
(1, N'DT001', N'Bac si Nguyen Van C', '2026-06-20', 1, N'Don thuoc demo');

INSERT INTO DrugInteractions
(IngredientAId, IngredientBId, Severity, Description, Recommendation)
VALUES
(2, 3, N'Medium', N'Amoxicillin va Ibuprofen can than trong khi su dung chung', N'Can tu van va theo doi trieu chung bat thuong');

INSERT INTO Contraindications
(MedicineId, IngredientId, DiseaseId, ConditionType, Severity, Description, Recommendation)
VALUES
(3, 3, 3, N'Disease', N'High', N'Ibuprofen khong phu hop voi benh nhan viem loet da day', N'Can doi sang thuoc khac an toan hon');

INSERT INTO Sales
(PatientId, PharmacistId, PrescriptionId, TotalAmount, FinalDecision, Status, Note)
VALUES
(1, 2, 1, 7000, N'AllowSale', N'Completed', N'Phieu ban thuoc demo');

INSERT INTO SaleDetails
(SaleId, MedicineId, Quantity, UnitPrice, DosageInstruction, TimesPerDay, Duration, AdviceNote)
VALUES
(1, 1, 2, 2000, N'Uong 1 vien khi sot', 3, N'3 ngay', N'Khong dung qua lieu'),
(1, 2, 1, 3000, N'Uong theo huong dan cua bac si', 2, N'5 ngay', N'Uong du lieu');

INSERT INTO SafetyChecks
(SaleId, HighestSeverity, Result, Recommendation)
VALUES
(1, N'Medium', N'Warning', N'Can xac nhan thuoc can don truoc khi ban');

INSERT INTO Warnings
(SafetyCheckId, PatientId, MedicineId, WarningType, Severity, Message, Recommendation, IsAcknowledged, AcknowledgedBy, AcknowledgedAt, Decision)
VALUES
(1, 1, 2, N'PrescriptionRequired', N'Medium', N'Thuoc Amoxicillin can co don bac si', N'Yeu cau benh nhan cung cap don thuoc', 1, 2, GETDATE(), N'AllowSale');
GO

SELECT * FROM Roles;
SELECT * FROM Users;
SELECT * FROM Patients;
SELECT * FROM Medicines;
SELECT * FROM Diseases;
SELECT * FROM PatientDiseases;
SELECT * FROM PatientAllergies;
SELECT * FROM PatientCurrentMedicines;
SELECT * FROM Prescriptions;
SELECT * FROM DrugInteractions;
SELECT * FROM Contraindications;
SELECT * FROM Sales;
SELECT * FROM SaleDetails;
SELECT * FROM SafetyChecks;
SELECT * FROM Warnings;