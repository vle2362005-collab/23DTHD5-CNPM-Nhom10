CREATE DATABASE PharmacySafetyDB;
GO

USE PharmacySafetyDB;
GO

CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    RoleId INT NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    Status NVARCHAR(50) DEFAULT N'Active',
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Users_Roles
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

CREATE TABLE Patients (
    PatientId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Gender NVARCHAR(20),
    DateOfBirth DATE,
    WeightKg DECIMAL(5,2),
    Address NVARCHAR(255),
    IsPregnant BIT DEFAULT 0,
    IsBreastfeeding BIT DEFAULT 0,
    Note NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE DrugGroups (
    DrugGroupId INT IDENTITY(1,1) PRIMARY KEY,
    GroupName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500)
);

CREATE TABLE ActiveIngredients (
    IngredientId INT IDENTITY(1,1) PRIMARY KEY,
    IngredientName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500)
);

CREATE TABLE Medicines (
    MedicineId INT IDENTITY(1,1) PRIMARY KEY,
    DrugGroupId INT NULL,
    MedicineName NVARCHAR(150) NOT NULL,
    Strength NVARCHAR(100),
    DosageForm NVARCHAR(100),
    Unit NVARCHAR(50),
    Price DECIMAL(18,2) DEFAULT 0,
    RequiresPrescription BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    Note NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Medicines_DrugGroups
    FOREIGN KEY (DrugGroupId) REFERENCES DrugGroups(DrugGroupId)
);

CREATE TABLE MedicineIngredients (
    MedicineId INT NOT NULL,
    IngredientId INT NOT NULL,
    Amount NVARCHAR(100),

    PRIMARY KEY (MedicineId, IngredientId),

    CONSTRAINT FK_MedicineIngredients_Medicines
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),

    CONSTRAINT FK_MedicineIngredients_ActiveIngredients
    FOREIGN KEY (IngredientId) REFERENCES ActiveIngredients(IngredientId)
);

CREATE TABLE Diseases (
    DiseaseId INT IDENTITY(1,1) PRIMARY KEY,
    DiseaseName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500)
);

CREATE TABLE PatientDiseases (
    PatientDiseaseId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    DiseaseId INT NOT NULL,
    Note NVARCHAR(500),

    CONSTRAINT FK_PatientDiseases_Patients
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),

    CONSTRAINT FK_PatientDiseases_Diseases
    FOREIGN KEY (DiseaseId) REFERENCES Diseases(DiseaseId)
);

CREATE TABLE PatientAllergies (
    AllergyId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    IngredientId INT NULL,
    MedicineId INT NULL,
    AllergyNote NVARCHAR(500),
    Severity NVARCHAR(50),

    CONSTRAINT FK_PatientAllergies_Patients
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),

    CONSTRAINT FK_PatientAllergies_Ingredients
    FOREIGN KEY (IngredientId) REFERENCES ActiveIngredients(IngredientId),

    CONSTRAINT FK_PatientAllergies_Medicines
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId)
);

CREATE TABLE PatientCurrentMedicines (
    CurrentMedicineId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    MedicineId INT NULL,
    MedicineNameText NVARCHAR(150),
    Note NVARCHAR(500),

    CONSTRAINT FK_CurrentMedicines_Patients
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),

    CONSTRAINT FK_CurrentMedicines_Medicines
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId)
);

CREATE TABLE Prescriptions (
    PrescriptionId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    PrescriptionCode NVARCHAR(100),
    DoctorName NVARCHAR(100),
    PrescribedDate DATE,
    ImageUrl NVARCHAR(500),
    IsValid BIT DEFAULT 1,
    Note NVARCHAR(500),

    CONSTRAINT FK_Prescriptions_Patients
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId)
);

CREATE TABLE Sales (
    SaleId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    PharmacistId INT NOT NULL,
    PrescriptionId INT NULL,
    SaleDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) DEFAULT 0,
    FinalDecision NVARCHAR(50) DEFAULT N'Pending',
    Status NVARCHAR(50) DEFAULT N'Pending',
    Note NVARCHAR(500),

    CONSTRAINT FK_Sales_Patients
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),

    CONSTRAINT FK_Sales_Users
    FOREIGN KEY (PharmacistId) REFERENCES Users(UserId),

    CONSTRAINT FK_Sales_Prescriptions
    FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(PrescriptionId)
);

