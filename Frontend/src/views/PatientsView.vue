<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { usePharmacyStore, type Patient } from '../store/pharmacy'
import { ApiService } from '../services/api'

const store = usePharmacyStore()

// Sub-tab navigation selection state
const activeSubTab = ref<'patients' | 'allergies'>('patients')

// Centralized drug allergies management state
const searchAllergyQuery = ref('')
const severityFilter = ref<string>('all')
const showAllergyModal = ref(false)
const showEditAllergyModal = ref(false)
const selectedAllergy = ref<any | null>(null)
const allergyPatientId = ref<number | null>(null)
const allergyIsIngredient = ref(true)
const allergyTargetId = ref<number | null>(null)
const allergySeverity = ref('Nghiêm trọng')
const allergyNote = ref('')
const modalPatientSearchQuery = ref('')

const clearAllergyFilters = () => {
  searchAllergyQuery.value = ''
  severityFilter.value = 'all'
}

const filteredModalPatients = computed(() => {
  const query = modalPatientSearchQuery.value.toLowerCase().trim()
  if (!query) return store.patients.value
  return store.patients.value.filter(p => 
    p.FullName.toLowerCase().includes(query) || 
    (p.Phone && p.Phone.includes(query))
  )
})

const allAllergies = computed(() => {
  return store.patientAllergies.value.map(pa => {
    const patient = store.patients.value.find(p => p.PatientId === pa.PatientId)
    let targetName = 'Không rõ'
    if (pa.IngredientId) {
      const ing = store.activeIngredients.value.find(i => i.IngredientId === pa.IngredientId)
      targetName = ing ? ing.IngredientName : ''
    } else if (pa.MedicineId) {
      const med = store.medicines.value.find(m => m.MedicineId === pa.MedicineId)
      targetName = med ? med.MedicineName : ''
    }
    return {
      allergyId: pa.AllergyId,
      patientId: pa.PatientId,
      patientName: patient ? patient.FullName : 'Không rõ',
      patientPhone: patient ? patient.Phone : '',
      patientGender: patient ? patient.Gender || 'Nam' : 'Nam',
      type: pa.IngredientId ? 'Hoạt chất' : 'Biệt dược',
      isIngredient: !!pa.IngredientId,
      targetId: pa.IngredientId || pa.MedicineId || 0,
      targetName,
      severity: pa.Severity || 'Nghiêm trọng',
      note: pa.AllergyNote || ''
    }
  })
})

const filteredAllergies = computed(() => {
  return allAllergies.value.filter(alg => {
    // 1. Search Query
    const query = searchAllergyQuery.value.toLowerCase().trim()
    let matchesSearch = true
    if (query) {
      const nameMatch = alg.patientName.toLowerCase().includes(query)
      const targetMatch = alg.targetName.toLowerCase().includes(query)
      matchesSearch = nameMatch || targetMatch
    }

    // 2. Severity Filter
    let matchesSeverity = true
    if (severityFilter.value !== 'all') {
      matchesSeverity = alg.severity === severityFilter.value
    }

    return matchesSearch && matchesSeverity
  })
})

const openAddAllergy = () => {
  if (!canManage.value) return
  allergyPatientId.value = store.patients.value[0]?.PatientId || null
  allergyIsIngredient.value = true
  allergyTargetId.value = store.activeIngredients.value[0]?.IngredientId || null
  allergySeverity.value = 'Nghiêm trọng'
  allergyNote.value = ''
  modalPatientSearchQuery.value = ''
  showAllergyModal.value = true
}

const toggleCentralAllergyType = () => {
  allergyIsIngredient.value = !allergyIsIngredient.value
  if (allergyIsIngredient.value) {
    allergyTargetId.value = store.activeIngredients.value[0]?.IngredientId || null
  } else {
    allergyTargetId.value = store.medicines.value[0]?.MedicineId || null
  }
}

const saveAllergy = async () => {
  if (!allergyPatientId.value) {
    alert('Vui lòng chọn bệnh nhân!')
    return
  }
  if (!allergyTargetId.value) {
    alert('Vui lòng chọn tác nhân gây dị ứng!')
    return
  }

  // Duplicate check
  const exists = store.patientAllergies.value.some(pa => 
    pa.PatientId === allergyPatientId.value &&
    (allergyIsIngredient.value ? pa.IngredientId === allergyTargetId.value : pa.MedicineId === allergyTargetId.value)
  )
  if (exists) {
    alert('Bệnh nhân này đã được khai báo dị ứng với tác nhân này rồi!')
    return
  }

  try {
    await store.addPatientAllergy({
      PatientId: allergyPatientId.value,
      IsIngredient: allergyIsIngredient.value,
      TargetId: allergyTargetId.value,
      Severity: allergySeverity.value,
      Note: allergyNote.value.trim() || null
    })
    showAllergyModal.value = false
    alert('Đã thêm dị ứng thuốc thành công!')
  } catch (err: any) {
    alert(err.message || 'Lỗi khi thêm dị ứng thuốc!')
  }
}

const openEditAllergy = (allergy: any) => {
  if (!canManage.value) return
  selectedAllergy.value = allergy
  allergySeverity.value = allergy.severity
  allergyNote.value = allergy.note
  showEditAllergyModal.value = true
}

const saveEditedAllergy = async () => {
  if (!selectedAllergy.value) return

  try {
    await store.updatePatientAllergyStore(selectedAllergy.value.allergyId, {
      Severity: allergySeverity.value,
      AllergyNote: allergyNote.value.trim() || null
    })
    showEditAllergyModal.value = false
    alert('Đã cập nhật thông tin dị ứng thành công!')
  } catch (err: any) {
    alert(err.message || 'Lỗi khi cập nhật dị ứng!')
  }
}

const deleteAllergy = async (allergy: any) => {
  if (!canManage.value) return
  if (confirm(`Bạn có chắc chắn muốn xóa ghi nhận dị ứng [${allergy.targetName}] của bệnh nhân "${allergy.patientName}" khỏi hệ thống?`)) {
    try {
      await store.deletePatientAllergyStore(allergy.allergyId)
      alert('Đã xóa ghi nhận dị ứng!')
    } catch (err: any) {
      alert(err.message || 'Lỗi khi xóa dị ứng!')
    }
  }
}

// Clinical statistics computations
const totalPatientsCount = computed(() => store.patients.value.length)
const pregnantCount = computed(() => store.patients.value.filter(p => p.IsPregnant).length)
const breastfeedingCount = computed(() => store.patients.value.filter(p => p.IsBreastfeeding).length)
const allergicPatientsCount = computed(() => {
  const allergyPatientIds = new Set(store.patientAllergies.value.map(pa => pa.PatientId))
  return store.patients.value.filter(p => allergyPatientIds.has(p.PatientId)).length
})

// Helper to get patient initials
const getInitials = (fullName: string) => {
  if (!fullName) return '?'
  const parts = fullName.trim().split(/\s+/)
  if (parts.length === 1) {
    const firstWord = parts[0]
    return firstWord && firstWord.length > 0 ? firstWord.charAt(0).toUpperCase() : '?'
  }
  const firstWord = parts[0]
  const lastWord = parts[parts.length - 1]
  if (firstWord && lastWord && firstWord.length > 0 && lastWord.length > 0) {
    return (firstWord.charAt(0) + lastWord.charAt(0)).toUpperCase()
  }
  return '?'
}

// State for search and filters
const searchQuery = ref('')
const genderFilter = ref<string>('all')
const specialConditionFilter = ref<string>('all')

const searchResults = ref<Patient[] | null>(null)
const isSearching = ref(false)
let debounceTimeout: any = null

const refreshSearch = async () => {
  const cleanQuery = searchQuery.value.trim()
  if (cleanQuery) {
    try {
      searchResults.value = await ApiService.getPatients(cleanQuery)
    } catch (err) {
      console.error('Error refreshing patients search:', err)
    }
  } else {
    searchResults.value = null
  }
}

