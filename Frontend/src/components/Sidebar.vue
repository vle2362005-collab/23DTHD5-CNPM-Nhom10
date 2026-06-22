<script setup lang="ts">
import { computed } from 'vue'

// Prop definitions
interface Props {
  isCollapsed: boolean
  currentRole: 'admin' | 'pharmacist' | 'manager'
  activeTab: string
}

const props = withDefaults(defineProps<Props>(), {
  isCollapsed: false,
  currentRole: 'pharmacist',
  activeTab: 'dashboard'
})

// Emit definitions
const emit = defineEmits<{
  (e: 'update:isCollapsed', value: boolean): void
  (e: 'changeTab', tab: string): void
}>()

// Navigation menu item configuration
interface MenuItem {
  id: string
  name: string
  roles: ('admin' | 'pharmacist' | 'manager')[]
}

const menuItems = computed<MenuItem[]>(() => [
  {
    id: 'dashboard',
    name: 'Tổng quan',
    roles: ['admin', 'pharmacist', 'manager']
  },
  {
    id: 'sell-medicine',
    name: 'Bán thuốc an toàn',
    roles: ['pharmacist']
  },
  {
    id: 'medicines',
    name: 'Danh mục thuốc',
    roles: ['admin', 'pharmacist', 'manager']
  },
  {
    id: 'patients',
    name: 'Hồ sơ khách hàng',
    roles: ['admin', 'pharmacist', 'manager']
  },
  {
    id: 'safety-alerts',
    name: 'Cảnh báo an toàn',
    roles: ['admin', 'pharmacist', 'manager']
  },
  {
    id: 'sales-history',
    name: 'Lịch sử bán thuốc',
    roles: ['admin', 'pharmacist', 'manager']
  },
  {
    id: 'reports',
    name: 'Báo cáo thống kê',
    roles: ['admin', 'manager']
  },
  {
    id: 'users',
    name: 'Quản lý nhân viên',
    roles: ['admin']
  },
  {
    id: 'settings',
    name: 'Cấu hình hệ thống',
    roles: ['admin']
  }
])

// Filter menu items by the user's current role
const filteredMenuItems = computed(() => {
  return menuItems.value.filter(item => item.roles.includes(props.currentRole))
})

const toggleSidebar = () => {
  emit('update:isCollapsed', !props.isCollapsed)
}

const handleTabSelect = (tabId: string) => {
  emit('changeTab', tabId)
}
</script>

<template>
  <aside :class="['sidebar', { 'collapsed': isCollapsed }]">
    <!-- Brand Logo Area -->
    <div class="brand">
      <div class="logo-container">
        <!-- SVG Shield and Pill logo -->
        <svg viewBox="0 0 24 24" class="logo-icon" fill="none" stroke="currentColor" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
        </svg>
      </div>
      <div class="brand-text-container" v-show="!isCollapsed">
        <span class="brand-name">Safe<span class="brand-accent">Pharm</span></span>
        <span class="brand-subtitle">An toàn là trên hết</span>
      </div>
    </div>

    <!-- Navigation Menu Items -->
    <nav class="nav-menu">
      <ul class="nav-list">
        <li v-for="item in filteredMenuItems" :key="item.id" class="nav-item">
          <button 
            :class="['nav-link', { 'active': activeTab === item.id }]"
            @click="handleTabSelect(item.id)"
            :title="item.name"
          >
            <!-- Custom SVG icons mapped by item.id -->
            <div class="icon-wrapper">
              <!-- Dashboard -->
              <svg v-if="item.id === 'dashboard'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="3" y="3" width="7" height="9" rx="1" />
                <rect x="14" y="3" width="7" height="5" rx="1" />
                <rect x="14" y="12" width="7" height="9" rx="1" />
                <rect x="3" y="16" width="7" height="5" rx="1" />
              </svg>
              <!-- Bán thuốc an toàn -->
              <svg v-else-if="item.id === 'sell-medicine'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" />
                <rect x="8" y="2" width="8" height="4" rx="1" />
                <path d="M12 11h4" />
                <path d="M12 16h4" />
                <path d="M8 11h.01" />
                <path d="M8 16h.01" />
              </svg>
              <!-- Danh mục thuốc -->
              <svg v-else-if="item.id === 'medicines'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M4.5 12.5l8-8a4.95 4.95 0 1 1 7 7l-8 8a4.95 4.95 0 0 1-7-7z" />
                <path d="M8.5 8.5l7 7" />
                <circle cx="16" cy="8" r="1.5" />
                <circle cx="8" cy="16" r="1.5" />
              </svg>
              <!-- Hồ sơ khách hàng -->
              <svg v-else-if="item.id === 'patients'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
                <circle cx="9" cy="7" r="4" />
                <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
                <path d="M16 3.13a4 4 0 0 1 0 7.75" />
              </svg>
              <!-- Cảnh báo an toàn -->
              <svg v-else-if="item.id === 'safety-alerts'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                <line x1="12" y1="9" x2="12" y2="13" />
                <line x1="12" y1="17" x2="12.01" y2="17" />
              </svg>
              <!-- Lịch sử bán thuốc -->
              <svg v-else-if="item.id === 'sales-history'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10" />
                <polyline points="12 6 12 12 16 14" />
                <path d="M12 2a10 10 0 0 1 10 10" />
              </svg>
              <!-- Báo cáo thống kê -->
              <svg v-else-if="item.id === 'reports'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="20" x2="18" y2="10" />
                <line x1="12" y1="20" x2="12" y2="4" />
                <line x1="6" y1="20" x2="6" y2="14" />
              </svg>
              <!-- Quản lý nhân viên -->
              <svg v-else-if="item.id === 'users'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                <polyline points="14 2 14 8 20 8" />
                <circle cx="10" cy="13" r="2" />
                <path d="M6 18c0-2 3-3 4-3s4 1 4 3" />
              </svg>
              <!-- Cấu hình hệ thống -->
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="3" />
                <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
              </svg>
            </div>
            <span class="label" v-show="!isCollapsed">{{ item.name }}</span>
            <div class="active-indicator" v-if="activeTab === item.id"></div>
          </button>
        </li>
      </ul>
    </nav>

    <!-- Sidebar footer with collapse action and info -->
    <div class="sidebar-footer">
      <button class="collapse-btn" @click="toggleSidebar" :title="isCollapsed ? 'Mở rộng' : 'Thu gọn'">
        <svg viewBox="0 0 24 24" class="arrow-icon" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline v-if="isCollapsed" points="9 18 15 12 9 6" />
          <polyline v-else points="15 18 9 12 15 6" />
        </svg>
        <span class="btn-text" v-show="!isCollapsed">Thu gọn menu</span>
      </button>
      <div class="version-info" v-show="!isCollapsed">
        <span>Phiên bản 1.0.0</span>
      </div>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  display: flex;
  flex-direction: column;
  width: var(--sidebar-width);
  height: 100vh;
  background-color: var(--bg-sidebar);
  border-right: 1px solid rgba(255, 255, 255, 0.05);
  color: var(--text-sidebar-light);
  transition: width var(--transition-normal);
  z-index: 100;
  flex-shrink: 0;
}

