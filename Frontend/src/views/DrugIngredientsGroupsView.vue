<script setup lang="ts">
import { ref, computed } from 'vue'
import { usePharmacyStore, type DrugGroup, type ActiveIngredient } from '../store/pharmacy'

const store = usePharmacyStore()

// Sub-tab selection state
const activeSubTab = ref<'groups' | 'ingredients'>('groups')

// Search filter query
const searchQuery = ref('')

// Modals display state
const showGroupModal = ref(false)
const showIngredientModal = ref(false)

// Edit mode states
const isEditing = ref(false)
const selectedGroupId = ref<number | null>(null)
const selectedIngredientId = ref<number | null>(null)

// Form inputs
const groupName = ref('')
const groupDescription = ref('')

const ingredientName = ref('')
const ingredientDescription = ref('')

// Authorization computed helper
const canManage = computed(() => {
  return store.currentRole.value === 'admin'
})

// Dynamic stats mapping
const getMedicineCountForGroup = (groupId: number) => {
  return store.medicines.value.filter(m => m.DrugGroupId === groupId).length
}

const getMedicineCountForIngredient = (ingredientId: number) => {
  return store.medicineIngredients.value.filter(mi => mi.IngredientId === ingredientId).length
}

// Filtered drug groups list
const filteredGroups = computed(() => {
  const query = searchQuery.value.toLowerCase().trim()
  return store.drugGroups.value.filter(dg => {
    return (
      dg.GroupName.toLowerCase().includes(query) ||
      (dg.Description && dg.Description.toLowerCase().includes(query))
    )
  })
})

// Filtered ingredients list
const filteredIngredients = computed(() => {
  const query = searchQuery.value.toLowerCase().trim()
  return store.activeIngredients.value.filter(ai => {
    return (
      ai.IngredientName.toLowerCase().includes(query) ||
      (ai.Description && ai.Description.toLowerCase().includes(query))
    )
  })
})

// ==========================================
// DRUG GROUP HANDLERS
// ==========================================
const openAddGroup = () => {
  if (!canManage.value) return
  isEditing.value = false
  selectedGroupId.value = null
  groupName.value = ''
  groupDescription.value = ''
  showGroupModal.value = true
}

const openEditGroup = (group: DrugGroup) => {
  if (!canManage.value) return
  isEditing.value = true
  selectedGroupId.value = group.DrugGroupId
  groupName.value = group.GroupName
  groupDescription.value = group.Description || ''
  showGroupModal.value = true
}

const saveGroup = async () => {
  if (!groupName.value.trim()) {
    alert('Vui lòng nhập tên nhóm thuốc!')
    return
  }

  const groupData = {
    GroupName: groupName.value.trim(),
    Description: groupDescription.value.trim() || null
  }

  try {
    if (isEditing.value && selectedGroupId.value !== null) {
      await store.updateDrugGroupStore(selectedGroupId.value, {
        DrugGroupId: selectedGroupId.value,
        ...groupData
      })
    } else {
      await store.addDrugGroup(groupData)
    }
    showGroupModal.value = false
  } catch (err: any) {
    alert(err.message || 'Lỗi khi lưu nhóm thuốc!')
  }
}

const deleteGroup = async (group: DrugGroup) => {
  if (!canManage.value) return
  
  // Safety check: block if there are referenced medicines
  const refCount = getMedicineCountForGroup(group.DrugGroupId)
  if (refCount > 0) {
    alert(`Không thể xóa! Có ${refCount} thuốc đang được phân loại vào nhóm thuốc "${group.GroupName}". Vui lòng chuyển các thuốc này sang nhóm khác trước khi xóa.`)
    return
  }

  if (confirm(`Bạn có chắc chắn muốn xóa nhóm thuốc "${group.GroupName}"?`)) {
    try {
      const success = await store.deleteDrugGroupStore(group.DrugGroupId)
      if (!success) {
        alert('Xóa thất bại! Không thể kết nối với API backend.')
      }
    } catch (err: any) {
      alert(err.message || 'Lỗi khi xóa nhóm thuốc!')
    }
  }
}

// ==========================================
// ACTIVE INGREDIENT HANDLERS
// ==========================================
const openAddIngredient = () => {
  if (!canManage.value) return
  isEditing.value = false
  selectedIngredientId.value = null
  ingredientName.value = ''
  ingredientDescription.value = ''
  showIngredientModal.value = true
}

const openEditIngredient = (ingredient: ActiveIngredient) => {
  if (!canManage.value) return
  isEditing.value = true
  selectedIngredientId.value = ingredient.IngredientId
  ingredientName.value = ingredient.IngredientName
  ingredientDescription.value = ingredient.Description || ''
  showIngredientModal.value = true
}

