<script setup lang="ts">
import { ref, computed } from 'vue'
import { usePharmacyStore, type Contraindication } from '../store/pharmacy'

const store = usePharmacyStore()

// Sub-tabs state
const activeSubTab = ref<'contraindications' | 'interactions' | 'allergies' | 'diseases'>('contraindications')

// Check permissions
const canManage = computed(() => {
  return store.currentRole.value === 'admin' || store.currentRole.value === 'manager'
})

// Search & Filter State
const contraSearchQuery = ref('')
const severityFilter = ref('all')

// Allergies Tab Search
const allergySearchQuery = ref('')
const filteredAllergies = computed(() => {
  return store.patientAllergies.value.filter(pa => {
    const p = store.patients.value.find(pat => pat.PatientId === pa.PatientId)
    const patientName = p ? p.FullName.toLowerCase() : ''
    const patientPhone = p?.Phone || ''
    
    let targetName = ''
    if (pa.IngredientId) {
      const ing = store.activeIngredients.value.find(i => i.IngredientId === pa.IngredientId)
      targetName = ing ? ing.IngredientName.toLowerCase() : ''
    } else if (pa.MedicineId) {
      const med = store.medicines.value.find(m => m.MedicineId === pa.MedicineId)
      targetName = med ? med.MedicineName.toLowerCase() : ''
    }
    
    const query = allergySearchQuery.value.toLowerCase().trim()
    if (!query) return true
    
    return patientName.includes(query) || patientPhone.includes(query) || targetName.includes(query) || (pa.AllergyNote || '').toLowerCase().includes(query)
  })
})

// Diseases Tab Search
const diseaseSearchQuery = ref('')
const filteredDiseases = computed(() => {
  return store.diseases.value.filter(d => {
    const name = d.DiseaseName.toLowerCase()
    const desc = (d.Description || '').toLowerCase()
    const query = diseaseSearchQuery.value.toLowerCase().trim()
    if (!query) return true
    return name.includes(query) || desc.includes(query)
  })
})

// Count patients with a specific disease
const getDiseasePatientsCount = (diseaseId: number) => {
  return store.patientDiseases.value.filter(pd => pd.DiseaseId === diseaseId).length
}

// Modal Form State
const showContraModal = ref(false)
const isEditingContra = ref(false)
const formContraId = ref<number | null>(null)
const formTargetType = ref<'medicine' | 'ingredient'>('medicine')
const formMedicineId = ref<string>('none')
const formIngredientId = ref<string>('none')
const formConditionType = ref<'Disease' | 'Special'>('Disease')
const formDiseaseId = ref<string>('none')
const formSpecialConditionText = ref('')
const formSeverity = ref<'High' | 'Medium' | 'Low'>('High')
const formDescription = ref('')
const formRecommendation = ref('')

// Filtered Contraindications
const filteredContraindications = computed(() => {
  return store.contraindications.value.filter(c => {
    // 1. Search Query
    const query = contraSearchQuery.value.toLowerCase().trim()
    let matchesSearch = true
    if (query) {
      const medName = c.MedicineId ? (store.medicines.value.find(m => m.MedicineId === c.MedicineId)?.MedicineName || '').toLowerCase() : ''
      const ingName = c.IngredientId ? (store.activeIngredients.value.find(ai => ai.IngredientId === c.IngredientId)?.IngredientName || '').toLowerCase() : ''
      const disName = c.DiseaseId ? (store.diseases.value.find(d => d.DiseaseId === c.DiseaseId)?.DiseaseName || '').toLowerCase() : ''
      const specText = c.ConditionType === 'Đối tượng đặc biệt' ? 'đối tượng đặc biệt mang thai cho con bú' : ''
      const desc = (c.Description || '').toLowerCase()
      const rec = (c.Recommendation || '').toLowerCase()

      matchesSearch = medName.includes(query) || 
                      ingName.includes(query) || 
                      disName.includes(query) || 
                      specText.includes(query) ||
                      desc.includes(query) || 
                      rec.includes(query)
    }

    // 2. Severity Filter
    let matchesSeverity = true
    if (severityFilter.value !== 'all') {
      const mappedFilter = severityFilter.value === 'High' ? ['High', 'Nghiêm trọng'] : severityFilter.value === 'Medium' ? ['Medium', 'Trung bình'] : ['Low', 'Nhẹ']
      matchesSeverity = mappedFilter.includes(c.Severity)
    }

    return matchesSearch && matchesSeverity
  })
})

