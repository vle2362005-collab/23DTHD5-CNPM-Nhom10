<script setup lang="ts">
import { ref, computed } from 'vue'
import { usePharmacyStore, type DrugInteraction, type Contraindication } from '../store/pharmacy'

const store = usePharmacyStore()

// Sub-tabs selection: interactions (Tương tác) or contraindications (Chống chỉ định)
const activeSubTab = ref<'interactions' | 'contraindications'>('interactions')

// Search & filter
const searchQuery = ref('')

// Modals display states
const showInteractionModal = ref(false)
const showContraindicationModal = ref(false)

// Edit mode states
const isEditing = ref(false)
const selectedInteractionId = ref<number | null>(null)
const selectedContraindicationId = ref<number | null>(null)

// Form fields: Drug Interactions
const ingredientAId = ref<string>('')
const ingredientBId = ref<string>('')
const interactionSeverity = ref<string>('Nghiêm trọng')
const interactionDescription = ref('')
const interactionRecommendation = ref('')

// Form fields: Contraindications
const contraTargetType = ref<'medicine' | 'ingredient'>('medicine')
const contraMedicineId = ref<string>('')
const contraIngredientId = ref<string>('')
const contraConditionType = ref<string>('Disease')
const contraDiseaseId = ref<string>('')
const contraSeverity = ref<string>('Nghiêm trọng')
const contraDescription = ref('')
const contraRecommendation = ref('')

// Authorization computed helper
const canManage = computed(() => {
  return store.currentRole.value === 'admin'
})

// Active ingredients sorted list for select dropdowns
const sortedIngredients = computed(() => {
  return [...store.activeIngredients.value].sort((a, b) => a.IngredientName.localeCompare(b.IngredientName))
})

// Medicines sorted list
const sortedMedicines = computed(() => {
  return [...store.medicines.value].sort((a, b) => a.MedicineName.localeCompare(b.MedicineName))
})

// Diseases sorted list
const sortedDiseases = computed(() => {
  return [...store.diseases.value].sort((a, b) => a.DiseaseName.localeCompare(b.DiseaseName))
})

const severityFilter = ref<string>('All')
const contraSeverityFilter = ref<string>('All')

// Thống kê tương tác hoạt chất
const interactionStats = computed(() => {
  const list = store.drugInteractions.value
  const high = list.filter(di => {
    const s = di.Severity.toLowerCase()
    return s.includes('nghiêm trọng') || s.includes('cao') || s.includes('high')
  }).length
  const medium = list.filter(di => {
    const s = di.Severity.toLowerCase()
    return s.includes('trung bình') || s.includes('vừa') || s.includes('medium')
  }).length
  const low = list.length - high - medium
  return { total: list.length, high, medium, low }
})

// Thống kê chống chỉ định
const contraindicationStats = computed(() => {
  const list = store.contraindications.value
  const disease = list.filter(c => c.DiseaseId).length
  const special = list.filter(c => c.ConditionType === 'Đối tượng đặc biệt').length
  return { total: list.length, disease, special }
})

// Filtered drug interactions list
const filteredInteractions = computed(() => {
  const query = searchQuery.value.toLowerCase().trim()
  return store.drugInteractions.value.filter(di => {
    const ingAName = store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientAId)?.IngredientName.toLowerCase() || ''
    const ingBName = store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientBId)?.IngredientName.toLowerCase() || ''
    const desc = di.Description?.toLowerCase() || ''
    const recom = di.Recommendation?.toLowerCase() || ''

    const matchesQuery = ingAName.includes(query) ||
      ingBName.includes(query) ||
      desc.includes(query) ||
      recom.includes(query)

    const matchesSeverity = severityFilter.value === 'All' || di.Severity === severityFilter.value

    return matchesQuery && matchesSeverity
  })
})

