import type { 
  User, Patient, DrugGroup, ActiveIngredient, Medicine, 
  MedicineIngredient, Disease, PatientDisease, PatientAllergy, 
  DrugInteraction, Contraindication, Sale, Warning 
} from '../store/pharmacy'

const BASE_URL = 'http://localhost:5097/api'

// Local mock databases for Fallback (Offline Mode)
const localMocks = {
  users: [
    { UserId: 1, RoleId: 1, FullName: 'Nguyễn Minh Quân', Email: 'admin@gmail.com', Phone: '0900000000', Status: 'Active', CreatedAt: '2026-01-10' },
    { UserId: 2, RoleId: 2, FullName: 'Ds. Trần Thị Mai', Email: 'duocsi@gmail.com', Phone: '0911111111', Status: 'Active', CreatedAt: '2026-01-15' },
    { UserId: 3, RoleId: 3, FullName: 'Ds. Phạm Thanh Sơn', Email: 'quanly@gmail.com', Phone: '0922222222', Status: 'Active', CreatedAt: '2026-01-20' }
  ] as User[],

  patients: [
    { PatientId: 1, FullName: 'Nguyen Van A', Phone: '0988888888', Gender: 'Nam', DateOfBirth: '1990-05-12', WeightKg: 65, Address: 'Gia Lai', IsPregnant: false, IsBreastfeeding: false, Note: 'Co benh nen cao huyet ap', CreatedAt: '2026-06-20' },
    { PatientId: 2, FullName: 'Tran Thi B', Phone: '0977777777', Gender: 'Nu', DateOfBirth: '1985-10-20', WeightKg: 52, Address: 'Gia Lai', IsPregnant: false, IsBreastfeeding: false, Note: 'Di ung thuoc giam dau', CreatedAt: '2026-06-20' }
  ] as Patient[],

  drugGroups: [
    { DrugGroupId: 1, GroupName: 'Thuoc giam dau ha sot', Description: 'Nhom thuoc dung de giam dau va ha sot' },
    { DrugGroupId: 2, GroupName: 'Khang sinh', Description: 'Nhom thuoc dieu tri nhiem khuan' },
    { DrugGroupId: 3, GroupName: 'Khang viem NSAID', Description: 'Nhom thuoc giam dau khang viem' }
  ] as DrugGroup[],

  activeIngredients: [
    { IngredientId: 1, IngredientName: 'Paracetamol', Description: 'Hoat chat giam dau ha sot' },
    { IngredientId: 2, IngredientName: 'Amoxicillin', Description: 'Hoat chat khang sinh nhom Penicillin' },
    { IngredientId: 3, IngredientName: 'Ibuprofen', Description: 'Hoat chat giam dau khang viem NSAID' }
  ] as ActiveIngredient[],

  medicines: [
    { MedicineId: 1, DrugGroupId: 1, MedicineName: 'Paracetamol 500mg', Strength: '500mg', DosageForm: 'Vien nen', Unit: 'Vien', Price: 2000, RequiresPrescription: false, IsActive: true, Note: 'Thuoc ha sot giam dau', CreatedAt: '2026-06-20' },
    { MedicineId: 2, DrugGroupId: 2, MedicineName: 'Amoxicillin 500mg', Strength: '500mg', DosageForm: 'Vien nang', Unit: 'Vien', Price: 3000, RequiresPrescription: true, IsActive: true, Note: 'Khang sinh can don', CreatedAt: '2026-06-20' },
    { MedicineId: 3, DrugGroupId: 3, MedicineName: 'Ibuprofen 400mg', Strength: '400mg', DosageForm: 'Vien nen', Unit: 'Vien', Price: 2500, RequiresPrescription: false, IsActive: true, Note: 'Giam dau khang viem', CreatedAt: '2026-06-20' }
  ] as Medicine[],

  medicineIngredients: [
    { MedicineId: 1, IngredientId: 1, Amount: '500mg' },
    { MedicineId: 2, IngredientId: 2, Amount: '500mg' },
    { MedicineId: 3, IngredientId: 3, Amount: '400mg' }
  ] as MedicineIngredient[],

  diseases: [
    { DiseaseId: 1, DiseaseName: 'Cao huyet ap', Description: 'Benh tang huyet ap' },
    { DiseaseId: 2, DiseaseName: 'Suy than', Description: 'Benh nhan suy giam chuc nang than' },
    { DiseaseId: 3, DiseaseName: 'Viem loet da day', Description: 'Benh ly da day' }
  ] as Disease[],

  patientDiseases: [
    { PatientDiseaseId: 1, PatientId: 1, DiseaseId: 1, Note: 'Benh nhan co tien su cao huyet ap' }
  ] as PatientDisease[],

  patientAllergies: [
    { AllergyId: 1, PatientId: 2, IngredientId: 3, MedicineId: 3, AllergyNote: 'Di ung voi Ibuprofen', Severity: 'High' }
  ] as PatientAllergy[],

  drugInteractions: [
    {
      InteractionId: 1,
      IngredientAId: 2,
      IngredientBId: 3,
      Severity: 'Trung bình',
      Description: 'Amoxicillin va Ibuprofen can than trong khi su dung chung.',
      Recommendation: 'Can tu van va theo doi trieu chung bat thuong.'
    }
  ] as DrugInteraction[],

  contraindications: [
    {
      ContraindicationId: 1,
      MedicineId: 3,
      IngredientId: 3,
      DiseaseId: 3,
      ConditionType: 'Disease',
      Severity: 'Nghiêm trọng',
      Description: 'Ibuprofen khong phu hop voi benh nhan viem loet da day.',
      Recommendation: 'Can doi sang thuoc khac an toan hon.'
    }
  ] as Contraindication[],

  sales: [
    { SaleId: 1, PatientId: 1, PharmacistId: 2, PrescriptionId: 1, TotalAmount: 7000, FinalDecision: 'Approved', Status: 'Completed', SaleDate: '2026-06-21 14:23', Note: 'Phieu ban thuoc demo' }
  ] as Sale[],

  warnings: [] as Warning[]
}

