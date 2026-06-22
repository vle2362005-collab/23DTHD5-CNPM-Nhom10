import { ref, computed } from 'vue'
import { ApiService } from '../services/api'

// ==========================================
// 1. TYPES matching PharmacySafetyDB
// ==========================================

export interface User {
  UserId: number
  RoleId: number
  FullName: string
  Email: string
  Phone: string | null
  Status: string
  CreatedAt: string
}

export interface Patient {
  PatientId: number
  FullName: string
  Phone: string | null
  Gender: string | null
  DateOfBirth: string
  WeightKg: number | null
  Address: string | null
  IsPregnant: boolean
  IsBreastfeeding: boolean
  Note: string | null
  CreatedAt: string
}

export interface DrugGroup {
  DrugGroupId: number
  GroupName: string
  Description: string | null
}

export interface ActiveIngredient {
  IngredientId: number
  IngredientName: string
  Description: string | null
}

export interface Medicine {
  MedicineId: number
  DrugGroupId: number | null
  MedicineName: string
  Strength: string | null
  DosageForm: string | null
  Unit: string | null
  Price: number
  RequiresPrescription: boolean
  IsActive: boolean
  SideEffects: string | null
  Note: string | null
  CreatedAt: string
}

export interface MedicineIngredient {
  MedicineId: number
  IngredientId: number
  Amount: string | null
}

export interface Disease {
  DiseaseId: number
  DiseaseName: string
  Description: string | null
}

export interface PatientDisease {
  PatientDiseaseId: number
  PatientId: number
  DiseaseId: number
  Note: string | null
}

export interface PatientAllergy {
  AllergyId: number
  PatientId: number
  IngredientId: number | null
  MedicineId: number | null
  AllergyNote: string | null
  Severity: string | null // 'High' | 'Medium' | 'Low'
}

export interface DrugInteraction {
  InteractionId: number
  IngredientAId: number
  IngredientBId: number
  Severity: string
  Description: string | null
  Recommendation: string | null
}

export interface Contraindication {
  ContraindicationId: number
  MedicineId: number | null
  IngredientId: number | null
  DiseaseId: number | null
  ConditionType: string
  Severity: string
  Description: string | null
  Recommendation: string | null
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
  Note: string | null
}

export interface SaleDetail {
  SaleDetailId: number
  SaleId: number
  MedicineId: number
  Quantity: number
  UnitPrice: number
  DosageInstruction: string
  TimesPerDay: number
  Duration: string | null
  AdviceNote: string | null
}

export interface Warning {
  WarningId: number
  SafetyCheckId: number
  PatientId: number
  MedicineId: number | null
  WarningType: string
  Severity: string
  Message: string
  Recommendation: string | null
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
// 2. SINGLETON STORE STATE (Initially empty, loaded via API)
// ==========================================

const users = ref<User[]>([])
const patients = ref<Patient[]>([])
const drugGroups = ref<DrugGroup[]>([])
const activeIngredients = ref<ActiveIngredient[]>([])
const medicines = ref<Medicine[]>([])
const medicineIngredients = ref<MedicineIngredient[]>([])
const diseases = ref<Disease[]>([])
const patientDiseases = ref<PatientDisease[]>([])
const patientAllergies = ref<PatientAllergy[]>([])
const drugInteractions = ref<DrugInteraction[]>([])
const contraindications = ref<Contraindication[]>([])
const sales = ref<Sale[]>([])
const saleDetails = ref<SaleDetail[]>([])
const warnings = ref<Warning[]>([])

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
    if (!dobString) return 0
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
      return dis ? { name: dis.DiseaseName, note: pd.Note || '' } : null
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
      return { target: targetName, severity: pa.Severity || 'Nghiêm trọng', note: pa.AllergyNote || '' }
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