watch(searchQuery, (newQuery) => {
  if (debounceTimeout) {
    clearTimeout(debounceTimeout)
  }
  const cleanQuery = newQuery.trim()
  if (!cleanQuery) {
    searchResults.value = null
    return
  }
  debounceTimeout = setTimeout(async () => {
    isSearching.value = true
    try {
      searchResults.value = await ApiService.getPatients(cleanQuery)
    } catch (err) {
      console.error('Error searching patients:', err)
    } finally {
      isSearching.value = false
    }
  }, 300)
})

// Modals State
const showDetailModal = ref(false)
const showFormModal = ref(false)
const selectedPatient = ref<Patient | null>(null)
const isEditing = ref(false)

// Form Fields State
const formPatientId = ref<number | null>(null)
const formFullName = ref('')
const formPhone = ref('')
const formGender = ref('Nam')
const formDateOfBirth = ref('')
const formWeightKg = ref<number | null>(null)
const formAddress = ref('')
const formIsPregnant = ref(false)
const formIsBreastfeeding = ref(false)
const formNote = ref('')
const formAllergies = ref<{ isIngredient: boolean; targetId: number; severity: string; note: string }[]>([])
const formDiseases = ref<{ diseaseId: number; note: string }[]>([])

// Helper: check user permission
const canManage = computed(() => {
  return store.currentRole.value === 'admin' || store.currentRole.value === 'manager'
})

// Age calculation helper
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

// Fetch allergies for a specific patient
const getPatientAllergiesList = (patientId: number) => {
  return store.patientAllergies.value
    .filter(pa => pa.PatientId === patientId)
    .map(pa => {
      let targetName = 'Không rõ'
      if (pa.IngredientId) {
        const ing = store.activeIngredients.value.find(i => i.IngredientId === pa.IngredientId)
        targetName = ing ? ing.IngredientName : ''
      } else if (pa.MedicineId) {
        const med = store.medicines.value.find(m => m.MedicineId === pa.MedicineId)
        targetName = med ? med.MedicineName : ''
      }
      return {
        id: pa.AllergyId,
        type: pa.IngredientId ? 'Hoạt chất' : 'Thuốc',
        isIngredient: !!pa.IngredientId,
        targetId: pa.IngredientId || pa.MedicineId || 0,
        targetName,
        severity: pa.Severity || 'Nghiêm trọng',
        note: pa.AllergyNote || ''
      }
    })
}

// Fetch diseases for a specific patient
const getPatientDiseasesList = (patientId: number) => {
  return store.patientDiseases.value
    .filter(pd => pd.PatientId === patientId)
    .map(pd => {
      const dis = store.diseases.value.find(d => d.DiseaseId === pd.DiseaseId)
      return {
        id: pd.PatientDiseaseId,
        diseaseId: pd.DiseaseId,
        name: dis ? dis.DiseaseName : 'Không rõ',
        note: pd.Note || ''
      }
    })
}

// Fetch transaction history (sales) for a patient
const getPatientSalesHistory = (patientId: number) => {
  return store.sales.value.filter(s => s.PatientId === patientId)
}

// Filtered patients list
const filteredPatients = computed(() => {
  const baseList = searchResults.value !== null ? searchResults.value : store.patients.value
  return baseList.filter(p => {
    // 1. Search Query
    const query = searchQuery.value.toLowerCase().trim()
    let matchesSearch = true
    if (!searchResults.value && query) {
      const nameMatch = p.FullName.toLowerCase().includes(query)
      const phoneMatch = p.Phone ? p.Phone.includes(query) : false
      matchesSearch = nameMatch || phoneMatch
    }

    // 2. Gender Filter
    const matchesGender = genderFilter.value === 'all' || p.Gender === genderFilter.value

    // 3. Special Condition Filter
    let matchesSpecial = true
    if (specialConditionFilter.value === 'pregnant') {
      matchesSpecial = p.IsPregnant
    } else if (specialConditionFilter.value === 'breastfeeding') {
      matchesSpecial = p.IsBreastfeeding
    } else if (specialConditionFilter.value === 'normal') {
      matchesSpecial = !p.IsPregnant && !p.IsBreastfeeding
    }

    return matchesSearch && matchesGender && matchesSpecial
  })
})

// Reset all search/filter controls
const clearFilters = () => {
  searchQuery.value = ''
  genderFilter.value = 'all'
  specialConditionFilter.value = 'all'
}

// Open detail modal
const openDetail = (patient: Patient) => {
  selectedPatient.value = patient
  showDetailModal.value = true
}

// Open add form modal
const openAddForm = () => {
  if (!canManage.value) return
  isEditing.value = false
  formPatientId.value = null
  formFullName.value = ''
  formPhone.value = ''
  formGender.value = 'Nam'
  formDateOfBirth.value = ''
  formWeightKg.value = null
  formAddress.value = ''
  formIsPregnant.value = false
  formIsBreastfeeding.value = false
  formNote.value = ''
  formAllergies.value = []
  formDiseases.value = []
  showFormModal.value = true
}

// Open edit form modal
const openEditForm = (patient: Patient) => {
  if (!canManage.value) return
  isEditing.value = true
  formPatientId.value = patient.PatientId
  formFullName.value = patient.FullName
  formPhone.value = patient.Phone || ''
  formGender.value = patient.Gender || ''
  formDateOfBirth.value = patient.DateOfBirth
  formWeightKg.value = patient.WeightKg
  formAddress.value = patient.Address || ''
  formIsPregnant.value = patient.IsPregnant
  formIsBreastfeeding.value = patient.IsBreastfeeding
  formNote.value = patient.Note || ''

  // Load patient allergies
  const allergies = getPatientAllergiesList(patient.PatientId)
  formAllergies.value = allergies.map(a => ({
    isIngredient: a.isIngredient,
    targetId: a.targetId,
    severity: a.severity || 'Nghiêm trọng',
    note: a.note || ''
  }))

  // Load patient diseases
  const diseases = getPatientDiseasesList(patient.PatientId)
  formDiseases.value = diseases.map(d => ({
    diseaseId: d.diseaseId,
    note: d.note || ''
  }))

  showFormModal.value = true
}

// Add allergy row on the form
const addAllergyRow = () => {
  const firstIng = store.activeIngredients.value[0]
  if (!firstIng) return
  formAllergies.value.push({
    isIngredient: true,
    targetId: firstIng.IngredientId,
    severity: 'Nhẹ',
    note: ''
  })
}

// Remove allergy row
const removeAllergyRow = (idx: number) => {
  formAllergies.value.splice(idx, 1)
}

// Toggle allergy type between active ingredient and medicine
const toggleAllergyType = (idx: number) => {
  const row = formAllergies.value[idx]
  if (!row) return
  row.isIngredient = !row.isIngredient
  if (row.isIngredient) {
    row.targetId = store.activeIngredients.value[0]?.IngredientId || 0
  } else {
    row.targetId = store.medicines.value[0]?.MedicineId || 0
  }
}

// Add disease row on the form
const addDiseaseRow = () => {
  const firstDis = store.diseases.value[0]
  if (!firstDis) return
  formDiseases.value.push({
    diseaseId: firstDis.DiseaseId,
    note: ''
  })
}

// Remove disease row
const removeDiseaseRow = (idx: number) => {
  formDiseases.value.splice(idx, 1)
}