// Filtered contraindications list
const filteredContraindications = computed(() => {
  const query = searchQuery.value.toLowerCase().trim()
  return store.contraindications.value.filter(c => {
    const medName = c.MedicineId ? store.medicines.value.find(m => m.MedicineId === c.MedicineId)?.MedicineName.toLowerCase() || '' : ''
    const ingName = c.IngredientId ? store.activeIngredients.value.find(ai => ai.IngredientId === c.IngredientId)?.IngredientName.toLowerCase() || '' : ''
    const diseaseName = c.DiseaseId ? store.diseases.value.find(d => d.DiseaseId === c.DiseaseId)?.DiseaseName.toLowerCase() || '' : ''
    const condType = c.ConditionType?.toLowerCase() || ''
    const desc = c.Description?.toLowerCase() || ''
    const recom = c.Recommendation?.toLowerCase() || ''

    const matchesQuery = medName.includes(query) ||
      ingName.includes(query) ||
      diseaseName.includes(query) ||
      condType.includes(query) ||
      desc.includes(query) ||
      recom.includes(query)

    const matchesSeverity = contraSeverityFilter.value === 'All' || c.Severity === contraSeverityFilter.value

    return matchesQuery && matchesSeverity
  })
})

// ==========================================
// DRUG INTERACTIONS HANDLERS
// ==========================================
const openAddInteraction = () => {
  if (!canManage.value) return
  isEditing.value = false
  selectedInteractionId.value = null
  ingredientAId.value = store.activeIngredients.value[0]?.IngredientId.toString() || ''
  ingredientBId.value = store.activeIngredients.value[1]?.IngredientId.toString() || ''
  interactionSeverity.value = 'Nghiêm trọng'
  interactionDescription.value = ''
  interactionRecommendation.value = ''
  showInteractionModal.value = true
}

const openEditInteraction = (di: DrugInteraction) => {
  if (!canManage.value) return
  isEditing.value = true
  selectedInteractionId.value = di.InteractionId
  ingredientAId.value = di.IngredientAId.toString()
  ingredientBId.value = di.IngredientBId.toString()
  interactionSeverity.value = di.Severity
  interactionDescription.value = di.Description || ''
  interactionRecommendation.value = di.Recommendation || ''
  showInteractionModal.value = true
}

const saveInteraction = async () => {
  if (ingredientAId.value === ingredientBId.value) {
    alert('Hoạt chất A và Hoạt chất B không được trùng nhau!')
    return
  }

  const diData = {
    IngredientAId: Number(ingredientAId.value),
    IngredientBId: Number(ingredientBId.value),
    Severity: interactionSeverity.value,
    Description: interactionDescription.value.trim() || null,
    Recommendation: interactionRecommendation.value.trim() || null
  }

  try {
    if (isEditing.value && selectedInteractionId.value !== null) {
      await store.updateDrugInteractionStore(selectedInteractionId.value, {
        InteractionId: selectedInteractionId.value,
        ...diData
      })
    } else {
      await store.addDrugInteraction(diData)
    }
    showInteractionModal.value = false
  } catch (err: any) {
    alert(err.message || 'Lỗi khi lưu tương tác thuốc!')
  }
}

const deleteInteraction = async (di: DrugInteraction) => {
  if (!canManage.value) return
  const ingA = store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientAId)?.IngredientName || ''
  const ingB = store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientBId)?.IngredientName || ''
  
  if (confirm(`Bạn có chắc chắn muốn xóa tương tác giữa "${ingA}" và "${ingB}"?`)) {
    try {
      await store.deleteDrugInteractionStore(di.InteractionId)
    } catch (err: any) {
      alert(err.message || 'Lỗi khi xóa tương tác thuốc!')
    }
  }
}

// ==========================================
// CONTRAINDICATIONS HANDLERS
// ==========================================
const openAddContraindication = () => {
  if (!canManage.value) return
  isEditing.value = false
  selectedContraindicationId.value = null
  contraTargetType.value = 'medicine'
  contraMedicineId.value = store.medicines.value[0]?.MedicineId.toString() || ''
  contraIngredientId.value = store.activeIngredients.value[0]?.IngredientId.toString() || ''
  contraConditionType.value = 'Disease'
  contraDiseaseId.value = store.diseases.value[0]?.DiseaseId.toString() || ''
  contraSeverity.value = 'Nghiêm trọng'
  contraDescription.value = ''
  contraRecommendation.value = ''
  showContraindicationModal.value = true
}

