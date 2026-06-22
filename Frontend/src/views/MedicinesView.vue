<script setup lang="ts">
import { ref, computed } from 'vue'
import { usePharmacyStore, type Medicine } from '../store/pharmacy'

const store = usePharmacyStore()

// Sub-tabs State
const activeSubTab = ref<'medicines' | 'ingredients' | 'druggroups'>('medicines')

// State for search and filters (Medicines)
const searchQuery = ref('')
const selectedGroupId = ref<string>('all')
const requiresPrescriptionFilter = ref<string>('all')
const statusFilter = ref<string>('all')

// Modals State (Medicines)
const showDetailModal = ref(false)
const showFormModal = ref(false)
const selectedMed = ref<Medicine | null>(null)
const isEditing = ref(false)

// Form Fields State (Medicines)
const formMedicineId = ref<number | null>(null)
const formMedicineName = ref('')
const formDrugGroupId = ref<string>('')
const formStrength = ref('')
const formDosageForm = ref('')
const formUnit = ref('')
const formPrice = ref<number>(0)
const formRequiresPrescription = ref(false)
const formIsActive = ref(true)
const formNote = ref('')
const formSideEffects = ref('') // Added for CNPM-76
const formIngredients = ref<{ IngredientId: number; Amount: string }[]>([])

// Active Ingredients States
const ingSearchQuery = ref('')
const showIngModal = ref(false)
const isEditingIng = ref(false)
const formIngId = ref<number | null>(null)
const formIngName = ref('')
const formIngDescription = ref('')

// Drug Groups States
const groupSearchQuery = ref('')
const showGroupModal = ref(false)
const isEditingGroup = ref(false)
const formGroupId = ref<number | null>(null)
const formGroupName = ref('')
const formGroupDescription = ref('')

// Helper: check user permission
const canManage = computed(() => {
  return store.currentRole.value === 'admin' || store.currentRole.value === 'manager'
})

// Filtered medicines list
const filteredMedicines = computed(() => {
  return store.medicines.value.filter(med => {
    // 1. Search Query
    const query = searchQuery.value.toLowerCase().trim()
    let matchesSearch = false
    if (!query) {
      matchesSearch = true
    } else {
      // Search by medicine name
      const nameMatch = med.MedicineName.toLowerCase().includes(query)
      
      // Search by active ingredient name
      const matchingIngredients = store.medicineIngredients.value.filter(mi => mi.MedicineId === med.MedicineId)
      const ingredientMatch = matchingIngredients.some(mi => {
        const ing = store.activeIngredients.value.find(ai => ai.IngredientId === mi.IngredientId)
        return ing && ing.IngredientName.toLowerCase().includes(query)
      })

      matchesSearch = nameMatch || ingredientMatch
    }

    // 2. Group Filter
    const matchesGroup = selectedGroupId.value === 'all' || med.DrugGroupId === Number(selectedGroupId.value)

    // 3. Prescription Filter
    let matchesPrescription = true
    if (requiresPrescriptionFilter.value === 'true') {
      matchesPrescription = med.RequiresPrescription
    } else if (requiresPrescriptionFilter.value === 'false') {
      matchesPrescription = !med.RequiresPrescription
    }

    // 4. Status Filter
    let matchesStatus = true
    if (statusFilter.value === 'true') {
      matchesStatus = med.IsActive
    } else if (statusFilter.value === 'false') {
      matchesStatus = !med.IsActive
    }

    return matchesSearch && matchesGroup && matchesPrescription && matchesStatus
  })
})

// Filtered Active Ingredients list
const filteredIngredients = computed(() => {
  const query = ingSearchQuery.value.toLowerCase().trim()
  if (!query) return store.activeIngredients.value
  return store.activeIngredients.value.filter(
    ing => ing.IngredientName.toLowerCase().includes(query) || 
           (ing.Description && ing.Description.toLowerCase().includes(query))
  )
})

// Filtered Drug Groups list
const filteredDrugGroups = computed(() => {
  const query = groupSearchQuery.value.toLowerCase().trim()
  if (!query) return store.drugGroups.value
  return store.drugGroups.value.filter(
    dg => dg.GroupName.toLowerCase().includes(query) || 
          (dg.Description && dg.Description.toLowerCase().includes(query))
  )
})

// Relation counters
const getIngredientMedicinesCount = (ingredientId: number) => {
  return store.medicineIngredients.value.filter(mi => mi.IngredientId === ingredientId).length
}

const getGroupMedicinesCount = (groupId: number) => {
  return store.medicines.value.filter(m => m.DrugGroupId === groupId).length
}

// Fetch active ingredients for a medicine
const getMedicineIngredients = (medId: number) => {
  return store.medicineIngredients.value
    .filter(mi => mi.MedicineId === medId)
    .map(mi => {
      const ing = store.activeIngredients.value.find(ai => ai.IngredientId === mi.IngredientId)
      return {
        id: mi.IngredientId,
        name: ing ? ing.IngredientName : 'Không rõ',
        amount: mi.Amount
      }
    })
}

// Fetch contraindications for a medicine
const getMedicineContraindications = (medId: number) => {
  const medicineIngredientIds = store.medicineIngredients.value
    .filter(mi => mi.MedicineId === medId)
    .map(mi => mi.IngredientId)

  return store.contraindications.value.filter(c => 
    c.MedicineId === medId || 
    (c.IngredientId !== null && medicineIngredientIds.includes(c.IngredientId))
  )
}