// Add Mode
const openAddContra = () => {
  if (!canManage.value) return
  isEditingContra.value = false
  formContraId.value = null
  formTargetType.value = 'medicine'
  formMedicineId.value = store.medicines.value[0] ? store.medicines.value[0].MedicineId.toString() : 'none'
  formIngredientId.value = store.activeIngredients.value[0] ? store.activeIngredients.value[0].IngredientId.toString() : 'none'
  formConditionType.value = 'Disease'
  formDiseaseId.value = store.diseases.value[0] ? store.diseases.value[0].DiseaseId.toString() : 'none'
  formSpecialConditionText.value = ''
  formSeverity.value = 'High'
  formDescription.value = ''
  formRecommendation.value = ''
  showContraModal.value = true
}

// Edit Mode
const openEditContra = (contra: Contraindication) => {
  if (!canManage.value) return
  isEditingContra.value = true
  formContraId.value = contra.ContraindicationId
  
  if (contra.MedicineId) {
    formTargetType.value = 'medicine'
    formMedicineId.value = contra.MedicineId.toString()
    formIngredientId.value = store.activeIngredients.value[0] ? store.activeIngredients.value[0].IngredientId.toString() : 'none'
  } else if (contra.IngredientId) {
    formTargetType.value = 'ingredient'
    formIngredientId.value = contra.IngredientId.toString()
    formMedicineId.value = store.medicines.value[0] ? store.medicines.value[0].MedicineId.toString() : 'none'
  }

  if (contra.DiseaseId) {
    formConditionType.value = 'Disease'
    formDiseaseId.value = contra.DiseaseId.toString()
    formSpecialConditionText.value = ''
  } else {
    formConditionType.value = 'Special'
    formDiseaseId.value = store.diseases.value[0] ? store.diseases.value[0].DiseaseId.toString() : 'none'
    formSpecialConditionText.value = contra.ConditionType === 'Đối tượng đặc biệt' ? 'Phụ nữ mang thai' : contra.ConditionType
  }

  formSeverity.value = (contra.Severity === 'Nghiêm trọng' || contra.Severity === 'High') ? 'High' : (contra.Severity === 'Trung bình' || contra.Severity === 'Medium') ? 'Medium' : 'Low'
  formDescription.value = contra.Description || ''
  formRecommendation.value = contra.Recommendation || ''
  showContraModal.value = true
}

// Save Action
const saveContra = async () => {
  const isMed = formTargetType.value === 'medicine'
  const targetMedId = isMed && formMedicineId.value !== 'none' ? Number(formMedicineId.value) : null
  const targetIngId = !isMed && formIngredientId.value !== 'none' ? Number(formIngredientId.value) : null

  if (isMed && !targetMedId) {
    alert('Vui lòng chọn thuốc!')
    return
  }
  if (!isMed && !targetIngId) {
    alert('Vui lòng chọn hoạt chất!')
    return
  }

  const isDisease = formConditionType.value === 'Disease'
  const targetDiseaseId = isDisease && formDiseaseId.value !== 'none' ? Number(formDiseaseId.value) : null
  const conditionTypeString = isDisease ? 'Disease' : (formSpecialConditionText.value.trim() || 'Đối tượng đặc biệt')

  if (isDisease && !targetDiseaseId) {
    alert('Vui lòng chọn bệnh nền!')
    return
  }
  if (!isDisease && !formSpecialConditionText.value.trim()) {
    alert('Vui lòng nhập đối tượng chống chỉ định!')
    return
  }

  if (!formDescription.value.trim()) {
    alert('Vui lòng nhập mô tả chống chỉ định!')
    return
  }

  const severityString = formSeverity.value === 'High' ? 'Nghiêm trọng' : formSeverity.value === 'Medium' ? 'Trung bình' : 'Nhẹ'

  const contraData = {
    MedicineId: targetMedId,
    IngredientId: targetIngId,
    DiseaseId: targetDiseaseId,
    ConditionType: conditionTypeString,
    Severity: severityString,
    Description: formDescription.value.trim(),
    Recommendation: formRecommendation.value.trim() || null
  }

  if (isEditingContra.value && formContraId.value !== null) {
    await store.updateContraindication(formContraId.value, {
      ContraindicationId: formContraId.value,
      ...contraData
    })
  } else {
    await store.addContraindication(contraData)
  }

  showContraModal.value = false
  alert('Đã lưu thông tin chống chỉ định thành công!')
}

