<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Sidebar from './components/Sidebar.vue'
import Header from './components/Header.vue'

// ==========================================
// 1. TYPES matching PharmacySafetyDB
// ==========================================

interface User {
  UserId: number
  RoleId: number
  FullName: string
  Email: string
  Phone: string
  Status: string
  CreatedAt: string
}

interface Patient {
  PatientId: number
  FullName: string
  Phone: string
  Gender: string
  DateOfBirth: string
  WeightKg: number | null
  Address: string
  IsPregnant: boolean
  IsBreastfeeding: boolean
  Note: string
  CreatedAt: string
}

interface DrugGroup {
  DrugGroupId: number
  GroupName: string
  Description: string
}

interface ActiveIngredient {
  IngredientId: number
  IngredientName: string
  Description: string
}

interface Medicine {
  MedicineId: number
  DrugGroupId: number | null
  MedicineName: string
  Strength: string
  DosageForm: string
  Unit: string
  Price: number
  RequiresPrescription: boolean
  IsActive: boolean
  Note: string
  CreatedAt: string
}

interface MedicineIngredient {
  MedicineId: number
  IngredientId: number
  Amount: string
}

interface Disease {
  DiseaseId: number
  DiseaseName: string
  Description: string
}

interface PatientDisease {
  PatientDiseaseId: number
  PatientId: number
  DiseaseId: number
  Note: string
}

interface PatientAllergy {
  AllergyId: number
  PatientId: number
  IngredientId: number | null
  MedicineId: number | null
  AllergyNote: string
  Severity: string // 'Nghiêm trọng' | 'Trung bình' | 'Nhẹ'
}

interface DrugInteraction {
  InteractionId: number
  IngredientAId: number
  IngredientBId: number
  Severity: string // 'Nghiêm trọng' | 'Trung bình' | 'Nhẹ'
  Description: string
  Recommendation: string
}

interface Contraindication {
  ContraindicationId: number
  MedicineId: number | null
  IngredientId: number | null
  DiseaseId: number | null
  ConditionType: string // 'Bệnh nền chống chỉ định' | 'Đối tượng đặc biệt'
  Severity: string // 'Nghiêm trọng' | 'Trung bình'
  Description: string
  Recommendation: string
}

interface Sale {
  SaleId: number
  PatientId: number
  PharmacistId: number
  PrescriptionId: number | null
  SaleDate: string
  TotalAmount: number
  FinalDecision: 'Approved' | 'Denied' | 'Pending'
  Status: 'Completed' | 'Cancelled' | 'Pending'
  Note: string
}

interface SaleDetail {
  SaleDetailId: number
  SaleId: number
  MedicineId: number
  Quantity: number
  UnitPrice: number
  DosageInstruction: string
  TimesPerDay: number
  Duration: string
  AdviceNote: string
}

interface Warning {
  WarningId: number
  SafetyCheckId: number
  PatientId: number
  MedicineId: number | null
  WarningType: string // 'Tương tác thuốc' | 'Dị ứng thuốc' | 'Chống chỉ định bệnh nền' | 'Đối tượng đặc biệt'
  Severity: string
  Message: string
  Recommendation: string
  IsAcknowledged: boolean
  AcknowledgedBy: number | null
  AcknowledgedAt: string | null
  Decision: string | null
}

// ==========================================
// 2. MOCKUP DATABASES
// ==========================================

const users = ref<User[]>([
  { UserId: 1, RoleId: 1, FullName: 'Admin He Thong', Email: 'admin@gmail.com', Phone: '0900000000', Status: 'Active', CreatedAt: '2026-01-10' },
  { UserId: 2, RoleId: 2, FullName: 'Duoc Si A', Email: 'duocsi@gmail.com', Phone: '0911111111', Status: 'Active', CreatedAt: '2026-01-15' }
])

const patients = ref<Patient[]>([
  { PatientId: 1, FullName: 'Nguyen Van A', Phone: '0988888888', Gender: 'Nam', DateOfBirth: '1990-05-12', WeightKg: 65, Address: 'Gia Lai', IsPregnant: false, IsBreastfeeding: false, Note: 'Co benh nen cao huyet ap', CreatedAt: '2026-06-20' },
  { PatientId: 2, FullName: 'Tran Thi B', Phone: '0977777777', Gender: 'Nu', DateOfBirth: '1985-10-20', WeightKg: 52, Address: 'Gia Lai', IsPregnant: false, IsBreastfeeding: false, Note: 'Di ung thuoc giam dau', CreatedAt: '2026-06-20' }
])

const drugGroups = ref<DrugGroup[]>([
  { DrugGroupId: 1, GroupName: 'Thuoc giam dau ha sot', Description: 'Nhom thuoc dung de giam dau va ha sot' },
  { DrugGroupId: 2, GroupName: 'Khang sinh', Description: 'Nhom thuoc dieu tri nhiem khuan' },
  { DrugGroupId: 3, GroupName: 'Khang viem NSAID', Description: 'Nhom thuoc giam dau khang viem' }
])

const activeIngredients = ref<ActiveIngredient[]>([
  { IngredientId: 1, IngredientName: 'Paracetamol', Description: 'Hoat chat giam dau ha sot' },
  { IngredientId: 2, IngredientName: 'Amoxicillin', Description: 'Hoat chat khang sinh nhom Penicillin' },
  { IngredientId: 3, IngredientName: 'Ibuprofen', Description: 'Hoat chat giam dau khang viem NSAID' }
])

const medicines = ref<Medicine[]>([
  { MedicineId: 1, DrugGroupId: 1, MedicineName: 'Paracetamol 500mg', Strength: '500mg', DosageForm: 'Vien nen', Unit: 'Vien', Price: 2000, RequiresPrescription: false, IsActive: true, Note: 'Thuoc ha sot giam dau', CreatedAt: '2026-06-20' },
  { MedicineId: 2, DrugGroupId: 2, MedicineName: 'Amoxicillin 500mg', Strength: '500mg', DosageForm: 'Vien nang', Unit: 'Vien', Price: 3000, RequiresPrescription: true, IsActive: true, Note: 'Khang sinh can don', CreatedAt: '2026-06-20' },
  { MedicineId: 3, DrugGroupId: 3, MedicineName: 'Ibuprofen 400mg', Strength: '400mg', DosageForm: 'Vien nen', Unit: 'Vien', Price: 2500, RequiresPrescription: false, IsActive: true, Note: 'Giam dau khang viem', CreatedAt: '2026-06-20' }
])

const medicineIngredients = ref<MedicineIngredient[]>([
  { MedicineId: 1, IngredientId: 1, Amount: '500mg' },
  { MedicineId: 2, IngredientId: 2, Amount: '500mg' },
  { MedicineId: 3, IngredientId: 3, Amount: '400mg' }
])

const diseases = ref<Disease[]>([
  { DiseaseId: 1, DiseaseName: 'Cao huyet ap', Description: 'Benh tang huyet ap' },
  { DiseaseId: 2, DiseaseName: 'Suy than', Description: 'Benh nhan suy giam chuc nang than' },
  { DiseaseId: 3, DiseaseName: 'Viem loet da day', Description: 'Benh ly da day' }
])

const patientDiseases = ref<PatientDisease[]>([
  { PatientDiseaseId: 1, PatientId: 1, DiseaseId: 1, Note: 'Benh nhan co tien su cao huyet ap' }
])

const patientAllergies = ref<PatientAllergy[]>([
  { AllergyId: 1, PatientId: 2, IngredientId: 3, MedicineId: 3, AllergyNote: 'Di ung voi Ibuprofen', Severity: 'High' }
])

const drugInteractions = ref<DrugInteraction[]>([
  {
    InteractionId: 1,
    IngredientAId: 2, // Amoxicillin
    IngredientBId: 3, // Ibuprofen
    Severity: 'Trung bình',
    Description: 'Amoxicillin va Ibuprofen can than trong khi su dung chung.',
    Recommendation: 'Can tu van va theo doi trieu chung bat thuong.'
  }
])

const contraindications = ref<Contraindication[]>([
  {
    ContraindicationId: 1,
    MedicineId: 3, // Ibuprofen
    IngredientId: 3, // Ibuprofen
    DiseaseId: 3, // Viem loet da day
    ConditionType: 'Disease',
    Severity: 'Nghiêm trọng',
    Description: 'Ibuprofen khong phu hop voi benh nhan viem loet da day.',
    Recommendation: 'Can doi sang thuoc khac an toan hon.'
  }
])