const openEditContraindication = (c: Contraindication) => {
  if (!canManage.value) return
  isEditing.value = true
  selectedContraindicationId.value = c.ContraindicationId
  
  if (c.MedicineId) {
    contraTargetType.value = 'medicine'
    contraMedicineId.value = c.MedicineId.toString()
  } else {
    contraTargetType.value = 'ingredient'
    contraIngredientId.value = c.IngredientId?.toString() || ''
  }
  
  contraConditionType.value = c.ConditionType || 'Disease'
  contraDiseaseId.value = c.DiseaseId?.toString() || ''
  contraSeverity.value = c.Severity
  contraDescription.value = c.Description || ''
  contraRecommendation.value = c.Recommendation || ''
  showContraindicationModal.value = true
}

const saveContraindication = async () => {
  const isMed = contraTargetType.value === 'medicine'
  const isDisease = contraConditionType.value === 'Disease'

  const cData = {
    MedicineId: isMed ? Number(contraMedicineId.value) : null,
    IngredientId: !isMed ? Number(contraIngredientId.value) : null,
    DiseaseId: isDisease ? Number(contraDiseaseId.value) : null,
    ConditionType: contraConditionType.value,
    Severity: contraSeverity.value,
    Description: contraDescription.value.trim() || null,
    Recommendation: contraRecommendation.value.trim() || null
  }

  try {
    if (isEditing.value && selectedContraindicationId.value !== null) {
      await store.updateContraindicationStore(selectedContraindicationId.value, {
        ContraindicationId: selectedContraindicationId.value,
        ...cData
      })
    } else {
      await store.addContraindication(cData)
    }
    showContraindicationModal.value = false
  } catch (err: any) {
    alert(err.message || 'Lỗi khi lưu chống chỉ định!')
  }
}

const deleteContraindication = async (c: Contraindication) => {
  if (!canManage.value) return
  
  const targetName = c.MedicineId 
    ? store.medicines.value.find(m => m.MedicineId === c.MedicineId)?.MedicineName
    : store.activeIngredients.value.find(ai => ai.IngredientId === c.IngredientId)?.IngredientName
    
  if (confirm(`Bạn có chắc chắn muốn xóa chống chỉ định đối với "${targetName}"?`)) {
    try {
      await store.deleteContraindicationStore(c.ContraindicationId)
    } catch (err: any) {
      alert(err.message || 'Lỗi khi xóa chống chỉ định!')
    }
  }
}

const getSeverityClass = (severity: string) => {
  const s = severity.toLowerCase()
  if (s.includes('nghiêm trọng') || s.includes('cao') || s.includes('high')) return 'danger'
  if (s.includes('trung bình') || s.includes('vừa') || s.includes('medium')) return 'warning'
  return 'info'
}
</script>