// Check server status
let isBackendOnline = true

const checkServerStatus = async () => {
  try {
    const res = await fetch(`${BASE_URL}/patients`, { method: 'GET', signal: AbortSignal.timeout(1000) })
    isBackendOnline = res.ok
  } catch {
    isBackendOnline = false
    console.warn('[API Service] Backend server offline. Operating in Standalone/Mock Mode.')
  }
}

// Request wrappers and headers helper
const getHeaders = (hasBody = false): HeadersInit => {
  const headers: Record<string, string> = {}
  if (hasBody) {
    headers['Content-Type'] = 'application/json'
  }
  const token = localStorage.getItem('safepharm_token')
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }
  return headers
}

async function apiGet<T>(endpoint: string, fallbackData: T): Promise<T> {
  if (!isBackendOnline) return fallbackData
  try {
    const res = await fetch(`${BASE_URL}${endpoint}`, {
      headers: getHeaders()
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    return await res.json() as T
  } catch (err) {
    console.error(`[API Service] Failed to GET ${endpoint}, using fallback mock data:`, err)
    return fallbackData
  }
}

async function apiPost<T, R = T>(endpoint: string, body: R, fallbackAction: () => T): Promise<T> {
  if (!isBackendOnline) return fallbackAction()
  try {
    const res = await fetch(`${BASE_URL}${endpoint}`, {
      method: 'POST',
      headers: getHeaders(true),
      body: JSON.stringify(body)
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    return await res.json() as T
  } catch (err) {
    console.error(`[API Service] Failed to POST ${endpoint}, running fallback mock action:`, err)
    return fallbackAction()
  }
}

async function apiPut<T, R = T>(endpoint: string, body: R, fallbackAction: () => T): Promise<T> {
  if (!isBackendOnline) return fallbackAction()
  try {
    const res = await fetch(`${BASE_URL}${endpoint}`, {
      method: 'PUT',
      headers: getHeaders(true),
      body: JSON.stringify(body)
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    return await res.json() as T
  } catch (err) {
    console.error(`[API Service] Failed to PUT ${endpoint}, running fallback mock action:`, err)
    return fallbackAction()
  }
}

async function apiDelete(endpoint: string, fallbackAction: () => boolean): Promise<boolean> {
  if (!isBackendOnline) return fallbackAction()
  try {
    const res = await fetch(`${BASE_URL}${endpoint}`, { 
      method: 'DELETE',
      headers: getHeaders()
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    return true
  } catch (err) {
    console.error(`[API Service] Failed to DELETE ${endpoint}, running fallback mock action:`, err)
    return fallbackAction()
  }
}

// ====================================================
// EXPORTED SERVICES
// ====================================================

export const ApiService = {
  async init() {
    await checkServerStatus()
  },

  // Auth API
  async loginApi(email: string, pin: string): Promise<{ Token: string; User: User } | null> {
    try {
      const res = await fetch(`${BASE_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Email: email, Pin: pin })
      })
      if (!res.ok) return null
      return await res.json() as { Token: string; User: User }
    } catch (err) {
      console.error('[API Service] loginApi error:', err)
      return null
    }
  },

  // Patients
  async getPatients(search?: string): Promise<Patient[]> {
    const url = search ? `/patients?search=${encodeURIComponent(search)}` : '/patients'
    return apiGet<Patient[]>(url, localMocks.patients.filter(p => {
      if (!search) return true
      const lower = search.toLowerCase().trim()
      return p.FullName.toLowerCase().includes(lower) || (p.Phone && p.Phone.includes(lower))
    }))
  },
  async createPatient(
    patient: Omit<Patient, 'PatientId' | 'CreatedAt'>,
    allergies: { isIngredient: boolean; targetId: number; severity: string; note: string }[],
    diseasesList: { diseaseId: number; note: string }[]
  ): Promise<Patient> {
    const payload = {
      Patient: patient,
      Allergies: allergies.map(a => ({
        IsIngredient: a.isIngredient,
        TargetId: a.targetId,
        Severity: a.severity,
        Note: a.note
      })),
      Diseases: diseasesList.map(d => ({
        DiseaseId: d.diseaseId,
        Note: d.note
      }))
    }
    return apiPost<Patient, typeof payload>('/patients', payload, () => {
      const newId = localMocks.patients.length > 0 ? Math.max(...localMocks.patients.map(p => p.PatientId)) + 1 : 1
      const newPat: Patient = {
        ...patient,
        PatientId: newId,
        CreatedAt: new Date().toISOString().substring(0, 10)
      }
      localMocks.patients.push(newPat)
      allergies.forEach(a => {
        localMocks.patientAllergies.push({
          AllergyId: Math.floor(Math.random() * 100000),
          PatientId: newId,
          IngredientId: a.isIngredient ? a.targetId : null,
          MedicineId: !a.isIngredient ? a.targetId : null,
          AllergyNote: a.note,
          Severity: a.severity
        })
      })
      diseasesList.forEach(d => {
        localMocks.patientDiseases.push({
          PatientDiseaseId: Math.floor(Math.random() * 100000),
          PatientId: newId,
          DiseaseId: d.diseaseId,
          Note: d.note
        })
      })
      return newPat
    })
  },
  async updatePatient(
    id: number,
    patient: Patient,
    allergies: { isIngredient: boolean; targetId: number; severity: string; note: string }[],
    diseasesList: { diseaseId: number; note: string }[]
  ): Promise<Patient> {
    const payload = {
      Patient: patient,
      Allergies: allergies.map(a => ({
        IsIngredient: a.isIngredient,
        TargetId: a.targetId,
        Severity: a.severity,
        Note: a.note
      })),
      Diseases: diseasesList.map(d => ({
        DiseaseId: d.diseaseId,
        Note: d.note
      }))
    }
    return apiPut<Patient, typeof payload>(`/patients/${id}`, payload, () => {
      const idx = localMocks.patients.findIndex(p => p.PatientId == id)
      if (idx >= 0) localMocks.patients[idx] = patient
      localMocks.patientAllergies = localMocks.patientAllergies.filter(pa => pa.PatientId !== id)
      allergies.forEach(a => {
        localMocks.patientAllergies.push({
          AllergyId: Math.floor(Math.random() * 100000),
          PatientId: id,
          IngredientId: a.isIngredient ? a.targetId : null,
          MedicineId: !a.isIngredient ? a.targetId : null,
          AllergyNote: a.note,
          Severity: a.severity
        })
      })
      localMocks.patientDiseases = localMocks.patientDiseases.filter(pd => pd.PatientId !== id)
      diseasesList.forEach(d => {
        localMocks.patientDiseases.push({
          PatientDiseaseId: Math.floor(Math.random() * 100000),
          PatientId: id,
          DiseaseId: d.diseaseId,
          Note: d.note
        })
      })
      return patient
    })
  },
  async deletePatient(id: number): Promise<boolean> {
    return apiDelete(`/patients/${id}`, () => {
      const idx = localMocks.patients.findIndex(p => p.PatientId == id)
      if (idx >= 0) {
        localMocks.patients.splice(idx, 1)
        localMocks.patientAllergies = localMocks.patientAllergies.filter(pa => pa.PatientId !== id)
        localMocks.patientDiseases = localMocks.patientDiseases.filter(pd => pd.PatientId !== id)
        return true
      }
      return false
    })
  },

  // Medicines
  async getMedicines(): Promise<Medicine[]> {
    return apiGet<Medicine[]>('/medicines', localMocks.medicines)
  },
  async createMedicine(medicine: Omit<Medicine, 'MedicineId' | 'CreatedAt'> & { Ingredients?: { IngredientId: number; Amount: string }[] }): Promise<Medicine> {
    return apiPost<Medicine, typeof medicine>('/medicines', medicine, () => {
      const newId = localMocks.medicines.length > 0 ? Math.max(...localMocks.medicines.map(m => m.MedicineId)) + 1 : 1
      const newMed: Medicine = {
        ...medicine,
        MedicineId: newId,
        CreatedAt: new Date().toISOString().substring(0, 10)
      }
      localMocks.medicines.push(newMed)
      if (medicine.Ingredients) {
        medicine.Ingredients.forEach(ing => {
          localMocks.medicineIngredients.push({
            MedicineId: newId,
            IngredientId: ing.IngredientId,
            Amount: ing.Amount
          })
        })
      }
      return newMed
    })
  },
  async updateMedicine(id: number, medicine: Medicine & { Ingredients?: { IngredientId: number; Amount: string }[] }): Promise<Medicine> {
    return apiPut<Medicine, typeof medicine>(`/medicines/${id}`, medicine, () => {
      const idx = localMocks.medicines.findIndex(m => m.MedicineId == id)
      if (idx >= 0) localMocks.medicines[idx] = medicine
      if (medicine.Ingredients) {
        localMocks.medicineIngredients = localMocks.medicineIngredients.filter(mi => mi.MedicineId !== id)
        medicine.Ingredients.forEach(ing => {
          localMocks.medicineIngredients.push({
            MedicineId: id,
            IngredientId: ing.IngredientId,
            Amount: ing.Amount
          })
        })
      }
      return medicine
    })
  },
  async deleteMedicine(id: number): Promise<boolean> {
    return apiDelete(`/medicines/${id}`, () => {
      const idx = localMocks.medicines.findIndex(m => m.MedicineId == id)
      if (idx >= 0) {
        localMocks.medicines.splice(idx, 1)
        return true
      }
      return false
    })
  },

  // Metadata
  async getUsers(): Promise<User[]> {
    return apiGet<User[]>('/users', localMocks.users)
  },
  async createUser(user: Omit<User, 'UserId' | 'CreatedAt'>): Promise<User> {
    return apiPost<User, Omit<User, 'UserId' | 'CreatedAt'>>('/users', user, () => {
      const newId = localMocks.users.length > 0 ? Math.max(...localMocks.users.map(u => u.UserId)) + 1 : 1
      const newUser: User = {
        ...user,
        UserId: newId,
        CreatedAt: new Date().toISOString().substring(0, 10)
      }
      localMocks.users.push(newUser)
      return newUser
    })
  },
  async updateUser(id: number, user: User): Promise<User> {
    return apiPut<User, User>(`/users/${id}`, user, () => {
      const idx = localMocks.users.findIndex(u => u.UserId === id)
      if (idx >= 0) localMocks.users[idx] = user
      return user
    })
  },
  async deleteUser(id: number): Promise<boolean> {
    return apiDelete(`/users/${id}`, () => {
      const idx = localMocks.users.findIndex(u => u.UserId === id)
      if (idx >= 0) {
        localMocks.users.splice(idx, 1)
        return true
      }
      return false
    })
  },
  async getDrugGroups(): Promise<DrugGroup[]> {
    return apiGet<DrugGroup[]>('/druggroups', localMocks.drugGroups)
  },
  async createDrugGroup(group: Omit<DrugGroup, 'DrugGroupId'>): Promise<DrugGroup> {
    return apiPost<DrugGroup, Omit<DrugGroup, 'DrugGroupId'>>('/druggroups', group, () => {
      const newId = localMocks.drugGroups.length > 0 ? Math.max(...localMocks.drugGroups.map(dg => dg.DrugGroupId)) + 1 : 1
      const newGroup: DrugGroup = {
        ...group,
        DrugGroupId: newId
      }
      localMocks.drugGroups.push(newGroup)
      return newGroup
    })
  },
  async updateDrugGroup(id: number, group: DrugGroup): Promise<DrugGroup> {
    return apiPut<DrugGroup, DrugGroup>(`/druggroups/${id}`, group, () => {
      const idx = localMocks.drugGroups.findIndex(dg => dg.DrugGroupId === id)
      if (idx >= 0) localMocks.drugGroups[idx] = group
      return group
    })
  },
  async deleteDrugGroup(id: number): Promise<boolean> {
    return apiDelete(`/druggroups/${id}`, () => {
      const idx = localMocks.drugGroups.findIndex(dg => dg.DrugGroupId === id)
      if (idx >= 0) {
        localMocks.drugGroups.splice(idx, 1)
        return true
      }
      return false
    })
  },
  async getIngredients(): Promise<ActiveIngredient[]> {
    return apiGet<ActiveIngredient[]>('/ingredients', localMocks.activeIngredients)
  },
  async createIngredient(ingredient: Omit<ActiveIngredient, 'IngredientId'>): Promise<ActiveIngredient> {
    return apiPost<ActiveIngredient, Omit<ActiveIngredient, 'IngredientId'>>('/ingredients', ingredient, () => {
      const newId = localMocks.activeIngredients.length > 0 ? Math.max(...localMocks.activeIngredients.map(ai => ai.IngredientId)) + 1 : 1
      const newIngredient: ActiveIngredient = {
        ...ingredient,
        IngredientId: newId
      }
      localMocks.activeIngredients.push(newIngredient)
      return newIngredient
    })
  },
  async updateIngredient(id: number, ingredient: ActiveIngredient): Promise<ActiveIngredient> {
    return apiPut<ActiveIngredient, ActiveIngredient>(`/ingredients/${id}`, ingredient, () => {
      const idx = localMocks.activeIngredients.findIndex(ai => ai.IngredientId === id)
      if (idx >= 0) localMocks.activeIngredients[idx] = ingredient
      return ingredient
    })
  },
  async deleteIngredient(id: number): Promise<boolean> {
    return apiDelete(`/ingredients/${id}`, () => {
      const idx = localMocks.activeIngredients.findIndex(ai => ai.IngredientId === id)
      if (idx >= 0) {
        localMocks.activeIngredients.splice(idx, 1)
        return true
      }
      return false
    })
  },
  async getMedicineIngredients(): Promise<MedicineIngredient[]> {
    return apiGet<MedicineIngredient[]>('/medicineingredients', localMocks.medicineIngredients)
  },
  async getDiseases(): Promise<Disease[]> {
    return apiGet<Disease[]>('/diseases', localMocks.diseases)
  },
  async createDisease(disease: Omit<Disease, 'DiseaseId'>): Promise<Disease> {
    return apiPost<Disease, Omit<Disease, 'DiseaseId'>>('/diseases', disease, () => {
      const newId = localMocks.diseases.length > 0 ? Math.max(...localMocks.diseases.map(d => d.DiseaseId)) + 1 : 1
      const newDis: Disease = {
        ...disease,
        DiseaseId: newId
      }
      localMocks.diseases.push(newDis)
      return newDis
    })
  },
  async updateDisease(id: number, disease: Disease): Promise<Disease> {
    return apiPut<Disease, Disease>(`/diseases/${id}`, disease, () => {
      const idx = localMocks.diseases.findIndex(d => d.DiseaseId === id)
      if (idx >= 0) localMocks.diseases[idx] = disease
      return disease
    })
  },
  async deleteDisease(id: number): Promise<boolean> {
    return apiDelete(`/diseases/${id}`, () => {
      const idx = localMocks.diseases.findIndex(d => d.DiseaseId === id)
      if (idx >= 0) {
        localMocks.diseases.splice(idx, 1)
        return true
      }
      return false
    })
  },
  async getPatientDiseases(): Promise<PatientDisease[]> {
    return apiGet<PatientDisease[]>('/patientdiseases', localMocks.patientDiseases)
  },
  async getPatientAllergies(): Promise<PatientAllergy[]> {
    return apiGet<PatientAllergy[]>('/patientallergies', localMocks.patientAllergies)
  },
  async createPatientAllergy(allergy: { PatientId: number; IsIngredient: boolean; TargetId: number; Severity: string; Note: string | null }): Promise<PatientAllergy> {
    return apiPost<PatientAllergy, typeof allergy>('/patientallergies', allergy, () => {
      const newId = Math.floor(Math.random() * 100000)
      const newAllergy: PatientAllergy = {
        AllergyId: newId,
        PatientId: allergy.PatientId,
        IngredientId: allergy.IsIngredient ? allergy.TargetId : null,
        MedicineId: !allergy.IsIngredient ? allergy.TargetId : null,
        AllergyNote: allergy.Note,
        Severity: allergy.Severity
      }
      localMocks.patientAllergies.push(newAllergy)
      return newAllergy
    })
  },
  async updatePatientAllergy(id: number, allergy: { Severity: string; AllergyNote: string | null }): Promise<PatientAllergy> {
    return apiPut<PatientAllergy, typeof allergy>(`/patientallergies/${id}`, allergy, () => {
      const existing = localMocks.patientAllergies.find(pa => pa.AllergyId === id)
      if (existing) {
        existing.Severity = allergy.Severity
        existing.AllergyNote = allergy.AllergyNote
        return existing
      }
      throw new Error("Mock allergy not found")
    })
  },
  async deletePatientAllergy(id: number): Promise<boolean> {
    return apiDelete(`/patientallergies/${id}`, () => {
      const idx = localMocks.patientAllergies.findIndex(pa => pa.AllergyId === id)
      if (idx >= 0) {
        localMocks.patientAllergies.splice(idx, 1)
        return true
      }
      return false
    })
  },
  async getDrugInteractions(): Promise<DrugInteraction[]> {
    return apiGet<DrugInteraction[]>('/druginteractions', localMocks.drugInteractions)
  },
  async createDrugInteraction(interaction: Omit<DrugInteraction, 'InteractionId'>): Promise<DrugInteraction> {
    return apiPost<DrugInteraction, Omit<DrugInteraction, 'InteractionId'>>('/druginteractions', interaction, () => {
      const newId = localMocks.drugInteractions.length > 0 ? Math.max(...localMocks.drugInteractions.map(di => di.InteractionId)) + 1 : 1
      const newDi: DrugInteraction = {
        ...interaction,
        InteractionId: newId
      }
      localMocks.drugInteractions.push(newDi)
      return newDi
    })
  },
  async updateDrugInteraction(id: number, interaction: DrugInteraction): Promise<DrugInteraction> {
    return apiPut<DrugInteraction, DrugInteraction>(`/druginteractions/${id}`, interaction, () => {
      const idx = localMocks.drugInteractions.findIndex(di => di.InteractionId === id)
      if (idx >= 0) localMocks.drugInteractions[idx] = interaction
      return interaction
    })
  },
  async deleteDrugInteraction(id: number): Promise<boolean> {
    return apiDelete(`/druginteractions/${id}`, () => {
      const idx = localMocks.drugInteractions.findIndex(di => di.InteractionId === id)
      if (idx >= 0) {
        localMocks.drugInteractions.splice(idx, 1)
        return true
      }
      return false
    })
  },
  async getContraindications(): Promise<Contraindication[]> {
    return apiGet<Contraindication[]>('/contraindications', localMocks.contraindications)
  },
  async createContraindication(contra: Omit<Contraindication, 'ContraindicationId'>): Promise<Contraindication> {
    return apiPost<Contraindication, Omit<Contraindication, 'ContraindicationId'>>('/contraindications', contra, () => {
      const newId = localMocks.contraindications.length > 0 ? Math.max(...localMocks.contraindications.map(c => c.ContraindicationId)) + 1 : 1
      const newContra: Contraindication = {
        ...contra,
        ContraindicationId: newId
      }
      localMocks.contraindications.push(newContra)
      return newContra
    })
  },
  async updateContraindication(id: number, contra: Contraindication): Promise<Contraindication> {
    return apiPut<Contraindication, Contraindication>(`/contraindications/${id}`, contra, () => {
      const idx = localMocks.contraindications.findIndex(c => c.ContraindicationId === id)
      if (idx >= 0) localMocks.contraindications[idx] = contra
      return contra
    })
  },
  async deleteContraindication(id: number): Promise<boolean> {
    return apiDelete(`/contraindications/${id}`, () => {
      const idx = localMocks.contraindications.findIndex(c => c.ContraindicationId === id)
      if (idx >= 0) {
        localMocks.contraindications.splice(idx, 1)
        return true
      }
      return false
    })
  },
  async getSales(): Promise<Sale[]> {
    return apiGet<Sale[]>('/sales', localMocks.sales)
  },

  // Clinical Safety check endpoint
  async runSafetyCheck(patientId: number, cartItems: { MedicineId: number; Quantity: number; DosageInstruction: string; TimesPerDay: number; Duration: string; AdviceNote: string }[]) {
    interface SafetyCheckResponse {
      warnings: Warning[]
      highestSeverity: string
      result: string
    }
    
    const requestDto = {
      PatientId: patientId,
      CartItems: cartItems.map(item => ({
        MedicineId: item.MedicineId,
        Quantity: item.Quantity,
        DosageInstruction: item.DosageInstruction,
        TimesPerDay: item.TimesPerDay,
        Duration: item.Duration,
        AdviceNote: item.AdviceNote
      }))
    }

    return apiPost<SafetyCheckResponse, typeof requestDto>('/safety-check', requestDto, () => {
      // Local fallback calculation logic (identical to backend)
      const generatedWarnings: Warning[] = []
      const patient = localMocks.patients.find(p => p.PatientId === patientId)
      if (!patient) return { warnings: [], highestSeverity: 'None', result: 'Approved' }

      const checkId = Math.floor(Math.random() * 1000) + 1

      // Ingredients in cart
      const cartIngredients: { medicineId: number; ingredientId: number; ingredientName: string }[] = []
      cartItems.forEach(item => {
        const ingredients = localMocks.medicineIngredients.filter(mi => mi.MedicineId === item.MedicineId)
        ingredients.forEach(mi => {
          const ingName = localMocks.activeIngredients.find(ai => ai.IngredientId === mi.IngredientId)?.IngredientName || ''
          cartIngredients.push({
            medicineId: item.MedicineId,
            ingredientId: mi.IngredientId,
            ingredientName: ingName
          })
        })
      })

      // 1. Allergies Check
      const patientAllergiesData = localMocks.patientAllergies.filter(pa => pa.PatientId === patientId)
      cartIngredients.forEach(cartIng => {
        const matchIng = patientAllergiesData.find(pa => pa.IngredientId === cartIng.ingredientId)
        if (matchIng) {
          const medName = localMocks.medicines.find(m => m.MedicineId === cartIng.medicineId)?.MedicineName || ''
          generatedWarnings.push({
            WarningId: Math.floor(Math.random() * 10000),
            SafetyCheckId: checkId,
            PatientId: patientId,
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

      // 2. Drug Interactions Check
      for (let i = 0; i < cartIngredients.length; i++) {
        for (let j = i + 1; j < cartIngredients.length; j++) {
          const ingA = cartIngredients[i]
          const ingB = cartIngredients[j]
          if (!ingA || !ingB) continue

          const interact = localMocks.drugInteractions.find(di =>
            (di.IngredientAId === ingA.ingredientId && di.IngredientBId === ingB.ingredientId) ||
            (di.IngredientAId === ingB.ingredientId && di.IngredientBId === ingA.ingredientId)
          )

          if (interact) {
            const medAName = localMocks.medicines.find(m => m.MedicineId === ingA.medicineId)?.MedicineName || ''
            const medBName = localMocks.medicines.find(m => m.MedicineId === ingB.medicineId)?.MedicineName || ''

            generatedWarnings.push({
              WarningId: Math.floor(Math.random() * 10000),
              SafetyCheckId: checkId,
              PatientId: patientId,
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

      // 3. Contraindications Check
      const patientDiseasesData = localMocks.patientDiseases.filter(pd => pd.PatientId === patientId)
      cartIngredients.forEach(cartIng => {
        patientDiseasesData.forEach(pDisease => {
          const contra = localMocks.contraindications.find(c =>
            c.DiseaseId === pDisease.DiseaseId &&
            (c.IngredientId === cartIng.ingredientId || c.MedicineId === cartIng.medicineId)
          )

          if (contra) {
            const disName = localMocks.diseases.find(d => d.DiseaseId === pDisease.DiseaseId)?.DiseaseName || ''
            const medName = localMocks.medicines.find(m => m.MedicineId === cartIng.medicineId)?.MedicineName || ''

            generatedWarnings.push({
              WarningId: Math.floor(Math.random() * 10000),
              SafetyCheckId: checkId,
              PatientId: patientId,
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
          const pregContra = localMocks.contraindications.find(c =>
            c.ConditionType === 'Đối tượng đặc biệt' &&
            (c.MedicineId === cartIng.medicineId || c.IngredientId === cartIng.ingredientId)
          )

          if (pregContra) {
            const medName = localMocks.medicines.find(m => m.MedicineId === cartIng.medicineId)?.MedicineName || ''
            generatedWarnings.push({
              WarningId: Math.floor(Math.random() * 10000),
              SafetyCheckId: checkId,
              PatientId: patientId,
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

      // 4. Prescription Required
      cartItems.forEach(item => {
        const med = localMocks.medicines.find(m => m.MedicineId === item.MedicineId)
        if (med && med.RequiresPrescription) {
          generatedWarnings.push({
            WarningId: Math.floor(Math.random() * 10000),
            SafetyCheckId: checkId,
            PatientId: patientId,
            MedicineId: item.MedicineId,
            WarningType: 'PrescriptionRequired',
            Severity: 'Trung bình',
            Message: `Thuốc [${med.MedicineName}] yêu cầu phải có đơn thuốc của bác sĩ.`,
            Recommendation: 'Yêu cầu bệnh nhân cung cấp đơn thuốc hoặc liên hệ bác sĩ kê toa.',
            IsAcknowledged: false,
            AcknowledgedBy: null,
            AcknowledgedAt: null,
            Decision: null
          })
        }
      })

      const highestSeverity = generatedWarnings.length > 0 ? 'Medium' : 'None'
      const result = generatedWarnings.length > 0 ? 'Warning' : 'Approved'
      return { warnings: generatedWarnings, highestSeverity, result }
    })
  },

  // Save Prescription Sale endpoint
  async createPrescriptionSale(patientId: number, cartItems: { MedicineId: number; Quantity: number; DosageInstruction: string; TimesPerDay: number; Duration: string; AdviceNote: string }[], finalDecision: 'Approved' | 'Denied' | 'Pending', safetyWarnings: Warning[], note: string): Promise<Sale> {
    const requestDto = {
      PatientId: patientId,
      CartItems: cartItems.map(item => ({
        MedicineId: item.MedicineId,
        Quantity: item.Quantity,
        DosageInstruction: item.DosageInstruction,
        TimesPerDay: item.TimesPerDay,
        Duration: item.Duration,
        AdviceNote: item.AdviceNote
      })),
      FinalDecision: finalDecision,
      Warnings: safetyWarnings,
      Note: note
    }

    return apiPost<Sale, typeof requestDto>('/sales', requestDto, () => {
      const newSaleId = localMocks.sales.length + 1
      const totalAmount = cartItems.reduce((total, item) => {
        const price = localMocks.medicines.find(m => m.MedicineId === item.MedicineId)?.Price || 0
        return total + (price * item.Quantity)
      }, 0)

      const hasWarnings = safetyWarnings.length > 0
      const saleNote = finalDecision === 'Approved' && hasWarnings
        ? 'Bán sau khi duyệt cảnh báo. ' + note
        : 'Bán an toàn thông thường. ' + note

      const newSale: Sale = {
        SaleId: newSaleId,
        PatientId: patientId,
        PharmacistId: 2,
        PrescriptionId: null,
        SaleDate: new Date().toISOString().replace('T', ' ').substring(0, 16),
        TotalAmount: totalAmount,
        FinalDecision: finalDecision,
        Status: finalDecision === 'Denied' ? 'Cancelled' : 'Completed',
        Note: saleNote
      }

      localMocks.sales.unshift(newSale)

      // Add to warning log
      safetyWarnings.forEach(w => {
        localMocks.warnings.unshift({
          ...w,
          WarningId: localMocks.warnings.length + 1
        })
      })

      return newSale
    })
  }
}
