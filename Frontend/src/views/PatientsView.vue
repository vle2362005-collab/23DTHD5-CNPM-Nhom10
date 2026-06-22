<script setup lang="ts">
import { ref, computed } from 'vue'
import { usePharmacyStore, type Patient } from '../store/pharmacy'

const store = usePharmacyStore()

// State for search and filters
const searchQuery = ref('')
const genderFilter = ref<string>('all')
const specialConditionFilter = ref<string>('all')

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

// Stats computations for cards
const totalPatients = computed(() => store.patients.value.length)
const specialConditionsCount = computed(() => store.patients.value.filter(p => p.IsPregnant || p.IsBreastfeeding).length)
const allergiesCount = computed(() => {
  const uniqueIds = new Set(store.patientAllergies.value.map(a => a.PatientId))
  return uniqueIds.size
})
const diseasesCount = computed(() => {
  const uniqueIds = new Set(store.patientDiseases.value.map(d => d.PatientId))
  return uniqueIds.size
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
  return store.patients.value.filter(p => {
    // 1. Search Query (Name or Phone)
    const query = searchQuery.value.toLowerCase().trim()
    let matchesSearch = true
    if (query) {
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
  alert('Đã lưu thông tin hồ sơ bệnh án thành công!')
}

// Delete patient from list
const deletePatient = async (patient: Patient) => {
  if (!canManage.value) return
  if (confirm(`Bạn có chắc chắn muốn xóa hồ sơ của bệnh nhân "${patient.FullName}" khỏi hệ thống không?`)) {
    await store.deletePatient(patient.PatientId)
    alert('Đã xóa hồ sơ bệnh nhân!')
  }
}
</script>

<template>
  <div class="view-container">
    <!-- Stats Cards Row -->
    <div class="stats-cards-row">
      <div class="stat-card total-card">
        <div class="stat-icon">👥</div>
        <div class="stat-info">
          <span class="stat-label">Tổng số bệnh nhân</span>
          <span class="stat-number">{{ totalPatients }}</span>
        </div>
      </div>
      <div class="stat-card special-card">
        <div class="stat-icon">🤰</div>
        <div class="stat-info">
          <span class="stat-label">Đối tượng đặc biệt</span>
          <span class="stat-number">{{ specialConditionsCount }}</span>
        </div>
      </div>
      <div class="stat-card allergy-card">
        <div class="stat-icon">⚠️</div>
        <div class="stat-info">
          <span class="stat-label">Bệnh nhân dị ứng</span>
          <span class="stat-number">{{ allergiesCount }}</span>
        </div>
      </div>
      <div class="stat-card disease-card">
        <div class="stat-icon">🏥</div>
        <div class="stat-info">
          <span class="stat-label">Có bệnh lý nền</span>
          <span class="stat-number">{{ diseasesCount }}</span>
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
            <input 
              type="text" 
              placeholder="Nhập họ tên hoặc số điện thoại..." 
              class="form-control"
              v-model="searchQuery" 
            />
            <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
        </div>

        <!-- Gender filter -->
        <div class="filter-col">
          <label class="filter-label">Giới tính:</label>
          <select v-model="genderFilter" class="form-control select-control">
            <option value="all">Tất cả</option>
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
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5" style="margin-right: 6px;">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          Thêm hồ sơ mới (Patients)
        </button>
      </div>
    </div>

    <!-- Patients Catalog List -->
    <div class="grid-card" style="margin-top: 20px; overflow-x: auto;">
      <h3 class="section-title" style="margin-bottom: 16px;">Danh mục hồ sơ khách hàng ({{ filteredPatients.length }} bệnh nhân)</h3>
      
      <table class="data-table" v-if="filteredPatients.length > 0">
        <thead>
          <tr>
            <th>Họ và tên</th>
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
              <div class="patient-name-cell">
                <span class="patient-title">{{ p.FullName }}</span>
                <small class="patient-sub">{{ p.Phone }}</small>
              </div>
            </td>
            <td>{{ p.DateOfBirth }} <small>({{ calculateAge(p.DateOfBirth) }}t)</small></td>
             <td>
              <span :class="['gender-tag', p.Gender === 'Nam' ? 'male' : 'female']">
                {{ p.Gender === 'Nam' ? '👨 Nam' : '👩 Nữ' }}
              </span>
             </td>
             <td><strong class="weight-text">{{ p.WeightKg ? p.WeightKg + ' kg' : '-' }}</strong></td>
             <td>
               <div class="special-badges-list">
                 <span v-if="p.IsPregnant" class="status-tag danger">🤰 Mang thai</span>
                 <span v-if="p.IsBreastfeeding" class="status-tag warning">🍼 Con bú</span>
                 <span v-if="!p.IsPregnant && !p.IsBreastfeeding" class="light-tag">Bình thường</span>
               </div>
             </td>
             <td>
               <div class="allergies-preview-list" v-if="getPatientAllergiesList(p.PatientId).length > 0">
                 <span v-for="alg in getPatientAllergiesList(p.PatientId)" :key="alg.id" :class="['alg-preview-tag', alg.severity === 'Nghiêm trọng' || alg.severity === 'High' ? 'high' : 'medium']" :title="alg.note || 'Không có ghi chú'">
                   {{ alg.targetName }}
                 </span>
               </div>
               <span v-else class="empty-preview">-</span>
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
                   <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5">
                     <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z" />
                     <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                   </svg>
                 </button>
                 <button class="action-btn-icon edit" v-if="canManage" @click="openEditForm(p)" title="Chỉnh sửa hồ sơ">
                   <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5">
                     <path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L6.832 19.82a4.5 4.5 0 01-1.897 1.13l-2.685.8.8-2.685a4.5 4.5 0 011.13-1.897L16.863 4.487zm0 0L19.5 7.125" />
                   </svg>
                 </button>
                 <button class="action-btn-icon delete" v-if="canManage" @click="deletePatient(p)" title="Xóa hồ sơ">
                   <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5">
                     <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
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

    <!-- ==========================================
      MODAL 1: VIEW MEDICAL RECORD DETAILS
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showDetailModal && selectedPatient">
      <div class="modal-card detail-modal">
        <div class="modal-header">
          <div class="modal-title-area">
            <span class="modal-indicator">HS-00{{ selectedPatient.PatientId }}</span>
            <h3>Hồ sơ bệnh án: {{ selectedPatient.FullName }}</h3>
          </div>
          <button class="close-modal-btn" @click="showDetailModal = false">×</button>
        </div>

        <div class="modal-body scrollable-body">
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Họ và tên bệnh nhân:</span>
              <span class="detail-val-strong">{{ selectedPatient.FullName }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Số điện thoại liên hệ:</span>
              <span>{{ selectedPatient.Phone }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Ngày sinh:</span>
              <span>{{ selectedPatient.DateOfBirth }} ({{ calculateAge(selectedPatient.DateOfBirth) }} tuổi)</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Giới tính:</span>
              <span>{{ selectedPatient.Gender }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Cân nặng hiện tại:</span>
              <span>{{ selectedPatient.WeightKg ? selectedPatient.WeightKg + ' kg' : 'Chưa cập nhật' }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Địa chỉ liên hệ:</span>
              <span>{{ selectedPatient.Address || '-' }}</span>
            </div>
            <div class="detail-item span-2">
              <span class="detail-label">Đối tượng đặc biệt:</span>
              <div class="special-badges-list" style="margin-top: 4px;">
                <span v-if="selectedPatient.IsPregnant" class="status-tag danger" style="padding: 4px 10px;">🤰 Phụ nữ mang thai</span>
                <span v-if="selectedPatient.IsBreastfeeding" class="status-tag warning" style="padding: 4px 10px;">🍼 Đang nuôi con bằng sữa mẹ</span>
                <span v-if="!selectedPatient.IsPregnant && !selectedPatient.IsBreastfeeding" class="light-tag" style="padding: 4px 10px;">Đối tượng bình thường</span>
              </div>
            </div>
            <div class="detail-item span-2" v-if="selectedPatient.Note">
              <span class="detail-label">Ghi chú lâm sàng / Tiểu sử:</span>
              <p class="detail-text-box">{{ selectedPatient.Note }}</p>
            </div>
          </div>

          <!-- Section A: Allergies list -->
          <div class="medical-section-box">
            <h4 class="sub-title text-danger">⚠️ Tiền sử Dị ứng thuốc & Hoạt chất</h4>
            <div class="table-container" v-if="getPatientAllergiesList(selectedPatient!.PatientId).length > 0">
              <table class="dashboard-table">
                <thead>
                  <tr>
                    <th>Loại</th>
                    <th>Tác nhân gây dị ứng</th>
                    <th>Mức độ nguy hiểm</th>
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
                    <td><small>{{ alg.note || '-' }}</small></td>
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
            <h4 class="sub-title text-warning">🏥 Bệnh lý nền đang mắc</h4>
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
                    <td><small>{{ dis.note || '-' }}</small></td>
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
            <h4 class="sub-title text-info">💰 Lịch sử mua thuốc tại cửa hàng</h4>
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
          <button class="close-modal-btn" @click="showFormModal = false">×</button>
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
                  <button type="button" class="delete-row-btn" @click="removeAllergyRow(idx)">×</button>
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
                  <button type="button" class="delete-row-btn" @click="removeDiseaseRow(idx)">×</button>
                </div>
              </div>
            </div>
            <div class="empty-ingredients-form" v-else>
              <p>Chưa khai báo bệnh nền mãn tính nào.</p>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showFormModal = false">Hủy</button>
          <button class="primary-btn" @click="savePatient">Lưu lại hồ sơ</button>
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
}

/* Stats Cards Row */
.stats-cards-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 8px;
}
.stat-card {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-lg);
  padding: 16px 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-normal);
  position: relative;
  overflow: hidden;
}
.stat-card::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
}
.stat-card.total-card::before { background: var(--info); }
.stat-card.special-card::before { background: var(--danger); }
.stat-card.allergy-card::before { background: var(--warning); }
.stat-card.disease-card::before { background: var(--primary-medium); }

.stat-card:hover {
  transform: translateY(-3px);
  box-shadow: var(--shadow-md);
  border-color: var(--primary-light);
}
.stat-icon {
  font-size: 26px;
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--bg-main);
  transition: transform var(--transition-normal);
}
.stat-card:hover .stat-icon {
  transform: scale(1.1);
}
.stat-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.stat-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
}
.stat-number {
  font-size: 22px;
  font-weight: 800;
  color: var(--text-main);
  line-height: 1;
}

/* Gender tags styling */
.gender-tag {
  font-size: 13px;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: var(--border-radius-sm);
  display: inline-block;
  white-space: nowrap;
}
.gender-tag.male {
  background-color: rgba(59, 130, 246, 0.1);
  color: var(--info);
  border: 1px solid rgba(59, 130, 246, 0.2);
}
.gender-tag.female {
  background-color: rgba(244, 63, 94, 0.1);
  color: var(--danger);
  border: 1px solid rgba(244, 63, 94, 0.2);
}

/* Search and Filters panel styling */
.search-filter-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
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
.search-input-wrapper input {
  padding-right: 36px;
}
.search-icon-svg {
  position: absolute;
  right: 12px;
  width: 18px;
  height: 18px;
  color: var(--text-muted);
  pointer-events: none;
}
.panel-actions-row {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  border-top: 1px solid var(--border-color);
  padding-top: 14px;
}

/* Patient Name Cell */
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
}
.weight-text {
  color: var(--text-main);
}