<template>
  <div class="view-container">
    <!-- Sub tabs Selector -->
    <div class="tabs-navigation">
      <div class="tabs-list">
        <button 
          :class="['tab-btn', { active: activeSubTab === 'interactions' }]" 
          @click="activeSubTab = 'interactions'"
        >
          <svg viewBox="0 0 24 24" class="tab-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M7.5 8a2.5 2.5 0 100-5 2.5 2.5 0 000 5zM16.5 8a2.5 2.5 0 100-5 2.5 2.5 0 000 5zM6 21h3m10 0h-3M12 3v18M12 12H3m18 0h-9" />
          </svg>
          Tương tác hoạt chất (Side Effects)
          <span class="tab-count-badge">{{ store.drugInteractions.value.length }}</span>
        </button>
        <button 
          :class="['tab-btn', { active: activeSubTab === 'contraindications' }]" 
          @click="activeSubTab = 'contraindications'"
        >
          <svg viewBox="0 0 24 24" class="tab-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          Chống chỉ định điều trị
          <span class="tab-count-badge">{{ store.contraindications.value.length }}</span>
        </button>
      </div>
    </div>

    <!-- Clinical Dashboard Interactive Stats -->
    <div v-if="activeSubTab === 'interactions'" class="stats-row">
      <div class="stat-card total">
        <div class="stat-icon-wrapper">🧪</div>
        <div class="stat-content">
          <span class="stat-label">Tổng cặp tương tác</span>
          <span class="stat-value">{{ interactionStats.total }}</span>
        </div>
      </div>
      <div class="stat-card high">
        <div class="stat-icon-wrapper">🔴</div>
        <div class="stat-content">
          <span class="stat-label">Nghiêm trọng (High)</span>
          <span class="stat-value text-danger">{{ interactionStats.high }}</span>
        </div>
      </div>
      <div class="stat-card medium">
        <div class="stat-icon-wrapper">🟡</div>
        <div class="stat-content">
          <span class="stat-label">Trung bình (Medium)</span>
          <span class="stat-value text-warning">{{ interactionStats.medium }}</span>
        </div>
      </div>
      <div class="stat-card low">
        <div class="stat-icon-wrapper">🔵</div>
        <div class="stat-content">
          <span class="stat-label">Nhẹ (Low)</span>
          <span class="stat-value text-info">{{ interactionStats.low }}</span>
        </div>
      </div>
    </div>
    <div v-else class="stats-row">
      <div class="stat-card total">
        <div class="stat-icon-wrapper">⚠️</div>
        <div class="stat-content">
          <span class="stat-label">Tổng chống chỉ định</span>
          <span class="stat-value">{{ contraindicationStats.total }}</span>
        </div>
      </div>
      <div class="stat-card disease">
        <div class="stat-icon-wrapper">🤒</div>
        <div class="stat-content">
          <span class="stat-label">Do bệnh nền</span>
          <span class="stat-value text-warning">{{ contraindicationStats.disease }}</span>
        </div>
      </div>
      <div class="stat-card special">
        <div class="stat-icon-wrapper">🤰</div>
        <div class="stat-content">
          <span class="stat-label">Đối tượng đặc biệt</span>
          <span class="stat-value text-danger">{{ contraindicationStats.special }}</span>
        </div>
      </div>
    </div>

    <!-- Actions Area -->
    <div class="table-container">
      <div class="table-actions">
        <div class="search-and-filters">
          <div class="search-wrapper">
            <svg viewBox="0 0 24 24" class="search-icon" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="11" cy="11" r="8" />
              <line x1="21" y1="21" x2="16.65" y2="16.65" />
            </svg>
            <input 
              type="text" 
              class="search-input-small form-control-with-icon" 
              v-model="searchQuery" 
              :placeholder="activeSubTab === 'interactions' ? 'Tìm hoạt chất, mô tả...' : 'Tìm thuốc, hoạt chất, bệnh nền...'"
            />
          </div>
          
          <div class="filter-wrapper" v-if="activeSubTab === 'interactions'">
            <select class="filter-select form-control" v-model="severityFilter">
              <option value="All">Tất cả mức độ</option>
              <option value="Nghiêm trọng">Nghiêm trọng (High)</option>
              <option value="Trung bình">Trung bình (Medium)</option>
              <option value="Nhẹ">Nhẹ (Low)</option>
            </select>
          </div>
          <div class="filter-wrapper" v-else>
            <select class="filter-select form-control" v-model="contraSeverityFilter">
              <option value="All">Tất cả mức độ</option>
              <option value="Nghiêm trọng">Nghiêm trọng (High)</option>
              <option value="Trung bình">Trung bình (Medium)</option>
              <option value="Nhẹ">Nhẹ (Low)</option>
            </select>
          </div>
        </div>
        
        <div v-if="canManage">
          <button v-if="activeSubTab === 'interactions'" class="primary-btn add-btn" @click="openAddInteraction">
            <svg viewBox="0 0 24 24" class="btn-icon" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            Thêm cặp tương tác
          </button>
          <button v-else class="primary-btn add-btn" @click="openAddContraindication">
            <svg viewBox="0 0 24 24" class="btn-icon" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            Thêm chống chỉ định
          </button>
        </div>
      </div>

      <!-- Tab Content: Drug Interactions -->
      <div v-if="activeSubTab === 'interactions'" class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th>Cặp hoạt chất đối kháng</th>
              <th style="width: 150px; text-align: center;">Mức độ cảnh báo</th>
              <th>Mô tả tương tác tác hại</th>
              <th>Khuyến cáo lâm sàng</th>
              <th v-if="canManage" style="width: 120px; text-align: center;">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="di in filteredInteractions" :key="di.InteractionId">
              <td>
                <div class="interaction-pair-cell">
                  <span class="ing-badge ing-a">
                    {{ store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientAId)?.IngredientName || `Mã #${di.IngredientAId}` }}
                  </span>
                  <div class="interaction-flow-arrow">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="flow-icon">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M7.5 21L3 16.5m0 0L7.5 12M3 16.5h13.5m0-13.5L21 7.5m0 0L16.5 12M21 7.5H7.5" />
                    </svg>
                  </div>
                  <span class="ing-badge ing-b">
                    {{ store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientBId)?.IngredientName || `Mã #${di.IngredientBId}` }}
                  </span>
                </div>
              </td>
              <td style="text-align: center;">
                <span :class="['status-tag', getSeverityClass(di.Severity)]">
                  {{ di.Severity }}
                </span>
              </td>
              <td class="text-muted"><small>{{ di.Description || 'Không có mô tả' }}</small></td>
              <td class="clinical-recom"><small>{{ di.Recommendation || 'Không có khuyến cáo' }}</small></td>
              <td v-if="canManage" style="text-align: center;">
                <div class="action-buttons-flex">
                  <button class="action-edit-btn" @click="openEditInteraction(di)" title="Sửa tương tác">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M12 20h9M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z" />
                    </svg>
                  </button>
                  <button class="action-delete-btn" @click="deleteInteraction(di)" title="Xóa tương tác">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <polyline points="3 6 5 6 21 6" />
                      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredInteractions.length === 0">
              <td :colspan="canManage ? 5 : 4" class="empty-placeholder">
                Không tìm thấy thông tin tương tác hoạt chất nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Tab Content: Contraindications -->
      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th>Đối tượng áp dụng</th>
              <th>Điều kiện chống chỉ định</th>
              <th style="width: 130px;">Phân loại đối tượng</th>
              <th style="width: 120px; text-align: center;">Mức độ</th>
              <th>Mô tả chống chỉ định</th>
              <th>Khuyến cáo lâm sàng</th>
              <th v-if="canManage" style="width: 120px; text-align: center;">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in filteredContraindications" :key="c.ContraindicationId">
              <td>
                <span v-if="c.MedicineId" class="tag-med">
                  💊 {{ store.medicines.value.find(m => m.MedicineId === c.MedicineId)?.MedicineName || 'Thuốc không rõ' }}
                </span>
                <span v-else-if="c.IngredientId" class="tag-ing">
                  🧪 {{ store.activeIngredients.value.find(ai => ai.IngredientId === c.IngredientId)?.IngredientName || 'Hoạt chất không rõ' }}
                </span>
              </td>
              <td>
                <span v-if="c.DiseaseId" class="badge-disease">
                  ⚠️ {{ store.diseases.value.find(d => d.DiseaseId === c.DiseaseId)?.DiseaseName || 'Bệnh nền không rõ' }}
                </span>
                <span v-else class="badge-special">
                  🤰 Đối tượng đặc biệt (Mang thai...)
                </span>
              </td>
              <td><span class="type-text">{{ c.ConditionType }}</span></td>
              <td style="text-align: center;">
                <span :class="['status-tag', getSeverityClass(c.Severity)]">
                  {{ c.Severity }}
                </span>
              </td>
              <td class="text-muted"><small>{{ c.Description || 'Không có mô tả' }}</small></td>
              <td class="clinical-recom"><small>{{ c.Recommendation || 'Không có khuyến cáo' }}</small></td>
              <td v-if="canManage" style="text-align: center;">
                <div class="action-buttons-flex">
                  <button class="action-edit-btn" @click="openEditContraindication(c)" title="Sửa chống chỉ định">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M12 20h9M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z" />
                    </svg>
                  </button>
                  <button class="action-delete-btn" @click="deleteContraindication(c)" title="Xóa chống chỉ định">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <polyline points="3 6 5 6 21 6" />
                      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredContraindications.length === 0">
              <td :colspan="canManage ? 7 : 6" class="empty-placeholder">
                Không tìm thấy dữ liệu chống chỉ định nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ==========================================
         MODAL FORM: DRUG INTERACTION
         ========================================== -->
    <div v-if="showInteractionModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="modal-header">
          <h3 class="modal-title">{{ isEditing ? 'Cập nhật tương tác hoạt chất' : 'Thêm tương tác hoạt chất mới' }}</h3>
          <button class="close-btn" @click="showInteractionModal = false">&times;</button>
        </div>
        <div class="modal-body">
          <div class="form-row">
            <div class="form-group flex-1">
              <label class="form-label required">Hoạt chất A</label>
              <select class="form-control select-control" v-model="ingredientAId">
                <option v-for="ing in sortedIngredients" :key="ing.IngredientId" :value="ing.IngredientId.toString()">
                  {{ ing.IngredientName }}
                </option>
              </select>
            </div>
            <div class="form-group flex-1" style="margin-top: 12px;">
              <label class="form-label required">Hoạt chất B</label>
              <select class="form-control select-control" v-model="ingredientBId">
                <option v-for="ing in sortedIngredients" :key="ing.IngredientId" :value="ing.IngredientId.toString()">
                  {{ ing.IngredientName }}
                </option>
              </select>
            </div>
          </div>
          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label required">Mức độ nghiêm trọng</label>
            <select class="form-control select-control" v-model="interactionSeverity">
              <option value="Nghiêm trọng">Nghiêm trọng (High)</option>
              <option value="Trung bình">Trung bình (Medium)</option>
              <option value="Nhẹ">Nhẹ (Low)</option>
            </select>
          </div>
          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label">Mô tả phản ứng / tác hại</label>
            <textarea 
              class="form-control" 
              rows="3" 
              v-model="interactionDescription"
              placeholder="Chi tiết về độc tính lâm sàng khi phối trộn hai hoạt chất này..."
            ></textarea>
          </div>
          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label">Khuyến cáo lâm sàng</label>
            <textarea 
              class="form-control" 
              rows="3" 
              v-model="interactionRecommendation"
              placeholder="Ví dụ: Không dùng chung, hoặc uống cách nhau tối thiểu 2 giờ..."
            ></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="secondary-btn" @click="showInteractionModal = false">Hủy</button>
          <button class="primary-btn" @click="saveInteraction">
            {{ isEditing ? 'Cập nhật' : 'Thêm mới' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ==========================================
         MODAL FORM: CONTRAINDICATION
         ========================================== -->
    <div v-if="showContraindicationModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="modal-header">
          <h3 class="modal-title">{{ isEditing ? 'Cập nhật chống chỉ định' : 'Thêm chống chỉ định mới' }}</h3>
          <button class="close-btn" @click="showContraindicationModal = false">&times;</button>
        </div>
        <div class="modal-body scrollable-modal-body">
          <div class="form-group">
            <label class="form-label required">Phân loại đối tượng áp dụng</label>
            <div class="radio-toggle-group">
              <label class="radio-label">
                <input type="radio" value="medicine" v-model="contraTargetType" />
                <span>Theo Thuốc cụ thể</span>
              </label>
              <label class="radio-label">
                <input type="radio" value="ingredient" v-model="contraTargetType" />
                <span>Theo Hoạt chất chung</span>
              </label>
            </div>
          </div>

          <div v-if="contraTargetType === 'medicine'" class="form-group" style="margin-top: 14px;">
            <label class="form-label required">Chọn thuốc</label>
            <select class="form-control select-control" v-model="contraMedicineId">
              <option v-for="med in sortedMedicines" :key="med.MedicineId" :value="med.MedicineId.toString()">
                {{ med.MedicineName }}
              </option>
            </select>
          </div>

          <div v-else class="form-group" style="margin-top: 14px;">
            <label class="form-label required">Chọn hoạt chất</label>
            <select class="form-control select-control" v-model="contraIngredientId">
              <option v-for="ing in sortedIngredients" :key="ing.IngredientId" :value="ing.IngredientId.toString()">
                {{ ing.IngredientName }}
              </option>
            </select>
          </div>

          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label required">Kiểu điều kiện cảnh báo</label>
            <select class="form-control select-control" v-model="contraConditionType">
              <option value="Disease">Chống chỉ định Bệnh nền (Disease)</option>
              <option value="Đối tượng đặc biệt">Đối tượng đặc biệt (Mang thai, cho con bú...)</option>
            </select>
          </div>

          <div v-if="contraConditionType === 'Disease'" class="form-group" style="margin-top: 14px;">
            <label class="form-label required">Chọn bệnh nền chống chỉ định</label>
            <select class="form-control select-control" v-model="contraDiseaseId">
              <option v-for="dis in sortedDiseases" :key="dis.DiseaseId" :value="dis.DiseaseId.toString()">
                {{ dis.DiseaseName }}
              </option>
            </select>
          </div>

          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label required">Mức độ cảnh báo</label>
            <select class="form-control select-control" v-model="contraSeverity">
              <option value="Nghiêm trọng">Nghiêm trọng (High)</option>
              <option value="Trung bình">Trung bình (Medium)</option>
              <option value="Nhẹ">Nhẹ (Low)</option>
            </select>
          </div>

          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label">Mô tả tác hại/cảnh báo</label>
            <textarea 
              class="form-control" 
              rows="3" 
              v-model="contraDescription"
              placeholder="Nguy cơ chảy máu, suy gan cấp hoặc ức chế hô hấp..."
            ></textarea>
          </div>

          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label">Khuyến cáo xử lý</label>
            <textarea 
              class="form-control" 
              rows="3" 
              v-model="contraRecommendation"
              placeholder="Ví dụ: Thay đổi sang thuốc giảm đau an toàn khác..."
            ></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="secondary-btn" @click="showContraindicationModal = false">Hủy</button>
          <button class="primary-btn" @click="saveContraindication">
            {{ isEditing ? 'Cập nhật' : 'Thêm mới' }}
          </button>
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

/* Sub-tabs styles */
.tabs-navigation {
  border-bottom: 2px solid var(--border-color);
  padding-bottom: 2px;
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

/* Search and Add controls */
.search-wrapper {
  position: relative;
  width: 320px;
}

.search-icon {
  position: absolute;
  left: 14px;
  top: 50%;
  transform: translateY(-50%);
  width: 18px;
  height: 18px;
  color: var(--text-muted);
}

.form-control-with-icon {
  padding-left: 42px !important;
}

.add-btn {
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn-icon {
  width: 16px;
  height: 16px;
}

/* Specific styling tags */
.clinical-recom {
  color: var(--success);
  font-weight: 600;
  max-width: 320px;
}

.tag-med {
  color: var(--text-main);
  font-weight: 600;
}

.tag-ing {
  color: var(--info);
  font-weight: 600;
}

.badge-disease {
  background-color: var(--warning-bg);
  color: var(--warning);
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
}

.badge-special {
  background-color: var(--danger-bg);
  color: var(--danger);
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
}

.type-text {
  font-size: 13px;
  color: var(--text-muted);
  text-transform: capitalize;
}

.action-buttons-flex {
  display: flex;
  justify-content: center;
  gap: 8px;
}

.action-edit-btn, .action-delete-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-sm);
  color: var(--text-muted);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.action-edit-btn:hover {
  color: var(--primary-medium);
  border-color: var(--primary-medium);
  background-color: var(--primary-bg);
}

.action-delete-btn:hover {
  color: var(--danger);
  border-color: var(--danger);
  background-color: var(--danger-bg);
}

.action-edit-btn svg, .action-delete-btn svg {
  width: 16px;
  height: 16px;
}

.empty-placeholder {
  padding: 40px !important;
  text-align: center;
  color: var(--text-muted);
  font-style: italic;
  font-size: 14px;
}

/* Modals styles */
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(15, 23, 42, 0.3);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
  animation: fadeIn 0.2s ease-out;
}

.modal-card {
  background-color: var(--bg-card);
  width: 100%;
  max-width: 500px;
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-premium);
  border: 1px solid var(--border-color);
  overflow: hidden;
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid var(--border-color);
}

.modal-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
}