// Reset filters
const clearFilters = () => {
  searchQuery.value = ''
  selectedGroupId.value = 'all'
  requiresPrescriptionFilter.value = 'all'
  statusFilter.value = 'all'
}

// Open Detail Modal
const openDetail = (med: Medicine) => {
  selectedMed.value = med
  showDetailModal.value = true
}

// Toggle status quickly
const toggleActiveStatus = (med: Medicine) => {
  if (!canManage.value) return
  med.IsActive = !med.IsActive
}

// Open Form Modal (Add mode)
const openAddForm = () => {
  if (!canManage.value) return
  isEditing.value = false
  formMedicineId.value = null
  formMedicineName.value = ''
  formDrugGroupId.value = store.drugGroups.value[0] ? store.drugGroups.value[0].DrugGroupId.toString() : ''
  formStrength.value = ''
  formDosageForm.value = ''
  formUnit.value = ''
  formPrice.value = 0
  formRequiresPrescription.value = false
  formIsActive.value = true
  formSideEffects.value = ''
  formNote.value = ''
  formIngredients.value = []
  showFormModal.value = true
}

// Open Form Modal (Edit mode)
const openEditForm = (med: Medicine) => {
  if (!canManage.value) return
  isEditing.value = true
  formMedicineId.value = med.MedicineId
  formMedicineName.value = med.MedicineName
  formDrugGroupId.value = med.DrugGroupId?.toString() || ''
  formStrength.value = med.Strength || ''
  formDosageForm.value = med.DosageForm || ''
  formUnit.value = med.Unit || ''
  formPrice.value = med.Price
  formRequiresPrescription.value = med.RequiresPrescription
  formIsActive.value = med.IsActive
  formSideEffects.value = med.SideEffects || ''
  formNote.value = med.Note || ''

  // Load current ingredients
  const currentIngredients = store.medicineIngredients.value.filter(mi => mi.MedicineId === med.MedicineId)
  formIngredients.value = currentIngredients.map(ci => ({
    IngredientId: ci.IngredientId,
    Amount: ci.Amount || ''
  }))

  showFormModal.value = true
}

// Add empty active ingredient row on the form
const addFormIngredientRow = () => {
  const firstIng = store.activeIngredients.value[0]
  if (!firstIng) return
  formIngredients.value.push({
    IngredientId: firstIng.IngredientId,
    Amount: ''
  })
}

// Remove active ingredient row on the form
const removeFormIngredientRow = (index: number) => {
  formIngredients.value.splice(index, 1)
}

// Save medicine form
const saveMedicine = async () => {
  if (!formMedicineName.value.trim()) {
    alert('Vui lòng nhập tên thuốc!')
    return
  }
  if (formPrice.value < 0) {
    alert('Đơn giá không được âm!')
    return
  }

  const medicineData = {
    DrugGroupId: formDrugGroupId.value ? Number(formDrugGroupId.value) : null,
    MedicineName: formMedicineName.value,
    Strength: formStrength.value,
    DosageForm: formDosageForm.value,
    Unit: formUnit.value,
    Price: formPrice.value,
    RequiresPrescription: formRequiresPrescription.value,
    IsActive: formIsActive.value,
    SideEffects: formSideEffects.value.trim() || null,
    Note: formNote.value
  }

  if (isEditing.value && formMedicineId.value !== null) {
    // 1. Edit existing medicine
    await store.updateMedicine(
      formMedicineId.value,
      {
        MedicineId: formMedicineId.value,
        ...medicineData,
        CreatedAt: store.medicines.value.find(m => m.MedicineId === formMedicineId.value)?.CreatedAt || new Date().toISOString().substring(0, 10)
      },
      formIngredients.value.map(fi => ({
        IngredientId: fi.IngredientId,
        Amount: fi.Amount
      }))
    )
  } else {
    // 2. Add new medicine
    await store.addMedicine(
      medicineData,
      formIngredients.value.map(fi => ({
        IngredientId: fi.IngredientId,
        Amount: fi.Amount
      }))
    )
  }

  showFormModal.value = false
  alert('Đã lưu thông tin thuốc thành công!')
}

// Delete/Remove medicine
const deleteMedicine = async (med: Medicine) => {
  if (!canManage.value) return
  if (confirm(`Bạn có chắc chắn muốn xóa thuốc "${med.MedicineName}" ra khỏi hệ thống không?`)) {
    await store.deleteMedicine(med.MedicineId)
    alert('Đã xóa thuốc khỏi hệ thống!')
  }
}

// Active Ingredients CRUD Actions
const openAddIngForm = () => {
  if (!canManage.value) return
  isEditingIng.value = false
  formIngId.value = null
  formIngName.value = ''
  formIngDescription.value = ''
  showIngModal.value = true
}

const openEditIngForm = (ing: any) => {
  if (!canManage.value) return
  isEditingIng.value = true
  formIngId.value = ing.IngredientId
  formIngName.value = ing.IngredientName
  formIngDescription.value = ing.Description || ''
  showIngModal.value = true
}