// Delete Action
const deleteContra = async (contra: Contraindication) => {
  if (!canManage.value) return
  const label = contra.MedicineId 
    ? (store.medicines.value.find(m => m.MedicineId === contra.MedicineId)?.MedicineName || '')
    : (store.activeIngredients.value.find(ai => ai.IngredientId === contra.IngredientId)?.IngredientName || '')

  if (confirm(`Bạn có chắc chắn muốn xóa chống chỉ định của "${label}" khỏi hệ thống không?`)) {
    await store.deleteContraindication(contra.ContraindicationId)
    alert('Đã xóa chống chỉ định thành công!')
  }
}
</script>

<template>
  <div class="view-container">
    <!-- Sub-tabs Navigation -->
    <div class="tabs-header-row">
      <button 
        :class="['tab-btn', { active: activeSubTab === 'contraindications' }]" 
        @click="activeSubTab = 'contraindications'"
      >
        🚫 Chống chỉ định lâm sàng
      </button>
      <button 
        :class="['tab-btn', { active: activeSubTab === 'interactions' }]" 
        @click="activeSubTab = 'interactions'"
      >
        ⚡ Tương tác thuốc
      </button>
      <button 
        :class="['tab-btn', { active: activeSubTab === 'allergies' }]" 
        @click="activeSubTab = 'allergies'"
      >
        ⚠️ Dị ứng thuốc bệnh nhân
      </button>
      <button 
        :class="['tab-btn', { active: activeSubTab === 'diseases' }]" 
        @click="activeSubTab = 'diseases'"
      >
        🏥 Danh mục Bệnh nền
      </button>
    </div>

    <!-- ==========================================
      TAB 1: CONTRAINDICATIONS
    ========================================== -->
    <div v-if="activeSubTab === 'contraindications'" class="grid-card text-section">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
        <h3 class="section-title" style="margin-bottom: 0;">Danh mục Chống chỉ định lâm sàng (Contraindications)</h3>
        <button class="primary-btn flex-center" v-if="canManage" @click="openAddContra">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5" style="margin-right: 6px;">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          Thêm chống chỉ định
        </button>
      </div>

      <!-- Filters Row -->
      <div class="filters-row-contra">
        <div class="filter-col-contra flex-1">
          <label class="filter-label">Tìm kiếm chống chỉ định:</label>
          <div class="search-input-wrapper">
            <input 
              type="text" 
              placeholder="Nhập tên thuốc, hoạt chất, bệnh nền hoặc mô tả..." 
              class="form-control"
              v-model="contraSearchQuery" 
            />
            <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
        </div>

        <div class="filter-col-contra">
          <label class="filter-label">Mức độ nghiêm trọng:</label>
          <select v-model="severityFilter" class="form-control select-control">
            <option value="all">Tất cả</option>
            <option value="High">Nghiêm trọng</option>
            <option value="Medium">Trung bình</option>
            <option value="Low">Nhẹ</option>
          </select>
        </div>
      </div>

      <!-- Table -->
      <table class="data-table" v-if="filteredContraindications.length > 0" style="margin-top: 16px;">
        <thead>
          <tr>
            <th>Thuốc / Hoạt chất</th>
            <th>Điều kiện chống chỉ định</th>
            <th>Phân loại</th>
            <th>Mức độ</th>
            <th>Mô tả tác hại</th>
            <th>Khuyến cáo lâm sàng</th>
            <th style="text-align: center; width: 120px;" v-if="canManage">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in filteredContraindications" :key="c.ContraindicationId">
            <td>
              <span v-if="c.MedicineId" class="ing-tag-mini" style="background-color: rgba(59, 130, 246, 0.1); color: var(--info); border: 1px solid rgba(59, 130, 246, 0.15)">Thuốc</span>
              <span v-else class="ing-tag-mini" style="background-color: var(--primary-bg); color: var(--primary-medium); border: 1px solid rgba(16, 185, 129, 0.15)">Hoạt chất</span>
              <strong style="margin-left: 8px;">
                {{ c.MedicineId ? store.medicines.value.find(m => m.MedicineId === c.MedicineId)?.MedicineName : store.activeIngredients.value.find(ai => ai.IngredientId === c.IngredientId)?.IngredientName }}
              </strong>
            </td>
            <td>
              <span v-if="c.DiseaseId" class="tag warning" style="background-color: var(--warning-bg); color: var(--warning); padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 700;">
                {{ store.diseases.value.find(d => d.DiseaseId === c.DiseaseId)?.DiseaseName }}
              </span>
              <span v-else class="tag danger" style="background-color: var(--danger-bg); color: var(--danger); padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 700;">
                🤰 {{ c.ConditionType }}
              </span>
            </td>
            <td><small style="font-weight: 600; color: var(--text-muted);">{{ c.DiseaseId ? 'Bệnh nền (Disease)' : 'Đối tượng đặc biệt' }}</small></td>
            <td>
              <span :class="['status-tag', c.Severity === 'Nghiêm trọng' || c.Severity === 'High' ? 'danger' : c.Severity === 'Trung bình' || c.Severity === 'Medium' ? 'warning' : 'safe']">
                {{ c.Severity }}
              </span>
            </td>
            <td><small>{{ c.Description }}</small></td>
            <td><small class="green" style="color: var(--success); font-weight: 600;">{{ c.Recommendation }}</small></td>
            <td v-if="canManage">
              <div class="action-buttons-group">
                <button class="action-btn-icon edit" @click="openEditContra(c)" title="Chỉnh sửa">
                  ✏️
                </button>
                <button class="action-btn-icon delete" @click="deleteContra(c)" title="Xóa khỏi hệ thống">
                  🗑️
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Empty state -->
      <div class="empty-state-container flex-center" v-else style="margin-top: 16px;">
        <div class="empty-content">
          <span class="empty-icon">⚠️</span>
          <h4>Không tìm thấy chống chỉ định phù hợp</h4>
          <p>Hãy thử thay đổi từ khóa tìm kiếm hoặc bộ lọc.</p>
        </div>
      </div>
    </div>

    <!-- ==========================================
      TAB 2: DRUG INTERACTIONS
    ========================================== -->
    <div v-if="activeSubTab === 'interactions'" class="grid-card text-section">
      <h3 class="section-title">Danh mục Tương tác Hoạt chất chéo (Drug Interactions)</h3>
      <table class="data-table">
        <thead>
          <tr>
            <th>Hoạt chất A</th>
            <th>Hoạt chất B</th>
            <th>Mức độ</th>
            <th>Mô tả tương tác hại</th>
            <th>Khuyến cáo lâm sàng (Recommendation)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="di in store.drugInteractions.value" :key="di.InteractionId">
            <td><strong>{{ store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientAId)?.IngredientName }}</strong></td>
            <td><strong>{{ store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientBId)?.IngredientName }}</strong></td>
            <td>
              <span :class="['status-tag', di.Severity === 'Nghiêm trọng' || di.Severity === 'High' ? 'danger' : 'warning']">
                {{ di.Severity }}
              </span>
            </td>
            <td><small>{{ di.Description }}</small></td>
            <td><small class="green" style="color: var(--success); font-weight: 600;">{{ di.Recommendation }}</small></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ==========================================
      TAB 3: PATIENT ALLERGIES
    ========================================== -->
    <div v-if="activeSubTab === 'allergies'" class="grid-card text-section">
      <h3 class="section-title" style="margin-bottom: 20px;">Danh sách Dị ứng thuốc của bệnh nhân</h3>
      
      <!-- Search Panel -->
      <div class="filters-row-contra" style="margin-bottom: 16px;">
        <div class="filter-col-contra flex-1">
          <label class="filter-label">Tìm kiếm dị ứng:</label>
          <div class="search-input-wrapper">
            <input 
              type="text" 
              placeholder="Nhập tên bệnh nhân, số điện thoại, tên thuốc hoặc hoạt chất..." 
              class="form-control"
              v-model="allergySearchQuery" 
            />
            <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Table -->
      <table class="data-table" v-if="filteredAllergies.length > 0">
        <thead>
          <tr>
            <th>Bệnh nhân</th>
            <th>Loại dị ứng</th>
            <th>Tác nhân gây dị ứng</th>
            <th>Mức độ</th>
            <th>Ghi chú lâm sàng (Triệu chứng)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="pa in filteredAllergies" :key="pa.AllergyId">
            <td>
              <div style="display: flex; flex-direction: column;">
                <strong>{{ store.patients.value.find(p => p.PatientId === pa.PatientId)?.FullName || 'Không rõ' }}</strong>
                <small class="text-muted">{{ store.patients.value.find(p => p.PatientId === pa.PatientId)?.Phone }}</small>
              </div>
            </td>
            <td>
              <span :class="['ing-tag-mini', pa.IngredientId ? 'ingredient' : 'medicine']" :style="pa.IngredientId ? 'background-color: var(--primary-bg); color: var(--primary-medium); border: 1px solid rgba(16, 185, 129, 0.15)' : 'background-color: rgba(59, 130, 246, 0.1); color: var(--info); border: 1px solid rgba(59, 130, 246, 0.15)'">
                {{ pa.IngredientId ? 'Hoạt chất' : 'Biệt dược' }}
              </span>
            </td>
            <td>
              <strong>
                {{ pa.IngredientId 
                  ? store.activeIngredients.value.find(i => i.IngredientId === pa.IngredientId)?.IngredientName 
                  : store.medicines.value.find(m => m.MedicineId === pa.MedicineId)?.MedicineName || 'Tác nhân khác' }}
              </strong>
            </td>
            <td>
              <span :class="['status-tag', pa.Severity === 'Nghiêm trọng' || pa.Severity === 'High' ? 'danger' : pa.Severity === 'Trung bình' || pa.Severity === 'Medium' ? 'warning' : 'safe']">
                {{ pa.Severity || 'Nghiêm trọng' }}
              </span>
            </td>
            <td><small>{{ pa.AllergyNote || '-' }}</small></td>
          </tr>
        </tbody>
      </table>

      <!-- Empty state -->
      <div class="empty-state-container flex-center" v-else>
        <div class="empty-content">
          <span class="empty-icon">⚠️</span>
          <h4>Không tìm thấy lịch sử dị ứng phù hợp</h4>
        </div>
      </div>
    </div>

    <!-- ==========================================
      TAB 4: DISEASES CATALOG
    ========================================== -->
    <div v-if="activeSubTab === 'diseases'" class="grid-card text-section">
      <h3 class="section-title" style="margin-bottom: 20px;">Danh mục Bệnh lý nền mãn tính (Diseases Catalog)</h3>
      
      <!-- Search Panel -->
      <div class="filters-row-contra" style="margin-bottom: 16px;">
        <div class="filter-col-contra flex-1">
          <label class="filter-label">Tìm kiếm bệnh lý:</label>
          <div class="search-input-wrapper">
            <input 
              type="text" 
              placeholder="Nhập tên bệnh hoặc mô tả..." 
              class="form-control"
              v-model="diseaseSearchQuery" 
            />
            <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Table -->
      <table class="data-table" v-if="filteredDiseases.length > 0">
        <thead>
          <tr>
            <th style="width: 250px;">Tên bệnh nền</th>
            <th>Mô tả chuyên khoa</th>
            <th style="text-align: center; width: 180px;">Số bệnh nhân đang mắc</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="d in filteredDiseases" :key="d.DiseaseId">
            <td><strong>{{ d.DiseaseName }}</strong></td>
            <td><small>{{ d.Description || '-' }}</small></td>
            <td style="text-align: center;">
              <span class="status-tag info" style="font-weight: 700; padding: 4px 10px;">
                {{ getDiseasePatientsCount(d.DiseaseId) }} bệnh nhân
              </span>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Empty state -->
      <div class="empty-state-container flex-center" v-else>
        <div class="empty-content">
          <span class="empty-icon">⚠️</span>
          <h4>Không tìm thấy bệnh lý nền nào phù hợp</h4>
        </div>
      </div>
    </div>

    <!-- ==========================================
      MODAL: ADD / EDIT CONTRAINDICATION
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showContraModal">
      <div class="modal-card form-modal">
        <div class="modal-header">
          <div class="modal-title-area">
            <h3>{{ isEditingContra ? 'Chỉnh sửa chống chỉ định lâm sàng' : 'Thêm chống chỉ định lâm sàng mới' }}</h3>
          </div>
          <button class="close-modal-btn" @click="showContraModal = false">×</button>
        </div>

        <div class="modal-body scrollable-body">
          <div class="form-inputs-grid-contra">
            <!-- Target Classification Selection -->
            <div class="form-group-contra">
              <label class="form-label-contra">Phân loại đối tượng tác dụng:</label>
              <div class="radio-group-contra">
                <label class="radio-label-contra">
                  <input type="radio" value="medicine" v-model="formTargetType" />
                  Theo Thuốc thương mại
                </label>
                <label class="radio-label-contra" style="margin-left: 20px;">
                  <input type="radio" value="ingredient" v-model="formTargetType" />
                  Theo Hoạt chất chính
                </label>
              </div>
            </div>

            <!-- Dropdown for Medicine or Ingredient -->
            <div class="form-group-contra" v-if="formTargetType === 'medicine'">
              <label class="form-label-contra required-label-contra">Chọn thuốc thương mại:</label>
              <select v-model="formMedicineId" class="form-control select-control">
                <option v-for="med in store.medicines.value" :key="med.MedicineId" :value="med.MedicineId.toString()">
                  {{ med.MedicineName }} ({{ med.Strength }})
                </option>
              </select>
            </div>
            <div class="form-group-contra" v-else>
              <label class="form-label-contra required-label-contra">Chọn hoạt chất chính:</label>
              <select v-model="formIngredientId" class="form-control select-control">
                <option v-for="ing in store.activeIngredients.value" :key="ing.IngredientId" :value="ing.IngredientId.toString()">
                  {{ ing.IngredientName }}
                </option>
              </select>
            </div>

            <!-- Disease or Special target classification -->
            <div class="form-group-contra">
              <label class="form-label-contra">Phân loại điều kiện chống chỉ định:</label>
              <div class="radio-group-contra">
                <label class="radio-label-contra">
                  <input type="radio" value="Disease" v-model="formConditionType" />
                  Theo Bệnh nền (Diseases)
                </label>
                <label class="radio-label-contra" style="margin-left: 20px;">
                  <input type="radio" value="Special" v-model="formConditionType" />
                  Đối tượng đặc biệt (Mang thai, tuổi...)
                </label>
              </div>
            </div>

            <!-- Dropdown for Disease or Text input for Special target -->
            <div class="form-group-contra" v-if="formConditionType === 'Disease'">
              <label class="form-label-contra required-label-contra">Chọn bệnh nền:</label>
              <select v-model="formDiseaseId" class="form-control select-control">
                <option v-for="dis in store.diseases.value" :key="dis.DiseaseId" :value="dis.DiseaseId.toString()">
                  {{ dis.DiseaseName }} - {{ dis.Description }}
                </option>
              </select>
            </div>
            <div class="form-group-contra" v-else>
              <label class="form-label-contra required-label-contra">Tên đối tượng đặc biệt (ví dụ: Phụ nữ mang thai, Trẻ em &lt; 12 tuổi):</label>
              <input type="text" v-model="formSpecialConditionText" class="form-control" placeholder="Nhập tên đối tượng chống chỉ định..." />
            </div>

            <!-- Severity selection -->
            <div class="form-group-contra">
              <label class="form-label-contra">Mức độ nghiêm trọng:</label>
              <select v-model="formSeverity" class="form-control select-control">
                <option value="High">Nghiêm trọng (Nguy hiểm tính mạng / Chống chỉ định tuyệt đối)</option>
                <option value="Medium">Trung bình (Thận trọng cao / Cần có đơn giám sát)</option>
                <option value="Low">Nhẹ (Thận trọng nhẹ / Theo dõi triệu chứng)</option>
              </select>
            </div>

            <!-- Description -->
            <div class="form-group-contra">
              <label class="form-label-contra required-label-contra">Mô tả tác hại lâm sàng:</label>
              <textarea v-model="formDescription" class="form-control textarea-control" rows="3" placeholder="Mô tả cơ chế gây tác hại hoặc triệu chứng phát sinh nguy hiểm..."></textarea>
            </div>

            <!-- Recommendation -->
            <div class="form-group-contra">
              <label class="form-label-contra">Khuyến cáo lâm sàng cho dược sĩ:</label>
              <textarea v-model="formRecommendation" class="form-control textarea-control" rows="2" placeholder="Ví dụ: Đổi sang nhóm giảm đau khác không có NSAID, khuyên bệnh nhân đi khám chuyên khoa..."></textarea>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showContraModal = false">Hủy</button>
          <button class="primary-btn" @click="saveContra">Lưu lại</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.view-container {
  display: flex;
  flex-direction: column;
}
.grid-card {
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 24px;
}

