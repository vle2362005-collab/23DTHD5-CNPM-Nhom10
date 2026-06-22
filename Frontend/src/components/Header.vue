<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePharmacyStore } from '../store/pharmacy'

const router = useRouter()
const store = usePharmacyStore()

const handleLogout = () => {
  store.logout()
  router.push({ name: 'login' })
}

// Props definitions
interface Props {
  isCollapsed: boolean
  currentRole: 'admin' | 'pharmacist' | 'manager'
}

const props = withDefaults(defineProps<Props>(), {
  isCollapsed: false,
  currentRole: 'pharmacist'
})

// Emits definitions
const emit = defineEmits<{
  (e: 'toggleSidebar'): void
  (e: 'update:currentRole', role: 'admin' | 'pharmacist' | 'manager'): void
}>()

// Component state
const showRoleDropdown = ref(false)
const showNotificationDropdown = ref(false)
const searchQuery = ref('')

// Mock Notifications data
const notifications = ref([
  { id: 1, title: 'Thuốc Paracetamol sắp hết hàng', desc: 'Lô hàng PM-092 chỉ còn 15 hộp.', type: 'warning', time: '5 phút trước' },
  { id: 2, title: 'Cảnh báo tương tác nghiêm trọng', desc: 'Hệ thống đã chặn đơn thuốc chứa Aspirin + Warfarin.', type: 'danger', time: '12 phút trước' },
  { id: 3, title: 'Cập nhật cơ sở dữ liệu thuốc', desc: 'Đã đồng bộ thành công 50 hoạt chất mới từ Bộ Y Tế.', type: 'success', time: '1 giờ trước' },
])

const unreadCount = computed(() => notifications.value.length)

// Click outside helper to close dropdowns
const dropdownRef = ref<HTMLElement | null>(null)
const notifRef = ref<HTMLElement | null>(null)

const clickOutside = (event: MouseEvent) => {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target as Node)) {
    showRoleDropdown.value = false
  }
  if (notifRef.value && !notifRef.value.contains(event.target as Node)) {
    showNotificationDropdown.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', clickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', clickOutside)
})

// User Information mapping based on role
const userProfile = computed(() => {
  switch (props.currentRole) {
    case 'admin':
      return { name: 'Nguyễn Minh Quân', title: 'Quản trị viên hệ thống', initials: 'MQ', color: '#ef4444' }
    case 'manager':
      return { name: 'Ds. Phạm Thanh Sơn', title: 'Quản lý nhà thuốc', initials: 'TS', color: '#3b82f6' }
    case 'pharmacist':
    default:
      return { name: 'Ds. Trần Thị Mai', title: 'Dược sĩ trực ca', initials: 'TM', color: '#10b981' }
  }
})

const handleRoleSelect = (role: 'admin' | 'pharmacist' | 'manager') => {
  emit('update:currentRole', role)
  showRoleDropdown.value = false
}

const toggleRoleDropdown = (e: Event) => {
  e.stopPropagation()
  showRoleDropdown.value = !showRoleDropdown.value
  showNotificationDropdown.value = false
}

const toggleNotificationDropdown = (e: Event) => {
  e.stopPropagation()
  showNotificationDropdown.value = !showNotificationDropdown.value
  showRoleDropdown.value = false
}

const clearNotifications = () => {
  notifications.value = []
  showNotificationDropdown.value = false
}
</script>