const saveIngredient = async () => {
  if (!formIngName.value.trim()) {
    alert('Vui lòng nhập tên hoạt chất!')
    return
  }
  const ingredientData = {
    IngredientName: formIngName.value.trim(),
    Description: formIngDescription.value.trim() || null
  }
  
  if (isEditingIng.value && formIngId.value !== null) {
    await store.updateActiveIngredient(formIngId.value, {
      IngredientId: formIngId.value,
      ...ingredientData
    })
  } else {
    await store.addActiveIngredient(ingredientData)
  }
  showIngModal.value = false
  alert('Đã lưu thông tin hoạt chất thành công!')
}

const deleteIngredient = async (ing: any) => {
  if (!canManage.value) return
  const count = getIngredientMedicinesCount(ing.IngredientId)
  if (count > 0) {
    alert(`Không thể xóa hoạt chất này vì đang có ${count} thuốc liên kết! Vui lòng gỡ liên kết trước.`)
    return
  }
  if (confirm(`Bạn có chắc chắn muốn xóa hoạt chất "${ing.IngredientName}" khỏi hệ thống?`)) {
    await store.deleteActiveIngredient(ing.IngredientId)
    alert('Đã xóa hoạt chất khỏi hệ thống!')
  }
}

// Drug Groups CRUD Actions
const openAddGroupForm = () => {
  if (!canManage.value) return
  isEditingGroup.value = false
  formGroupId.value = null
  formGroupName.value = ''
  formGroupDescription.value = ''
  showGroupModal.value = true
}

const openEditGroupForm = (group: any) => {
  if (!canManage.value) return
  isEditingGroup.value = true
  formGroupId.value = group.DrugGroupId
  formGroupName.value = group.GroupName
  formGroupDescription.value = group.Description || ''
  showGroupModal.value = true
}

const saveGroup = async () => {
  if (!formGroupName.value.trim()) {
    alert('Vui lòng nhập tên nhóm thuốc!')
    return
  }
  const groupData = {
    GroupName: formGroupName.value.trim(),
    Description: formGroupDescription.value.trim() || null
  }
  
  if (isEditingGroup.value && formGroupId.value !== null) {
    await store.updateDrugGroup(formGroupId.value, {
      DrugGroupId: formGroupId.value,
      ...groupData
    })
  } else {
    await store.addDrugGroup(groupData)
  }
  showGroupModal.value = false
  alert('Đã lưu thông tin nhóm thuốc thành công!')
}

const deleteGroup = async (group: any) => {
  if (!canManage.value) return
  const count = getGroupMedicinesCount(group.DrugGroupId)
  if (count > 0) {
    alert(`Không thể xóa nhóm thuốc này vì đang có ${count} thuốc thuộc nhóm này! Vui lòng gỡ liên kết hoặc chuyển thuốc sang nhóm khác trước.`)
    return
  }
  if (confirm(`Bạn có chắc chắn muốn xóa nhóm thuốc "${group.GroupName}" khỏi hệ thống?`)) {
    await store.deleteDrugGroup(group.DrugGroupId)
    alert('Đã xóa nhóm thuốc khỏi hệ thống!')
  }
}
</script>

