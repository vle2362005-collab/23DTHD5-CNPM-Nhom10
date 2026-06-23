<script setup lang="ts">
import { ref, computed } from 'vue'
import { usePharmacyStore, type User } from '../store/pharmacy'

const store = usePharmacyStore()

// State
const searchQuery = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const modalError = ref('')

const defaultForm: User = {
  UserId: 0,
  RoleId: 2, // Default to Pharmacist
  FullName: '',
  Email: '',
  Phone: '',
  Status: 'Active',
  CreatedAt: ''
}

const form = ref<User>({ ...defaultForm })

// Computed filtered users
const filteredUsers = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return store.users.value
  return store.users.value.filter(u => 
    u.FullName.toLowerCase().includes(q) || 
    u.Email.toLowerCase().includes(q) ||
    (u.Phone && u.Phone.includes(q))
  )
})

// Actions
const openAddModal = () => {
  form.value = { ...defaultForm }
  isEditing.value = false
  modalError.value = ''
  showModal.value = true
}

const openEditModal = (user: User) => {
  form.value = { ...user }
  isEditing.value = true
  modalError.value = ''
  showModal.value = true
}

const handleSave = async () => {
  // Validate email
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!emailRegex.test(form.value.Email)) {
    modalError.value = 'Địa chỉ Email không hợp lệ.'
    return
  }
  
  if (!form.value.FullName.trim()) {
    modalError.value = 'Vui lòng nhập họ và tên nhân viên.'
    return
  }

  try {
    if (isEditing.value) {
      await store.updateUserStore(form.value.UserId, form.value)
      alert('Đã cập nhật thông tin tài khoản thành công!')
    } else {
      await store.addUser(form.value)
      alert('Đã tạo tài khoản nhân viên mới thành công!')
    }
    showModal.value = false
  } catch (err) {
    modalError.value = 'Có lỗi xảy ra khi lưu thông tin tài khoản.'
  }
}

const handleDelete = async (user: User) => {
  if (user.UserId === store.currentUser.value?.UserId) {
    alert('Bạn không thể tự xóa tài khoản của chính mình!')
    return
  }

  const confirmDelete = confirm(`Bạn có chắc chắn muốn xóa tài khoản của nhân viên "${user.FullName}"?`)
  if (!confirmDelete) return

  try {
    const success = await store.deleteUserStore(user.UserId)
    if (success) {
      alert('Đã xóa tài khoản thành công!')
    } else {
      alert('Không thể xóa tài khoản này.')
    }
  } catch (err) {
    alert('Có lỗi xảy ra khi thực hiện xóa tài khoản.')
  }
}
</script>