const sales = ref<Sale[]>([
  { SaleId: 1, PatientId: 1, PharmacistId: 2, PrescriptionId: 1, TotalAmount: 7000, FinalDecision: 'Approved', Status: 'Completed', SaleDate: '2026-06-21 14:23', Note: 'Phieu ban thuoc demo' }
])

const saleDetails = ref<SaleDetail[]>([
  { SaleDetailId: 1, SaleId: 1, MedicineId: 1, Quantity: 2, UnitPrice: 2000, DosageInstruction: 'Uong 1 vien khi sot', TimesPerDay: 3, Duration: '3 ngay', AdviceNote: 'Khong dung qua lieu' },
  { SaleDetailId: 2, SaleId: 1, MedicineId: 2, Quantity: 1, UnitPrice: 3000, DosageInstruction: 'Uong theo huong dan cua bac si', TimesPerDay: 2, Duration: '5 ngay', AdviceNote: 'Uong du lieu' }
])

const warnings = ref<Warning[]>([
  {
    WarningId: 1,
    SafetyCheckId: 1,
    PatientId: 1,
    MedicineId: 2,
    WarningType: 'PrescriptionRequired',
    Severity: 'Trung bình',
    Message: 'Thuoc Amoxicillin can co don bac si.',
    Recommendation: 'Yeu cau benh nhan cung cap don thuoc.',
    IsAcknowledged: true,
    AcknowledgedBy: 2,
    AcknowledgedAt: '2026-06-21 14:20',
    Decision: 'AllowSale'
  }
])

// ==========================================
// 3. REACTIVE STATES FOR WORKFLOWS
// ==========================================

const isCollapsed = ref(false)
const currentRole = ref<'admin' | 'pharmacist' | 'manager'>('pharmacist')
const activeTab = ref('dashboard')

// Watch role change to check accessibility
const roleAllowedTabs: Record<'admin' | 'pharmacist' | 'manager', string[]> = {
  admin: ['dashboard', 'medicines', 'patients', 'safety-alerts', 'sales-history', 'users', 'settings'],
  pharmacist: ['dashboard', 'sell-medicine', 'medicines', 'patients', 'safety-alerts', 'sales-history'],
  manager: ['dashboard', 'medicines', 'patients', 'safety-alerts', 'sales-history']
}

watch(currentRole, (newRole) => {
  const allowed = roleAllowedTabs[newRole]
  if (!allowed.includes(activeTab.value)) {
    activeTab.value = 'dashboard'
  }
})

const handleToggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
}

const handleTabChange = (tabId: string) => {
  activeTab.value = tabId
}

// Helper: Age calculation from DateOfBirth
const calculateAge = (dobString: string) => {
  const birthday = new Date(dobString)
  const today = new Date()
  let age = today.getFullYear() - birthday.getFullYear()
  const m = today.getMonth() - birthday.getMonth()
  if (m < 0 || (m === 0 && today.getDate() < birthday.getDate())) {
    age--
  }
  return age
}

// ==========================================
// 4. INTERACTIVE PRESCRIPTION BUILDER STATE
// ==========================================

const selectedPatientId = ref<number>(1)
const selectedMedicineId = ref<number>(1)
const qtyToAdd = ref<number>(10)
const dosageText = ref<string>('Ngày uống 2 lần, mỗi lần 1 viên sau ăn')
const timesPerDayInput = ref<number>(2)
const durationInput = ref<string>('5 ngày')
const adviceNoteInput = ref<string>('')

// Current Active Prescription Cart for selling
interface CartItem {
  medicine: Medicine
  quantity: number
  dosageInstruction: string
  timesPerDay: number
  duration: string
  adviceNote: string
}
const prescriptionCart = ref<CartItem[]>([])

// Safety check engine results
const safetyWarnings = ref<Warning[]>([])
const hasCheckedSafety = ref(false)
const showSafetyResultsModal = ref(false)
const finalDecision = ref<'Approved' | 'Denied' | 'Pending'>('Pending')
const warningDecisions = ref<Record<number, string>>({})

// Load patient details reactively
const activePatient = computed(() => {
  return patients.value.find(p => p.PatientId === selectedPatientId.value) || patients.value[0]
})

// Load active patient diseases
const activePatientDiseasesList = computed(() => {
  const pDiseases = patientDiseases.value.filter(pd => pd.PatientId === selectedPatientId.value)
  return pDiseases.map(pd => {
    const dis = diseases.value.find(d => d.DiseaseId === pd.DiseaseId)
    return dis ? { name: dis.DiseaseName, note: pd.Note } : null
  }).filter(Boolean) as { name: string; note: string }[]
})

// Load active patient allergies
const activePatientAllergiesList = computed(() => {
  const pAllergies = patientAllergies.value.filter(pa => pa.PatientId === selectedPatientId.value)
  return pAllergies.map(pa => {
    let targetName = 'Không rõ'
    if (pa.IngredientId) {
      const ing = activeIngredients.value.find(i => i.IngredientId === pa.IngredientId)
      targetName = ing ? ing.IngredientName : ''
    } else if (pa.MedicineId) {
      const med = medicines.value.find(m => m.MedicineId === pa.MedicineId)
      targetName = med ? med.MedicineName : ''
    }
    return { target: targetName, severity: pa.Severity, note: pa.AllergyNote }
  })
})

// Total price in cart
const cartTotalAmount = computed(() => {
  return prescriptionCart.value.reduce((total, item) => total + (item.medicine.Price * item.quantity), 0)
})

// Add medicine to cart list
const addMedicineToCart = () => {
  const med = medicines.value.find(m => m.MedicineId === selectedMedicineId.value)
  if (!med) return

  // Check if already in cart
  const exists = prescriptionCart.value.find(item => item.medicine.MedicineId === med.MedicineId)
  if (exists) {
    exists.quantity += qtyToAdd.value
  } else {
    prescriptionCart.value.push({
      medicine: med,
      quantity: qtyToAdd.value,
      dosageInstruction: dosageText.value,
      timesPerDay: timesPerDayInput.value,
      duration: durationInput.value,
      adviceNote: adviceNoteInput.value
    })
  }

  // Reset checker state when cart modifies
  hasCheckedSafety.value = false
  safetyWarnings.value = []
}

// Remove medicine from cart
const removeFromCart = (index: number) => {
  prescriptionCart.value.splice(index, 1)
  hasCheckedSafety.value = false
  safetyWarnings.value = []
}

// ==========================================
// 5. SAFETY AUDIT ENGINE LOGIC (PharmacySafetyDB)
// ==========================================