.close-btn {
  font-size: 24px;
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  line-height: 1;
}

.close-btn:hover {
  color: var(--text-main);
}

.modal-body {
  padding: 24px;
}

.scrollable-modal-body {
  max-height: 70vh;
  overflow-y: auto;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-row {
  display: flex;
  flex-direction: column;
}

.required::after {
  content: ' *';
  color: var(--danger);
}

.radio-toggle-group {
  display: flex;
  gap: 16px;
  margin-top: 8px;
}

.radio-label {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 16px 24px;
  border-top: 1px solid var(--border-color);
  background-color: var(--bg-main);
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from { transform: translateY(20px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

/* Clinical Dashboard Stats Styling */
.stats-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 20px;
}
.stat-card {
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-lg);
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 14px;
  box-shadow: var(--shadow-sm);
  transition: transform var(--transition-fast), box-shadow var(--transition-fast);
}
.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}
.stat-icon-wrapper {
  font-size: 24px;
  width: 48px;
  height: 48px;
  background-color: var(--bg-main);
  border-radius: var(--border-radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
}
.stat-content {
  display: flex;
  flex-direction: column;
}
.stat-label {
  font-size: 12px;
  font-weight: 700;
  color: var(--text-muted);
}
.stat-value {
  font-size: 20px;
  font-weight: 800;
  color: var(--text-main);
  margin-top: 2px;
}

/* Severity text coloring helpers */
.text-danger {
  color: var(--danger) !important;
}
.text-warning {
  color: var(--warning) !important;
}
.text-info {
  color: var(--info) !important;
}

/* Search and Filters wrapper styling */
.search-and-filters {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}
.filter-wrapper {
  width: 180px;
}
.filter-select {
  height: 38px;
  font-size: 13.5px;
  font-weight: 600;
  border-color: var(--border-color);
}

/* Visual Interaction Link Cell Styling */
.interaction-pair-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}
.ing-badge {
  display: inline-block;
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 700;
  border-radius: var(--border-radius-md);
  border: 1px solid transparent;
  white-space: nowrap;
}
.ing-badge.ing-a {
  background-color: rgba(13, 148, 136, 0.05);
  color: var(--primary-medium);
  border-color: rgba(13, 148, 136, 0.15);
}
.ing-badge.ing-b {
  background-color: rgba(59, 130, 246, 0.05);
  color: var(--info);
  border-color: rgba(59, 130, 246, 0.15);
}
.interaction-flow-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
}
.flow-icon {
  width: 18px;
  height: 18px;
}

@media (max-width: 768px) {
  .table-actions {
    flex-direction: column;
    align-items: stretch !important;
  }
  
  .search-wrapper {
    width: 100%;
  }
  
  .add-btn {
    width: 100%;
    justify-content: center;
  }
}
</style>