// Save patient form
const savePatient = async () => {
  if (!formFullName.value.trim()) {
    alert('Vui lòng nhập họ và tên bệnh nhân!')
    return
  }
  if (!formPhone.value.trim()) {
    alert('Vui lòng nhập số điện thoại!')
    return
  }

  const patientData = {
    FullName: formFullName.value,
    Phone: formPhone.value,
    Gender: formGender.value,
    DateOfBirth: formDateOfBirth.value || new Date().toISOString().substring(0, 10),
    WeightKg: formWeightKg.value,
    Address: formAddress.value,
    IsPregnant: formIsPregnant.value,
    IsBreastfeeding: formIsBreastfeeding.value,
    Note: formNote.value
  }

  let activePatientId = 0

  if (isEditing.value && formPatientId.value !== null) {
    activePatientId = formPatientId.value
    await store.updatePatient(
      activePatientId,
      {
        PatientId: activePatientId,
        ...patientData,
        CreatedAt: store.patients.value.find(p => p.PatientId === activePatientId)?.CreatedAt || new Date().toISOString().substring(0, 10)
      },
      formAllergies.value.map(fa => ({
        isIngredient: fa.isIngredient,
        targetId: fa.targetId,
        severity: fa.severity,
        note: fa.note
      })),
      formDiseases.value.map(fd => ({
        diseaseId: fd.diseaseId,
        note: fd.note
      }))
    )
  } else {
    const newPat = await store.addPatient(
      patientData,
      formAllergies.value.map(fa => ({
        isIngredient: fa.isIngredient,
        targetId: fa.targetId,
        severity: fa.severity,
        note: fa.note
      })),
      formDiseases.value.map(fd => ({
        diseaseId: fd.diseaseId,
        note: fd.note
      }))
    )
    activePatientId = newPat.PatientId
  }

  showFormModal.value = false
  await refreshSearch()
  alert('Đã lưu thông tin hồ sơ bệnh án thành công!')
}

// Delete patient from list
const deletePatient = async (patient: Patient) => {
  if (!canManage.value) return
  if (confirm(`Bạn có chắc chắn muốn xóa hồ sơ của bệnh nhân "${patient.FullName}" khỏi hệ thống không?`)) {
    await store.deletePatient(patient.PatientId)
    await refreshSearch()
    alert('Đã xóa hồ sơ bệnh nhân!')
  }
}
</script>