const runSafetyCheck = () => {
  if (prescriptionCart.value.length === 0) return

  const generatedWarnings: Warning[] = []
  const patient = activePatient.value
  if (!patient) return
  let checkId = Math.floor(Math.random() * 1000) + 1

  // Extract all active ingredient IDs present in the current cart
  const cartIngredients: { medicineId: number; ingredientId: number; ingredientName: string }[] = []
  prescriptionCart.value.forEach(item => {
    const ingredients = medicineIngredients.value.filter(mi => mi.MedicineId === item.medicine.MedicineId)
    ingredients.forEach(mi => {
      const ingName = activeIngredients.value.find(ai => ai.IngredientId === mi.IngredientId)?.IngredientName || ''
      cartIngredients.push({
        medicineId: item.medicine.MedicineId,
        ingredientId: mi.IngredientId,
        ingredientName: ingName
      })
    })
  })

  // --- RULE CHECK 1: PATIENT ALLERGIES ---
  const patientAllergiesData = patientAllergies.value.filter(pa => pa.PatientId === patient.PatientId)
  cartIngredients.forEach(cartIng => {
    // Check allergy by ingredient
    const matchIng = patientAllergiesData.find(pa => pa.IngredientId === cartIng.ingredientId)
    if (matchIng) {
      const medName = medicines.value.find(m => m.MedicineId === cartIng.medicineId)?.MedicineName || ''
      generatedWarnings.push({
        WarningId: Math.floor(Math.random() * 10000),
        SafetyCheckId: checkId,
        PatientId: patient.PatientId,
        MedicineId: cartIng.medicineId,
        WarningType: 'Dị ứng thuốc',
        Severity: matchIng.Severity || 'Nghiêm trọng',
        Message: `Bệnh nhân dị ứng với hoạt chất [${cartIng.ingredientName}]. Thuốc [${medName}] có chứa hoạt chất này.`,
        Recommendation: `Ngay lập tức thay thế thuốc [${medName}] bằng một thuốc khác không thuộc cùng nhóm dược lý.`,
        IsAcknowledged: false,
        AcknowledgedBy: null,
        AcknowledgedAt: null,
        Decision: null
      })
    }
  })

  // --- RULE CHECK 2: DRUG INTERACTIONS (BETWEEN CART ITEMS) ---
  for (let i = 0; i < cartIngredients.length; i++) {
    for (let j = i + 1; j < cartIngredients.length; j++) {
      const ingA = cartIngredients[i]
      const ingB = cartIngredients[j]
      if (!ingA || !ingB) continue

      // Check if interaction exists in database (both directions)
      const interact = drugInteractions.value.find(di =>
        (di.IngredientAId === ingA.ingredientId && di.IngredientBId === ingB.ingredientId) ||
        (di.IngredientAId === ingB.ingredientId && di.IngredientBId === ingA.ingredientId)
      )

      if (interact) {
        const medAName = medicines.value.find(m => m.MedicineId === ingA.medicineId)?.MedicineName || ''
        const medBName = medicines.value.find(m => m.MedicineId === ingB.medicineId)?.MedicineName || ''

        generatedWarnings.push({
          WarningId: Math.floor(Math.random() * 10000),
          SafetyCheckId: checkId,
          PatientId: patient.PatientId,
          MedicineId: ingA.medicineId,
          WarningType: 'Tương tác thuốc',
          Severity: interact.Severity,
          Message: `Tương tác nghiêm trọng giữa [${medAName}] (${ingA.ingredientName}) và [${medBName}] (${ingB.ingredientName}). ${interact.Description}`,
          Recommendation: interact.Recommendation,
          IsAcknowledged: false,
          AcknowledgedBy: null,
          AcknowledgedAt: null,
          Decision: null
        })
      }
    }
  }

  // --- RULE CHECK 3: CONTRAINDICATIONS (DISEASES / SPECIAL STATUS) ---
  const patientDiseasesData = patientDiseases.value.filter(pd => pd.PatientId === patient.PatientId)

  cartIngredients.forEach(cartIng => {
    // A. Check contraindications by Disease
    patientDiseasesData.forEach(pDisease => {
      const contra = contraindications.value.find(c =>
        c.DiseaseId === pDisease.DiseaseId &&
        (c.IngredientId === cartIng.ingredientId || c.MedicineId === cartIng.medicineId)
      )

      if (contra) {
        const disName = diseases.value.find(d => d.DiseaseId === pDisease.DiseaseId)?.DiseaseName || ''
        const medName = medicines.value.find(m => m.MedicineId === cartIng.medicineId)?.MedicineName || ''

        generatedWarnings.push({
          WarningId: Math.floor(Math.random() * 10000),
          SafetyCheckId: checkId,
          PatientId: patient.PatientId,
          MedicineId: cartIng.medicineId,
          WarningType: 'Chống chỉ định bệnh nền',
          Severity: contra.Severity,
          Message: `Thuốc [${medName}] chống chỉ định ở người có bệnh nền [${disName}]. ${contra.Description}`,
          Recommendation: contra.Recommendation,
          IsAcknowledged: false,
          AcknowledgedBy: null,
          AcknowledgedAt: null,
          Decision: null
        })
      }
    })

    // B. Check Special Condition: Pregnancy
    if (patient.IsPregnant) {
      // Find Lisinopril pregnancy contraindication for Zestril
      const pregContra = contraindications.value.find(c =>
        c.ConditionType === 'Đối tượng đặc biệt' &&
        (c.MedicineId === cartIng.medicineId || c.IngredientId === cartIng.ingredientId)
      )

      if (pregContra) {
        const medName = medicines.value.find(m => m.MedicineId === cartIng.medicineId)?.MedicineName || ''
        generatedWarnings.push({
          WarningId: Math.floor(Math.random() * 10000),
          SafetyCheckId: checkId,
          PatientId: patient.PatientId,
          MedicineId: cartIng.medicineId,
          WarningType: 'Đối tượng đặc biệt',
          Severity: pregContra.Severity,
          Message: `Thuốc [${medName}] chống chỉ định ở phụ nữ mang thai. ${pregContra.Description}`,
          Recommendation: pregContra.Recommendation,
          IsAcknowledged: false,
          AcknowledgedBy: null,
          AcknowledgedAt: null,
          Decision: null
        })
      }
    }
  })

  // --- RULE CHECK 4: PRESCRIPTION REQUIRED ---
  prescriptionCart.value.forEach(item => {
    if (item.medicine.RequiresPrescription) {
      generatedWarnings.push({
        WarningId: Math.floor(Math.random() * 10000),
        SafetyCheckId: checkId,
        PatientId: patient.PatientId,
        MedicineId: item.medicine.MedicineId,
        WarningType: 'PrescriptionRequired',
        Severity: 'Trung bình',
        Message: `Thuốc [${item.medicine.MedicineName}] yêu cầu phải có đơn thuốc của bác sĩ.`,
        Recommendation: 'Yêu cầu bệnh nhân cung cấp đơn thuốc hoặc liên hệ bác sĩ kê toa.',
        IsAcknowledged: false,
        AcknowledgedBy: null,
        AcknowledgedAt: null,
        Decision: null
      })
    }
  })

  safetyWarnings.value = generatedWarnings
  hasCheckedSafety.value = true
  showSafetyResultsModal.value = true
  finalDecision.value = generatedWarnings.length > 0 ? 'Pending' : 'Approved'
}

// Simulating resolution of warning
const acknowledgeWarning = (warningId: number, decisionText: string) => {
  const w = safetyWarnings.value.find(warn => warn.WarningId === warningId)
  if (w) {
    w.IsAcknowledged = true
    w.AcknowledgedBy = 2 // Ds. Trần Thị Mai
    w.AcknowledgedAt = new Date().toISOString().replace('T', ' ').substring(0, 19)
    w.Decision = decisionText
  }

  // If all are acknowledged, set decision to Approved
  if (safetyWarnings.value.every(warn => warn.IsAcknowledged)) {
    finalDecision.value = 'Approved'
  }
}

// Cancel prescription
const cancelPrescription = () => {
  finalDecision.value = 'Denied'
  showSafetyResultsModal.value = false

  // Save to sales database mockup
  sales.value.unshift({
    SaleId: sales.value.length + 1,
    PatientId: selectedPatientId.value,
    PharmacistId: 2,
    PrescriptionId: null,
    SaleDate: new Date().toISOString().replace('T', ' ').substring(0, 16),
    TotalAmount: 0,
    FinalDecision: 'Denied',
    Status: 'Cancelled',
    Note: 'Bị từ chối do cảnh báo an toàn nghiêm trọng.'
  })

  // Clear cart
  prescriptionCart.value = []
  safetyWarnings.value = []
  hasCheckedSafety.value = false
}

// Complete prescription sales
const completePrescriptionSales = () => {
  showSafetyResultsModal.value = false

  // Save transaction
  const newSaleId = sales.value.length + 1
  sales.value.unshift({
    SaleId: newSaleId,
    PatientId: selectedPatientId.value,
    PharmacistId: 2,
    PrescriptionId: null,
    SaleDate: new Date().toISOString().replace('T', ' ').substring(0, 16),
    TotalAmount: cartTotalAmount.value,
    FinalDecision: finalDecision.value,
    Status: 'Completed',
    Note: finalDecision.value === 'Approved' && safetyWarnings.value.length > 0
      ? 'Bán sau khi duyệt cảnh báo.'
      : 'Bán an toàn thông thường.'
  })

  // Save details
  prescriptionCart.value.forEach((item, index) => {
    saleDetails.value.push({
      SaleDetailId: saleDetails.value.length + 1,
      SaleId: newSaleId,
      MedicineId: item.medicine.MedicineId,
      Quantity: item.quantity,
      UnitPrice: item.medicine.Price,
      DosageInstruction: item.dosageInstruction,
      TimesPerDay: item.timesPerDay,
      Duration: item.duration,
      AdviceNote: item.adviceNote
    })
  })

  // Add safety checks and warnings to global warnings db if exist
  if (safetyWarnings.value.length > 0) {
    safetyWarnings.value.forEach(w => {
      warnings.value.unshift({
        ...w,
        SafetyCheckId: warnings.value.length + 1
      })
    })
  }

  // Clear states
  prescriptionCart.value = []
  safetyWarnings.value = []
  hasCheckedSafety.value = false
  alert('Đã hoàn tất phiếu bán thuốc thành công và cập nhật lịch sử CSDL!')
}
</script>

