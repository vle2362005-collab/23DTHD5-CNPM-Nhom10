import { ref, computed } from 'vue'

// ==========================================
// 1. TYPES matching PharmacySafetyDB
// ==========================================

export interface User {
  UserId: number
  RoleId: number
  FullName: string
  Email: string
  Phone: string
  Status: string
  CreatedAt: string
}

export interface Patient {
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

export interface DrugGroup {
  DrugGroupId: number
  GroupName: string
  Description: string
}

export interface ActiveIngredient {
  IngredientId: number
  IngredientName: string
  Description: string
}

export interface Medicine {
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

export interface MedicineIngredient {
  MedicineId: number
  IngredientId: number
  Amount: string
}

export interface Disease {
  DiseaseId: number
  DiseaseName: string
  Description: string
}

export interface PatientDisease {
  PatientDiseaseId: number
  PatientId: number
  DiseaseId: number
  Note: string
}

export interface PatientAllergy {
  AllergyId: number
  PatientId: number
  IngredientId: number | null
  MedicineId: number | null
  AllergyNote: string
  Severity: string // 'Nghiêm trọng' | 'Trung bình' | 'Nhẹ'
}

export interface DrugInteraction {
  InteractionId: number
  IngredientAId: number
  IngredientBId: number
  Severity: string // 'Nghiêm trọng' | 'Trung bình' | 'Nhẹ'
  Description: string
  Recommendation: string
}

export interface Contraindication {
  ContraindicationId: number
  MedicineId: number | null
  IngredientId: number | null
  DiseaseId: number | null
  ConditionType: string // 'Bệnh nền chống chỉ định' | 'Đối tượng đặc biệt'
  Severity: string // 'Nghiêm trọng' | 'Trung bình'
  Description: string
  Recommendation: string
}

export interface Sale {
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

export interface SaleDetail {
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

export interface Warning {
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

export interface CartItem {
  medicine: Medicine
  quantity: number
  dosageInstruction: string
  timesPerDay: number
  duration: string
  adviceNote: string
}

// ==========================================
// 2. SINGLETON STORE STATE
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
// 3. CART & SAFELY ENGINE INTERACTIVE STATE
// ==========================================

const currentRole = ref<'admin' | 'pharmacist' | 'manager'>('pharmacist')
const selectedPatientId = ref<number>(1)
const selectedMedicineId = ref<number>(1)
const qtyToAdd = ref<number>(10)
const dosageText = ref<string>('Ngày uống 2 lần, mỗi lần 1 viên sau ăn')
const timesPerDayInput = ref<number>(2)
const durationInput = ref<string>('5 ngày')
const adviceNoteInput = ref<string>('')

const prescriptionCart = ref<CartItem[]>([])
const safetyWarnings = ref<Warning[]>([])
const hasCheckedSafety = ref(false)
const showSafetyResultsModal = ref(false)
const finalDecision = ref<'Approved' | 'Denied' | 'Pending'>('Pending')
const warningDecisions = ref<Record<number, string>>({})

// ==========================================
// 4. THE STORE FUNCTIONS COMPOSABLE
// ==========================================

export function usePharmacyStore() {
  
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

    // Reset safety checker status when items modify
    hasCheckedSafety.value = false
    safetyWarnings.value = []
  }

  // Remove medicine from cart
  const removeFromCart = (index: number) => {
    prescriptionCart.value.splice(index, 1)
    hasCheckedSafety.value = false
    safetyWarnings.value = []
  }

  // Run Safety Check Engine
  const runSafetyCheck = () => {
    if (prescriptionCart.value.length === 0) return

    const generatedWarnings: Warning[] = []
    const patient = activePatient.value
    if (!patient) return
    const checkId = Math.floor(Math.random() * 1000) + 1

    // Extract ingredients from cart items
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

    // 1. Patient Allergies check
    const patientAllergiesData = patientAllergies.value.filter(pa => pa.PatientId === patient.PatientId)
    cartIngredients.forEach(cartIng => {
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

    // 2. Drug Interactions check
    for (let i = 0; i < cartIngredients.length; i++) {
      for (let j = i + 1; j < cartIngredients.length; j++) {
        const ingA = cartIngredients[i]
        const ingB = cartIngredients[j]
        if (!ingA || !ingB) continue

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

    // 3. Contraindications check
    const patientDiseasesData = patientDiseases.value.filter(pd => pd.PatientId === patient.PatientId)
    cartIngredients.forEach(cartIng => {
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

      if (patient.IsPregnant) {
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

    // 4. Prescription Required check
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

  // Acknowledge single safety warning
  const acknowledgeWarning = (warningId: number, decisionText: string) => {
    const w = safetyWarnings.value.find(warn => warn.WarningId === warningId)
    if (w) {
      w.IsAcknowledged = true
      w.AcknowledgedBy = 2
      w.AcknowledgedAt = new Date().toISOString().replace('T', ' ').substring(0, 19)
      w.Decision = decisionText
    }

    if (safetyWarnings.value.every(warn => warn.IsAcknowledged)) {
      finalDecision.value = 'Approved'
    }
  }

  // Cancel transaction
  const cancelPrescription = () => {
    finalDecision.value = 'Denied'
    showSafetyResultsModal.value = false

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

    prescriptionCart.value = []
    safetyWarnings.value = []
    hasCheckedSafety.value = false
  }

  // Complete checkout sales transaction
  const completePrescriptionSales = () => {
    showSafetyResultsModal.value = false
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

    prescriptionCart.value.forEach(item => {
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

    if (safetyWarnings.value.length > 0) {
      safetyWarnings.value.forEach(w => {
        warnings.value.unshift({
          ...w,
          SafetyCheckId: warnings.value.length + 1
        })
      })
    }

    prescriptionCart.value = []
    safetyWarnings.value = []
    hasCheckedSafety.value = false
    alert('Đã hoàn tất phiếu bán thuốc thành công và cập nhật lịch sử CSDL!')
  }

  return {
    // Database tables
    users,
    patients,
    drugGroups,
    activeIngredients,
    medicines,
    medicineIngredients,
    diseases,
    patientDiseases,
    patientAllergies,
    drugInteractions,
    contraindications,
    sales,
    saleDetails,
    warnings,

    // Form builder states
    currentRole,
    selectedPatientId,
    selectedMedicineId,
    qtyToAdd,
    dosageText,
    timesPerDayInput,
    durationInput,
    adviceNoteInput,
    prescriptionCart,

    // Safety checks engine state
    safetyWarnings,
    hasCheckedSafety,
    showSafetyResultsModal,
    finalDecision,
    warningDecisions,

    // Computeds & helpers
    calculateAge,
    activePatient,
    activePatientDiseasesList,
    activePatientAllergiesList,
    cartTotalAmount,

    // Methods
    addMedicineToCart,
    removeFromCart,
    runSafetyCheck,
    acknowledgeWarning,
    cancelPrescription,
    completePrescriptionSales
  }
}