<template>
  <div class="view-container">
    <!-- Sub tabs Selector -->
    <div class="tabs-navigation">
      <div class="tabs-list">
        <button 
          :class="['tab-btn', { active: activeSubTab === 'patients' }]" 
          @click="activeSubTab = 'patients'"
        >
          <svg viewBox="0 0 24 24" class="tab-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
          Hồ sơ bệnh nhân
          <span class="tab-count-badge">{{ store.patients.value.length }}</span>
        </button>
        <button 
          :class="['tab-btn', { active: activeSubTab === 'allergies' }]" 
          @click="activeSubTab = 'allergies'"
        >
          <svg viewBox="0 0 24 24" class="tab-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          Quản lý Dị ứng thuốc
          <span class="tab-count-badge">{{ store.patientAllergies.value.length }}</span>
        </button>
      </div>
    </div>

    <!-- Tab 1: Patients Profiles Catalog -->
    <div v-if="activeSubTab === 'patients'" class="sub-tab-content">
      <!-- Clinical Stats Dashboard -->
    <div class="stats-dashboard-grid">
      <div class="stat-card total-patients">
        <div class="stat-icon-wrapper">
          <svg viewBox="0 0 24 24" class="stat-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
        </div>
        <div class="stat-info">
          <span class="stat-label">Tổng bệnh nhân</span>
          <span class="stat-value">{{ totalPatientsCount }}</span>
        </div>
      </div>

      <div class="stat-card pregnant-patients">
        <div class="stat-icon-wrapper">
          <span class="stat-emoji">🤰</span>
        </div>
        <div class="stat-info">
          <span class="stat-label">Thai kỳ lưu ý</span>
          <span class="stat-value">{{ pregnantCount }}</span>
        </div>
      </div>

      <div class="stat-card breastfeeding-patients">
        <div class="stat-icon-wrapper">
          <span class="stat-emoji">🍼</span>
        </div>
        <div class="stat-info">
          <span class="stat-label">Nuôi con nhỏ</span>
          <span class="stat-value">{{ breastfeedingCount }}</span>
        </div>
      </div>

      <div class="stat-card allergic-patients">
        <div class="stat-icon-wrapper">
          <svg viewBox="0 0 24 24" class="stat-icon warning" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
        <div class="stat-info">
          <span class="stat-label">Tiền sử dị ứng</span>
          <span class="stat-value">{{ allergicPatientsCount }}</span>
        </div>
      </div>
    </div>

    <!-- Filter & Search Panel -->
    <div class="grid-card search-filter-panel">
      <div class="filters-row">
        <!-- Search input -->
        <div class="filter-col flex-1">
          <label class="filter-label">Tìm kiếm bệnh nhân:</label>
          <div class="search-input-wrapper">
            <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input 
              type="text" 
              placeholder="Nhập họ tên hoặc số điện thoại..." 
              class="form-control form-control-with-icon"
              v-model="searchQuery" 
            />
          </div>
        </div>

        <!-- Gender filter -->
        <div class="filter-col">
          <label class="filter-label">Giới tính:</label>
          <select v-model="genderFilter" class="form-control select-control">
            <option value="all">Tất cả giới tính</option>
            <option value="Nam">Nam</option>
            <option value="Nữ">Nữ</option>
          </select>
        </div>

        <!-- Special Conditions filter -->
        <div class="filter-col">
          <label class="filter-label">Đối tượng đặc biệt:</label>
          <select v-model="specialConditionFilter" class="form-control select-control">
            <option value="all">Tất cả bệnh nhân</option>
            <option value="pregnant">🤰 Phụ nữ mang thai</option>
            <option value="breastfeeding">🍼 Phụ nữ cho con bú</option>
            <option value="normal">Bình thường</option>
          </select>
        </div>
      </div>

      <!-- Action buttons -->
      <div class="panel-actions-row">
        <button class="secondary-btn" @click="clearFilters" :disabled="!searchQuery && genderFilter === 'all' && specialConditionFilter === 'all'">
          Xóa bộ lọc
        </button>
        <button class="primary-btn flex-center" v-if="canManage" @click="openAddForm">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5" class="btn-icon">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          Thêm hồ sơ mới
        </button>
      </div>
    </div>

    <!-- Patients Catalog List -->
    <div class="grid-card patients-catalog-card" style="margin-top: 20px; overflow-x: auto;">
      <h3 class="section-title" style="margin-bottom: 16px;">Danh mục hồ sơ khách hàng ({{ filteredPatients.length }} bệnh nhân)</h3>
      
      <table class="data-table" v-if="filteredPatients.length > 0">
        <thead>
          <tr>
            <th>Bệnh nhân</th>
            <th>Ngày sinh / Tuổi</th>
            <th>Giới tính</th>
            <th>Cân nặng</th>
            <th>Trạng thái đặc biệt</th>
            <th>Dị ứng (Allergies)</th>
            <th>Bệnh nền (Diseases)</th>
            <th style="text-align: center;">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in filteredPatients" :key="p.PatientId">
            <td>
              <div class="patient-profile-cell">
                <div :class="['patient-avatar', p.Gender === 'Nữ' ? 'female' : 'male']">
                  {{ getInitials(p.FullName) }}
                </div>
                <div class="patient-name-cell">
                  <span class="patient-title">{{ p.FullName }}</span>
                  <small class="patient-sub">{{ p.Phone }}</small>
                </div>
              </div>
            </td>
            <td>
              <div class="birth-cell">
                <span class="birth-date">{{ p.DateOfBirth }}</span>
                <small class="birth-age">({{ calculateAge(p.DateOfBirth) }} tuổi)</small>
              </div>
            </td>
            <td>{{ p.Gender }}</td>
            <td><strong class="weight-text">{{ p.WeightKg ? p.WeightKg + ' kg' : '-' }}</strong></td>
            <td>
              <div class="special-badges-list">
                <span v-if="p.IsPregnant" class="status-tag danger pregnant-tag">🤰 Mang thai</span>
                <span v-if="p.IsBreastfeeding" class="status-tag warning breastfeeding-tag">🍼 Con bú</span>
                <span v-if="!p.IsPregnant && !p.IsBreastfeeding" class="light-tag normal-tag">✓ Bình thường</span>
              </div>
            </td>
            <td>
              <div class="allergies-preview-list" v-if="getPatientAllergiesList(p.PatientId).length > 0">
                <span v-for="alg in getPatientAllergiesList(p.PatientId)" :key="alg.id" :class="['alg-preview-tag', alg.severity === 'Nghiêm trọng' || alg.severity === 'High' ? 'high' : 'medium']" :title="alg.note || 'Không có ghi chú'">
                  <span class="dot"></span>
                  {{ alg.targetName }}
                </span>
              </div>
              <span v-else class="empty-preview">✓ Không dị ứng</span>
            </td>
            <td>
              <div class="diseases-preview-list" v-if="getPatientDiseasesList(p.PatientId).length > 0">
                <span v-for="dis in getPatientDiseasesList(p.PatientId)" :key="dis.id" class="dis-preview-tag" :title="dis.note || 'Không có ghi chú'">
                  {{ dis.name }}
                </span>
              </div>
              <span v-else class="empty-preview">-</span>
            </td>
            <td>
              <div class="action-buttons-group">
                <button class="action-btn-icon view" @click="openDetail(p)" title="Xem chi tiết bệnh án">
                  <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                  </svg>
                </button>
                <button class="action-btn-icon edit" v-if="canManage" @click="openEditForm(p)" title="Chỉnh sửa hồ sơ">
                  <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <button class="action-btn-icon delete" v-if="canManage" @click="deletePatient(p)" title="Xóa hồ sơ">
                  <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Empty state -->
      <div class="empty-state-container flex-center" v-else>
        <div class="empty-content">
          <span class="empty-icon">👥</span>
          <h4>Không tìm thấy bệnh nhân phù hợp</h4>
          <p>Hãy thử thay đổi điều kiện tìm kiếm hoặc bộ lọc.</p>
        </div>
      </div>
    </div>

    </div>

    <!-- Tab 2: Central Drug Allergies Catalog -->
    <div v-else-if="activeSubTab === 'allergies'" class="sub-tab-content">
      <!-- Filter & Search Panel for Allergies -->
      <div class="grid-card search-filter-panel">
        <div class="filters-row">
          <!-- Search input -->
          <div class="filter-col flex-1">
            <label class="filter-label">Tìm kiếm dị ứng:</label>
            <div class="search-input-wrapper">
              <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              <input 
                type="text" 
                placeholder="Nhập tên bệnh nhân hoặc tên tác nhân..." 
                class="form-control form-control-with-icon"
                v-model="searchAllergyQuery" 
              />
            </div>
          </div>

          <!-- Severity filter -->
          <div class="filter-col">
            <label class="filter-label">Mức độ nghiêm trọng:</label>
            <select v-model="severityFilter" class="form-control select-control">
              <option value="all">Tất cả mức độ</option>
              <option value="Nghiêm trọng">Nghiêm trọng</option>
              <option value="Trung bình">Trung bình</option>
              <option value="Nhẹ">Nhẹ</option>
            </select>
          </div>
        </div>

        <!-- Action buttons -->
        <div class="panel-actions-row">
          <button class="secondary-btn" @click="clearAllergyFilters" :disabled="!searchAllergyQuery && severityFilter === 'all'">
            Xóa bộ lọc
          </button>
          <button class="primary-btn flex-center" v-if="canManage" @click="openAddAllergy">
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5" class="btn-icon">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            Thêm dị ứng nhanh
          </button>
        </div>
      </div>

      <!-- Central Allergies Catalog List -->
      <div class="grid-card patients-catalog-card" style="margin-top: 20px; overflow-x: auto;">
        <h3 class="section-title" style="margin-bottom: 16px;">Danh mục dị ứng thuốc toàn hệ thống ({{ filteredAllergies.length }} ca dị ứng)</h3>
        
        <table class="data-table" v-if="filteredAllergies.length > 0">
          <thead>
            <tr>
              <th>Bệnh nhân</th>
              <th>Loại tác nhân</th>
              <th>Tác nhân gây dị ứng</th>
              <th>Mức độ nghiêm trọng</th>
              <th>Ghi chú / Triệu chứng lâm sàng</th>
              <th style="text-align: center;" v-if="canManage">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="alg in filteredAllergies" :key="alg.allergyId">
              <td>
                <div class="patient-profile-cell">
                  <div :class="['patient-avatar', alg.patientGender === 'Nữ' ? 'female' : 'male']">
                    {{ getInitials(alg.patientName) }}
                  </div>
                  <div class="patient-name-cell">
                    <span class="patient-title">{{ alg.patientName }}</span>
                    <small class="patient-sub">{{ alg.patientPhone || 'Không có SĐT' }}</small>
                  </div>
                </div>
              </td>
              <td>
                <span class="light-tag">{{ alg.type }}</span>
              </td>
              <td>
                <strong class="text-main">{{ alg.targetName }}</strong>
              </td>
              <td>
                <span :class="['status-tag', alg.severity === 'Nghiêm trọng' || alg.severity === 'High' ? 'danger' : alg.severity === 'Trung bình' ? 'warning' : 'safe']">
                  {{ alg.severity }}
                </span>
              </td>
              <td class="text-muted">{{ alg.note || '-' }}</td>
              <td v-if="canManage">
                <div class="action-buttons-group">
                  <button class="action-btn-icon edit" @click="openEditAllergy(alg)" title="Chỉnh sửa mức độ/ghi chú">
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                  </button>
                  <button class="action-btn-icon delete" @click="deleteAllergy(alg)" title="Xóa ghi nhận dị ứng">
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Empty state -->
        <div class="empty-state-container flex-center" v-else>
          <div class="empty-content">
            <span class="empty-icon">🛡️</span>
            <h4>Không tìm thấy ghi nhận dị ứng nào</h4>
            <p>Hệ thống hiện tại chưa ghi nhận ca dị ứng nào khớp với bộ lọc.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- ==========================================
      MODAL 1: VIEW MEDICAL RECORD DETAILS
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showDetailModal && selectedPatient">
      <div class="modal-card detail-modal">
        <div class="modal-header">
          <div class="modal-title-area">
            <span class="modal-indicator">Mã HS: #HS-00{{ selectedPatient.PatientId }}</span>
            <h3>Hồ sơ bệnh án: {{ selectedPatient.FullName }}</h3>
          </div>
          <button class="close-modal-btn" @click="showDetailModal = false">
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>

        <div class="modal-body scrollable-body">
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Họ và tên bệnh nhân:</span>
              <span class="detail-val-strong">{{ selectedPatient.FullName }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Số điện thoại liên hệ:</span>
              <span class="detail-val-text">{{ selectedPatient.Phone }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Ngày sinh:</span>
              <span class="detail-val-text">{{ selectedPatient.DateOfBirth }} <small>({{ calculateAge(selectedPatient.DateOfBirth) }} tuổi)</small></span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Giới tính:</span>
              <span class="detail-val-text">{{ selectedPatient.Gender }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Cân nặng hiện tại:</span>
              <span class="detail-val-text"><strong class="weight-text">{{ selectedPatient.WeightKg ? selectedPatient.WeightKg + ' kg' : 'Chưa cập nhật' }}</strong></span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Địa chỉ liên hệ:</span>
              <span class="detail-val-text">{{ selectedPatient.Address || '-' }}</span>
            </div>
            <div class="detail-item span-2">
              <span class="detail-label">Đối tượng đặc biệt:</span>
              <div class="special-badges-list" style="margin-top: 4px;">
                <span v-if="selectedPatient.IsPregnant" class="status-tag danger pregnant-tag" style="padding: 4px 10px;">🤰 Phụ nữ mang thai</span>
                <span v-if="selectedPatient.IsBreastfeeding" class="status-tag warning breastfeeding-tag" style="padding: 4px 10px;">🍼 Nuôi con bằng sữa mẹ</span>
                <span v-if="!selectedPatient.IsPregnant && !selectedPatient.IsBreastfeeding" class="light-tag normal-tag" style="padding: 4px 10px;">Đối tượng bình thường</span>
              </div>
            </div>
            <div class="detail-item span-2" v-if="selectedPatient.Note">
              <span class="detail-label">Ghi chú lâm sàng / Tiểu sử:</span>
              <p class="detail-text-box">{{ selectedPatient.Note }}</p>
            </div>
          </div>

          <!-- Section A: Allergies list -->
          <div class="medical-section-box">
            <h4 class="sub-title text-danger flex-center" style="justify-content: flex-start; gap: 6px;">
              <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" class="text-danger-icon">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
              Tiền sử Dị ứng thuốc & Hoạt chất
            </h4>
            <div class="table-container" v-if="getPatientAllergiesList(selectedPatient!.PatientId).length > 0">
              <table class="dashboard-table">
                <thead>
                  <tr>
                    <th>Loại tác nhân</th>
                    <th>Tác nhân gây dị ứng</th>
                    <th>Mức độ</th>
                    <th>Ghi chú lâm sàng</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="alg in getPatientAllergiesList(selectedPatient!.PatientId)" :key="alg.id">
                    <td><span class="light-tag">{{ alg.type }}</span></td>
                    <td><strong>{{ alg.targetName }}</strong></td>
                    <td>
                      <span :class="['status-tag', alg.severity === 'Nghiêm trọng' || alg.severity === 'High' ? 'danger' : alg.severity === 'Trung bình' ? 'warning' : 'safe']">
                        {{ alg.severity }}
                      </span>
                    </td>
                    <td><small class="text-muted">{{ alg.note || '-' }}</small></td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div class="empty-medical-box" v-else>
              <p>✓ Không ghi nhận tiền sử dị ứng thuốc ở bệnh nhân này.</p>
            </div>
          </div>

          <!-- Section B: Diseases list -->
          <div class="medical-section-box" style="margin-top: 16px;">
            <h4 class="sub-title text-warning flex-center" style="justify-content: flex-start; gap: 6px;">
              <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" class="text-warning-icon">
                <path stroke-linecap="round" stroke-linejoin="round" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
              Bệnh lý nền đang mắc
            </h4>
            <div class="table-container" v-if="getPatientDiseasesList(selectedPatient!.PatientId).length > 0">
              <table class="dashboard-table">
                <thead>
                  <tr>
                    <th>Tên bệnh lý</th>
                    <th>Ghi chú / Chi tiết bệnh án</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="dis in getPatientDiseasesList(selectedPatient!.PatientId)" :key="dis.id">
                    <td><strong>{{ dis.name }}</strong></td>
                    <td><small class="text-muted">{{ dis.note || '-' }}</small></td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div class="empty-medical-box" v-else>
              <p>✓ Bệnh nhân không có bệnh lý nền mãn tính.</p>
            </div>
          </div>

          <!-- Section C: Transaction history -->
          <div class="medical-section-box" style="margin-top: 16px;">
            <h4 class="sub-title text-info flex-center" style="justify-content: flex-start; gap: 6px;">
              <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" class="text-info-icon">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 8h6m-5 0a3 3 0 110 6H9l3 3m-3-6h6m6 1a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Lịch sử mua thuốc tại cửa hàng
            </h4>
            <div class="table-container" v-if="getPatientSalesHistory(selectedPatient!.PatientId).length > 0">
              <table class="dashboard-table">
                <thead>
                  <tr>
                    <th>Mã HD</th>
                    <th>Thời gian mua</th>
                    <th>Tổng hóa đơn</th>
                    <th>Đánh giá an toàn</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="sale in getPatientSalesHistory(selectedPatient!.PatientId)" :key="sale.SaleId">
                    <td>HD-00{{ sale.SaleId }}</td>
                    <td>{{ sale.SaleDate }}</td>
                    <td><strong class="price-text">{{ sale.TotalAmount.toLocaleString() }}đ</strong></td>
                    <td>
                      <span :class="['status-tag', sale.FinalDecision === 'Approved' ? 'safe' : sale.FinalDecision === 'Denied' ? 'danger' : 'warning']">
                        {{ sale.FinalDecision === 'Approved' ? 'An toàn' : sale.FinalDecision === 'Denied' ? 'Từ chối' : 'Chờ xử lý' }}
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
            <div class="empty-medical-box" v-else>
              <p>Bệnh nhân chưa có giao dịch mua thuốc nào trên hệ thống.</p>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showDetailModal = false">Đóng</button>
          <button class="primary-btn" v-if="canManage" @click="showDetailModal = false; openEditForm(selectedPatient!)">Chỉnh sửa bệnh án</button>
        </div>
      </div>
    </div>

    <!-- ==========================================
      MODAL 2: ADD / EDIT PATIENT FORM
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showFormModal">
      <div class="modal-card form-modal">
        <div class="modal-header">
          <div class="modal-title-area">
            <h3>{{ isEditing ? 'Chỉnh sửa thông tin hồ sơ bệnh án' : 'Tạo hồ sơ bệnh nhân mới' }}</h3>
          </div>
          <button class="close-modal-btn" @click="showFormModal = false">
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>

        <div class="modal-body scrollable-body">
          <div class="form-inputs-grid">
            <!-- Full Name -->
            <div class="form-group span-2">
              <label class="form-label required-label">Họ và tên bệnh nhân:</label>
              <input type="text" v-model="formFullName" class="form-control" placeholder="Ví dụ: Nguyễn Văn A..." />
            </div>

            <!-- Phone number -->
            <div class="form-group">
              <label class="form-label required-label">Số điện thoại liên hệ:</label>
              <input type="text" v-model="formPhone" class="form-control" placeholder="Ví dụ: 09xxxxxxxx..." />
            </div>

            <!-- Gender selector -->
            <div class="form-group">
              <label class="form-label">Giới tính:</label>
              <div class="gender-radio-row">
                <label class="radio-container">
                  <input type="radio" value="Nam" v-model="formGender" />
                  <span class="radiomark"></span>
                  Nam
                </label>
                <label class="radio-container" style="margin-left: 20px;">
                  <input type="radio" value="Nữ" v-model="formGender" />
                  <span class="radiomark"></span>
                  Nữ
                </label>
              </div>
            </div>

            <!-- Date of Birth -->
            <div class="form-group">
              <label class="form-label">Ngày tháng năm sinh:</label>
              <input type="date" v-model="formDateOfBirth" class="form-control" />
            </div>

            <!-- Weight -->
            <div class="form-group">
              <label class="form-label">Cân nặng hiện tại (kg):</label>
              <input type="number" v-model.number="formWeightKg" class="form-control" placeholder="Ví dụ: 65..." min="1" />
            </div>

            <!-- Address -->
            <div class="form-group span-2">
              <label class="form-label">Địa chỉ liên hệ:</label>
              <input type="text" v-model="formAddress" class="form-control" placeholder="Tỉnh/Thành phố..." />
            </div>

            <!-- Special Conditions checkboxes -->
            <div class="form-group flex-checkbox-row" v-if="formGender === 'Nữ'">
              <label class="checkbox-container">
                <input type="checkbox" v-model="formIsPregnant" />
                <span class="checkmark"></span>
                Thai kỳ (Bệnh nhân đang mang thai)
              </label>
              <label class="checkbox-container">
                <input type="checkbox" v-model="formIsBreastfeeding" />
                <span class="checkmark"></span>
                Đang nuôi con bằng sữa mẹ
              </label>
            </div>

            <!-- General clinic notes -->
            <div class="form-group span-2">
              <label class="form-label">Tiểu sử bệnh án / Ghi chú lâm sàng:</label>
              <textarea v-model="formNote" class="form-control textarea-control" rows="2" placeholder="Tình trạng chung, tiền sử huyết áp, tim mạch..."></textarea>
            </div>
          </div>

          <!-- Dynamic management of Allergies association -->
          <div class="form-ingredients-mapping-section">
            <div class="ingredients-header-row">
              <h4 class="sub-title">Tiền sử Dị ứng hoạt chất / biệt dược</h4>
              <button type="button" class="add-row-btn" @click="addAllergyRow">+ Thêm dị ứng</button>
            </div>

            <div class="form-ingredients-list" v-if="formAllergies.length > 0">
              <div v-for="(item, idx) in formAllergies" :key="idx" class="form-allergy-row-flex">
                <!-- Toggle allergy type button -->
                <button type="button" class="allergy-type-toggle-btn" @click="toggleAllergyType(idx)">
                  {{ item.isIngredient ? 'Hoạt chất' : 'Biệt dược' }}
                </button>

                <!-- Select Ingredient or Medicine -->
                <div class="col-select">
                  <select v-if="item.isIngredient" v-model="item.targetId" class="form-control select-control-sm">
                    <option v-for="ing in store.activeIngredients.value" :key="ing.IngredientId" :value="ing.IngredientId">
                      {{ ing.IngredientName }}
                    </option>
                  </select>
                  <select v-else v-model="item.targetId" class="form-control select-control-sm">
                    <option v-for="med in store.medicines.value" :key="med.MedicineId" :value="med.MedicineId">
                      {{ med.MedicineName }}
                    </option>
                  </select>
                </div>

                <!-- Select severity -->
                <div class="col-severity">
                  <select v-model="item.severity" class="form-control select-control-sm">
                    <option value="Nghiêm trọng">Nghiêm trọng</option>
                    <option value="Trung bình">Trung bình</option>
                    <option value="Nhẹ">Nhẹ</option>
                  </select>
                </div>

                <!-- Allergy Note input -->
                <div class="col-note flex-1">
                  <input type="text" v-model="item.note" class="form-control text-control-sm" placeholder="Triệu chứng..." />
                </div>

                <!-- Remove row button -->
                <div class="col-delete">
                  <button type="button" class="delete-row-btn" @click="removeAllergyRow(idx)">
                    <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5">
                      <line x1="18" y1="6" x2="6" y2="18"></line>
                      <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
            <div class="empty-ingredients-form" v-else>
              <p>Chưa khai báo dị ứng nào cho bệnh nhân.</p>
            </div>
          </div>

          <!-- Dynamic management of Diseases association -->
          <div class="form-ingredients-mapping-section" style="margin-top: 20px;">
            <div class="ingredients-header-row">
              <h4 class="sub-title">Bệnh lý nền đang mắc</h4>
              <button type="button" class="add-row-btn" @click="addDiseaseRow">+ Thêm bệnh nền</button>
            </div>

            <div class="form-ingredients-list" v-if="formDiseases.length > 0">
              <div v-for="(item, idx) in formDiseases" :key="idx" class="form-ingredient-row">
                <!-- Select Disease -->
                <div class="col-select">
                  <select v-model="item.diseaseId" class="form-control select-control-sm">
                    <option v-for="dis in store.diseases.value" :key="dis.DiseaseId" :value="dis.DiseaseId">
                      {{ dis.DiseaseName }}
                    </option>
                  </select>
                </div>

                <!-- Note input -->
                <div class="col-note flex-1">
                  <input type="text" v-model="item.note" class="form-control text-control-sm" placeholder="Chi tiết chẩn đoán..." />
                </div>

                <!-- Remove row button -->
                <div class="col-delete">
                  <button type="button" class="delete-row-btn" @click="removeDiseaseRow(idx)">
                    <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5">
                      <line x1="18" y1="6" x2="6" y2="18"></line>
                      <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
            <div class="empty-ingredients-form" v-else>
              <p>Chưa khai báo bệnh nền mãn tính nào.</p>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- ==========================================
      MODAL 3: ADD ALLERGY FORM
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showAllergyModal">
      <div class="modal-card form-modal" style="max-width: 550px;">
        <div class="modal-header">
          <div class="modal-title-area">
            <h3>Khai báo dị ứng thuốc nhanh</h3>
          </div>
          <button class="close-modal-btn" @click="showAllergyModal = false">
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>

        <div class="modal-body scrollable-body">
          <div class="form-inputs-grid" style="grid-template-columns: 1fr;">
            <!-- Select Patient -->
            <div class="form-group">
              <label class="form-label required-label">Chọn bệnh nhân:</label>
              <!-- Search patient inside modal -->
              <input 
                type="text" 
                placeholder="Tìm nhanh bệnh nhân theo tên..." 
                class="form-control" 
                v-model="modalPatientSearchQuery"
                style="margin-bottom: 8px;"
              />
              <select v-model="allergyPatientId" class="form-control select-control">
                <option v-for="p in filteredModalPatients" :key="p.PatientId" :value="p.PatientId">
                  {{ p.FullName }} (SĐT: {{ p.Phone || 'Không có' }})
                </option>
              </select>
            </div>

            <!-- Target Type Selector -->
            <div class="form-group">
              <label class="form-label">Loại tác nhân dị ứng:</label>
              <div style="display: flex; gap: 12px; align-items: center;">
                <button type="button" class="allergy-type-toggle-btn" @click="toggleCentralAllergyType" style="min-width: 120px;">
                  {{ allergyIsIngredient ? 'Hoạt chất' : 'Biệt dược' }}
                </button>
                <small class="text-muted">Click để chuyển giữa nhóm Hoạt chất và thuốc Biệt dược.</small>
              </div>
            </div>

            <!-- Allergen Target Select -->
            <div class="form-group">
              <label class="form-label required-label">Tác nhân gây dị ứng:</label>
              <select v-if="allergyIsIngredient" v-model="allergyTargetId" class="form-control select-control">
                <option v-for="ing in store.activeIngredients.value" :key="ing.IngredientId" :value="ing.IngredientId">
                  {{ ing.IngredientName }}
                </option>
              </select>
              <select v-else v-model="allergyTargetId" class="form-control select-control">
                <option v-for="med in store.medicines.value" :key="med.MedicineId" :value="med.MedicineId">
                  {{ med.MedicineName }}
                </option>
              </select>
            </div>

            <!-- Severity Select -->
            <div class="form-group">
              <label class="form-label">Mức độ nghiêm trọng:</label>
              <select v-model="allergySeverity" class="form-control select-control">
                <option value="Nghiêm trọng">Nghiêm trọng</option>
                <option value="Trung bình">Trung bình</option>
                <option value="Nhẹ">Nhẹ</option>
              </select>
            </div>

            <!-- Allergy Note -->
            <div class="form-group">
              <label class="form-label">Ghi chú triệu chứng lâm sàng:</label>
              <textarea v-model="allergyNote" class="form-control textarea-control" rows="2" placeholder="Ví dụ: Nổi mề đay mẩn ngứa, buồn nôn, khó thở..."></textarea>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showAllergyModal = false">Hủy</button>
          <button class="primary-btn" @click="saveAllergy">Ghi nhận dị ứng</button>
        </div>
      </div>
    </div>

    <!-- ==========================================
      MODAL 4: EDIT ALLERGY FORM
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showEditAllergyModal && selectedAllergy">
      <div class="modal-card form-modal" style="max-width: 500px;">
        <div class="modal-header">
          <div class="modal-title-area">
            <h3>Chỉnh sửa ghi nhận dị ứng</h3>
          </div>
          <button class="close-modal-btn" @click="showEditAllergyModal = false">
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>

        <div class="modal-body scrollable-body">
          <div class="form-inputs-grid" style="grid-template-columns: 1fr;">
            <!-- Display static info -->
            <div class="form-group">
              <label class="form-label">Bệnh nhân:</label>
              <span class="detail-val-text">{{ selectedAllergy.patientName }}</span>
            </div>
            
            <div class="form-group">
              <label class="form-label">Tác nhân gây dị ứng:</label>
              <span class="detail-val-text">{{ selectedAllergy.targetName }} ({{ selectedAllergy.type }})</span>
            </div>

            <!-- Severity Select -->
            <div class="form-group">
              <label class="form-label">Mức độ nghiêm trọng:</label>
              <select v-model="allergySeverity" class="form-control select-control">
                <option value="Nghiêm trọng">Nghiêm trọng</option>
                <option value="Trung bình">Trung bình</option>
                <option value="Nhẹ">Nhẹ</option>
              </select>
            </div>

            <!-- Allergy Note -->
            <div class="form-group">
              <label class="form-label">Ghi chú triệu chứng lâm sàng:</label>
              <textarea v-model="allergyNote" class="form-control textarea-control" rows="2" placeholder="Ví dụ: Nổi mề đay, mẩn ngứa..."></textarea>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showEditAllergyModal = false">Hủy</button>
          <button class="primary-btn" @click="saveEditedAllergy">Cập nhật</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.view-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
  animation: fadeIn 0.4s ease-out;
}

/* Dashboard Statistics Grid */
.stats-dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 20px;
}
.stat-card {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  padding: 20px;
  border-radius: var(--border-radius-md);
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: var(--shadow-sm);
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}
.stat-card:hover {
  transform: translateY(-3px);
  box-shadow: var(--shadow-md);
  border-color: var(--primary-light);
}
.stat-icon-wrapper {
  width: 48px;
  height: 48px;
  border-radius: var(--border-radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
}
.total-patients .stat-icon-wrapper {
  background: var(--primary-bg);
  color: var(--primary-medium);
}
.pregnant-patients .stat-icon-wrapper {
  background: var(--danger-bg);
  color: var(--danger);
}
.breastfeeding-patients .stat-icon-wrapper {
  background: var(--warning-bg);
  color: var(--warning);
}
.allergic-patients .stat-icon-wrapper {
  background: rgba(239, 68, 68, 0.08);
  color: var(--danger);
}
.stat-icon {
  width: 24px;
  height: 24px;
  color: currentColor;
}
.stat-emoji {
  line-height: 1;
}
.stat-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.stat-label {
  font-size: 13px;
  color: var(--text-muted);
  font-weight: 600;
}
.stat-value {
  font-size: 24px;
  font-weight: 800;
  color: var(--text-main);
  line-height: 1.2;
}

/* Search and Filters panel styling */
.search-filter-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  padding: 20px;
  box-shadow: var(--shadow-sm);
}
.filters-row {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}
.filter-col {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 180px;
}
.filter-col.flex-1 {
  flex: 1;
  min-width: 260px;
}
.filter-label {
  font-size: 13px;
  font-weight: 700;
  color: var(--text-muted);
}
.search-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}
.form-control-with-icon {
  padding-left: 38px !important;
}
.search-icon-svg {
  position: absolute;
  left: 14px;
  width: 18px;
  height: 18px;
  color: var(--text-muted);
  pointer-events: none;
  transition: color var(--transition-fast);
}
.search-input-wrapper input:focus ~ .search-icon-svg {
  color: var(--primary-medium);
}
.panel-actions-row {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  border-top: 1px solid var(--border-color);
  padding-top: 14px;
}