<template>
  <header class="header">
    <div class="left-section">
      <!-- Hamburger toggle button -->
      <button class="toggle-btn" @click="$emit('toggleSidebar')" aria-label="Toggle Sidebar">
        <svg viewBox="0 0 24 24" class="icon" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <g v-if="isCollapsed">
            <line x1="4" y1="6" x2="20" y2="6" />
            <line x1="4" y1="12" x2="20" y2="12" />
            <line x1="4" y1="18" x2="20" y2="18" />
          </g>
          <g v-else>
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </g>
        </svg>
      </button>

      <!-- Global Search Bar -->
      <div class="search-bar">
        <svg viewBox="0 0 24 24" class="search-icon" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="11" cy="11" r="8" />
          <line x1="21" y1="21" x2="16.65" y2="16.65" />
        </svg>
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Tìm kiếm thuốc, khách hàng hoặc đơn thuốc..." 
          class="search-input"
        />
      </div>
    </div>

    <div class="right-section">
      <!-- Active Pharmacology Engine Status Indicator -->
      <div class="status-indicator">
        <span class="pulsing-dot"></span>
        <span class="status-text" v-show="!isCollapsed">Hệ thống An toàn Online</span>
      </div>

      <!-- Notification Center Dropdown -->
      <div class="dropdown-container" ref="notifRef">
        <button class="icon-btn" @click="toggleNotificationDropdown" aria-label="Notifications">
          <svg viewBox="0 0 24 24" class="icon" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
            <path d="M13.73 21a2 2 0 0 1-3.46 0" />
          </svg>
          <span class="badge" v-if="unreadCount > 0">{{ unreadCount }}</span>
        </button>

        <Transition name="fade">
          <div class="notif-dropdown" v-if="showNotificationDropdown">
            <div class="dropdown-header">
              <span class="dropdown-title">Thông báo hệ thống</span>
              <button class="clear-btn" v-if="unreadCount > 0" @click="clearNotifications">Xóa tất cả</button>
            </div>
            <div class="dropdown-body">
              <div v-if="unreadCount === 0" class="empty-notif">
                <svg viewBox="0 0 24 24" class="empty-icon" fill="none" stroke="currentColor" stroke-width="1.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12c0 1.268-.63 2.39-1.593 3.068a3.745 3.745 0 01-1.043 3.296 3.745 3.745 0 01-3.296 1.043A3.745 3.745 0 0112 21c-1.268 0-2.39-.63-3.068-1.593a3.746 3.746 0 01-3.296-1.043 3.745 3.745 0 01-1.043-3.296A3.745 3.745 0 013 12c0-1.268.63-2.39 1.593-3.068a3.745 3.745 0 011.043-3.296 3.746 3.746 0 013.296-1.043A3.746 3.746 0 0112 3c1.268 0 2.39.63 3.068 1.593a3.746 3.746 0 013.296 1.043 3.746 3.746 0 011.043 3.296A3.745 3.745 0 0121 12z" />
                </svg>
                <span>Không có thông báo mới</span>
              </div>
              <ul v-else class="notif-list">
                <li v-for="notif in notifications" :key="notif.id" :class="['notif-item', notif.type]">
                  <div class="notif-indicator"></div>
                  <div class="notif-content">
                    <span class="notif-title">{{ notif.title }}</span>
                    <span class="notif-desc">{{ notif.desc }}</span>
                    <span class="notif-time">{{ notif.time }}</span>
                  </div>
                </li>
              </ul>
            </div>
          </div>
        </Transition>
      </div>

      <div class="divider"></div>

      <!-- Role Selector and Profile details -->
      <div class="dropdown-container" ref="dropdownRef">
        <div class="profile-trigger" @click="toggleRoleDropdown">
          <div class="avatar" :style="{ background: `linear-gradient(135deg, ${userProfile.color}dd, ${userProfile.color})` }">
            {{ userProfile.initials }}
          </div>
          <div class="profile-info">
            <span class="profile-name">{{ userProfile.name }}</span>
            <div class="role-badge-container">
              <span :class="['role-badge', props.currentRole]">
                {{ props.currentRole === 'admin' ? 'Admin' : props.currentRole === 'manager' ? 'Quản lý' : 'Dược sĩ' }}
              </span>
            </div>
          </div>
          <svg viewBox="0 0 24 24" :class="['arrow-icon', { 'open': showRoleDropdown }]" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="6 9 12 15 18 9" />
          </svg>
        </div>

        <!-- Role Selector Dropdown Menu -->
        <Transition name="fade">
          <div class="role-dropdown" v-if="showRoleDropdown">
            <div class="dropdown-label">Giả lập Vai trò (Testing)</div>
            <ul class="role-list">
              <li>
                <button 
                  :class="['role-option', { 'active': currentRole === 'pharmacist' }]" 
                  @click="handleRoleSelect('pharmacist')"
                >
                  <span class="option-dot pharmacist"></span>
                  <div class="option-text">
                    <span class="option-title">Dược sĩ (Pharmacist)</span>
                    <span class="option-desc">Bán thuốc, kiểm tra dị ứng, tương tác</span>
                  </div>
                </button>
              </li>
              <li>
                <button 
                  :class="['role-option', { 'active': currentRole === 'manager' }]" 
                  @click="handleRoleSelect('manager')"
                >
                  <span class="option-dot manager"></span>
                  <div class="option-text">
                    <span class="option-title">Quản lý (Manager)</span>
                    <span class="option-desc">Xem thống kê, báo cáo, lịch sử</span>
                  </div>
                </button>
              </li>
              <li>
                <button 
                  :class="['role-option', { 'active': currentRole === 'admin' }]" 
                  @click="handleRoleSelect('admin')"
                >
                  <span class="option-dot admin"></span>
                  <div class="option-text">
                    <span class="option-title">Quản trị viên (Admin)</span>
                    <span class="option-desc">Cấu hình hệ thống, quản lý tài khoản</span>
                  </div>
                </button>
              </li>
            </ul>
            <div class="dropdown-footer">
              <button class="logout-btn" @click="handleLogout">
                <svg viewBox="0 0 24 24" class="logout-icon" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                  <polyline points="16 17 21 12 16 7" />
                  <line x1="21" y1="12" x2="9" y2="12" />
                </svg>
                <span>Đăng xuất tài khoản</span>
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </div>
  </header>
