# CNPM-15 - Thiết kế ERD cho hệ thống

## 1. Mục tiêu

Thiết kế sơ đồ ERD cho hệ thống Web hỗ trợ dược sĩ ra quyết định bán thuốc an toàn.

Thiết kế cơ sở dữ liệu dựa trên các nghiệp vụ đã khảo sát ở CNPM-13 và đặc tả yêu cầu ở CNPM-14, gồm:

- Quản lý người dùng và phân quyền.
- Quản lý bệnh nhân/khách hàng.
- Quản lý thuốc, hoạt chất, nhóm thuốc.
- Ghi nhận dị ứng thuốc, bệnh nền, thuốc đang sử dụng.
- Tạo phiếu bán thuốc.
- Kiểm tra dị ứng, chống chỉ định, tương tác thuốc.
- Lưu cảnh báo an toàn và kết quả xử lý.

---

# CNPM-47 - Xác định thực thể chính của hệ thống

## 1. Danh sách thực thể chính

Các thực thể chính của hệ thống gồm:

| STT | Thực thể | Ý nghĩa |
|---|---|---|
| 1 | Roles | Lưu vai trò người dùng như Admin, Dược sĩ, Quản lý |
| 2 | Users | Lưu tài khoản người dùng trong hệ thống |
| 3 | Patients | Lưu thông tin bệnh nhân/khách hàng |
| 4 | DrugGroups | Lưu nhóm thuốc |
| 5 | ActiveIngredients | Lưu hoạt chất thuốc |
| 6 | Medicines | Lưu danh mục thuốc |
| 7 | MedicineIngredients | Lưu quan hệ thuốc và hoạt chất |
| 8 | Diseases | Lưu danh mục bệnh nền |
| 9 | PatientDiseases | Lưu bệnh nền của từng bệnh nhân |
| 10 | PatientAllergies | Lưu thông tin dị ứng thuốc/hoạt chất |
| 11 | PatientCurrentMedicines | Lưu thuốc bệnh nhân đang sử dụng |
| 12 | Prescriptions | Lưu thông tin đơn thuốc |
| 13 | Sales | Lưu phiếu bán thuốc |
| 14 | SaleDetails | Lưu chi tiết thuốc trong phiếu bán |
| 15 | DrugInteractions | Lưu tương tác giữa các hoạt chất/thuốc |
| 16 | Contraindications | Lưu chống chỉ định thuốc theo bệnh nền hoặc đối tượng đặc biệt |
| 17 | SafetyChecks | Lưu kết quả kiểm tra an toàn trước khi bán |
| 18 | Warnings | Lưu cảnh báo an toàn phát sinh |

## 2. Giải thích

Các thực thể trên được xác định dựa trên nghiệp vụ bán thuốc an toàn:

- Dược sĩ cần đăng nhập và có vai trò trong hệ thống.
- Bệnh nhân cần có hồ sơ sức khỏe, bệnh nền, dị ứng và thuốc đang dùng.
- Thuốc cần có thông tin nhóm thuốc, hoạt chất, hàm lượng, dạng bào chế.
- Khi bán thuốc, hệ thống tạo phiếu bán và chi tiết phiếu bán.
- Trước khi hoàn tất giao dịch, hệ thống kiểm tra an toàn và sinh cảnh báo nếu có rủi ro.

---

# CNPM-48 - Thiết kế quan hệ thuốc, bệnh nhân, bán thuốc và cảnh báo

## 1. Quan hệ tổng quát

Các quan hệ chính của hệ thống gồm:

| Quan hệ | Ý nghĩa |
|---|---|
| Roles 1 - n Users | Một vai trò có nhiều người dùng |
| Users 1 - n Sales | Một dược sĩ có thể lập nhiều phiếu bán |
| Patients 1 - n Sales | Một bệnh nhân có nhiều lần mua thuốc |
| Patients 1 - n PatientDiseases | Một bệnh nhân có nhiều bệnh nền |
| Diseases 1 - n PatientDiseases | Một bệnh nền có thể thuộc nhiều bệnh nhân |
| Patients 1 - n PatientAllergies | Một bệnh nhân có nhiều dị ứng |
| ActiveIngredients 1 - n PatientAllergies | Một hoạt chất có thể gây dị ứng cho nhiều bệnh nhân |
| DrugGroups 1 - n Medicines | Một nhóm thuốc có nhiều thuốc |
| Medicines 1 - n MedicineIngredients | Một thuốc có thể có nhiều hoạt chất |
| ActiveIngredients 1 - n MedicineIngredients | Một hoạt chất có thể thuộc nhiều thuốc |
| Patients 1 - n PatientCurrentMedicines | Một bệnh nhân có nhiều thuốc đang dùng |
| Medicines 1 - n PatientCurrentMedicines | Một thuốc có thể được nhiều bệnh nhân sử dụng |
| Patients 1 - n Prescriptions | Một bệnh nhân có nhiều đơn thuốc |
| Prescriptions 1 - n Sales | Một đơn thuốc có thể được dùng cho một hoặc nhiều phiếu bán |
| Sales 1 - n SaleDetails | Một phiếu bán có nhiều thuốc |
| Medicines 1 - n SaleDetails | Một thuốc có thể nằm trong nhiều phiếu bán |
| Sales 1 - n SafetyChecks | Một phiếu bán có thể có kết quả kiểm tra an toàn |
| SafetyChecks 1 - n Warnings | Một lần kiểm tra có thể sinh nhiều cảnh báo |
| Patients 1 - n Warnings | Một bệnh nhân có thể phát sinh nhiều cảnh báo |
| Medicines 1 - n Warnings | Một thuốc có thể phát sinh nhiều cảnh báo |