<template>
  <div class="app-layout">
    <!-- Left Sidebar component -->
    <Sidebar 
      :is-collapsed="isCollapsed" 
      :current-role="currentRole" 
      :active-tab="activeTab"
      @update:is-collapsed="isCollapsed = $event"
      @change-tab="handleTabChange"
    />

    <!-- Right Content container -->
    <div class="main-container">
      <!-- Upper Header component -->
      <Header 
        :is-collapsed="isCollapsed" 
        :current-role="currentRole"
        @toggle-sidebar="handleToggleSidebar"
        @update:current-role="currentRole = $event"
      />

      <!-- Scrollable Main Content Area -->
      <main class="content-area">
        <div class="content-wrapper">
          <!-- Active Tab Headings -->
          <div class="page-header">
            <div>
              <span class="breadcrumb">SafePharmacy / CSDL: PharmacySafetyDB</span>
              <h1 class="page-title">
                <span v-if="activeTab === 'dashboard'">Bảng tổng quan</span>
                <span v-else-if="activeTab === 'sell-medicine'">Khu vực bán thuốc</span>
                <span v-else-if="activeTab === 'medicines'">Danh mục thuốc (Medicines)</span>
                <span v-else-if="activeTab === 'patients'">Hồ sơ bệnh nhân (Patients)</span>
                <span v-else-if="activeTab === 'safety-alerts'">Dữ liệu An toàn (Interactions & Contraindications)</span>
                <span v-else-if="activeTab === 'sales-history'">Lịch sử giao dịch (Sales & Warnings)</span>
                <span v-else-if="activeTab === 'users'">Quản lý nhân viên (Users & Roles)</span>
                <span v-else-if="activeTab === 'settings'">Cấu hình hệ thống (Settings)</span>
              </h1>
            </div>
            <div class="date-badge">
              <svg viewBox="0 0 24 24" class="calendar-icon" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                <line x1="16" y1="2" x2="16" y2="6" />
                <line x1="8" y1="2" x2="8" y2="6" />
                <line x1="3" y1="10" x2="21" y2="10" />
              </svg>
              <span>Hôm nay: 21 Tháng 6, 2026</span>
            </div>
          </div>

          <!-- Component screens depending on activeTab -->

          <!-- 1. TỔNG QUAN SCREEN -->
          <section v-if="activeTab === 'dashboard'" class="tab-screen">
            <div class="stat-grid">
              <div class="stat-card">
                <div class="stat-info">
                  <span class="stat-label">Số phiếu bán (Sales)</span>
                  <span class="stat-value">{{ sales.length }}</span>
                  <span class="stat-trend green">+12% so với hôm qua</span>
                </div>
                <div class="stat-icon flex-center success-bg">
                  <svg viewBox="0 0 24 24" class="teal-icon" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" />
                    <rect x="8" y="2" width="8" height="4" rx="1" />
                  </svg>
                </div>
              </div>
              <div class="stat-card">
                <div class="stat-info">
                  <span class="stat-label">Số cảnh báo ghi nhận</span>
                  <span class="stat-value">{{ warnings.length }}</span>
                  <span class="stat-trend red">Cơ sở dữ liệu an toàn online</span>
                </div>
                <div class="stat-icon flex-center danger-bg">
                  <svg viewBox="0 0 24 24" class="red-icon" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
                    <line x1="12" y1="8" x2="12" y2="12" />
                    <line x1="12" y1="16" x2="12.01" y2="16" />
                  </svg>
                </div>
              </div>
              <div class="stat-card">
                <div class="stat-info">
                  <span class="stat-label">Tổng thuốc quản lý</span>
                  <span class="stat-value">{{ medicines.length }}</span>
                  <span class="stat-trend green">4 nhóm trị liệu khác nhau</span>
                </div>
                <div class="stat-icon flex-center info-bg">
                  <svg viewBox="0 0 24 24" class="blue-icon" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M4.5 12.5l8-8a4.95 4.95 0 1 1 7 7l-8 8a4.95 4.95 0 0 1-7-7z" />
                  </svg>
                </div>
              </div>
            </div>

            <div class="dashboard-grid">
              <div class="grid-card recent-sales">
                <div class="card-header">
                  <h2 class="card-title">Phiếu bán hàng mới nhất (Sales)</h2>
                  <button class="text-btn" @click="handleTabChange('sales-history')">Chi tiết lịch sử</button>
                </div>
                <table class="dashboard-table">
                  <thead>
                    <tr>
                      <th>Mã</th>
                      <th>Bệnh nhân</th>
                      <th>Ngày bán</th>
                      <th>Tổng tiền</th>
                      <th>Duyệt an toàn</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="sale in sales.slice(0, 4)" :key="sale.SaleId">
                      <td>HD-00{{ sale.SaleId }}</td>
                      <td>{{ patients.find(p => p.PatientId === sale.PatientId)?.FullName }}</td>
                      <td>{{ sale.SaleDate }}</td>
                      <td>{{ sale.TotalAmount.toLocaleString() }}đ</td>
                      <td>
                        <span :class="['status-tag', sale.FinalDecision === 'Approved' ? 'safe' : sale.FinalDecision === 'Denied' ? 'danger' : 'warning']">
                          {{ sale.FinalDecision === 'Approved' ? 'Đã duyệt' : sale.FinalDecision === 'Denied' ? 'Từ chối' : 'Chờ kiểm tra' }}
                        </span>
                      </td>
                      <td>
                        <span :class="['status-tag', sale.Status === 'Completed' ? 'safe' : 'danger']">
                          {{ sale.Status === 'Completed' ? 'Hoàn tất' : 'Hủy bỏ' }}
                        </span>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <div class="grid-card warnings-summary">
                <div class="card-header">
                  <h2 class="card-title">Cảnh báo vừa xử lý (Warnings)</h2>
                </div>
                <div class="alert-list" v-if="warnings.length > 0">
                  <div v-for="w in warnings.slice(0, 2)" :key="w.WarningId" class="alert-item high-risk">
                    <span class="alert-badge">{{ w.Severity }}</span>
                    <p class="alert-desc"><strong>{{ w.WarningType }}</strong>: {{ w.Message }}</p>
                    <span class="alert-time">Dược sĩ xử lý: {{ w.Decision }}</span>
                  </div>
                </div>
                <div v-else class="empty-notif">
                  <p>Chưa có lịch sử cảnh báo an toàn nào phát sinh.</p>
                </div>
              </div>
            </div>
          </section>

          <!-- 2. BÁN THUỐC SCREEN -->
          <section v-else-if="activeTab === 'sell-medicine'" class="tab-screen">
            <div class="form-container">
              <!-- Left Column: Patient Selector & Profile -->
              <div class="form-section patient-details">
                <h3 class="section-title">1. Chọn bệnh nhân & Tiền sử bệnh (Patients)</h3>
                
                <div class="patient-selector-wrapper">
                  <label class="form-label">Tên bệnh nhân đăng ký:</label>
                  <select v-model="selectedPatientId" class="form-control select-control">
                    <option v-for="p in patients" :key="p.PatientId" :value="p.PatientId">
                      {{ p.FullName }} ({{ p.Phone }})
                    </option>
                  </select>
                </div>

                <div class="patient-card-demo" v-if="activePatient">
                  <div class="patient-header">
                    <h4>{{ activePatient.FullName }}</h4>
                    <span class="gender-age">{{ activePatient.Gender }} - {{ calculateAge(activePatient.DateOfBirth) }} tuổi</span>
                  </div>
                  
                  <div class="patient-details-grid">
                    <div class="detail-row">
                      <span class="detail-label">Số điện thoại:</span>
                      <span class="detail-value">{{ activePatient.Phone }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Cân nặng:</span>
                      <span class="detail-value">{{ activePatient.WeightKg ? activePatient.WeightKg + ' kg' : 'Chưa nhập' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Địa chỉ:</span>
                      <span class="detail-value text-ellipsis" :title="activePatient.Address">{{ activePatient.Address }}</span>
                    </div>
                  </div>

                  <!-- Special conditions flags -->
                  <div class="condition-toggles">
                    <span :class="['cond-badge', { 'active': activePatient.IsPregnant }]">
                      🤰 Mang thai: {{ activePatient.IsPregnant ? 'CÓ' : 'KHÔNG' }}
                    </span>
                    <span :class="['cond-badge', { 'active': activePatient.IsBreastfeeding }]">
                      🍼 Cho con bú: {{ activePatient.IsBreastfeeding ? 'CÓ' : 'KHÔNG' }}
                    </span>
                  </div>

                  <!-- Allergies (PatientAllergies) -->
                  <div class="allergy-tags">
                    <span class="tag-title">Tiền sử Dị ứng:</span>
                    <div class="tag-list" v-if="activePatientAllergiesList.length > 0">
                      <span v-for="(alg, idx) in activePatientAllergiesList" :key="idx" class="tag danger">
                        {{ alg.target }} ({{ alg.severity }})
                      </span>
                    </div>
                    <span v-else class="empty-text">Không ghi nhận dị ứng</span>
                  </div>

                  <!-- Diseases (PatientDiseases) -->
                  <div class="allergy-tags">
                    <span class="tag-title">Bệnh nền ghi nhận:</span>
                    <div class="tag-list" v-if="activePatientDiseasesList.length > 0">
                      <span v-for="(d, idx) in activePatientDiseasesList" :key="idx" class="tag warning" :title="d.note">
                        {{ d.name }}
                      </span>
                    </div>
                    <span v-else class="empty-text">Không có bệnh nền</span>
                  </div>
                </div>
              </div>

              <!-- Right Column: Cart builder -->
              <div class="form-section prescription-builder">
                <h3 class="section-title">2. Xây dựng giỏ hàng thuốc (Sale Details)</h3>
                
                <div class="builder-inputs">
                  <div class="input-row">
                    <div class="input-col">
                      <label class="form-label">Chọn thuốc:</label>
                      <select v-model="selectedMedicineId" class="form-control select-control">
                        <option v-for="m in medicines.filter(med => med.IsActive)" :key="m.MedicineId" :value="m.MedicineId">
                          {{ m.MedicineName }} ({{ m.Strength }}) - {{ m.Price }}đ
                        </option>
                      </select>
                    </div>
                    <div class="input-col max-100">
                      <label class="form-label">Số lượng:</label>
                      <input type="number" v-model.number="qtyToAdd" class="form-control" min="1" />
                    </div>
                  </div>

                  <div class="input-row">
                    <div class="input-col">
                      <label class="form-label">Hướng dẫn liều dùng (Dosage Instruction):</label>
                      <input type="text" v-model="dosageText" class="form-control" />
                    </div>
                  </div>

                  <div class="input-row">
                    <div class="input-col">
                      <label class="form-label">Số lần/ngày:</label>
                      <input type="number" v-model.number="timesPerDayInput" class="form-control" min="1" />
                    </div>
                    <div class="input-col">
                      <label class="form-label">Thời gian dùng:</label>
                      <input type="text" v-model="durationInput" class="form-control" />
                    </div>
                  </div>

                  <div class="input-row">
                    <button class="primary-btn full-width" @click="addMedicineToCart">Thêm vào giỏ hàng thuốc</button>
                  </div>
                </div>

                <div class="divider"></div>

                <!-- Prescription Cart Table -->
                <div class="cart-table-wrapper" v-if="prescriptionCart.length > 0">
                  <table class="dashboard-table">
                    <thead>
                      <tr>
                        <th>Tên thuốc</th>
                        <th>Hàm lượng</th>
                        <th>SL</th>
                        <th>Đơn giá</th>
                        <th>Liều dùng</th>
                        <th>Xóa</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(item, idx) in prescriptionCart" :key="idx">
                        <td><strong>{{ item.medicine.MedicineName }}</strong></td>
                        <td>{{ item.medicine.Strength }}</td>
                        <td>{{ item.quantity }}</td>
                        <td>{{ item.medicine.Price }}đ</td>
                        <td><small>{{ item.dosageInstruction }} ({{ item.duration }})</small></td>
                        <td>
                          <button class="delete-btn" @click="removeFromCart(idx)" title="Xóa khỏi đơn">×</button>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                  
                  <div class="cart-total-row">
                    <span>Tổng tiền thuốc: <strong>{{ cartTotalAmount.toLocaleString() }}đ</strong></span>
                  </div>

                  <div class="cart-actions-row">
                    <button class="safety-btn flex-center" @click="runSafetyCheck">
                      <svg viewBox="0 0 24 24" class="safety-icon-btn" fill="none" stroke="currentColor" stroke-width="2.5">
                        <path d="M9 12.75L11.25 15 15 9.75M21 12c0 1.268-.63 2.39-1.593 3.068a3.745 3.745 0 01-1.043 3.296" />
                      </svg>
                      Kiểm tra An toàn Bán thuốc (Safety Check)
                    </button>
                  </div>
                </div>

                <div v-else class="cart-placeholder flex-center">
                  <div class="placeholder-content">
                    <svg viewBox="0 0 24 24" class="cart-icon" fill="none" stroke="currentColor" stroke-width="1.5">
                      <circle cx="9" cy="21" r="1" />
                      <circle cx="20" cy="21" r="1" />
                      <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6" />
                    </svg>
                    <p>Giỏ hàng thuốc đang trống. Hãy thêm các thuốc được chỉ định vào đơn.</p>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <!-- 3. DANH MỤC THUỐC SCREEN -->
          <section v-else-if="activeTab === 'medicines'" class="tab-screen">
            <div class="table-container">
              <div class="table-actions">
                <input type="text" placeholder="Lọc theo tên thuốc, hoạt chất..." class="search-input-small" />
                <button class="primary-btn" v-if="currentRole === 'admin'">+ Thêm thuốc (Medicines)</button>
              </div>
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Mã thuốc</th>
                    <th>Tên thuốc</th>
                    <th>Nhóm thuốc (Drug Group)</th>
                    <th>Hàm lượng (Strength)</th>
                    <th>Dạng bào chế</th>
                    <th>ĐVT</th>
                    <th>Kê đơn</th>
                    <th>Đơn giá</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="med in medicines" :key="med.MedicineId">
                    <td>MED-00{{ med.MedicineId }}</td>
                    <td><strong>{{ med.MedicineName }}</strong></td>
                    <td>{{ drugGroups.find(dg => dg.DrugGroupId === med.DrugGroupId)?.GroupName || 'Mặc định' }}</td>
                    <td>{{ med.Strength }}</td>
                    <td>{{ med.DosageForm }}</td>
                    <td>{{ med.Unit }}</td>
                    <td>
                      <span :class="['status-tag', med.RequiresPrescription ? 'danger' : 'safe']">
                        {{ med.RequiresPrescription ? 'Yêu cầu đơn' : 'Không kê đơn' }}
                      </span>
                    </td>
                    <td>{{ med.Price.toLocaleString() }}đ</td>
                    <td>
                      <span :class="['status-tag', med.IsActive ? 'safe' : 'danger']">
                        {{ med.IsActive ? 'Hoạt động' : 'Tạm ngừng' }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- 4. HỒ SƠ BỆNH NHÂN SCREEN -->
          <section v-else-if="activeTab === 'patients'" class="tab-screen">
            <div class="table-container">
              <div class="table-actions">
                <input type="text" placeholder="Tìm kiếm bệnh nhân..." class="search-input-small" />
                <button class="primary-btn">+ Thêm hồ sơ (Patients)</button>
              </div>
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Họ tên</th>
                    <th>Ngày sinh</th>
                    <th>Giới tính</th>
                    <th>Cân nặng</th>
                    <th>Trạng thái đặc biệt</th>
                    <th>Dị ứng (Allergies)</th>
                    <th>Bệnh nền (Diseases)</th>
                    <th>Ghi chú</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="p in patients" :key="p.PatientId">
                    <td><strong>{{ p.FullName }}</strong><br><small>{{ p.Phone }}</small></td>
                    <td>{{ p.DateOfBirth }}</td>
                    <td>{{ p.Gender }}</td>
                    <td>{{ p.WeightKg ? p.WeightKg + ' kg' : '-' }}</td>
                    <td>
                      <div class="spec-cond-badges">
                        <span v-if="p.IsPregnant" class="status-tag danger">🤰 Mang thai</span>
                        <span v-if="p.IsBreastfeeding" class="status-tag warning">🍼 Con bú</span>
                        <span v-if="!p.IsPregnant && !p.IsBreastfeeding">Bình thường</span>
                      </div>
                    </td>
                    <td>
                      <div v-if="patientAllergies.some(pa => pa.PatientId === p.PatientId)">
                        <span v-for="pa in patientAllergies.filter(pa => pa.PatientId === p.PatientId)" :key="pa.AllergyId" class="tag danger inline-block">
                          {{ activeIngredients.find(ai => ai.IngredientId === pa.IngredientId)?.IngredientName }}
                        </span>
                      </div>
                      <span v-else>-</span>
                    </td>
                    <td>
                      <div v-if="patientDiseases.some(pd => pd.PatientId === p.PatientId)">
                        <span v-for="pd in patientDiseases.filter(pd => pd.PatientId === p.PatientId)" :key="pd.PatientDiseaseId" class="tag warning inline-block">
                          {{ diseases.find(d => d.DiseaseId === pd.DiseaseId)?.DiseaseName }}
                        </span>
                      </div>
                      <span v-else>-</span>
                    </td>
                    <td><small>{{ p.Note || '-' }}</small></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- 5. DỮ LIỆU AN TOÀN SCREEN -->
          <section v-else-if="activeTab === 'safety-alerts'" class="tab-screen">
            <!-- Drug Interactions (Tương tác thuốc) -->
            <div class="grid-card text-section">
              <h3 class="section-title">Danh mục Tương tác Thuốc (Drug Interactions)</h3>
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Hoạt chất A</th>
                    <th>Hoạt chất B</th>
                    <th>Mức độ</th>
                    <th>Mô tả tác hại</th>
                    <th>Khuyến cáo lâm sàng (Recommendation)</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="di in drugInteractions" :key="di.InteractionId">
                    <td><strong>{{ activeIngredients.find(ai => ai.IngredientId === di.IngredientAId)?.IngredientName }}</strong></td>
                    <td><strong>{{ activeIngredients.find(ai => ai.IngredientId === di.IngredientBId)?.IngredientName }}</strong></td>
                    <td><span class="status-tag danger">{{ di.Severity }}</span></td>
                    <td><small>{{ di.Description }}</small></td>
                    <td><small class="green">{{ di.Recommendation }}</small></td>
                  </tr>
                </tbody>
              </table>
            </div>

            <br>

            <!-- Contraindications (Chống chỉ định) -->
            <div class="grid-card text-section">
              <h3 class="section-title">Danh mục Chống chỉ định (Contraindications)</h3>
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Thuốc / Hoạt chất</th>
                    <th>Điều kiện chống chỉ định</th>
                    <th>Phân loại</th>
                    <th>Mức độ</th>
                    <th>Mô tả tác hại</th>
                    <th>Khuyến cáo lâm sàng</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="c in contraindications" :key="c.ContraindicationId">
                    <td>
                      <strong v-if="c.MedicineId">{{ medicines.find(m => m.MedicineId === c.MedicineId)?.MedicineName }}</strong>
                      <strong v-else-if="c.IngredientId">{{ activeIngredients.find(ai => ai.IngredientId === c.IngredientId)?.IngredientName }}</strong>
                    </td>
                    <td>
                      <span v-if="c.DiseaseId" class="tag warning">{{ diseases.find(d => d.DiseaseId === c.DiseaseId)?.DiseaseName }}</span>
                      <span v-else class="tag danger">🤰 Phụ nữ mang thai</span>
                    </td>
                    <td><small>{{ c.ConditionType }}</small></td>
                    <td><span class="status-tag danger">{{ c.Severity }}</span></td>
                    <td><small>{{ c.Description }}</small></td>
                    <td><small class="green">{{ c.Recommendation }}</small></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- 6. LỊCH SỬ GIAO DỊCH SCREEN -->
          <section v-else-if="activeTab === 'sales-history'" class="tab-screen">
            <!-- Sales logs table -->
            <div class="grid-card">
              <h3 class="section-title">Lịch sử Phiếu bán thuốc (Sales)</h3>
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Mã HD</th>
                    <th>Bệnh nhân</th>
                    <th>Dược sĩ bán</th>
                    <th>Ngày giao dịch</th>
                    <th>Tổng tiền</th>
                    <th>Duyệt an toàn</th>
                    <th>Trạng thái</th>
                    <th>Ghi chú</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="sale in sales" :key="sale.SaleId">
                    <td>HD-00{{ sale.SaleId }}</td>
                    <td><strong>{{ patients.find(p => p.PatientId === sale.PatientId)?.FullName }}</strong></td>
                    <td>{{ users.find(u => u.UserId === sale.PharmacistId)?.FullName }}</td>
                    <td>{{ sale.SaleDate }}</td>
                    <td>{{ sale.TotalAmount.toLocaleString() }}đ</td>
                    <td>
                      <span :class="['status-tag', sale.FinalDecision === 'Approved' ? 'safe' : sale.FinalDecision === 'Denied' ? 'danger' : 'warning']">
                        {{ sale.FinalDecision === 'Approved' ? 'Đã duyệt' : sale.FinalDecision === 'Denied' ? 'Từ chối' : 'Chờ duyệt' }}
                      </span>
                    </td>
                    <td>
                      <span :class="['status-tag', sale.Status === 'Completed' ? 'safe' : 'danger']">
                        {{ sale.Status === 'Completed' ? 'Hoàn tất' : 'Hủy bỏ' }}
                      </span>
                    </td>
                    <td><small>{{ sale.Note }}</small></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- 7. QUẢN LÝ NHÂN VIÊN SCREEN -->
          <section v-else-if="activeTab === 'users'" class="tab-screen">
            <div class="grid-card">
              <div class="table-actions">
                <input type="text" placeholder="Tìm tài khoản nhân viên..." class="search-input-small" />
                <button class="primary-btn">+ Tạo tài khoản mới</button>
              </div>
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Mã số</th>
                    <th>Họ và tên</th>
                    <th>Email liên hệ</th>
                    <th>Điện thoại</th>
                    <th>Vai trò (Role)</th>
                    <th>Ngày tạo</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="u in users" :key="u.UserId">
                    <td>USR-00{{ u.UserId }}</td>
                    <td><strong>{{ u.FullName }}</strong></td>
                    <td>{{ u.Email }}</td>
                    <td>{{ u.Phone }}</td>
                    <td>
                      <span :class="['role-badge', u.RoleId === 1 ? 'admin' : u.RoleId === 3 ? 'manager' : 'pharmacist']">
                        {{ u.RoleId === 1 ? 'Quản trị viên' : u.RoleId === 3 ? 'Quản lý' : 'Dược sĩ' }}
                      </span>
                    </td>
                    <td>{{ u.CreatedAt }}</td>
                    <td><span class="status-tag active">Hoạt động</span></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- 8. CẤU HÌNH HỆ THỐNG SCREEN -->
          <section v-else-if="activeTab === 'settings'" class="tab-screen">
            <div class="form-container">
              <div class="grid-card">
                <h3 class="section-title">Cấu hình CSDL & Logic động cơ cảnh báo</h3>
                <div class="setting-item">
                  <div class="setting-info">
                    <span class="setting-name">Kiểm tra dị ứng nghiêm trọng</span>
                    <span class="setting-desc">Tự động đối chiếu giỏ hàng với danh mục dị ứng của Patients.</span>
                  </div>
                  <label class="switch">
                    <input type="checkbox" checked />
                    <span class="slider"></span>
                  </label>
                </div>
                <div class="setting-item">
                  <div class="setting-info">
                    <span class="setting-name">Cảnh báo tương tác chéo (Drug Interactions)</span>
                    <span class="setting-desc">Quét các cặp hoạt chất tương tác từ bảng DrugInteractions trong đơn.</span>
                  </div>
                  <label class="switch">
                    <input type="checkbox" checked />
                    <span class="slider"></span>
                  </label>
                </div>
                <div class="setting-item">
                  <div class="setting-info">
                    <span class="setting-name">Cảnh báo đối tượng mang thai & cho con bú</span>
                    <span class="setting-desc">Tự động phát hiện thuốc chống chỉ định cho bà bầu.</span>
                  </div>
                  <label class="switch">
                    <input type="checkbox" checked />
                    <span class="slider"></span>
                  </label>
                </div>
              </div>
            </div>
          </section>
        </div>
      </main>
    </div>

    <!-- ==========================================
      SAFETY CHECK RESULTS MODAL DIALOG
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showSafetyResultsModal">
      <div class="modal-card">
        <div class="modal-header">
          <div class="modal-title-area">
            <svg viewBox="0 0 24 24" class="safety-modal-icon" fill="none" stroke="currentColor" stroke-width="2.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
            </svg>
            <h3>Kết quả Kiểm tra An toàn Bán thuốc</h3>
          </div>
          <button class="close-modal-btn" @click="showSafetyResultsModal = false">×</button>
        </div>
        
        <div class="modal-body">
          <div class="patient-quick-summary" v-if="activePatient">
            <span>Bệnh nhân: <strong>{{ activePatient.FullName }}</strong></span>
            <span>Cân nặng: <strong>{{ activePatient.WeightKg }} kg</strong></span>
            <span v-if="activePatient.IsPregnant" class="status-tag danger">Mang thai</span>
          </div>

          <!-- Warnings List -->
          <div class="warnings-holder" v-if="safetyWarnings.length > 0">
            <p class="warning-alert-count">⚠️ Hệ thống phát hiện <strong>{{ safetyWarnings.length }}</strong> mối nguy hại nguy hiểm!</p>
            
            <div class="warnings-scroll-list">
              <div v-for="w in safetyWarnings" :key="w.WarningId" :class="['safety-warning-card', { 'acknowledged': w.IsAcknowledged }]">
                <div class="warning-card-head">
                  <span class="warning-tag">{{ w.WarningType }}</span>
                  <span class="severity-badge-high">{{ w.Severity }}</span>
                </div>
                <p class="warning-msg">{{ w.Message }}</p>
                
                <div class="recommendation-box">
                  <strong>Khuyến cáo y khoa:</strong>
                  <p>{{ w.Recommendation }}</p>
                </div>

                <!-- Resolution Area -->
                <div class="resolution-row">
                  <div v-if="!w.IsAcknowledged" class="ack-input-group">
                    <input 
                      type="text" 
                      placeholder="Nhập lý do/quyết định (ví dụ: Thay đổi thuốc sang Paracetamol)..." 
                      class="form-control text-control-sm"
                      v-model="warningDecisions[w.WarningId]"
                    />
                    <button 
                      class="ack-btn" 
                      @click="acknowledgeWarning(w.WarningId, warningDecisions[w.WarningId] || 'Đã xác nhận và điều chỉnh đơn thuốc')"
                    >
                      Xác nhận đã xử lý
                    </button>
                  </div>
                  <div v-else class="ack-done">
                    <span>✓ Đã xác nhận xử lý: <strong>{{ w.Decision }}</strong> (Bởi: Ds. Mai)</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="safety-success-message flex-center" v-else>
            <div class="success-content">
              <svg viewBox="0 0 24 24" class="success-tick-icon" fill="none" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <h4>Không phát hiện rủi ro!</h4>
              <p>Đơn thuốc an toàn với các dữ liệu dị ứng, tương tác thuốc và chống chỉ định bệnh nền hiện tại của bệnh nhân.</p>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="cancelPrescription">Hủy bán thuốc (Hủy giao dịch)</button>
          
          <button 
            class="primary-btn" 
            :disabled="finalDecision === 'Pending'"
            @click="completePrescriptionSales"
          >
            {{ finalDecision === 'Pending' ? 'Cần xác nhận các cảnh báo để bán' : 'Hoàn tất & Xuất hóa đơn' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  width: 100vw;
  height: 100vh;
  overflow: hidden;
  background-color: var(--bg-main);
}

.main-container {
  display: flex;
  flex-direction: column;
  flex: 1;
  height: 100vh;
  overflow: hidden;
}

.content-area {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
  overflow-x: hidden;
  background-color: var(--bg-main);
}

.content-wrapper {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Page Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 16px;
}

.breadcrumb {
  font-size: 11px;
  font-weight: 700;
  color: var(--text-muted);
  letter-spacing: 0.5px;
  text-transform: uppercase;
}

.page-title {
  font-size: 24px;
  font-weight: 800;
  color: var(--text-main);
  margin-top: 4px;
}

.date-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: var(--bg-card);
  padding: 8px 16px;
  border-radius: var(--border-radius-md);
  border: 1px solid var(--border-color);
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
  box-shadow: var(--shadow-sm);
}

.calendar-icon {
  width: 16px;
  height: 16px;
  color: var(--primary-medium);
}

/* Dashboard Statistics Grid */
.stat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
}

.stat-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: var(--bg-card);
  padding: 24px;
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  transition: transform var(--transition-normal), box-shadow var(--transition-normal);
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
}

.stat-value {
  font-size: 32px;
  font-weight: 800;
  color: var(--text-main);
  margin: 6px 0;
  line-height: 1;
}

.stat-trend {
  font-size: 12px;
  font-weight: 600;
}

.stat-trend.green { color: var(--success); }
.stat-trend.red { color: var(--danger); }

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--border-radius-md);
}