</template>

<style scoped>
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: var(--header-height);
  padding: 0 24px;
  background-color: var(--bg-card);
  border-bottom: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  z-index: 90;
  width: 100%;
}

.left-section, .right-section {
  display: flex;
  align-items: center;
  gap: 20px;
}

/* Toggle Menu Button styling */
.toggle-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  color: var(--text-muted);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.toggle-btn:hover {
  background-color: var(--bg-main);
  color: var(--primary);
  border-color: var(--primary-medium);
}

.toggle-btn .icon {
  width: 20px;
  height: 20px;
}

/* Search Bar Styling */
.search-bar {
  position: relative;
  display: flex;
  align-items: center;
  width: 380px;
  max-width: 100%;
}

.search-icon {
  position: absolute;
  left: 14px;
  width: 18px;
  height: 18px;
  color: var(--text-muted);
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 10px 14px 10px 42px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  color: var(--text-main);
  outline: none;
  font-size: 14px;
  transition: all var(--transition-fast);
}

.search-input:focus {
  border-color: var(--border-focus);
  background-color: #ffffff;
  box-shadow: 0 0 0 3px var(--primary-glow);
}

/* Status Indicator led styling */
.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: var(--success-bg);
  padding: 6px 12px;
  border-radius: var(--border-radius-full);
  border: 1px solid rgba(16, 185, 129, 0.2);
}

.pulsing-dot {
  width: 8px;
  height: 8px;
  background-color: var(--success);
  border-radius: 50%;
  box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
  animation: pulse 1.8s infinite;
}

@keyframes pulse {
  0% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
  }
  70% {
    transform: scale(1);
    box-shadow: 0 0 0 6px rgba(16, 185, 129, 0);
  }
  100% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0);
  }
}

.status-text {
  font-size: 12px;
  font-weight: 600;
  color: var(--success);
}

/* Button & Dropdown containers */
.dropdown-container {
  position: relative;
}

.icon-btn {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  color: var(--text-muted);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.icon-btn:hover {
  background-color: var(--bg-main);
  color: var(--text-main);
}

.icon-btn .icon {
  width: 20px;
  height: 20px;
}

.badge {
  position: absolute;
  top: -4px;
  right: -4px;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  background-color: var(--danger);
  color: white;
  font-size: 11px;
  font-weight: 700;
  border-radius: var(--border-radius-full);
  border: 2px solid #ffffff;
}

.divider {
  width: 1px;
  height: 28px;
  background-color: var(--border-color);
}

/* User Profile Trigger Styling */
.profile-trigger {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 12px 6px 6px;
  border-radius: var(--border-radius-full);
  cursor: pointer;
  transition: all var(--transition-fast);
  border: 1px solid transparent;
}

.profile-trigger:hover {
  background-color: var(--bg-main);
  border-color: var(--border-color);
}

.avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  color: white;
  font-weight: 700;
  font-size: 14px;
  border-radius: 50%;
  box-shadow: var(--shadow-sm);
}

.profile-info {
  display: flex;
  flex-direction: column;
}

.profile-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
}

.role-badge-container {
  display: flex;
  margin-top: 1px;
}