CREATE TABLE SaleDetails (
    SaleDetailId INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    MedicineId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DosageInstruction NVARCHAR(500),
    TimesPerDay INT,
    Duration NVARCHAR(100),
    AdviceNote NVARCHAR(500),

    CONSTRAINT FK_SaleDetails_Sales
    FOREIGN KEY (SaleId) REFERENCES Sales(SaleId),

    CONSTRAINT FK_SaleDetails_Medicines
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId)
);

CREATE TABLE DrugInteractions (
    InteractionId INT IDENTITY(1,1) PRIMARY KEY,
    IngredientAId INT NOT NULL,
    IngredientBId INT NOT NULL,
    Severity NVARCHAR(50) NOT NULL,
    Description NVARCHAR(1000),
    Recommendation NVARCHAR(1000),

    CONSTRAINT FK_DrugInteractions_IngredientA
    FOREIGN KEY (IngredientAId) REFERENCES ActiveIngredients(IngredientId),

    CONSTRAINT FK_DrugInteractions_IngredientB
    FOREIGN KEY (IngredientBId) REFERENCES ActiveIngredients(IngredientId)
);

CREATE TABLE Contraindications (
    ContraindicationId INT IDENTITY(1,1) PRIMARY KEY,
    MedicineId INT NULL,
    IngredientId INT NULL,
    DiseaseId INT NULL,
    ConditionType NVARCHAR(100),
    Severity NVARCHAR(50) NOT NULL,
    Description NVARCHAR(1000),
    Recommendation NVARCHAR(1000),

    CONSTRAINT FK_Contraindications_Medicines
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),

    CONSTRAINT FK_Contraindications_Ingredients
    FOREIGN KEY (IngredientId) REFERENCES ActiveIngredients(IngredientId),

    CONSTRAINT FK_Contraindications_Diseases
    FOREIGN KEY (DiseaseId) REFERENCES Diseases(DiseaseId)
);

CREATE TABLE SafetyChecks (
    SafetyCheckId INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    CheckedAt DATETIME DEFAULT GETDATE(),
    HighestSeverity NVARCHAR(50),
    Result NVARCHAR(50) NOT NULL,
    Recommendation NVARCHAR(500),

    CONSTRAINT FK_SafetyChecks_Sales
    FOREIGN KEY (SaleId) REFERENCES Sales(SaleId)
);

CREATE TABLE Warnings (
    WarningId INT IDENTITY(1,1) PRIMARY KEY,
    SafetyCheckId INT NOT NULL,
    PatientId INT NOT NULL,
    MedicineId INT NULL,
    WarningType NVARCHAR(100) NOT NULL,
    Severity NVARCHAR(50) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Recommendation NVARCHAR(1000),
    IsAcknowledged BIT DEFAULT 0,
    AcknowledgedBy INT NULL,
    AcknowledgedAt DATETIME NULL,
    Decision NVARCHAR(50),
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Warnings_SafetyChecks
    FOREIGN KEY (SafetyCheckId) REFERENCES SafetyChecks(SafetyCheckId),

    CONSTRAINT FK_Warnings_Patients
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),

    CONSTRAINT FK_Warnings_Medicines
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId),

    CONSTRAINT FK_Warnings_Users
    FOREIGN KEY (AcknowledgedBy) REFERENCES Users(UserId)
);
GO

-- ====================================================
-- SEED DATA FOR ROLES & USERS
-- ====================================================

-- Enable identity insert to explicitly seed IDs
SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (RoleId, RoleName) VALUES 
(1, N'Admin'),
(2, N'Pharmacist'),
(3, N'Manager');
SET IDENTITY_INSERT Roles OFF;
GO

SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserId, RoleId, FullName, Email, PasswordHash, Phone, Status, CreatedAt) VALUES 
(1, 1, N'Nguyễn Minh Quân', N'admin@gmail.com', N'$2a$11$9Wv6x6T5rD8R1n1W1n1W1uX1qX1qX1qX1qX1qX1qX1qX1qX1qX1qX', N'0900000000', N'Active', GETDATE()),
(2, 2, N'Ds. Trần Thị Mai', N'duocsi@gmail.com', N'$2a$11$9Wv6x6T5rD8R1n1W1n1W1uX1qX1qX1qX1qX1qX1qX1qX1qX1qX1qX', N'0911111111', N'Active', GETDATE()),
(3, 3, N'Ds. Phạm Thanh Sơn', N'quanly@gmail.com', N'$2a$11$9Wv6x6T5rD8R1n1W1n1W1uX1qX1qX1qX1qX1qX1qX1qX1qX1qX1qX', N'0922222222', N'Active', GETDATE());
SET IDENTITY_INSERT Users OFF;
GO