/* Patient profile Avatar & Name cell */
.patient-profile-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}
.patient-avatar {
  width: 40px;
  height: 40px;
  border-radius: var(--border-radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 700;
  border: 1.5px solid transparent;
  user-select: none;
}
.patient-avatar.male {
  background-color: var(--primary-bg);
  color: var(--primary);
  border-color: rgba(13, 148, 136, 0.15);
}
.patient-avatar.female {
  background-color: rgba(236, 72, 153, 0.08);
  color: #db2777;
  border-color: rgba(236, 72, 153, 0.15);
}
.patient-name-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.patient-title {
  font-weight: 700;
  color: var(--text-main);
}
.patient-sub {
  color: var(--text-muted);
  font-size: 12.5px;
}
.birth-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.birth-date {
  font-weight: 500;
}
.birth-age {
  color: var(--text-muted);
  font-size: 12px;
}
.weight-text {
  color: var(--text-main);
}

/* Badges styling */
.special-badges-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}
.light-tag {
  font-size: 11px;
  font-weight: 600;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  color: var(--text-muted);
  padding: 3px 8px;
  border-radius: var(--border-radius-sm);
  display: inline-block;
}
.pregnant-tag {
  background-color: var(--danger-bg);
  color: var(--danger);
  border: 1px solid rgba(239, 68, 68, 0.12);
}
.breastfeeding-tag {
  background-color: var(--warning-bg);
  color: var(--warning);
  border: 1px solid rgba(245, 158, 11, 0.12);
}
.normal-tag {
  background-color: var(--success-bg);
  color: var(--success);
  border: 1px solid rgba(16, 185, 129, 0.12);
}