.teal-icon { color: var(--primary-medium); width: 24px; height: 24px;}
.red-icon { color: var(--danger); width: 24px; height: 24px;}
.blue-icon { color: var(--info); width: 24px; height: 24px;}

.success-bg { background-color: var(--primary-bg); }
.danger-bg { background-color: var(--danger-bg); }
.info-bg { background-color: var(--info-bg); }

/* Dashboard Layout Grid */
.dashboard-grid {
  display: grid;
  grid-template-columns: 1.6fr 1fr;
  gap: 24px;
}

@media (max-width: 1024px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}

.grid-card {
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 24px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.card-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
}

.text-btn {
  background: transparent;
  border: none;
  font-size: 13px;
  font-weight: 600;
  color: var(--primary-medium);
  cursor: pointer;
}

.text-btn:hover {
  color: var(--primary);
  text-decoration: underline;
}

/* Dashboard Table styling */
.dashboard-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.dashboard-table th {
  padding: 12px 8px;
  font-size: 12px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  border-bottom: 1px solid var(--border-color);
}

.dashboard-table td {
  padding: 14px 8px;
  font-size: 14px;
  border-bottom: 1px solid #f1f5f9;
}

.dashboard-table tr:last-child td {
  border-bottom: none;
}

.status-tag {
  font-size: 11px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: var(--border-radius-sm);
  display: inline-block;
}