const saveIngredient = async () => {
  if (!ingredientName.value.trim()) {
    alert('Vui lòng nhập tên hoạt chất!')
    return
  }

  const ingredientData = {
    IngredientName: ingredientName.value.trim(),
    Description: ingredientDescription.value.trim() || null
  }

  try {
    if (isEditing.value && selectedIngredientId.value !== null) {
      await store.updateIngredientStore(selectedIngredientId.value, {
        IngredientId: selectedIngredientId.value,
        ...ingredientData
      })
    } else {
      await store.addIngredient(ingredientData)
    }
    showIngredientModal.value = false
  } catch (err: any) {
    alert(err.message || 'Lỗi khi lưu hoạt chất!')
  }
}

const deleteIngredient = async (ingredient: ActiveIngredient) => {
  if (!canManage.value) return

  // Safety check: block if active ingredient is in any medicines
  const refCount = getMedicineCountForIngredient(ingredient.IngredientId)
  if (refCount > 0) {
    alert(`Không thể xóa! Hoạt chất "${ingredient.IngredientName}" đang được phối trộn trong ${refCount} thuốc khác nhau. Vui lòng gỡ bỏ hoạt chất này khỏi các công thức thuốc trước khi xóa.`)
    return
  }

  if (confirm(`Bạn có chắc chắn muốn xóa hoạt chất "${ingredient.IngredientName}"?`)) {
    try {
      const success = await store.deleteIngredientStore(ingredient.IngredientId)
      if (!success) {
        alert('Xóa thất bại! Không thể kết nối với API backend.')
      }
    } catch (err: any) {
      alert(err.message || 'Lỗi khi xóa hoạt chất!')
    }
  }
}
</script>