<template>
  <div class="view-container">
    <!-- Tabs Header Row -->
    <div class="tabs-header-row">
      <button 
        :class="['tab-btn', { active: activeSubTab === 'medicines' }]" 
        @click="activeSubTab = 'medicines'"
      >
        💊 Danh mục Thuốc
      </button>
      <button 
        :class="['tab-btn', { active: activeSubTab === 'ingredients' }]" 
        @click="activeSubTab = 'ingredients'"
      >
        🧪 Danh mục Hoạt chất
      </button>
      <button 
        :class="['tab-btn', { active: activeSubTab === 'druggroups' }]" 
        @click="activeSubTab = 'druggroups'"
      >
        📦 Danh mục Nhóm thuốc
      </button>
    </div>

    <!-- ==========================================
      TAB 1: MEDICINES CATALOG
    ========================================== -->
    <div v-if="activeSubTab === 'medicines'">
      <!-- Filter & Search Panel -->
      <div class="grid-card search-filter-panel">
        <div class="filters-row">
          <!-- Search input -->
          <div class="filter-col flex-1">
            <label class="filter-label">Tìm kiếm thuốc:</label>
            <div class="search-input-wrapper">
              <input 
                type="text" 
                placeholder="Nhập tên thuốc, hoạt chất..." 
                class="form-control"
                v-model="searchQuery" 
              />
              <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>

          <!-- Drug Group filter -->
          <div class="filter-col">
            <label class="filter-label">Nhóm thuốc:</label>
            <select v-model="selectedGroupId" class="form-control select-control">
              <option value="all">-- Tất cả nhóm thuốc --</option>
              <option v-for="dg in store.drugGroups.value" :key="dg.DrugGroupId" :value="dg.DrugGroupId.toString()">
                {{ dg.GroupName }}
              </option>
            </select>
          </div>

          <!-- Requires Prescription filter -->
          <div class="filter-col">
            <label class="filter-label">Loại đơn thuốc:</label>
            <select v-model="requiresPrescriptionFilter" class="form-control select-control">
              <option value="all">Tất cả</option>
              <option value="true">Yêu cầu đơn thuốc</option>
              <option value="false">Không kê đơn</option>
            </select>
          </div>

          <!-- Status filter -->
          <div class="filter-col">
            <label class="filter-label">Trạng thái:</label>
            <select v-model="statusFilter" class="form-control select-control">
              <option value="all">Tất cả trạng thái</option>
              <option value="true">Hoạt động</option>
              <option value="false">Tạm ngừng</option>
            </select>
          </div>
        </div>

        <!-- Action buttons -->
        <div class="panel-actions-row">
          <button class="secondary-btn" @click="clearFilters" :disabled="!searchQuery && selectedGroupId === 'all' && requiresPrescriptionFilter === 'all' && statusFilter === 'all'">
            Xóa bộ lọc
          </button>
          <button class="primary-btn flex-center" v-if="canManage" @click="openAddForm">
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5" style="margin-right: 6px;">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            Thêm thuốc mới (Medicines)
          </button>
        </div>
      </div>

      <!-- Medicines Catalog List -->
      <div class="grid-card" style="margin-top: 20px; overflow-x: auto;">
        <h3 class="section-title" style="margin-bottom: 16px;">Danh mục thông tin thuốc ({{ filteredMedicines.length }} kết quả)</h3>
        
        <table class="data-table" v-if="filteredMedicines.length > 0">
          <thead>
            <tr>
              <th>Mã số</th>
              <th>Tên thuốc</th>
              <th>Nhóm thuốc</th>
              <th>Hàm lượng</th>
              <th>Bào chế / ĐVT</th>
              <th>Loại đơn</th>
              <th>Đơn giá</th>
              <th>Trạng thái</th>
              <th style="text-align: center;">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="med in filteredMedicines" :key="med.MedicineId">
              <td>MED-00{{ med.MedicineId }}</td>
              <td>
                <div class="med-name-cell">
                  <span class="med-title">{{ med.MedicineName }}</span>
                  <!-- Brief active ingredients tag preview -->
                  <div class="med-ings-preview" v-if="getMedicineIngredients(med.MedicineId).length > 0">
                    <span v-for="ing in getMedicineIngredients(med.MedicineId)" :key="ing.id" class="ing-tag-mini">
                      {{ ing.name }}
                    </span>
                  </div>
                </div>
              </td>
              <td>{{ store.drugGroups.value.find(dg => dg.DrugGroupId === med.DrugGroupId)?.GroupName || 'Mặc định' }}</td>
              <td><span class="strength-text">{{ med.Strength || '-' }}</span></td>
              <td><small>{{ med.DosageForm }} / {{ med.Unit }}</small></td>
              <td>
                <span :class="['status-tag', med.RequiresPrescription ? 'danger' : 'safe']">
                  {{ med.RequiresPrescription ? 'Yêu cầu đơn' : 'Không kê đơn' }}
                </span>
              </td>
              <td><strong class="price-text">{{ med.Price.toLocaleString() }}đ</strong></td>
              <td>
                <span :class="['status-tag', med.IsActive ? 'safe' : 'danger']">
                  {{ med.IsActive ? 'Hoạt động' : 'Tạm ngừng' }}
                </span>
              </td>
              <td>
                <div class="action-buttons-group">
                  <button class="action-btn-icon view" @click="openDetail(med)" title="Xem chi tiết">
                    👁️
                  </button>
                  <button class="action-btn-icon edit" v-if="canManage" @click="openEditForm(med)" title="Chỉnh sửa">
                    ✏️
                  </button>
                  <button class="action-btn-icon toggle-status" v-if="canManage" @click="toggleActiveStatus(med)" :title="med.IsActive ? 'Tạm ngưng hoạt động' : 'Kích hoạt lại'">
                    🔄
                  </button>
                  <button class="action-btn-icon delete" v-if="canManage" @click="deleteMedicine(med)" title="Xóa khỏi hệ thống">
                    🗑️
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Empty state -->
        <div class="empty-state-container flex-center" v-else>
          <div class="empty-content">
            <span class="empty-icon">💊</span>
            <h4>Không tìm thấy thuốc phù hợp</h4>
            <p>Hãy thử thay đổi điều kiện tìm kiếm hoặc bộ lọc.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- ==========================================
      TAB 2: ACTIVE INGREDIENTS CATALOG
    ========================================== -->
    <div v-else-if="activeSubTab === 'ingredients'">
      <!-- Search & Filters -->
      <div class="grid-card search-filter-panel">
        <div class="filters-row">
          <div class="filter-col flex-1">
            <label class="filter-label">Tìm kiếm hoạt chất:</label>
            <div class="search-input-wrapper">
              <input 
                type="text" 
                placeholder="Nhập tên hoạt chất, mô tả..." 
                class="form-control"
                v-model="ingSearchQuery" 
              />
              <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>
        </div>
        
        <div class="panel-actions-row">
          <button class="secondary-btn" @click="ingSearchQuery = ''" :disabled="!ingSearchQuery">
            Xóa bộ lọc
          </button>
          <button class="primary-btn flex-center" v-if="canManage" @click="openAddIngForm">
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5" style="margin-right: 6px;">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            Thêm hoạt chất mới (Ingredients)
          </button>
        </div>
      </div>

      <!-- Active Ingredients Table -->
      <div class="grid-card" style="margin-top: 20px; overflow-x: auto;">
        <h3 class="section-title" style="margin-bottom: 16px;">Danh mục hoạt chất ({{ filteredIngredients.length }} kết quả)</h3>
        
        <table class="data-table" v-if="filteredIngredients.length > 0">
          <thead>
            <tr>
              <th style="width: 150px;">Mã hoạt chất</th>
              <th>Tên hoạt chất</th>
              <th>Mô tả / Vai trò dược lý</th>
              <th style="width: 180px; text-align: center;">Số thuốc liên kết</th>
              <th style="text-align: center; width: 150px;">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="ing in filteredIngredients" :key="ing.IngredientId">
              <td>ING-00{{ ing.IngredientId }}</td>
              <td><strong class="med-title">{{ ing.IngredientName }}</strong></td>
              <td>{{ ing.Description || '-' }}</td>
              <td style="text-align: center;">
                <span class="ing-tag-mini" style="font-size: 12px; padding: 4px 8px;">
                  {{ getIngredientMedicinesCount(ing.IngredientId) }} thuốc
                </span>
              </td>
              <td>
                <div class="action-buttons-group">
                  <button class="action-btn-icon edit" v-if="canManage" @click="openEditIngForm(ing)" title="Chỉnh sửa">
                    ✏️
                  </button>
                  <button class="action-btn-icon delete" v-if="canManage" @click="deleteIngredient(ing)" title="Xóa khỏi hệ thống">
                    🗑️
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Empty state -->
        <div class="empty-state-container flex-center" v-else>
          <div class="empty-content">
            <span class="empty-icon">🧪</span>
            <h4>Không tìm thấy hoạt chất phù hợp</h4>
            <p>Hãy thử thay đổi từ khóa tìm kiếm.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- ==========================================
      TAB 3: DRUG GROUPS CATALOG
    ========================================== -->
    <div v-else-if="activeSubTab === 'druggroups'">
      <!-- Search & Filters -->
      <div class="grid-card search-filter-panel">
        <div class="filters-row">
          <div class="filter-col flex-1">
            <label class="filter-label">Tìm kiếm nhóm thuốc:</label>
            <div class="search-input-wrapper">
              <input 
                type="text" 
                placeholder="Nhập tên nhóm thuốc, mô tả..." 
                class="form-control"
                v-model="groupSearchQuery" 
              />
              <svg viewBox="0 0 24 24" class="search-icon-svg" fill="none" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>
        </div>
        
        <div class="panel-actions-row">
          <button class="secondary-btn" @click="groupSearchQuery = ''" :disabled="!groupSearchQuery">
            Xóa bộ lọc
          </button>
          <button class="primary-btn flex-center" v-if="canManage" @click="openAddGroupForm">
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.5" style="margin-right: 6px;">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            Thêm nhóm thuốc mới (Drug Groups)
          </button>
        </div>
      </div>

      <!-- Drug Groups Table -->
      <div class="grid-card" style="margin-top: 20px; overflow-x: auto;">
        <h3 class="section-title" style="margin-bottom: 16px;">Danh mục nhóm thuốc ({{ filteredDrugGroups.length }} kết quả)</h3>
        
        <table class="data-table" v-if="filteredDrugGroups.length > 0">
          <thead>
            <tr>
              <th style="width: 150px;">Mã nhóm</th>
              <th>Tên nhóm thuốc</th>
              <th>Mô tả nhóm dược lý</th>
              <th style="width: 180px; text-align: center;">Số lượng thuốc</th>
              <th style="text-align: center; width: 150px;">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="g in filteredDrugGroups" :key="g.DrugGroupId">
              <td>GP-00{{ g.DrugGroupId }}</td>
              <td><strong class="med-title">{{ g.GroupName }}</strong></td>
              <td>{{ g.Description || '-' }}</td>
              <td style="text-align: center;">
                <span class="ing-tag-mini" style="font-size: 12px; padding: 4px 8px; background-color: rgba(59, 130, 246, 0.1); color: var(--info); border: 1px solid rgba(59, 130, 246, 0.15)">
                  {{ getGroupMedicinesCount(g.DrugGroupId) }} thuốc
                </span>
              </td>
              <td>
                <div class="action-buttons-group">
                  <button class="action-btn-icon edit" v-if="canManage" @click="openEditGroupForm(g)" title="Chỉnh sửa">
                    ✏️
                  </button>
                  <button class="action-btn-icon delete" v-if="canManage" @click="deleteGroup(g)" title="Xóa khỏi hệ thống">
                    🗑️
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Empty state -->
        <div class="empty-state-container flex-center" v-else>
          <div class="empty-content">
            <span class="empty-icon">📦</span>
            <h4>Không tìm thấy nhóm thuốc phù hợp</h4>
            <p>Hãy thử thay đổi từ khóa tìm kiếm.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- ==========================================
      MODAL 1: VIEW MEDICINE DETAILS
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showDetailModal && selectedMed">
      <div class="modal-card detail-modal">
        <div class="modal-header">
          <div class="modal-title-area">
            <span class="modal-indicator">MED-00{{ selectedMed.MedicineId }}</span>
            <h3>Chi tiết thuốc: {{ selectedMed.MedicineName }}</h3>
          </div>
          <button class="close-modal-btn" @click="showDetailModal = false">×</button>
        </div>

        <div class="modal-body">
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Tên thuốc:</span>
              <span class="detail-val-strong">{{ selectedMed.MedicineName }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Nhóm thuốc:</span>
              <span>{{ store.drugGroups.value.find(dg => dg.DrugGroupId === selectedMed?.DrugGroupId)?.GroupName || 'Chưa phân nhóm' }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Hàm lượng:</span>
              <span>{{ selectedMed.Strength || '-' }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Dạng bào chế:</span>
              <span>{{ selectedMed.DosageForm }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Đơn vị tính:</span>
              <span>{{ selectedMed.Unit }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Giá bán niêm yết:</span>
              <span class="price-highlight">{{ selectedMed.Price.toLocaleString() }}đ / {{ selectedMed.Unit }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Phân loại kê đơn:</span>
              <span :class="['status-tag', selectedMed.RequiresPrescription ? 'danger' : 'safe']">
                {{ selectedMed.RequiresPrescription ? 'Thuốc phải kê đơn' : 'Không yêu cầu đơn thuốc' }}
              </span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Trạng thái:</span>
              <span :class="['status-tag', selectedMed.IsActive ? 'safe' : 'danger']">
                {{ selectedMed.IsActive ? 'Đang hoạt động bán lẻ' : 'Tạm ngừng sử dụng' }}
              </span>
            </div>
            <div class="detail-item span-2" v-if="selectedMed.Note">
              <span class="detail-label">Ghi chú y khoa:</span>
              <p class="detail-text-box">{{ selectedMed.Note }}</p>
            </div>
            <div class="detail-item span-2" v-if="selectedMed.SideEffects">
              <span class="detail-label" style="color: var(--danger);">Tác dụng phụ chính:</span>
              <p class="detail-text-box" style="border-color: rgba(239, 68, 68, 0.15); background-color: rgba(239, 68, 68, 0.02);">{{ selectedMed.SideEffects }}</p>
            </div>
          </div>

          <!-- Active Ingredients linkage block -->
          <div class="ingredients-linkage-section">
            <h4 class="sub-title">Hoạt chất cấu thành (Active Ingredients)</h4>
            <div class="ingredients-badge-list" v-if="getMedicineIngredients(selectedMed.MedicineId).length > 0">
              <div v-for="ing in getMedicineIngredients(selectedMed.MedicineId)" :key="ing.id" class="ingredient-badge-card">
                <span class="ing-name">{{ ing.name }}</span>
                <span class="ing-amount">{{ ing.amount || 'Không rõ hàm lượng' }}</span>
              </div>
            </div>
            <div class="empty-ingredients-box" v-else>
              <p>Chưa ghi nhận liên kết hoạt chất cho thuốc này trong CSDL.</p>
            </div>
          </div>

          <!-- Contraindications linkage block -->
          <div class="ingredients-linkage-section" style="border-top: 1px solid var(--border-color); padding-top: 16px; margin-top: 16px;">
            <h4 class="sub-title" style="color: var(--danger);">Chống chỉ định lâm sàng (Contraindications)</h4>
            <div class="alert-list" v-if="getMedicineContraindications(selectedMed.MedicineId).length > 0">
              <div v-for="contra in getMedicineContraindications(selectedMed.MedicineId)" :key="contra.ContraindicationId" class="alert-item high-risk" style="padding: 12px; margin-bottom: 8px; border-radius: 8px;">
                <div style="display: flex; justify-content: space-between; align-items: center;">
                  <span class="alert-badge" style="font-size: 10px; padding: 2px 6px; background-color: var(--danger); color: white;">{{ contra.Severity }}</span>
                  <span style="font-size: 11px; font-weight: 700; color: var(--text-muted);">
                    {{ contra.DiseaseId ? 'Chống chỉ định bệnh nền' : 'Đối tượng đặc biệt' }}
                  </span>
                </div>
                <div class="alert-desc" style="font-size: 13px; margin-top: 6px;">
                  <strong>Điều kiện:</strong> 
                  <span v-if="contra.DiseaseId" class="tag warning" style="margin-left: 6px; font-size: 11px; background-color: var(--warning-bg); color: var(--warning); padding: 2px 6px; border-radius: 4px;">{{ store.diseases.value.find(d => d.DiseaseId === contra.DiseaseId)?.DiseaseName }}</span>
                  <span v-else class="tag danger" style="margin-left: 6px; font-size: 11px; background-color: var(--danger-bg); color: var(--danger); padding: 2px 6px; border-radius: 4px;">🤰 Phụ nữ mang thai / Đối tượng đặc biệt</span>
                </div>
                <div class="alert-desc" style="font-size: 13px; margin-top: 4px; color: var(--text-main);">
                  <strong>Mô tả:</strong> {{ contra.Description }}
                </div>
                <div class="alert-desc" style="font-size: 13px; margin-top: 4px; color: var(--success); font-weight: 600;">
                  <strong>Khuyến cáo:</strong> {{ contra.Recommendation }}
                </div>
              </div>
            </div>
            <div class="empty-ingredients-box" v-else>
              <p>Chưa ghi nhận chống chỉ định đặc thù cho thuốc này.</p>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showDetailModal = false">Đóng cửa sổ</button>
          <button class="primary-btn" v-if="canManage" @click="showDetailModal = false; openEditForm(selectedMed!)">Chỉnh sửa thông tin</button>
        </div>
      </div>
    </div>

    <!-- ==========================================
      MODAL 2: ADD / EDIT MEDICINE FORM
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showFormModal">
      <div class="modal-card form-modal">
        <div class="modal-header">
          <div class="modal-title-area">
            <h3>{{ isEditing ? 'Chỉnh sửa thông tin thuốc' : 'Thêm thuốc mới vào CSDL' }}</h3>
          </div>
          <button class="close-modal-btn" @click="showFormModal = false">×</button>
        </div>

        <div class="modal-body scrollable-body">
          <div class="form-inputs-grid">
            <!-- Medicine Name -->
            <div class="form-group span-2">
              <label class="form-label required-label">Tên thuốc:</label>
              <input type="text" v-model="formMedicineName" class="form-control" placeholder="Nhập tên thuốc thương mại (ví dụ: Panadol Extra 500mg)..." />
            </div>

            <!-- Drug Group -->
            <div class="form-group">
              <label class="form-label">Nhóm dược lý:</label>
              <select v-model="formDrugGroupId" class="form-control select-control">
                <option v-for="dg in store.drugGroups.value" :key="dg.DrugGroupId" :value="dg.DrugGroupId.toString()">
                  {{ dg.GroupName }}
                </option>
              </select>
            </div>

            <!-- Strength -->
            <div class="form-group">
              <label class="form-label">Hàm lượng thuốc:</label>
              <input type="text" v-model="formStrength" class="form-control" placeholder="Ví dụ: 500mg, 10mg..." />
            </div>

            <!-- Dosage Form -->
            <div class="form-group">
              <label class="form-label">Dạng bào chế:</label>
              <input type="text" v-model="formDosageForm" class="form-control" placeholder="Ví dụ: Viên nén, Viên nang, Siro..." />
            </div>

            <!-- Unit -->
            <div class="form-group">
              <label class="form-label">Đơn vị tính:</label>
              <input type="text" v-model="formUnit" class="form-control" placeholder="Ví dụ: Viên, Vỉ, Hộp, Chai..." />
            </div>

            <!-- Price -->
            <div class="form-group">
              <label class="form-label">Giá bán niêm yết (VND):</label>
              <input type="number" v-model.number="formPrice" class="form-control" min="0" step="500" />
            </div>

            <!-- Requires Prescription / Is Active -->
            <div class="form-group flex-checkbox-row">
              <label class="checkbox-container">
                <input type="checkbox" v-model="formRequiresPrescription" />
                <span class="checkmark"></span>
                Thuốc kê đơn (Requires Prescription)
              </label>
              
              <label class="checkbox-container">
                <input type="checkbox" v-model="formIsActive" />
                <span class="checkmark"></span>
                Cho phép bán lẻ (Active)
              </label>
            </div>

            <!-- Side Effects -->
            <div class="form-group span-2">
              <label class="form-label">Tác dụng phụ chính:</label>
              <textarea v-model="formSideEffects" class="form-control textarea-control" rows="2" placeholder="Nhập các tác dụng phụ có hại thường gặp (ví dụ: gây buồn ngủ, hại gan, kích ứng dạ dày)..."></textarea>
            </div>

            <!-- Notes -->
            <div class="form-group span-2">
              <label class="form-label">Ghi chú lâm sàng / Hướng dẫn:</label>
              <textarea v-model="formNote" class="form-control textarea-control" rows="2" placeholder="Ví dụ: Uống sau bữa ăn, tránh dùng kèm rượu bia..."></textarea>
            </div>
          </div>

          <!-- Dynamic Active Ingredients Mapping Form -->
          <div class="form-ingredients-mapping-section">
            <div class="ingredients-header-row">
              <h4 class="sub-title">Liên kết Hoạt chất & Hàm lượng (Active Ingredients)</h4>
              <button type="button" class="add-row-btn" @click="addFormIngredientRow">+ Thêm hoạt chất</button>
            </div>

            <div class="form-ingredients-list" v-if="formIngredients.length > 0">
              <div v-for="(item, idx) in formIngredients" :key="idx" class="form-ingredient-row">
                <!-- Select active ingredient -->
                <div class="col-select">
                  <select v-model="item.IngredientId" class="form-control select-control-sm">
                    <option v-for="ing in store.activeIngredients.value" :key="ing.IngredientId" :value="ing.IngredientId">
                      {{ ing.IngredientName }}
                    </option>
                  </select>
                </div>
                <!-- Amount input -->
                <div class="col-amount">
                  <input type="text" v-model="item.Amount" class="form-control text-control-sm" placeholder="Hàm lượng (ví dụ: 500mg)" />
                </div>
                <!-- Delete row button -->
                <div class="col-delete">
                  <button type="button" class="delete-row-btn" @click="removeFormIngredientRow(idx)">×</button>
                </div>
              </div>
            </div>
            <div class="empty-ingredients-form" v-else>
              <p>Chưa có hoạt chất nào được chọn. Nhấn "+ Thêm hoạt chất" bên trên để liên kết hoạt chất chống dị ứng/tương tác thuốc.</p>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showFormModal = false">Hủy</button>
          <button class="primary-btn" @click="saveMedicine">Lưu lại</button>
        </div>
      </div>
    </div>

    <!-- ==========================================
      MODAL 3: ADD / EDIT ACTIVE INGREDIENT FORM
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showIngModal">
      <div class="modal-card form-modal">
        <div class="modal-header">
          <div class="modal-title-area">
            <h3>{{ isEditingIng ? 'Chỉnh sửa hoạt chất' : 'Thêm hoạt chất mới' }}</h3>
          </div>
          <button class="close-modal-btn" @click="showIngModal = false">×</button>
        </div>

        <div class="modal-body">
          <div class="form-inputs-grid">
            <div class="form-group span-2">
              <label class="form-label required-label">Tên hoạt chất (ví dụ: Paracetamol, Amoxicillin):</label>
              <input type="text" v-model="formIngName" class="form-control" placeholder="Nhập tên danh pháp quốc tế..." />
            </div>

            <div class="form-group span-2">
              <label class="form-label">Mô tả / Ghi chú dược học:</label>
              <textarea v-model="formIngDescription" class="form-control textarea-control" rows="3" placeholder="Mô tả tác dụng, lưu ý khi phối hợp thuốc..."></textarea>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showIngModal = false">Hủy</button>
          <button class="primary-btn" @click="saveIngredient">Lưu lại</button>
        </div>
      </div>
    </div>

    <!-- ==========================================
      MODAL 4: ADD / EDIT DRUG GROUP FORM
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="showGroupModal">
      <div class="modal-card form-modal">
        <div class="modal-header">
          <div class="modal-title-area">
            <h3>{{ isEditingGroup ? 'Chỉnh sửa nhóm thuốc' : 'Thêm nhóm thuốc mới' }}</h3>
          </div>
          <button class="close-modal-btn" @click="showGroupModal = false">×</button>
        </div>

        <div class="modal-body">
          <div class="form-inputs-grid">
            <div class="form-group span-2">
              <label class="form-label required-label">Tên nhóm thuốc (ví dụ: Kháng sinh, Giảm đau hạ sốt):</label>
              <input type="text" v-model="formGroupName" class="form-control" placeholder="Nhập tên phân nhóm..." />
            </div>

            <div class="form-group span-2">
              <label class="form-label">Mô tả tác dụng lâm sàng chính:</label>
              <textarea v-model="formGroupDescription" class="form-control textarea-control" rows="3" placeholder="Mô tả công dụng và các lưu ý của nhóm thuốc này..."></textarea>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="showGroupModal = false">Hủy</button>
          <button class="primary-btn" @click="saveGroup">Lưu lại</button>
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

/* Filter Panel Styling */
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

/* Table name cell styling */
.med-name-cell {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.med-title {
  font-weight: 700;
  color: var(--text-main);
}
.med-ings-preview {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}
.ing-tag-mini {
  font-size: 10px;
  font-weight: 600;
  background-color: var(--primary-bg);
  color: var(--primary-medium);
  padding: 1px 4px;
  border-radius: 3px;
  border: 1px solid rgba(16, 185, 129, 0.15);
}
.strength-text {
  font-weight: 600;
  color: var(--text-main);
}
.price-text {
  color: var(--primary-medium);
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
.action-btn-icon.toggle-status:hover {
  background-color: rgba(16, 185, 129, 0.1);
  border-color: var(--primary-medium);
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

/* Modals layout styles */
.detail-modal, .form-modal {
  width: 100%;
  max-width: 650px;
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
  margin-bottom: 20px;
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
  font-size: 12px;
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
.price-highlight {
  font-size: 16px;
  font-weight: 800;
  color: var(--primary-medium);
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

/* Ingredients section in details */
.ingredients-linkage-section {
  border-top: 1px solid var(--border-color);
  padding-top: 16px;
  margin-top: 16px;
}
.sub-title {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-main);
  margin-bottom: 12px;
}
.ingredients-badge-list {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}
.ingredient-badge-card {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 13px;
}
.ingredient-badge-card .ing-name {
  font-weight: 700;
  color: var(--text-main);
}
.ingredient-badge-card .ing-amount {
  font-size: 11px;
  background-color: var(--primary-bg);
  color: var(--primary-medium);
  padding: 2px 6px;
  border-radius: 12px;
  font-weight: 600;
}
.empty-ingredients-box {
  background-color: var(--bg-main);
  border: 1px dashed var(--border-color);
  padding: 16px;
  border-radius: var(--border-radius-md);
  text-align: center;
  color: var(--text-muted);
  font-size: 13px;
}

/* Form input fields */
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

/* Custom checkbox sliders */
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

/* Dynamic active ingredients form mapping */
.form-ingredients-mapping-section {
  border-top: 1px solid var(--border-color);
  padding-top: 16px;
  margin-top: 16px;
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
.col-select {
  flex: 1;
}
.col-amount {
  width: 200px;
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
.empty-ingredients-form {
  background-color: var(--bg-main);
  border: 1px dashed var(--border-color);
  padding: 16px;
  border-radius: var(--border-radius-md);
  text-align: center;
  color: var(--text-muted);
  font-size: 13px;
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