## 2. Luồng quan hệ nghiệp vụ

Luồng xử lý chính:

1. Dược sĩ đăng nhập hệ thống.
2. Dược sĩ tìm hoặc tạo hồ sơ bệnh nhân.
3. Dược sĩ nhập bệnh nền, dị ứng, thuốc đang dùng.
4. Dược sĩ chọn thuốc dự định bán.
5. Hệ thống kiểm tra:
   - Dị ứng thuốc.
   - Bệnh nền/chống chỉ định.
   - Tương tác thuốc.
   - Trùng hoạt chất.
   - Thuốc kê đơn.
   - Đối tượng đặc biệt như trẻ em, người cao tuổi, phụ nữ mang thai, cho con bú.
6. Nếu có rủi ro, hệ thống sinh cảnh báo.
7. Dược sĩ xem cảnh báo và quyết định bán, đổi thuốc, hủy giao dịch hoặc tư vấn đi khám.
8. Hệ thống lưu phiếu bán, chi tiết thuốc và lịch sử cảnh báo.

---

# CNPM-49 - Vẽ ERD và kiểm tra khóa chính, khóa ngoại

## 1. Danh sách bảng, khóa chính và khóa ngoại

### 1. Roles

| Trường | Loại khóa |
|---|---|
| RoleId | PK |
| RoleName |  |

### 2. Users

| Trường | Loại khóa |
|---|---|
| UserId | PK |
| RoleId | FK |
| FullName |  |
| Email |  |
| PasswordHash |  |
| Phone |  |
| Status |  |
| CreatedAt |  |

Khóa ngoại:

- Users.RoleId -> Roles.RoleId

### 3. Patients

| Trường | Loại khóa |
|---|---|
| PatientId | PK |
| FullName |  |
| Phone |  |
| Gender |  |
| DateOfBirth |  |
| WeightKg |  |
| Address |  |
| IsPregnant |  |
| IsBreastfeeding |  |
| Note |  |
| CreatedAt |  |

### 4. DrugGroups

| Trường | Loại khóa |
|---|---|
| DrugGroupId | PK |
| GroupName |  |
| Description |  |

### 5. ActiveIngredients

| Trường | Loại khóa |
|---|---|
| IngredientId | PK |
| IngredientName |  |
| Description |  |

### 6. Medicines

| Trường | Loại khóa |
|---|---|
| MedicineId | PK |
| DrugGroupId | FK |
| MedicineName |  |
| Strength |  |
| DosageForm |  |
| Unit |  |
| Price |  |
| RequiresPrescription |  |
| IsActive |  |
| Note |  |
| CreatedAt |  |

Khóa ngoại:

- Medicines.DrugGroupId -> DrugGroups.DrugGroupId

### 7. MedicineIngredients

| Trường | Loại khóa |
|---|---|
| MedicineId | PK, FK |
| IngredientId | PK, FK |
| Amount |  |

Khóa ngoại:

- MedicineIngredients.MedicineId -> Medicines.MedicineId
- MedicineIngredients.IngredientId -> ActiveIngredients.IngredientId

### 8. Diseases

| Trường | Loại khóa |
|---|---|
| DiseaseId | PK |
| DiseaseName |  |
| Description |  |

### 9. PatientDiseases

| Trường | Loại khóa |
|---|---|
| PatientDiseaseId | PK |
| PatientId | FK |
| DiseaseId | FK |
| Note |  |

Khóa ngoại:

- PatientDiseases.PatientId -> Patients.PatientId
- PatientDiseases.DiseaseId -> Diseases.DiseaseId

### 10. PatientAllergies

| Trường | Loại khóa |
|---|---|
| AllergyId | PK |
| PatientId | FK |
| IngredientId | FK |
| MedicineId | FK |
| AllergyNote |  |
| Severity |  |

Khóa ngoại:

- PatientAllergies.PatientId -> Patients.PatientId
- PatientAllergies.IngredientId -> ActiveIngredients.IngredientId
- PatientAllergies.MedicineId -> Medicines.MedicineId

### 11. PatientCurrentMedicines

| Trường | Loại khóa |
|---|---|
| CurrentMedicineId | PK |
| PatientId | FK |
| MedicineId | FK |
| MedicineNameText |  |
| Note |  |