.status-tag.safe { background-color: var(--success-bg); color: var(--success); }
.status-tag.warning { background-color: var(--warning-bg); color: var(--warning); }
.status-tag.danger { background-color: var(--danger-bg); color: var(--danger); }

/* Warning / Alert Items list */
.alert-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.alert-item {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 16px;
  border-radius: var(--border-radius-md);
  border: 1px solid transparent;
}

.alert-item.high-risk {
  background-color: var(--danger-bg);
  border-color: rgba(239, 68, 68, 0.15);
}

.alert-item.patient-allergy {
  background-color: var(--warning-bg);
  border-color: rgba(245, 158, 11, 0.15);
}

.alert-badge {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  padding: 2px 6px;
  border-radius: 4px;
  align-self: flex-start;
  background-color: var(--danger);
  color: white;
}

.alert-desc {
  font-size: 13px;
  color: var(--text-main);
  line-height: 1.4;
}

.alert-time {
  font-size: 11px;
  color: var(--text-muted);
}

/* Common form & layouts */
.form-container {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 24px;
}

@media (max-width: 900px) {
  .form-container {
    grid-template-columns: 1fr;
  }
}

.form-section {
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 24px;
}

.section-title {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-main);
  margin-bottom: 20px;
  border-left: 3px solid var(--primary-medium);
  padding-left: 10px;
}