/* Filters Row Contra Styling */
.filters-row-contra {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-top: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border-color);
}
.filter-col-contra {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 200px;
}
.filter-col-contra.flex-1 {
  flex: 1;
  min-width: 280px;
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
.action-btn-icon.edit:hover {
  background-color: rgba(245, 158, 11, 0.1);
  border-color: var(--warning);
}
.action-btn-icon.delete:hover {
  background-color: rgba(239, 68, 68, 0.1);
  border-color: var(--danger);
}

/* Empty State Styling */
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

/* Modal Form Styles */
.form-modal {
  width: 100%;
  max-width: 650px;
}
.scrollable-body {
  max-height: 65vh;
  overflow-y: auto;
  padding-right: 6px;
}
.form-inputs-grid-contra {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 20px;
}
.form-group-contra {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.form-label-contra {
  font-size: 13px;
  font-weight: 700;
  color: var(--text-muted);
}
.required-label-contra::after {
  content: " *";
  color: var(--danger);
  font-weight: bold;
}
.radio-group-contra {
  display: flex;
  align-items: center;
  padding: 6px 0;
}
.radio-label-contra {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
  cursor: pointer;
}
.textarea-control {
  resize: vertical;
}

/* Tabs navigation styles */
.tabs-header-row {
  display: flex;
  gap: 8px;
  border-bottom: 2px solid var(--border-color);
  margin-bottom: 20px;
}
.tab-btn {
  background: transparent;
  border: none;
  border-bottom: 3px solid transparent;
  padding: 10px 20px;
  font-size: 14px;
  font-weight: 700;
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 6px;
}
.tab-btn:hover {
  color: var(--text-main);
}
.tab-btn.active {
  color: var(--primary-medium);
  border-bottom-color: var(--primary-medium);
}
</style>
