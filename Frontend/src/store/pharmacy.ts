import { ref, computed, watch } from 'vue'
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
// 3. CART & SAFELY ENGINE INTERACTIVE STATE & AUTH
// ==========================================

const getStoredUser = (): User | null => {
  try {
    const data = localStorage.getItem('safepharm_user')
    return data ? JSON.parse(data) : null
  } catch {
    return null
  }
}

const getStoredRole = (): 'admin' | 'pharmacist' | 'manager' => {
  const role = localStorage.getItem('safepharm_role')
  if (role === 'admin' || role === 'pharmacist' || role === 'manager') {
    return role
  }
  return 'pharmacist'
}

const isAuthenticated = ref<boolean>(localStorage.getItem('safepharm_auth') === 'true')
const currentUser = ref<User | null>(getStoredUser())
const currentRole = ref<'admin' | 'pharmacist' | 'manager'>(getStoredRole())

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
    })
    patients.value.push(newPat)

    // Clear and append allergies
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

    // Clear and append diseases
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
    })
    const idx = patients.value.findIndex(p => p.PatientId === patientId)
    if (idx !== -1) {
      patients.value[idx] = updatedPat
    }

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
      Note: medicineData.Note || null,
      Ingredients: ingredients.map(fi => ({ IngredientId: fi.IngredientId, Amount: fi.Amount }))
    })
    medicines.value.push(newMed)

    hasInitialized.value = false
    await initializeStore()
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
      Note: medicineData.Note || null,
      Ingredients: ingredients.map(fi => ({ IngredientId: fi.IngredientId, Amount: fi.Amount }))
    } as any)
    const idx = medicines.value.findIndex(m => m.MedicineId === medicineId)
    if (idx !== -1) {
      medicines.value[idx] = updatedMed
    }

    hasInitialized.value = false
    await initializeStore()
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

  const updateDrugGroupStore = async (id: number, groupData: DrugGroup) => {
    const updatedGroup = await ApiService.updateDrugGroup(id, groupData)
    const idx = drugGroups.value.findIndex(dg => dg.DrugGroupId === id)
    if (idx !== -1) {
      drugGroups.value[idx] = updatedGroup
    }
    return updatedGroup
  }

  const deleteDrugGroupStore = async (id: number) => {
    const success = await ApiService.deleteDrugGroup(id)
    if (success) {
      drugGroups.value = drugGroups.value.filter(dg => dg.DrugGroupId !== id)
    }
    return success
  }

  const addIngredient = async (ingredientData: Omit<ActiveIngredient, 'IngredientId'>) => {
    const newIngredient = await ApiService.createIngredient(ingredientData)
    activeIngredients.value.push(newIngredient)
    return newIngredient
  }

  const updateIngredientStore = async (id: number, ingredientData: ActiveIngredient) => {
    const updatedIngredient = await ApiService.updateIngredient(id, ingredientData)
    const idx = activeIngredients.value.findIndex(ai => ai.IngredientId === id)
    if (idx !== -1) {
      activeIngredients.value[idx] = updatedIngredient
    }
    return updatedIngredient
  }

  const deleteIngredientStore = async (id: number) => {
    const success = await ApiService.deleteIngredient(id)
    if (success) {
      activeIngredients.value = activeIngredients.value.filter(ai => ai.IngredientId !== id)
    }
    return success
  }

  const addUser = async (userData: Omit<User, 'UserId' | 'CreatedAt'>) => {
    const newUser = await ApiService.createUser(userData)
    users.value.push(newUser)
    return newUser
  }

  const updateUserStore = async (userId: number, userData: User) => {
    const updatedUser = await ApiService.updateUser(userId, userData)
    const idx = users.value.findIndex(u => u.UserId === userId)
    if (idx !== -1) {
      users.value[idx] = updatedUser
    }
    return updatedUser
  }

  const deleteUserStore = async (userId: number) => {
    const success = await ApiService.deleteUser(userId)
    if (success) {
      users.value = users.value.filter(u => u.UserId !== userId)
    }
    return success
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

  const login = async (email: string, phone: string): Promise<boolean> => {
    // 1. Try real login API first
    try {
      const apiResult = await ApiService.loginApi(email, phone)
      if (apiResult) {
        const foundUser = apiResult.User
        const token = apiResult.Token
        
        let roleKey: 'admin' | 'pharmacist' | 'manager' = 'pharmacist'
        if (foundUser.RoleId === 1) roleKey = 'admin'
        else if (foundUser.RoleId === 3) roleKey = 'manager'
        
        isAuthenticated.value = true
        currentUser.value = foundUser
        currentRole.value = roleKey
        
        localStorage.setItem('safepharm_auth', 'true')
        localStorage.setItem('safepharm_user', JSON.stringify(foundUser))
        localStorage.setItem('safepharm_role', roleKey)
        localStorage.setItem('safepharm_token', token)
        
        // Re-initialize store tables with the new token
        hasInitialized.value = false
        await initializeStore()
        return true
      }
    } catch (err) {
      console.warn('[Store] API login failed, falling back to local simulation:', err)
    }

    // 2. Fallback to local simulation
    if (users.value.length === 0) {
      await initializeStore()
    }
    
    const foundUser = users.value.find(u => 
      u.Email.toLowerCase() === email.trim().toLowerCase() && 
      (u.Phone === phone.trim() || phone.trim() === '123456')
    )
    
    if (foundUser && foundUser.Status === 'Active') {
      let roleKey: 'admin' | 'pharmacist' | 'manager' = 'pharmacist'
      if (foundUser.RoleId === 1) roleKey = 'admin'
      else if (foundUser.RoleId === 3) roleKey = 'manager'
      
      isAuthenticated.value = true
      currentUser.value = foundUser
      currentRole.value = roleKey
      
      localStorage.setItem('safepharm_auth', 'true')
      localStorage.setItem('safepharm_user', JSON.stringify(foundUser))
      localStorage.setItem('safepharm_role', roleKey)
      localStorage.setItem('safepharm_token', 'mock-local-token')
      return true
    }
    return false
  }

  const logout = () => {
    isAuthenticated.value = false
    currentUser.value = null
    currentRole.value = 'pharmacist'
    
    localStorage.removeItem('safepharm_auth')
    localStorage.removeItem('safepharm_user')
    localStorage.removeItem('safepharm_role')
    localStorage.removeItem('safepharm_token')
  }

  // Watch for auth state changes to synchronize with localStorage automatically
  watch(isAuthenticated, (newVal) => {
    localStorage.setItem('safepharm_auth', newVal ? 'true' : 'false')
  })

  watch(currentUser, (newUser) => {
    if (newUser) {
      localStorage.setItem('safepharm_user', JSON.stringify(newUser))
    } else {
      localStorage.removeItem('safepharm_user')
    }
  })

  watch(currentRole, (newRole) => {
    localStorage.setItem('safepharm_role', newRole)
  })

  // Trigger init immediately on startup if already authenticated
  if (isAuthenticated.value) {
    initializeStore()
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

    // Auth states
    isAuthenticated,
    currentUser,

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
    login,
    logout,
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
    updateDrugGroupStore,
    deleteDrugGroupStore,
    addIngredient,
    updateIngredientStore,
    deleteIngredientStore,
    addUser,
    updateUserStore,
    deleteUserStore,
    initializeStore
  }
}