<template>
  <div class="view-container">
    <div class="grid-card">
      <div class="table-actions">
        <div class="search-bar-wrapper">
          <svg viewBox="0 0 24 24" class="search-icon" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input 
            type="text" 
            v-model="searchQuery"
            placeholder="Tìm tài khoản nhân viên theo tên, email..." 
            class="search-input-small" 
          />
        </div>
        <button 
          class="primary-btn flex-btn" 
          v-if="store.currentRole.value === 'admin'"
          @click="openAddModal"
        >
          <svg viewBox="0 0 24 24" class="btn-icon" fill="none" stroke="currentColor" stroke-width="2.5">
            <line x1="12" y1="5" x2="12" y2="19" />
            <line x1="5" y1="12" x2="19" y2="12" />
          </svg>
          <span>Tạo tài khoản mới</span>
        </button>
      </div>

      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>Mã số</th>
              <th>Họ và tên</th>
              <th>Email liên hệ</th>
              <th>Điện thoại</th>
              <th>Vai trò (Role)</th>
              <th>Ngày tạo</th>
              <th>Trạng thái</th>
              <th v-if="store.currentRole.value === 'admin'">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="filteredUsers.length === 0">
              <td colspan="8" class="empty-row">Không tìm thấy tài khoản nhân viên phù hợp.</td>
            </tr>
            <tr v-for="u in filteredUsers" :key="u.UserId">
              <td>USR-00{{ u.UserId }}</td>
              <td><strong>{{ u.FullName }}</strong></td>
              <td>{{ u.Email }}</td>
              <td>{{ u.Phone || 'Chưa cung cấp' }}</td>
              <td>
                <span :class="['role-badge', u.RoleId === 1 ? 'admin' : u.RoleId === 3 ? 'manager' : 'pharmacist']">
                  {{ u.RoleId === 1 ? 'Quản trị viên' : u.RoleId === 3 ? 'Quản lý' : 'Dược sĩ' }}
                </span>
              </td>
              <td>{{ u.CreatedAt }}</td>
              <td>
                <span :class="['status-tag-dot', u.Status === 'Active' ? 'active' : 'inactive']">
                  {{ u.Status === 'Active' ? 'Đang hoạt động' : 'Đã khóa' }}
                </span>
              </td>
              <td v-if="store.currentRole.value === 'admin'">
                <div class="row-actions">
                  <button class="icon-action-btn edit" @click="openEditModal(u)" title="Sửa thông tin">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                      <path d="M18.5 2.5a2.121 2.121 0 1 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                    </svg>
                  </button>
                  <button class="icon-action-btn delete" @click="handleDelete(u)" title="Xóa tài khoản">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <polyline points="3 6 5 6 21 6" />
                      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                      <line x1="10" y1="11" x2="10" y2="17" />
                      <line x1="14" y1="11" x2="14" y2="17" />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal Form (Add / Edit User) -->
    <Transition name="modal-fade">
      <div class="modal-overlay" v-if="showModal" @click.self="showModal = false">
        <div class="modal-card">
          <div class="modal-header">
            <h4>{{ isEditing ? 'Chỉnh sửa tài khoản' : 'Tạo tài khoản nhân sự mới' }}</h4>
            <button class="close-btn" @click="showModal = false">&times;</button>
          </div>
          
          <div class="modal-body">
            <div class="error-text" v-if="modalError">
              <svg viewBox="0 0 24 24" class="err-icon" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10"/>
                <line x1="12" y1="8" x2="12" y2="12"/>
                <line x1="12" y1="16" x2="12.01" y2="16"/>
              </svg>
              <span>{{ modalError }}</span>
            </div>

            <form @submit.prevent="handleSave" class="modal-form">
              <div class="form-group">
                <label class="form-label" for="user-name">Họ và tên nhân sự</label>
                <input 
                  type="text" 
                  id="user-name" 
                  v-model="form.FullName" 
                  placeholder="Nhập họ và tên đầy đủ..." 
                  class="form-control"
                  required
                />
              </div>

              <div class="form-group">
                <label class="form-label" for="user-email">Địa chỉ Email</label>
                <input 
                  type="email" 
                  id="user-email" 
                  v-model="form.Email" 
                  placeholder="name@gmail.com" 
                  class="form-control"
                  required
                />
              </div>

              <div class="form-group">
                <label class="form-label" for="user-phone">Số điện thoại / Mã PIN đăng nhập</label>
                <input 
                  type="text" 
                  id="user-phone" 
                  v-model="form.Phone" 
                  placeholder="09xxxxxxxx..." 
                  class="form-control"
                />
              </div>

              <div class="form-row">
                <div class="form-group half">
                  <label class="form-label" for="user-role">Vai trò quyền hạn</label>
                  <select id="user-role" v-model="form.RoleId" class="form-control select-control">
                    <option :value="1">Quản trị viên (Admin)</option>
                    <option :value="2">Dược sĩ (Pharmacist)</option>
                    <option :value="3">Quản lý (Manager)</option>
                  </select>
                </div>

                <div class="form-group half">
                  <label class="form-label" for="user-status">Trạng thái hoạt động</label>
                  <select id="user-status" v-model="form.Status" class="form-control select-control">
                    <option value="Active">Đang hoạt động</option>
                    <option value="Inactive">Đã khóa</option>
                  </select>
                </div>
              </div>

              <div class="modal-actions">
                <button type="button" class="secondary-btn" @click="showModal = false">Hủy bỏ</button>
                <button type="submit" class="primary-btn">Lưu thông tin</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.view-container {
  display: flex;
  flex-direction: column;
}