/* Allergies and Diseases previews */
.allergies-preview-list, .diseases-preview-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}
.alg-preview-tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 6px;
}
.alg-preview-tag .dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}
.alg-preview-tag.high {
  background-color: var(--danger-bg);
  color: var(--danger);
  border: 1px solid rgba(239, 68, 68, 0.15);
}
.alg-preview-tag.high .dot {
  background-color: var(--danger);
  animation: pulse 1.5s infinite;
}
.alg-preview-tag.medium {
  background-color: var(--warning-bg);
  color: var(--warning);
  border: 1px solid rgba(245, 158, 11, 0.15);
}
.alg-preview-tag.medium .dot {
  background-color: var(--warning);
}
.dis-preview-tag {
  font-size: 11px;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 6px;
  background-color: var(--info-bg);
  color: var(--info);
  border: 1px solid rgba(59, 130, 246, 0.15);
  display: inline-block;
}
.empty-preview {
  color: var(--text-muted);
  font-size: 13px;
}

/* Action button icons in table */
.action-buttons-group {
  display: flex;
  justify-content: center;
  gap: 8px;
}
.action-btn-icon {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  width: 32px;
  height: 32px;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
.action-btn-icon:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
}
.action-btn-icon.view:hover {
  background-color: var(--info-bg);
  color: var(--info);
  border-color: var(--info);
}
.action-btn-icon.edit:hover {
  background-color: var(--warning-bg);
  color: var(--warning);
  border-color: var(--warning);
}
.action-btn-icon.delete:hover {
  background-color: var(--danger-bg);
  color: var(--danger);
  border-color: var(--danger);
}