.patient-selector-wrapper {
  margin-bottom: 16px;
}

.form-label {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
  margin-bottom: 6px;
}

.form-control {
  width: 100%;
  padding: 10px 14px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  outline: none;
  font-size: 14px;
  transition: all var(--transition-fast);
}

.form-control:focus {
  border-color: var(--border-focus);
  background-color: #ffffff;
}

.select-control {
  appearance: none;
  background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%2364748b' stroke-width='2'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 14px center;
  background-size: 16px;
  padding-right: 40px;
}

.patient-card-demo {
  border: 1px dashed var(--border-color);
  border-radius: var(--border-radius-md);
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  background-color: #fafbfd;
  margin-top: 16px;
}

.patient-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #f1f5f9;
  padding-bottom: 10px;
}

.patient-header h4 {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
}

.gender-age {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
}

.patient-details-grid {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-row {
  display: flex;
  font-size: 13px;
}

.detail-label {
  width: 110px;
  color: var(--text-muted);
  font-weight: 500;
}

.detail-value {
  flex: 1;
  color: var(--text-main);
  font-weight: 600;
}

.condition-toggles {
  display: flex;
  gap: 10px;
}

.cond-badge {
  font-size: 11px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: var(--border-radius-sm);
  background-color: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.cond-badge.active {
  background-color: var(--danger-bg);
  color: var(--danger);
  border-color: rgba(239, 68, 68, 0.2);
}

.allergy-tags {
  display: flex;
  flex-direction: column;
  gap: 6px;
  border-top: 1px solid #f1f5f9;
  padding-top: 10px;
}

.tag-title {
  font-size: 12px;
  color: var(--text-muted);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.tag {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 4px;
}

.tag.danger { background-color: var(--danger-bg); color: var(--danger); }
.tag.warning { background-color: var(--warning-bg); color: var(--warning); }

.empty-text {
  font-size: 13px;
  color: #94a3b8;
  font-style: italic;
}

/* Builder Inputs Grid */
.builder-inputs {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.input-row {
  display: flex;
  gap: 12px;
}

.input-col {
  flex: 1;
}

.max-100 {
  max-width: 100px;
}

.primary-btn {
  background-color: var(--primary-medium);
  color: white;
  border: none;
  padding: 11px 20px;
  border-radius: var(--border-radius-md);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  box-shadow: 0 2px 4px rgba(13, 148, 136, 0.15);
}

.primary-btn:hover {
  background-color: var(--primary);
}

.primary-btn:disabled {
  background-color: #cbd5e1;
  color: #94a3b8;
  cursor: not-allowed;
  box-shadow: none;
}

.secondary-btn {
  background-color: var(--bg-main);
  color: var(--text-main);
  border: 1px solid var(--border-color);
  padding: 11px 20px;
  border-radius: var(--border-radius-md);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.secondary-btn:hover {
  background-color: #e2e8f0;
}

.full-width {
  width: 100%;
}

.cart-table-wrapper {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.cart-total-row {
  display: flex;
  justify-content: flex-end;
  font-size: 15px;
  color: var(--text-main);
  border-top: 1px solid var(--border-color);
  padding-top: 12px;
}

.cart-total-row strong {
  font-size: 18px;
  color: var(--primary);
  margin-left: 6px;
}

.cart-actions-row {
  display: flex;
  justify-content: flex-end;
}

.safety-btn {
  background: linear-gradient(135deg, var(--primary-light), var(--primary));
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: var(--border-radius-md);
  font-weight: 700;
  cursor: pointer;
  transition: all var(--transition-fast);
  box-shadow: 0 4px 10px rgba(13, 148, 136, 0.3);
  gap: 10px;
}

.safety-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 14px rgba(13, 148, 136, 0.4);
}

.safety-icon-btn {
  width: 18px;
  height: 18px;
}

.delete-btn {
  background: transparent;
  border: none;
  font-size: 18px;
  color: var(--danger);
  cursor: pointer;
  font-weight: 700;
}

.delete-btn:hover {
  transform: scale(1.2);
}

.cart-placeholder {
  min-height: 200px;
  border: 2px dashed #e2e8f0;
  border-radius: var(--border-radius-lg);
  padding: 30px;
  text-align: center;
  color: var(--text-muted);
}

.cart-icon {
  width: 48px;
  height: 48px;
  margin-bottom: 12px;
  color: #cbd5e1;
}

/* Large Data Tables */
.table-container {
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 24px;
}

.table-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  gap: 16px;
}

.search-input-small {
  width: 280px;
  padding: 8px 12px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  font-size: 14px;
  outline: none;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.data-table th {
  padding: 14px 16px;
  font-size: 12px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  border-bottom: 1px solid var(--border-color);
  background-color: var(--bg-main);
}

.data-table td {
  padding: 16px;
  font-size: 14px;
  border-bottom: 1px solid #f1f5f9;
}

.data-table tr:hover td {
  background-color: #fafbfd;
}

.spec-cond-badges {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.inline-block {
  display: inline-block;
  margin-right: 4px;
  margin-bottom: 4px;
}

/* Switch styling for settings screen */
.setting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
  border-bottom: 1px solid #f1f5f9;
}

.setting-item:last-child {
  border-bottom: none;
}

.setting-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.setting-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
}

.setting-desc {
  font-size: 12px;
  color: var(--text-muted);
}

.switch {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
  flex-shrink: 0;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #cbd5e1;
  transition: .3s;
  border-radius: 24px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: .3s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: var(--primary-medium);
}

input:checked + .slider:before {
  transform: translateX(20px);
}

/* Modal Overlay Styling */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(4px);
  z-index: 1000;
  padding: 20px;
}

.modal-card {
  width: 680px;
  max-width: 100%;
  max-height: 90vh;
  background-color: #ffffff;
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-premium);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid var(--border-color);
  background-color: var(--bg-main);
}

.modal-title-area {
  display: flex;
  align-items: center;
  gap: 12px;
}

.modal-title-area h3 {
  font-size: 18px;
  font-weight: 800;
  color: var(--text-main);
}

.safety-modal-icon {
  width: 26px;
  height: 26px;
  color: var(--primary-medium);
}

.close-modal-btn {
  background: transparent;
  border: none;
  font-size: 28px;
  line-height: 1;
  color: var(--text-muted);
  cursor: pointer;
}

.close-modal-btn:hover {
  color: var(--text-main);
}

.modal-body {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.patient-quick-summary {
  display: flex;
  gap: 20px;
  background-color: var(--bg-main);
  padding: 12px 16px;
  border-radius: var(--border-radius-md);
  font-size: 13px;
  border: 1px solid var(--border-color);
}

.warnings-holder {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.warning-alert-count {
  font-size: 14px;
  font-weight: 700;
  color: var(--danger);
}

.warnings-scroll-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.safety-warning-card {
  border: 1px solid rgba(239, 68, 68, 0.2);
  background-color: var(--danger-bg);
  border-radius: var(--border-radius-md);
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  transition: opacity var(--transition-fast);
}

.safety-warning-card.acknowledged {
  opacity: 0.65;
  border-color: var(--border-color);
  background-color: var(--bg-main);
}

.warning-card-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.warning-tag {
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--danger);
  background-color: rgba(239, 68, 68, 0.08);
  padding: 2px 8px;
  border-radius: var(--border-radius-sm);
}

.acknowledged .warning-tag {
  color: var(--text-muted);
  background-color: #e2e8f0;
}

.severity-badge-high {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  background-color: var(--danger);
  color: white;
  padding: 2px 6px;
  border-radius: 4px;
}

.warning-msg {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
  line-height: 1.45;
}

.recommendation-box {
  background-color: white;
  border: 1px solid rgba(239, 68, 68, 0.1);
  padding: 10px 14px;
  border-radius: var(--border-radius-sm);
  font-size: 13px;
}

.recommendation-box strong {
  color: #b91c1c;
  display: block;
  margin-bottom: 4px;
}

.resolution-row {
  margin-top: 6px;
  border-top: 1px dashed rgba(239, 68, 68, 0.2);
  padding-top: 12px;
}

.ack-input-group {
  display: flex;
  gap: 10px;
}

.text-control-sm {
  padding: 8px 12px;
  font-size: 13px;
}

.ack-btn {
  background-color: var(--warning);
  color: white;
  border: none;
  padding: 0 16px;
  border-radius: var(--border-radius-md);
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  white-space: nowrap;
  transition: background var(--transition-fast);
}

.ack-btn:hover {
  background-color: #d97706;
}

.ack-done {
  font-size: 13px;
  color: var(--success);
  font-weight: 600;
}

.safety-success-message {
  flex-direction: column;
  text-align: center;
  padding: 40px 20px;
}

.success-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  max-width: 420px;
}

.success-tick-icon {
  width: 56px;
  height: 56px;
  color: var(--success);
}

.success-content h4 {
  font-size: 18px;
  font-weight: 800;
  color: var(--text-main);
}

.success-content p {
  font-size: 14px;
  color: var(--text-muted);
  line-height: 1.45;
}

.modal-footer {
  display: flex;
  justify-content: space-between;
  padding: 16px 24px;
  border-top: 1px solid var(--border-color);
  background-color: var(--bg-main);
}
</style>