.table-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  gap: 16px;
}

.search-bar-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 12px;
  width: 16px;
  height: 16px;
  color: var(--text-muted);
}

.search-input-small {
  width: 320px;
  padding: 10px 14px 10px 38px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  font-size: 14px;
  outline: none;
  color: var(--text-main);
  transition: all var(--transition-fast);
}

.search-input-small:focus {
  border-color: var(--border-focus);
  background-color: #ffffff;
  box-shadow: 0 0 0 3px var(--primary-glow);
}

.flex-btn {
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn-icon {
  width: 16px;
  height: 16px;
}

.table-wrapper {
  overflow-x: auto;
  border-radius: var(--border-radius-md);
}

/* Badges and dot states */
.role-badge {
  display: inline-block;
  padding: 3px 8px;
  font-size: 11px;
  font-weight: 700;
  border-radius: var(--border-radius-sm);
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.role-badge.admin {
  background-color: var(--danger-bg);
  color: var(--danger);
  border: 1px solid rgba(239, 68, 68, 0.15);
}

.role-badge.manager {
  background-color: var(--info-bg);
  color: var(--info);
  border: 1px solid rgba(59, 130, 246, 0.15);
}

.role-badge.pharmacist {
  background-color: var(--success-bg);
  color: var(--success);
  border: 1px solid rgba(16, 185, 129, 0.15);
}

.status-tag-dot {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
}

.status-tag-dot::before {
  content: '';
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.status-tag-dot.active {
  color: var(--success);
}

.status-tag-dot.active::before {
  background-color: var(--success);
}

.status-tag-dot.inactive {
  color: var(--text-muted);
}

.status-tag-dot.inactive::before {
  background-color: var(--text-muted);
}

.empty-row {
  text-align: center;
  padding: 32px !important;
  color: var(--text-muted);
  font-style: italic;
}

/* Actions in rows */
.row-actions {
  display: flex;
  gap: 8px;
}

.icon-action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: var(--border-radius-sm);
  border: 1px solid var(--border-color);
  background-color: var(--bg-card);
  color: var(--text-muted);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.icon-action-btn svg {
  width: 16px;
  height: 16px;
}

.icon-action-btn:hover {
  background-color: var(--bg-main);
  color: var(--text-main);
}

.icon-action-btn.edit:hover {
  color: var(--primary-medium);
  border-color: var(--primary-medium);
  background-color: var(--primary-bg);
}

.icon-action-btn.delete:hover {
  color: var(--danger);
  border-color: var(--danger);
  background-color: var(--danger-bg);
}

/* Modal Windows styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 150;
  padding: 20px;
}

.modal-card {
  width: 100%;
  max-width: 500px;
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-premium);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: modalScale var(--transition-normal);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid var(--border-color);
  background-color: var(--bg-main);
}

.modal-header h4 {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
}

.close-btn {
  background: transparent;
  border: none;
  font-size: 24px;
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

.error-text {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: var(--danger-bg);
  border: 1px solid rgba(239, 68, 68, 0.15);
  border-radius: var(--border-radius-md);
  padding: 10px 14px;
  color: var(--danger);
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 16px;
}

.err-icon {
  width: 16px;
  height: 16px;
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-row {
  display: flex;
  gap: 16px;
}

.form-group.half {
  flex: 1;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 12px;
}

/* Animations transitions */
@keyframes modalScale {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity var(--transition-fast);
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
</style>