/* Hover effect on table rows */
.data-table tbody tr {
  transition: background-color 0.2s;
}
.data-table tbody tr:hover {
  background-color: rgba(13, 148, 136, 0.02) !important;
}

/* Empty State */
.empty-state-container {
  padding: 48px 16px;
  text-align: center;
  color: var(--text-muted);
}
.empty-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}
.empty-icon {
  font-size: 40px;
}
.empty-content h4 {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0;
}
.empty-content p {
  font-size: 13px;
  margin: 0;
}

/* Modals Refactored Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(15, 23, 42, 0.35);
  backdrop-filter: blur(8px);
  z-index: 1000;
  animation: fadeIn 0.2s ease-out;
}
.modal-card {
  background: var(--bg-card);
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-premium);
  border: 1px solid var(--border-color);
  animation: slideUp 0.35s cubic-bezier(0.16, 1, 0.3, 1);
  width: 100%;
  max-width: 750px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid var(--border-color);
  background: linear-gradient(135deg, rgba(13, 148, 136, 0.03) 0%, rgba(255, 255, 255, 0) 100%);
}
.modal-title-area {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.modal-title-area h3 {
  font-size: 18px;
  font-weight: 800;
  color: var(--text-main);
}
.modal-indicator {
  align-self: flex-start;
  background: var(--primary-bg);
  color: var(--primary-medium);
  padding: 3px 8px;
  border-radius: var(--border-radius-sm);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  border: 1px solid rgba(13, 148, 136, 0.12);
}
.close-modal-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  border-radius: 50%;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}
.close-modal-btn:hover {
  background-color: var(--bg-main);
  color: var(--danger);
  transform: rotate(90deg);
}
.scrollable-body {
  max-height: 65vh;
  overflow-y: auto;
  padding: 24px;
}
.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px 24px;
  margin-bottom: 24px;
}
.detail-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.detail-item.span-2 {
  grid-column: span 2;
}
.detail-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.detail-val-text {
  font-size: 14.5px;
  color: var(--text-main);
  font-weight: 600;
}
.detail-val-strong {
  font-size: 20px;
  font-weight: 800;
  color: var(--primary);
}
.detail-text-box {
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  padding: 12px 16px;
  border-radius: var(--border-radius-md);
  font-size: 13.5px;
  color: var(--text-main);
  line-height: 1.6;
  margin-top: 4px;
}

/* Medical sections inside modal */
.medical-section-box {
  border-top: 1px solid var(--border-color);
  padding-top: 20px;
  margin-top: 20px;
}
.medical-section-box .sub-title {
  font-size: 14px;
  font-weight: 800;
  margin-bottom: 12px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}