Khóa ngoại:

- PatientCurrentMedicines.PatientId -> Patients.PatientId
- PatientCurrentMedicines.MedicineId -> Medicines.MedicineId

### 12. Prescriptions

| Trường | Loại khóa |
|---|---|
| PrescriptionId | PK |
| PatientId | FK |
| PrescriptionCode |  |
| DoctorName |  |
| PrescribedDate |  |
| ImageUrl |  |
| IsValid |  |
| Note |  |

Khóa ngoại:

- Prescriptions.PatientId -> Patients.PatientId

### 13. Sales

| Trường | Loại khóa |
|---|---|
| SaleId | PK |
| PatientId | FK |
| PharmacistId | FK |
| PrescriptionId | FK |
| SaleDate |  |
| TotalAmount |  |
| FinalDecision |  |
| Status |  |
| Note |  |

Khóa ngoại:

- Sales.PatientId -> Patients.PatientId
- Sales.PharmacistId -> Users.UserId
- Sales.PrescriptionId -> Prescriptions.PrescriptionId

### 14. SaleDetails

| Trường | Loại khóa |
|---|---|
| SaleDetailId | PK |
| SaleId | FK |
| MedicineId | FK |
| Quantity |  |
| UnitPrice |  |
| DosageInstruction |  |
| TimesPerDay |  |
| Duration |  |
| AdviceNote |  |

Khóa ngoại:

- SaleDetails.SaleId -> Sales.SaleId
- SaleDetails.MedicineId -> Medicines.MedicineId

### 15. DrugInteractions

| Trường | Loại khóa |
|---|---|
| InteractionId | PK |
| IngredientAId | FK |
| IngredientBId | FK |
| Severity |  |
| Description |  |
| Recommendation |  |

Khóa ngoại:

- DrugInteractions.IngredientAId -> ActiveIngredients.IngredientId
- DrugInteractions.IngredientBId -> ActiveIngredients.IngredientId

### 16. Contraindications

| Trường | Loại khóa |
|---|---|
| ContraindicationId | PK |
| MedicineId | FK |
| IngredientId | FK |
| DiseaseId | FK |
| ConditionType |  |
| Severity |  |
| Description |  |
| Recommendation |  |

Khóa ngoại:

- Contraindications.MedicineId -> Medicines.MedicineId
- Contraindications.IngredientId -> ActiveIngredients.IngredientId
- Contraindications.DiseaseId -> Diseases.DiseaseId

### 17. SafetyChecks

| Trường | Loại khóa |
|---|---|
| SafetyCheckId | PK |
| SaleId | FK |
| CheckedAt |  |
| HighestSeverity |  |
| Result |  |
| Recommendation |  |

Khóa ngoại:

- SafetyChecks.SaleId -> Sales.SaleId

### 18. Warnings

| Trường | Loại khóa |
|---|---|
| WarningId | PK |
| SafetyCheckId | FK |
| PatientId | FK |
| MedicineId | FK |
| WarningType |  |
| Severity |  |
| Message |  |
| Recommendation |  |
| IsAcknowledged |  |
| AcknowledgedBy | FK |
| AcknowledgedAt |  |
| Decision |  |
| CreatedAt |  |

Khóa ngoại:

- Warnings.SafetyCheckId -> SafetyChecks.SafetyCheckId
- Warnings.PatientId -> Patients.PatientId
- Warnings.MedicineId -> Medicines.MedicineId
- Warnings.AcknowledgedBy -> Users.UserId

## 2. Kiểm tra ERD

Đã kiểm tra:

- Mỗi bảng đều có khóa chính.
- Các bảng liên kết đều có khóa ngoại.
- Các quan hệ thuốc, bệnh nhân, bán thuốc và cảnh báo đã đúng với nghiệp vụ.
- Thiết kế đáp ứng yêu cầu lưu lịch sử bán thuốc và cảnh báo an toàn.
- Cơ sở dữ liệu có thể triển khai trên SQL Server.

---

# CNPM-50 - Cập nhật ERD vào tài liệu dự án

## 1. Nội dung cập nhật

Đã cập nhật thiết kế ERD vào tài liệu dự án, bao gồm:

1. Danh sách thực thể chính.
2. Danh sách quan hệ giữa các thực thể.
3. Mô tả khóa chính, khóa ngoại.
4. Mô tả luồng nghiệp vụ từ bệnh nhân, thuốc, bán thuốc đến cảnh báo.
5. Cơ sở để triển khai database SQL Server ở CNPM-16.

## 2. Kết luận

Thiết kế ERD đã phản ánh đúng nghiệp vụ bán thuốc an toàn. Cơ sở dữ liệu hỗ trợ quản lý bệnh nhân, thuốc, bệnh nền, dị ứng, tương tác thuốc, chống chỉ định, phiếu bán và cảnh báo phát sinh trong quá trình bán thuốc.