.sidebar.collapsed {
  width: var(--sidebar-collapsed-width);
}

/* Brand Area Styling */
.brand {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 24px;
  height: var(--header-height);
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  overflow: hidden;
}

.logo-container {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  background: linear-gradient(135deg, var(--primary-light), var(--primary));
  border-radius: var(--border-radius-md);
  color: #ffffff;
  flex-shrink: 0;
  box-shadow: 0 4px 10px rgba(13, 148, 136, 0.3);
}

.logo-icon {
  width: 22px;
  height: 22px;
}

.brand-text-container {
  display: flex;
  flex-direction: column;
  white-space: nowrap;
}

.brand-name {
  font-size: 18px;
  font-weight: 800;
  letter-spacing: 0.5px;
  color: #ffffff;
}

.brand-accent {
  color: var(--primary-light);
}

.brand-subtitle {
  font-size: 10px;
  color: #64748b;
  font-weight: 500;
}

/* Navigation List Styling */
.nav-menu {
  flex: 1;
  padding: 20px 12px;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Custom Scrollbar for Sidebar menu */
.nav-menu::-webkit-scrollbar {
  width: 4px;
}
.nav-menu::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
}

.nav-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  list-style: none;
}

.nav-item {
  width: 100%;
}

.nav-link {
  position: relative;
  display: flex;
  align-items: center;
  gap: 14px;
  width: 100%;
  padding: 12px 14px;
  background: transparent;
  border: none;
  border-radius: var(--border-radius-md);
  color: var(--text-sidebar-light);
  cursor: pointer;
  text-align: left;
  transition: all var(--transition-fast);
}

.icon-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  flex-shrink: 0;
}

.icon-wrapper svg {
  width: 22px;
  height: 22px;
}

.label {
  font-size: 14px;
  font-weight: 500;
  white-space: nowrap;
  transition: opacity var(--transition-fast);
}

/* Hover & Active States */
.nav-link:hover {
  color: #ffffff;
  background-color: rgba(255, 255, 255, 0.05);
}

.nav-link.active {
  color: var(--text-sidebar-active);
  background: linear-gradient(90deg, rgba(13, 148, 136, 0.2) 0%, rgba(13, 148, 136, 0.05) 100%);
  font-weight: 600;
}

.nav-link.active .icon-wrapper {
  color: var(--primary-light);
}

.active-indicator {
  position: absolute;
  left: 0;
  top: 15%;
  height: 70%;
  width: 4px;
  background-color: var(--primary-light);
  border-radius: 0 4px 4px 0;
  box-shadow: 0 0 10px var(--primary-light);
}

/* Sidebar Footer Styling */
.sidebar-footer {
  display: flex;
  flex-direction: column;
  padding: 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  gap: 10px;
}

.collapse-btn {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 10px 12px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: var(--border-radius-sm);
  color: var(--text-sidebar-light);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.collapse-btn:hover {
  color: #ffffff;
  background-color: rgba(255, 255, 255, 0.08);
  border-color: rgba(255, 255, 255, 0.15);
}

.arrow-icon {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
}

.btn-text {
  font-size: 13px;
  font-weight: 500;
  white-space: nowrap;
}

.version-info {
  text-align: center;
  font-size: 11px;
  color: #475569;
}

@media (max-width: 768px) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    width: var(--sidebar-width) !important;
    transform: translateX(0);
    transition: transform var(--transition-normal);
    box-shadow: var(--shadow-premium);
  }
  
  .sidebar.collapsed {
    transform: translateX(-100%);
  }
  
  .sidebar .brand-text-container,
  .sidebar .label,
  .sidebar .btn-text,
  .sidebar .version-info {
    display: block !important;
  }
}
</style>