.text-danger-icon { color: var(--danger); }
.text-warning-icon { color: var(--warning); }
.text-info-icon { color: var(--info); }

.table-container {
  overflow-x: auto;
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
}
.dashboard-table {
  width: 100%;
  border-collapse: collapse;
}
.dashboard-table th {
  background-color: var(--bg-main);
  padding: 10px 14px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--text-muted);
  border-bottom: 1.5px solid var(--border-color);
}
.dashboard-table td {
  padding: 12px 14px;
  border-bottom: 1px solid var(--border-color);
  font-size: 13.5px;
}
.dashboard-table tr:last-child td {
  border-bottom: none;
}
.empty-medical-box {
  background-color: var(--bg-main);
  border: 1px dashed var(--border-color);
  padding: 14px;
  border-radius: var(--border-radius-md);
  font-size: 13.5px;
  color: var(--text-muted);
  text-align: center;
}
.price-text {
  color: var(--primary-medium);
}

/* Form structure */
.form-inputs-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px 20px;
  margin-bottom: 24px;
}
.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.form-group.span-2 {
  grid-column: span 2;
}
.required-label::after {
  content: " *";
  color: var(--danger);
  font-weight: bold;
}
.textarea-control {
  resize: vertical;
}

/* Gender radio inputs */
.gender-radio-row {
  display: flex;
  align-items: center;
  height: 40px;
}
.radio-container {
  display: flex;
  align-items: center;
  position: relative;
  padding-left: 24px;
  cursor: pointer;
  font-size: 14.5px;
  font-weight: 600;
  color: var(--text-main);
  user-select: none;
}
.radio-container input {
  position: absolute;
  opacity: 0;
  cursor: pointer;
}
.radiomark {
  position: absolute;
  top: 0;
  left: 0;
  height: 16px;
  width: 16px;
  background-color: var(--bg-main);
  border: 2px solid var(--border-color);
  border-radius: 50%;
  transition: all 0.2s;
}
.radio-container:hover input ~ .radiomark {
  border-color: var(--primary-medium);
}
.radio-container input:checked ~ .radiomark {
  background-color: var(--bg-main);
  border-color: var(--primary-medium);
}
.radiomark:after {
  content: "";
  position: absolute;
  display: none;
}
.radio-container input:checked ~ .radiomark:after {
  display: block;
}
.radio-container .radiomark:after {
  top: 3px;
  left: 3px;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--primary-medium);
}

/* Checkbox sliders */
.flex-checkbox-row {
  grid-column: span 2;
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
  align-items: center;
  padding: 8px 0;
}
.checkbox-container {
  display: flex;
  align-items: center;
  position: relative;
  padding-left: 28px;
  cursor: pointer;
  font-size: 14.5px;
  font-weight: 600;
  color: var(--text-main);
  user-select: none;
}
.checkbox-container input {
  position: absolute;
  opacity: 0;
  cursor: pointer;
  height: 0;
  width: 0;
}
.checkmark {
  position: absolute;
  top: 0;
  left: 0;
  height: 18px;
  width: 18px;
  background-color: var(--bg-main);
  border: 2px solid var(--border-color);
  border-radius: 4px;
  transition: all 0.2s;
}
.checkbox-container:hover input ~ .checkmark {
  border-color: var(--primary-medium);
}
.checkbox-container input:checked ~ .checkmark {
  background-color: var(--primary-medium);
  border-color: var(--primary-medium);
}
.checkmark:after {
  content: "";
  position: absolute;
  display: none;
}
.checkbox-container input:checked ~ .checkmark:after {
  display: block;
}
.checkbox-container .checkmark:after {
  left: 5px;
  top: 1px;
  width: 4px;
  height: 8px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

/* Form Allergy & Mapping rows styling */
.form-allergy-row-flex {
  display: flex;
  gap: 12px;
  align-items: center;
  background: var(--bg-main);
  padding: 8px 12px;
  border-radius: var(--border-radius-md);
  border: 1px solid var(--border-color);
}
.allergy-type-toggle-btn {
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  color: var(--text-main);
  font-size: 12px;
  font-weight: 700;
  padding: 8px 10px;
  border-radius: var(--border-radius-sm);
  cursor: pointer;
  transition: all 0.2s;
  min-width: 90px;
  text-align: center;
}
.allergy-type-toggle-btn:hover {
  border-color: var(--primary-medium);
  color: var(--primary-medium);
  background-color: var(--primary-bg);
}
.col-select {
  flex: 2;
  min-width: 140px;
}
.col-severity {
  width: 130px;
}
.col-note {
  flex: 3;
  min-width: 160px;
}
.col-delete {
  display: flex;
  justify-content: center;
}
.delete-row-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
  padding: 4px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
}
.delete-row-btn:hover {
  color: var(--danger);
  background-color: var(--danger-bg);
}

/* Dynamic ingredients mappings form */
.form-ingredients-mapping-section {
  border-top: 1px solid var(--border-color);
  padding-top: 20px;
}
.ingredients-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 14px;
}
.add-row-btn {
  background-color: var(--primary-bg);
  color: var(--primary-medium);
  border: 1px solid rgba(13, 148, 136, 0.15);
  padding: 6px 14px;
  font-size: 12px;
  font-weight: 700;
  border-radius: var(--border-radius-sm);
  cursor: pointer;
  transition: all 0.2s;
}
.add-row-btn:hover {
  background-color: var(--primary-medium);
  color: #fff;
  border-color: var(--primary-medium);
}
.form-ingredients-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.form-ingredient-row {
  display: flex;
  gap: 12px;
  align-items: center;
  background: var(--bg-main);
  padding: 8px 12px;
  border-radius: var(--border-radius-md);
  border: 1px solid var(--border-color);
}
.empty-ingredients-form {
  background-color: var(--bg-main);
  border: 1px dashed var(--border-color);
  padding: 16px;
  border-radius: var(--border-radius-md);
  text-align: center;
  color: var(--text-muted);
  font-size: 13.5px;
}

/* Modal footers */
.modal-footer {
  padding: 16px 24px;
  border-top: 1px solid var(--border-color);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  background-color: var(--bg-main);
}

/* Animations */
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}
@keyframes pulse {
  0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7); }
  70% { transform: scale(1); box-shadow: 0 0 0 6px rgba(239, 68, 68, 0); }
  100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
}

/* Sub-tabs styling */
.tabs-navigation {
  border-bottom: 2px solid var(--border-color);
  padding-bottom: 2px;
  margin-bottom: 8px;
}

.tabs-list {
  display: flex;
  gap: 8px;
}

.tab-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 20px;
  background: transparent;
  border: none;
  border-bottom: 2px solid transparent;
  color: var(--text-muted);
  font-weight: 600;
  font-size: 15px;
  cursor: pointer;
  transition: all var(--transition-fast);
  margin-bottom: -4px;
}

.tab-btn:hover {
  color: var(--primary-medium);
  background-color: rgba(13, 148, 136, 0.03);
  border-radius: var(--border-radius-sm) var(--border-radius-sm) 0 0;
}

.tab-btn.active {
  color: var(--primary-medium);
  border-bottom-color: var(--primary-medium);
  font-weight: 700;
}

.tab-icon {
  width: 18px;
  height: 18px;
}

.tab-count-badge {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 6px;
  background-color: var(--border-color);
  color: var(--text-muted);
  border-radius: var(--border-radius-full);
}

.tab-btn.active .tab-count-badge {
  background-color: var(--primary-bg);
  color: var(--primary-medium);
}

.sub-tab-content {
  display: flex;
  flex-direction: column;
  gap: 20px;
}
</style>