<template>
  <div class="view-container">
    <!-- Sub tabs Selector -->
    <div class="tabs-navigation">
      <div class="tabs-list">
        <button 
          :class="['tab-btn', { active: activeSubTab === 'groups' }]" 
          @click="activeSubTab = 'groups'"
        >
          <svg viewBox="0 0 24 24" class="tab-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
          Nhóm dược lý / Nhóm thuốc
          <span class="tab-count-badge">{{ store.drugGroups.value.length }}</span>
        </button>
        <button 
          :class="['tab-btn', { active: activeSubTab === 'ingredients' }]" 
          @click="activeSubTab = 'ingredients'"
        >
          <svg viewBox="0 0 24 24" class="tab-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
          </svg>
          Hoạt chất điều trị
          <span class="tab-count-badge">{{ store.activeIngredients.value.length }}</span>
        </button>
      </div>
    </div>

    <!-- Actions area (Search & Add Button) -->
    <div class="table-container">
      <div class="table-actions">
        <div class="search-wrapper">
          <svg viewBox="0 0 24 24" class="search-icon" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input 
            type="text" 
            class="search-input-small form-control-with-icon" 
            v-model="searchQuery" 
            :placeholder="activeSubTab === 'groups' ? 'Tìm theo tên nhóm, mô tả...' : 'Tìm theo tên hoạt chất, mô tả...'"
          />
        </div>
        
        <div v-if="canManage">
          <button v-if="activeSubTab === 'groups'" class="primary-btn add-btn" @click="openAddGroup">
            <svg viewBox="0 0 24 24" class="btn-icon" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            Thêm nhóm thuốc
          </button>
          <button v-else class="primary-btn add-btn" @click="openAddIngredient">
            <svg viewBox="0 0 24 24" class="btn-icon" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            Thêm hoạt chất
          </button>
        </div>
      </div>

      <!-- Tab Content: Drug Groups -->
      <div v-if="activeSubTab === 'groups'" class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width: 80px;">Mã nhóm</th>
              <th>Tên nhóm thuốc</th>
              <th>Mô tả</th>
              <th style="width: 150px; text-align: center;">Số lượng thuốc</th>
              <th v-if="canManage" style="width: 120px; text-align: center;">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="group in filteredGroups" :key="group.DrugGroupId">
              <td><strong>#{{ group.DrugGroupId }}</strong></td>
              <td class="font-semibold">{{ group.GroupName }}</td>
              <td class="text-muted text-truncate-custom">{{ group.Description || 'Không có mô tả' }}</td>
              <td style="text-align: center;">
                <span :class="['medicine-badge', { 'zero': getMedicineCountForGroup(group.DrugGroupId) === 0 }]">
                  {{ getMedicineCountForGroup(group.DrugGroupId) }} loại thuốc
                </span>
              </td>
              <td v-if="canManage" style="text-align: center;">
                <div class="action-buttons-flex">
                  <button class="action-edit-btn" @click="openEditGroup(group)" title="Sửa nhóm thuốc">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M12 20h9M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z" />
                    </svg>
                  </button>
                  <button class="action-delete-btn" @click="deleteGroup(group)" title="Xóa nhóm thuốc">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <polyline points="3 6 5 6 21 6" />
                      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredGroups.length === 0">
              <td :colspan="canManage ? 5 : 4" class="empty-placeholder">
                Không tìm thấy nhóm thuốc nào trùng khớp với từ khóa tìm kiếm.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Tab Content: Active Ingredients -->
      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width: 80px;">Mã HC</th>
              <th>Tên hoạt chất</th>
              <th>Mô tả chi tiết</th>
              <th style="width: 150px; text-align: center;">Số lượng thuốc</th>
              <th v-if="canManage" style="width: 120px; text-align: center;">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="ing in filteredIngredients" :key="ing.IngredientId">
              <td><strong>#{{ ing.IngredientId }}</strong></td>
              <td class="font-semibold text-primary">{{ ing.IngredientName }}</td>
              <td class="text-muted text-truncate-custom">{{ ing.Description || 'Không có mô tả' }}</td>
              <td style="text-align: center;">
                <span :class="['medicine-badge ingredient', { 'zero': getMedicineCountForIngredient(ing.IngredientId) === 0 }]">
                  {{ getMedicineCountForIngredient(ing.IngredientId) }} thuốc chứa
                </span>
              </td>
              <td v-if="canManage" style="text-align: center;">
                <div class="action-buttons-flex">
                  <button class="action-edit-btn" @click="openEditIngredient(ing)" title="Sửa hoạt chất">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M12 20h9M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z" />
                    </svg>
                  </button>
                  <button class="action-delete-btn" @click="deleteIngredient(ing)" title="Xóa hoạt chất">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <polyline points="3 6 5 6 21 6" />
                      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredIngredients.length === 0">
              <td :colspan="canManage ? 5 : 4" class="empty-placeholder">
                Không tìm thấy hoạt chất nào trùng khớp với từ khóa tìm kiếm.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ==========================================
         MODAL FORM: DRUG GROUP
         ========================================== -->
    <div v-if="showGroupModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="modal-header">
          <h3 class="modal-title">{{ isEditing ? 'Cập nhật Nhóm thuốc' : 'Thêm Nhóm thuốc mới' }}</h3>
          <button class="close-btn" @click="showGroupModal = false">&times;</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label required">Tên nhóm thuốc</label>
            <input 
              type="text" 
              class="form-control" 
              v-model="groupName" 
              placeholder="Ví dụ: Kháng sinh, Giảm đau hạ sốt..."
              required
            />
          </div>
          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label">Mô tả nhóm thuốc</label>
            <textarea 
              class="form-control" 
              rows="4" 
              v-model="groupDescription"
              placeholder="Mô tả công dụng chính hoặc các thông tin đặc biệt của nhóm dược lý này..."
            ></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="secondary-btn" @click="showGroupModal = false">Hủy bỏ</button>
          <button class="primary-btn" @click="saveGroup">
            {{ isEditing ? 'Cập nhật' : 'Thêm mới' }}
          </button>
        </div>
      </div>
    </div>

    <!-- ==========================================
         MODAL FORM: ACTIVE INGREDIENT
         ========================================== -->
    <div v-if="showIngredientModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="modal-header">
          <h3 class="modal-title">{{ isEditing ? 'Cập nhật Hoạt chất' : 'Thêm Hoạt chất mới' }}</h3>
          <button class="close-btn" @click="showIngredientModal = false">&times;</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label required">Tên hoạt chất</label>
            <input 
              type="text" 
              class="form-control" 
              v-model="ingredientName" 
              placeholder="Ví dụ: Paracetamol, Ibuprofen, Amoxicillin..."
              required
            />
          </div>
          <div class="form-group" style="margin-top: 16px;">
            <label class="form-label">Mô tả hoạt chất</label>
            <textarea 
              class="form-control" 
              rows="4" 
              v-model="ingredientDescription"
              placeholder="Mô tả cơ chế hoạt động, liều dùng phổ biến hoặc chống chỉ định chính..."
            ></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="secondary-btn" @click="showIngredientModal = false">Hủy bỏ</button>
          <button class="primary-btn" @click="saveIngredient">
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

/* Sub-tabs styling */
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

/* Search and Add Header */
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

/* Medicine Badges counts */
.medicine-badge {
  display: inline-block;
  font-size: 12px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: var(--border-radius-full);
  background-color: var(--primary-bg);
  color: var(--primary-medium);
  border: 1px solid rgba(13, 148, 136, 0.15);
}

.medicine-badge.ingredient {
  background-color: var(--info-bg);
  color: var(--info);
  border: 1px solid rgba(59, 130, 246, 0.15);
}

.medicine-badge.zero {
  background-color: var(--border-color);
  color: var(--text-muted);
  border-color: transparent;
}

/* Action actions lists */
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

.font-semibold {
  font-weight: 600;
}

.text-primary {
  color: var(--primary-medium);
}

.text-truncate-custom {
  max-width: 400px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.empty-placeholder {
  padding: 40px !important;
  text-align: center;
  color: var(--text-muted);
  font-style: italic;
  font-size: 14px;
}

/* Modals styling */
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

.form-group {
  display: flex;
  flex-direction: column;
}

.required::after {
  content: ' *';
  color: var(--danger);
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
  
  .text-truncate-custom {
    max-width: 180px;
  }
}
</style>