.role-badge {
  font-size: 10px;
  font-weight: 700;
  padding: 1px 6px;
  border-radius: 4px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.role-badge.admin {
  background-color: var(--danger-bg);
  color: var(--danger);
}

.role-badge.pharmacist {
  background-color: var(--success-bg);
  color: var(--success);
}

.role-badge.manager {
  background-color: var(--info-bg);
  color: var(--info);
}

.arrow-icon {
  width: 16px;
  height: 16px;
  color: var(--text-muted);
  transition: transform var(--transition-fast);
}

.arrow-icon.open {
  transform: rotate(180deg);
}

/* Notifications Dropdown Panel styling */
.notif-dropdown {
  position: absolute;
  top: 50px;
  right: 0;
  width: 360px;
  background-color: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-premium);
  overflow: hidden;
  animation: slideDown var(--transition-fast);
}

.dropdown-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 18px;
  border-bottom: 1px solid var(--border-color);
  background-color: var(--bg-main);
}

.dropdown-title {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-main);
}

.clear-btn {
  background: transparent;
  border: none;
  font-size: 12px;
  font-weight: 600;
  color: var(--primary-medium);
  cursor: pointer;
}

.clear-btn:hover {
  color: var(--primary);
  text-decoration: underline;
}

.dropdown-body {
  max-height: 320px;
  overflow-y: auto;
}

.empty-notif {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 30px;
  color: var(--text-muted);
  gap: 8px;
}

.empty-icon {
  width: 36px;
  height: 36px;
}

.notif-list {
  list-style: none;
}

.notif-item {
  position: relative;
  display: flex;
  gap: 12px;
  padding: 14px 18px;
  border-bottom: 1px solid #f1f5f9;
  transition: background-color var(--transition-fast);
}

.notif-item:hover {
  background-color: var(--bg-main);
}

.notif-indicator {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
}

.notif-item.warning .notif-indicator { background-color: var(--warning); }
.notif-item.danger .notif-indicator { background-color: var(--danger); }
.notif-item.success .notif-indicator { background-color: var(--success); }

.notif-content {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.notif-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-main);
}

.notif-desc {
  font-size: 12px;
  color: var(--text-muted);
}

.notif-time {
  font-size: 11px;
  color: #94a3b8;
  margin-top: 2px;
}

/* Role dropdown panel styling */
.role-dropdown {
  position: absolute;
  top: 55px;
  right: 0;
  width: 280px;
  background-color: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-premium);
  overflow: hidden;
  animation: slideDown var(--transition-fast);
}

.dropdown-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  padding: 12px 16px;
  background-color: var(--bg-main);
  border-bottom: 1px solid var(--border-color);
  letter-spacing: 0.5px;
}

.role-list {
  list-style: none;
}

.role-option {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  width: 100%;
  padding: 12px 16px;
  background: transparent;
  border: none;
  cursor: pointer;
  transition: all var(--transition-fast);
  text-align: left;
  border-bottom: 1px solid #f1f5f9;
}

.role-option:last-child {
  border-bottom: none;
}

.role-option:hover {
  background-color: var(--bg-main);
}

.role-option.active {
  background-color: var(--primary-bg);
}

.option-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-top: 5px;
  flex-shrink: 0;
}

.option-dot.admin { background-color: var(--danger); }
.option-dot.pharmacist { background-color: var(--success); }
.option-dot.manager { background-color: var(--info); }

.option-text {
  display: flex;
  flex-direction: column;
}

.option-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-main);
}

.option-desc {
  font-size: 11px;
  color: var(--text-muted);
  line-height: 1.3;
  margin-top: 1px;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 768px) {
  .search-bar {
    width: 160px;
  }
  .status-text {
    display: none !important;
  }
  .profile-info {
    display: none !important;
  }
  .profile-trigger {
    padding: 0;
  }
}

@media (max-width: 480px) {
  .search-bar {
    display: none;
  }
}

.dropdown-footer {
  padding: 8px 12px;
  background-color: var(--bg-main);
  border-top: 1px solid var(--border-color);
}

.logout-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 8px 12px;
  background: transparent;
  border: 1px solid transparent;
  border-radius: var(--border-radius-sm);
  color: var(--danger);
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.logout-btn:hover {
  background-color: var(--danger-bg);
  border-color: rgba(239, 68, 68, 0.15);
}

.logout-icon {
  width: 16px;
  height: 16px;
}
</style>