/* Special badges list */
.special-badges-list {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
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

/* Allergies and Diseases previews */
.allergies-preview-list, .diseases-preview-list {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}
.alg-preview-tag, .dis-preview-tag {
  font-size: 11px;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
  display: inline-block;
}
.alg-preview-tag.high {
  background-color: var(--danger-bg);
  color: var(--danger);
  border: 1px solid rgba(239, 68, 68, 0.15);
}
.alg-preview-tag.medium {
  background-color: var(--warning-bg);
  color: var(--warning);
  border: 1px solid rgba(245, 158, 11, 0.15);
}
.dis-preview-tag {
  background-color: var(--info-bg);
  color: var(--info);
  border: 1px solid rgba(59, 130, 246, 0.15);
}
.empty-preview {
  color: var(--text-muted);
}

/* Action button icons in table */
.action-buttons-group {
  display: flex;
  justify-content: center;
  gap: 8px;
}
.action-btn-icon {
  background: var(--bg-main);
  border: 1px solid var(--border-color);
  width: 28px;
  height: 28px;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  transition: all 0.2s;
}
.action-btn-icon:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
}
.action-btn-icon.view:hover {
  background-color: rgba(59, 130, 246, 0.1);
  border-color: var(--info);
}
.action-btn-icon.edit:hover {
  background-color: rgba(245, 158, 11, 0.1);
  border-color: var(--warning);
}
.action-btn-icon.delete:hover {
  background-color: rgba(239, 68, 68, 0.1);
  border-color: var(--danger);
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

/* Modals */
.detail-modal, .form-modal {
  width: 100%;
  max-width: 700px;
}
.scrollable-body {
  max-height: 60vh;
  overflow-y: auto;
  padding-right: 6px;
}
.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
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
.detail-val-strong {
  font-size: 18px;
  font-weight: 800;
  color: var(--text-main);
}
.detail-text-box {
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  padding: 12px;
  border-radius: var(--border-radius-md);
  font-size: 13px;
  color: var(--text-main);
  line-height: 1.5;
  margin: 0;
}

/* Medical sections inside modal */
.medical-section-box {
  border-top: 1px solid var(--border-color);
  padding-top: 16px;
}
.sub-title {
  font-size: 14px;
  font-weight: 700;
  margin-bottom: 12px;
}
.empty-medical-box {
  background-color: var(--bg-main);
  border: 1px dashed var(--border-color);
  padding: 14px;
  border-radius: var(--border-radius-md);
  font-size: 13px;
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
  gap: 16px;
  margin-bottom: 20px;
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
  font-size: 14px;
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
  font-size: 14px;
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

/* Form Allergy Row */
.form-allergy-row-flex {
  display: flex;
  gap: 12px;
  align-items: center;
}
.allergy-type-toggle-btn {
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  color: var(--text-main);
  font-size: 12px;
  font-weight: 700;
  padding: 8px 10px;
  border-radius: var(--border-radius-md);
  cursor: pointer;
  transition: all 0.2s;
  min-width: 90px;
}
.allergy-type-toggle-btn:hover {
  border-color: var(--primary-medium);
  color: var(--primary-medium);
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
  font-size: 24px;
  line-height: 1;
  color: var(--text-muted);
  cursor: pointer;
  transition: color 0.2s;
  padding: 0 4px;
}
.delete-row-btn:hover {
  color: var(--danger);
}

/* Dynamic ingredients mappings form */
.form-ingredients-mapping-section {
  border-top: 1px solid var(--border-color);
  padding-top: 16px;
}
.ingredients-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}
.add-row-btn {
  background-color: rgba(16, 185, 129, 0.1);
  color: var(--primary-medium);
  border: 1px solid rgba(16, 185, 129, 0.2);
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 700;
  border-radius: var(--border-radius-md);
  cursor: pointer;
  transition: all 0.2s;
}
.add-row-btn:hover {
  background-color: var(--primary-medium);
  color: #fff;
}
.form-ingredients-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.form-ingredient-row {
  display: flex;
  gap: 12px;
  align-items: center;
}
.empty-ingredients-form {
  background-color: var(--bg-main);
  border: 1px dashed var(--border-color);
  padding: 16px;
  border-radius: var(--border-radius-md);
  text-align: center;
  color: var(--text-muted);
  font-size: 13px;
}
</style>