  // Run Safety Check Engine via API
  const runSafetyCheck = async () => {
    if (prescriptionCart.value.length === 0) return

    const patient = activePatient.value
    if (!patient) return

    const cartItemsDto = prescriptionCart.value.map(item => ({
      MedicineId: item.medicine.MedicineId,
      Quantity: item.quantity,
      DosageInstruction: item.dosageInstruction,
      TimesPerDay: item.timesPerDay,
      Duration: item.duration,
      AdviceNote: item.adviceNote
    }))

    try {
      const res = await ApiService.runSafetyCheck(patient.PatientId, cartItemsDto)
      safetyWarnings.value = res.warnings
      hasCheckedSafety.value = true
      showSafetyResultsModal.value = true
      finalDecision.value = res.result as 'Approved' | 'Denied' | 'Pending'
    } catch (e) {
      console.error('[Safety Check API error]', e)
    }
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
  const cancelPrescription = async () => {
    finalDecision.value = 'Denied'
    showSafetyResultsModal.value = false

    try {
      const newSale = await ApiService.createPrescriptionSale(
        selectedPatientId.value,
        [],
        'Denied',
        [],
        'Bị từ chối do cảnh báo an toàn nghiêm trọng.'
      )
      sales.value.unshift(newSale)
    } catch (e) {
      console.error('[Cancel Sale API error]', e)
    }

    prescriptionCart.value = []
    safetyWarnings.value = []
    hasCheckedSafety.value = false
  }

  // Complete checkout sales transaction via API
  const completePrescriptionSales = async () => {
    showSafetyResultsModal.value = false
    const patient = activePatient.value
    if (!patient) return

    const cartItemsDto = prescriptionCart.value.map(item => ({
      MedicineId: item.medicine.MedicineId,
      Quantity: item.quantity,
      DosageInstruction: item.dosageInstruction,
      TimesPerDay: item.timesPerDay,
      Duration: item.duration,
      AdviceNote: item.adviceNote
    }))

    try {
      const newSale = await ApiService.createPrescriptionSale(
        patient.PatientId,
        cartItemsDto,
        finalDecision.value,
        safetyWarnings.value,
        finalDecision.value === 'Approved' && safetyWarnings.value.length > 0
          ? 'Bán sau khi duyệt cảnh báo.'
          : 'Bán an toàn thông thường.'
      )
      
      // Update local store sales list
      sales.value.unshift(newSale)
      
      // Reload warnings & details to keep reactive views in sync
      const updatedSales = await ApiService.getSales()
      sales.value = updatedSales
      
      prescriptionCart.value = []
      safetyWarnings.value = []
      hasCheckedSafety.value = false
      alert('Đã hoàn tất phiếu bán thuốc thành công và cập nhật lịch sử CSDL!')
    } catch (e) {
      console.error('[Complete Sale API error]', e)
    }
  }

  // ==========================================
  // 5. CRUD ACTION METHODS (For UI integration)
  // ==========================================

  const addPatient = async (
    patientData: Omit<Patient, 'PatientId' | 'CreatedAt'>, 
    allergies: { isIngredient: boolean; targetId: number; severity: string; note: string }[], 
    diseasesList: { diseaseId: number; note: string }[]
  ) => {
    const newPat = await ApiService.createPatient({
      ...patientData,
      Phone: patientData.Phone || null,
      Gender: patientData.Gender || null,
      Address: patientData.Address || null,
      Note: patientData.Note || null
    }, allergies, diseasesList)
    patients.value.push(newPat)

    // Clear and append allergies locally for immediate reactivity
    patientAllergies.value = patientAllergies.value.filter(pa => pa.PatientId !== newPat.PatientId)
    allergies.forEach(fa => {
      patientAllergies.value.push({
        AllergyId: Math.floor(Math.random() * 100000),
        PatientId: newPat.PatientId,
        IngredientId: fa.isIngredient ? fa.targetId : null,
        MedicineId: !fa.isIngredient ? fa.targetId : null,
        AllergyNote: fa.note,
        Severity: fa.severity
      })
    })

    // Clear and append diseases locally for immediate reactivity
    patientDiseases.value = patientDiseases.value.filter(pd => pd.PatientId !== newPat.PatientId)
    diseasesList.forEach(fd => {
      patientDiseases.value.push({
        PatientDiseaseId: Math.floor(Math.random() * 100000),
        PatientId: newPat.PatientId,
        DiseaseId: fd.diseaseId,
        Note: fd.note
      })
    })

    return newPat
  }

  const updatePatient = async (
    patientId: number, 
    patientData: Patient, 
    allergies: { isIngredient: boolean; targetId: number; severity: string; note: string }[], 
    diseasesList: { diseaseId: number; note: string }[]
  ) => {
    const updatedPat = await ApiService.updatePatient(patientId, {
      ...patientData,
      Phone: patientData.Phone || null,
      Gender: patientData.Gender || null,
      Address: patientData.Address || null,
      Note: patientData.Note || null
    }, allergies, diseasesList)
    const idx = patients.value.findIndex(p => p.PatientId === patientId)
    if (idx !== -1) {
      patients.value[idx] = updatedPat
    }

    // Clear and append allergies locally for immediate reactivity
    patientAllergies.value = patientAllergies.value.filter(pa => pa.PatientId !== patientId)
    allergies.forEach(fa => {
      patientAllergies.value.push({
        AllergyId: Math.floor(Math.random() * 100000),
        PatientId: patientId,
        IngredientId: fa.isIngredient ? fa.targetId : null,
        MedicineId: !fa.isIngredient ? fa.targetId : null,
        AllergyNote: fa.note,
        Severity: fa.severity
      })
    })

    // Clear and append diseases locally for immediate reactivity
    patientDiseases.value = patientDiseases.value.filter(pd => pd.PatientId !== patientId)
    diseasesList.forEach(fd => {
      patientDiseases.value.push({
        PatientDiseaseId: Math.floor(Math.random() * 100000),
        PatientId: patientId,
        DiseaseId: fd.diseaseId,
        Note: fd.note
      })
    })
  }

  const deletePatient = async (patientId: number) => {
    await ApiService.deletePatient(patientId)
    patients.value = patients.value.filter(p => p.PatientId !== patientId)
    patientAllergies.value = patientAllergies.value.filter(pa => pa.PatientId !== patientId)
    patientDiseases.value = patientDiseases.value.filter(pd => pd.PatientId !== patientId)
  }

  const addMedicine = async (
    medicineData: Omit<Medicine, 'MedicineId' | 'CreatedAt'>, 
    ingredients: { IngredientId: number; Amount: string }[]
  ) => {
    const newMed = await ApiService.createMedicine({
      ...medicineData,
      Strength: medicineData.Strength || null,
      DosageForm: medicineData.DosageForm || null,
      Unit: medicineData.Unit || null,
      Note: medicineData.Note || null
    })
    medicines.value.push(newMed)

    ingredients.forEach(fi => {
      medicineIngredients.value.push({
        MedicineId: newMed.MedicineId,
        IngredientId: fi.IngredientId,
        Amount: fi.Amount
      })
    })
    return newMed
  }

  const updateMedicine = async (
    medicineId: number, 
    medicineData: Medicine, 
    ingredients: { IngredientId: number; Amount: string }[]
  ) => {
    const updatedMed = await ApiService.updateMedicine(medicineId, {
      ...medicineData,
      Strength: medicineData.Strength || null,
      DosageForm: medicineData.DosageForm || null,
      Unit: medicineData.Unit || null,
      Note: medicineData.Note || null
    })
    const idx = medicines.value.findIndex(m => m.MedicineId === medicineId)
    if (idx !== -1) {
      medicines.value[idx] = updatedMed
    }

    medicineIngredients.value = medicineIngredients.value.filter(mi => mi.MedicineId !== medicineId)
    ingredients.forEach(fi => {
      medicineIngredients.value.push({
        MedicineId: medicineId,
        IngredientId: fi.IngredientId,
        Amount: fi.Amount
      })
    })
  }

  const deleteMedicine = async (medicineId: number) => {
    await ApiService.deleteMedicine(medicineId)
    medicines.value = medicines.value.filter(m => m.MedicineId !== medicineId)
    medicineIngredients.value = medicineIngredients.value.filter(mi => mi.MedicineId !== medicineId)
  }

  const addDrugGroup = async (groupData: Omit<DrugGroup, 'DrugGroupId'>) => {
    const newGroup = await ApiService.createDrugGroup(groupData)
    drugGroups.value.push(newGroup)
    return newGroup
  }

  const updateDrugGroup = async (groupId: number, groupData: DrugGroup) => {
    const updatedGroup = await ApiService.updateDrugGroup(groupId, groupData)
    const idx = drugGroups.value.findIndex(dg => dg.DrugGroupId === groupId)
    if (idx !== -1) {
      drugGroups.value[idx] = updatedGroup
    }
  }

  const deleteDrugGroup = async (groupId: number) => {
    await ApiService.deleteDrugGroup(groupId)
    drugGroups.value = drugGroups.value.filter(dg => dg.DrugGroupId !== groupId)
  }

  const addActiveIngredient = async (ingredientData: Omit<ActiveIngredient, 'IngredientId'>) => {
    const newIngredient = await ApiService.createIngredient(ingredientData)
    activeIngredients.value.push(newIngredient)
    return newIngredient
  }

  const updateActiveIngredient = async (ingredientId: number, ingredientData: ActiveIngredient) => {
    const updatedIngredient = await ApiService.updateIngredient(ingredientId, ingredientData)
    const idx = activeIngredients.value.findIndex(ai => ai.IngredientId === ingredientId)
    if (idx !== -1) {
      activeIngredients.value[idx] = updatedIngredient
    }
  }

  const deleteActiveIngredient = async (ingredientId: number) => {
    await ApiService.deleteIngredient(ingredientId)
    activeIngredients.value = activeIngredients.value.filter(ai => ai.IngredientId !== ingredientId)
  }

  const addContraindication = async (contraData: Omit<Contraindication, 'ContraindicationId'>) => {
    const newContra = await ApiService.createContraindication(contraData)
    contraindications.value.push(newContra)
    return newContra
  }

  const updateContraindication = async (contraId: number, contraData: Contraindication) => {
    const updatedContra = await ApiService.updateContraindication(contraId, contraData)
    const idx = contraindications.value.findIndex(c => c.ContraindicationId === contraId)
    if (idx !== -1) {
      contraindications.value[idx] = updatedContra
    }
  }

  const deleteContraindication = async (contraId: number) => {
    await ApiService.deleteContraindication(contraId)
    contraindications.value = contraindications.value.filter(c => c.ContraindicationId !== contraId)
  }

  const hasInitialized = ref(false)

  const initializeStore = async () => {
    if (hasInitialized.value) return
    hasInitialized.value = true
    try {
      await ApiService.init()
      users.value = await ApiService.getUsers()
      patients.value = await ApiService.getPatients()
      drugGroups.value = await ApiService.getDrugGroups()
      activeIngredients.value = await ApiService.getIngredients()
      medicines.value = await ApiService.getMedicines()
      medicineIngredients.value = await ApiService.getMedicineIngredients()
      diseases.value = await ApiService.getDiseases()
      patientDiseases.value = await ApiService.getPatientDiseases()
      patientAllergies.value = await ApiService.getPatientAllergies()
      drugInteractions.value = await ApiService.getDrugInteractions()
      contraindications.value = await ApiService.getContraindications()
      sales.value = await ApiService.getSales()
    } catch (e) {
      console.error('[Store] Failed to initialize store from API:', e)
    }
  }

  // Trigger init immediately on startup
  initializeStore()

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
    completePrescriptionSales,

    // New API integrations
    addPatient,
    updatePatient,
    deletePatient,
    addMedicine,
    updateMedicine,
    deleteMedicine,
    addDrugGroup,
    updateDrugGroup,
    deleteDrugGroup,
    addActiveIngredient,
    updateActiveIngredient,
    deleteActiveIngredient,
    addContraindication,
    updateContraindication,
    deleteContraindication,
    initializeStore
  }
}
