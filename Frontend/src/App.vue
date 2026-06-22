<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Sidebar from './components/Sidebar.vue'
import Header from './components/Header.vue'
import { usePharmacyStore } from './store/pharmacy'

const store = usePharmacyStore()
const currentRole = store.currentRole

const isCollapsed = ref(window.innerWidth <= 768)
const route = useRoute()
const router = useRouter()

// Get active tab from current route name
const activeTab = computed(() => {
  return (route.name as string) || 'dashboard'
})

// Watch role change to check accessibility and redirect if needed
const roleAllowedTabs: Record<'admin' | 'pharmacist' | 'manager', string[]> = {
  admin: ['dashboard', 'medicines', 'patients', 'safety-alerts', 'sales-history', 'users', 'settings'],
  pharmacist: ['dashboard', 'sell-medicine', 'medicines', 'patients', 'safety-alerts', 'sales-history'],
  manager: ['dashboard', 'medicines', 'patients', 'safety-alerts', 'sales-history']
}

watch(currentRole, (newRole) => {
  const allowed = roleAllowedTabs[newRole]
  if (!allowed.includes(activeTab.value)) {
    router.push({ name: 'dashboard' })
  }
})

// Also redirect on route change if role doesn't have permission
watch(() => route.name, (newRouteName) => {
  if (newRouteName) {
    const allowed = roleAllowedTabs[currentRole.value]
    if (!allowed.includes(newRouteName as string)) {
      router.push({ name: 'dashboard' })
    }
  }
})

const handleToggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
}

const handleTabChange = (tabId: string) => {
  router.push({ name: tabId })
}
</script>

<template>
  <div class="app-layout">
    <!-- Left Sidebar component -->
    <Sidebar 
      :is-collapsed="isCollapsed" 
      :current-role="currentRole" 
      :active-tab="activeTab"
      @update:is-collapsed="isCollapsed = $event"
      @change-tab="handleTabChange"
    />

    <!-- Sidebar backdrop for mobile overlay -->
    <div 
      class="sidebar-backdrop" 
      v-show="!isCollapsed" 
      @click="isCollapsed = true"
    ></div>

    <!-- Right Content container -->
    <div class="main-container">
      <!-- Upper Header component -->
      <Header 
        :is-collapsed="isCollapsed" 
        :current-role="currentRole"
        @toggle-sidebar="handleToggleSidebar"
        @update:current-role="currentRole = $event"
      />

      <!-- Scrollable Main Content Area -->
      <main class="content-area">
        <div class="content-wrapper">
          <!-- Active Tab Headings -->
          <div class="page-header">
            <div>
              <span class="breadcrumb">SafePharmacy / CSDL: PharmacySafetyDB</span>
              <h1 class="page-title">
                <span v-if="activeTab === 'dashboard'">Bảng tổng quan</span>
                <span v-else-if="activeTab === 'sell-medicine'">Khu vực bán thuốc</span>
                <span v-else-if="activeTab === 'medicines'">Danh mục thuốc (Medicines)</span>
                <span v-else-if="activeTab === 'patients'">Hồ sơ bệnh nhân (Patients)</span>
                <span v-else-if="activeTab === 'safety-alerts'">Dữ liệu An toàn (Interactions & Contraindications)</span>
                <span v-else-if="activeTab === 'sales-history'">Lịch sử giao dịch (Sales & Warnings)</span>
                <span v-else-if="activeTab === 'users'">Quản lý nhân viên (Users & Roles)</span>
                <span v-else-if="activeTab === 'settings'">Cấu hình hệ thống (Settings)</span>
                <span v-else-if="activeTab === 'reports'">Báo cáo thống kê (Reports)</span>
              </h1>
            </div>
            <div class="date-badge">
              <svg viewBox="0 0 24 24" class="calendar-icon" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                <line x1="16" y1="2" x2="16" y2="6" />
                <line x1="8" y1="2" x2="8" y2="6" />
                <line x1="3" y1="10" x2="21" y2="10" />
              </svg>
              <span>Hôm nay: 21 Tháng 6, 2026</span>
            </div>
          </div>

          <!-- Router view replaces individual tab sections -->
          <router-view />
        </div>
      </main>
    </div>
  </div>
</template>

<style>
.app-layout {
  display: flex;
  width: 100vw;
  height: 100vh;
  overflow: hidden;
  background-color: var(--bg-main);
}

.main-container {
  display: flex;
  flex-direction: column;
  flex: 1;
  height: 100vh;
  overflow: hidden;
  min-width: 0;
}

.content-area {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
  overflow-x: hidden;
  background-color: var(--bg-main);
}

.content-wrapper {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Page Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 16px;
}

.breadcrumb {
  font-size: 11px;
  font-weight: 700;
  color: var(--text-muted);
  letter-spacing: 0.5px;
  text-transform: uppercase;
}

.page-title {
  font-size: 24px;
  font-weight: 800;
  color: var(--text-main);
  margin-top: 4px;
}

.date-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: var(--bg-card);
  padding: 8px 16px;
  border-radius: var(--border-radius-md);
  border: 1px solid var(--border-color);
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
  box-shadow: var(--shadow-sm);
}

.calendar-icon {
  width: 16px;
  height: 16px;
  color: var(--primary-medium);
}

/* Dashboard Statistics Grid */
.stat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
}

.stat-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: var(--bg-card);
  padding: 24px;
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  transition: transform var(--transition-normal), box-shadow var(--transition-normal);
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
}

.stat-value {
  font-size: 32px;
  font-weight: 800;
  color: var(--text-main);
  margin: 6px 0;
  line-height: 1;
}

.stat-trend {
  font-size: 12px;
  font-weight: 600;
}

.stat-trend.green { color: var(--success); }
.stat-trend.red { color: var(--danger); }

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--border-radius-md);
}

.teal-icon { color: var(--primary-medium); width: 24px; height: 24px;}
.red-icon { color: var(--danger); width: 24px; height: 24px;}
.blue-icon { color: var(--info); width: 24px; height: 24px;}

.success-bg { background-color: var(--primary-bg); }
.danger-bg { background-color: var(--danger-bg); }
.info-bg { background-color: var(--info-bg); }

/* Dashboard Layout Grid */
.dashboard-grid {
  display: grid;
  grid-template-columns: 1.6fr 1fr;
  gap: 24px;
}

@media (max-width: 1024px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}

.grid-card {
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 24px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.card-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
}

.text-btn {
  background: transparent;
  border: none;
  font-size: 13px;
  font-weight: 600;
  color: var(--primary-medium);
  cursor: pointer;
}

.text-btn:hover {
  color: var(--primary);
  text-decoration: underline;
}

/* Dashboard Table styling */
.dashboard-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.dashboard-table th {
  padding: 12px 8px;
  font-size: 12px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  border-bottom: 1px solid var(--border-color);
}

.dashboard-table td {
  padding: 14px 8px;
  font-size: 14px;
  border-bottom: 1px solid #f1f5f9;
}

.dashboard-table tr:last-child td {
  border-bottom: none;
}

.status-tag {
  font-size: 11px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: var(--border-radius-sm);
  display: inline-block;
}

.status-tag.safe { background-color: var(--success-bg); color: var(--success); }
.status-tag.warning { background-color: var(--warning-bg); color: var(--warning); }
.status-tag.danger { background-color: var(--danger-bg); color: var(--danger); }

/* Warning / Alert Items list */
.alert-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.alert-item {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 16px;
  border-radius: var(--border-radius-md);
  border: 1px solid transparent;
}

.alert-item.high-risk {
  background-color: var(--danger-bg);
  border-color: rgba(239, 68, 68, 0.15);
}

.alert-item.patient-allergy {
  background-color: var(--warning-bg);
  border-color: rgba(245, 158, 11, 0.15);
}

.alert-badge {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  padding: 2px 6px;
  border-radius: 4px;
  align-self: flex-start;
  background-color: var(--danger);
  color: white;
}

.alert-desc {
  font-size: 13px;
  color: var(--text-main);
  line-height: 1.4;
}

.alert-time {
  font-size: 11px;
  color: var(--text-muted);
}

/* Common form & layouts */
.form-container {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 24px;
}

@media (max-width: 900px) {
  .form-container {
    grid-template-columns: 1fr;
  }
}

.form-section {
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 24px;
}

.section-title {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-main);
  margin-bottom: 20px;
  border-left: 3px solid var(--primary-medium);
  padding-left: 10px;
}

.patient-selector-wrapper {
  margin-bottom: 16px;
}

.form-label {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
  margin-bottom: 6px;
}

.form-control {
  width: 100%;
  padding: 10px 14px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  outline: none;
  font-size: 14px;
  transition: all var(--transition-fast);
}

.form-control:focus {
  border-color: var(--border-focus);
  background-color: #ffffff;
}

.select-control {
  appearance: none;
  background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%2364748b' stroke-width='2'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 14px center;
  background-size: 16px;
  padding-right: 40px;
}

.patient-card-demo {
  border: 1px dashed var(--border-color);
  border-radius: var(--border-radius-md);
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  background-color: #fafbfd;
  margin-top: 16px;
}

.patient-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #f1f5f9;
  padding-bottom: 10px;
}

.patient-header h4 {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
}

.gender-age {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
}

.patient-details-grid {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-row {
  display: flex;
  font-size: 13px;
}

.detail-label {
  width: 110px;
  color: var(--text-muted);
  font-weight: 500;
}

.detail-value {
  flex: 1;
  color: var(--text-main);
  font-weight: 600;
}

.condition-toggles {
  display: flex;
  gap: 10px;
}

.cond-badge {
  font-size: 11px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: var(--border-radius-sm);
  background-color: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.cond-badge.active {
  background-color: var(--danger-bg);
  color: var(--danger);
  border-color: rgba(239, 68, 68, 0.2);
}

.allergy-tags {
  display: flex;
  flex-direction: column;
  gap: 6px;
  border-top: 1px solid #f1f5f9;
  padding-top: 10px;
}

.tag-title {
  font-size: 12px;
  color: var(--text-muted);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.tag {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 4px;
}

.tag.danger { background-color: var(--danger-bg); color: var(--danger); }
.tag.warning { background-color: var(--warning-bg); color: var(--warning); }

.empty-text {
  font-size: 13px;
  color: #94a3b8;
  font-style: italic;
}

/* Builder Inputs Grid */
.builder-inputs {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.input-row {
  display: flex;
  gap: 12px;
}

.input-col {
  flex: 1;
}

.max-100 {
  max-width: 100px;
}

.primary-btn {
  background-color: var(--primary-medium);
  color: white;
  border: none;
  padding: 11px 20px;
  border-radius: var(--border-radius-md);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  box-shadow: 0 2px 4px rgba(13, 148, 136, 0.15);
}

.primary-btn:hover {
  background-color: var(--primary);
}

.primary-btn:disabled {
  background-color: #cbd5e1;
  color: #94a3b8;
  cursor: not-allowed;
  box-shadow: none;
}

.secondary-btn {
  background-color: var(--bg-main);
  color: var(--text-main);
  border: 1px solid var(--border-color);
  padding: 11px 20px;
  border-radius: var(--border-radius-md);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.secondary-btn:hover {
  background-color: #e2e8f0;
}

.full-width {
  width: 100%;
}

.cart-table-wrapper {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.cart-total-row {
  display: flex;
  justify-content: flex-end;
  font-size: 15px;
  color: var(--text-main);
  border-top: 1px solid var(--border-color);
  padding-top: 12px;
}

.cart-total-row strong {
  font-size: 18px;
  color: var(--primary);
  margin-left: 6px;
}

.cart-actions-row {
  display: flex;
  justify-content: flex-end;
}

.safety-btn {
  background: linear-gradient(135deg, var(--primary-light), var(--primary));
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: var(--border-radius-md);
  font-weight: 700;
  cursor: pointer;
  transition: all var(--transition-fast);
  box-shadow: 0 4px 10px rgba(13, 148, 136, 0.3);
  gap: 10px;
}

.safety-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 14px rgba(13, 148, 136, 0.4);
}

.safety-icon-btn {
  width: 18px;
  height: 18px;
}

.delete-btn {
  background: transparent;
  border: none;
  font-size: 18px;
  color: var(--danger);
  cursor: pointer;
  font-weight: 700;
}

.delete-btn:hover {
  transform: scale(1.2);
}

.cart-placeholder {
  min-height: 200px;
  border: 2px dashed #e2e8f0;
  border-radius: var(--border-radius-lg);
  padding: 30px;
  text-align: center;
  color: var(--text-muted);
}

.cart-icon {
  width: 48px;
  height: 48px;
  margin-bottom: 12px;
  color: #cbd5e1;
}

/* Large Data Tables */
.table-container {
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 24px;
}

.table-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  gap: 16px;
}

.search-input-small {
  width: 280px;
  padding: 8px 12px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  font-size: 14px;
  outline: none;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.data-table th {
  padding: 14px 16px;
  font-size: 12px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  border-bottom: 1px solid var(--border-color);
  background-color: var(--bg-main);
}

.data-table td {
  padding: 16px;
  font-size: 14px;
  border-bottom: 1px solid #f1f5f9;
}

.data-table tr:hover td {
  background-color: #fafbfd;
}

.spec-cond-badges {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.inline-block {
  display: inline-block;
  margin-right: 4px;
  margin-bottom: 4px;
}

/* Switch styling for settings screen */
.setting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
  border-bottom: 1px solid #f1f5f9;
}

.setting-item:last-child {
  border-bottom: none;
}

.setting-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.setting-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
}

.setting-desc {
  font-size: 12px;
  color: var(--text-muted);
}

.switch {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
  flex-shrink: 0;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #cbd5e1;
  transition: .3s;
  border-radius: 24px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: .3s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: var(--primary-medium);
}

input:checked + .slider:before {
  transform: translateX(20px);
}

/* Modal Overlay Styling */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(4px);
  z-index: 1000;
  padding: 20px;
}

.modal-card {
  width: 680px;
  max-width: 100%;
  max-height: 90vh;
  background-color: #ffffff;
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-premium);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid var(--border-color);
  background-color: var(--bg-main);
}

.modal-title-area {
  display: flex;
  align-items: center;
  gap: 12px;
}

.modal-title-area h3 {
  font-size: 18px;
  font-weight: 800;
  color: var(--text-main);
}

.safety-modal-icon {
  width: 26px;
  height: 26px;
  color: var(--primary-medium);
}

.close-modal-btn {
  background: transparent;
  border: none;
  font-size: 28px;
  line-height: 1;
  color: var(--text-muted);
  cursor: pointer;
}

.close-modal-btn:hover {
  color: var(--text-main);
}

.modal-body {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.patient-quick-summary {
  display: flex;
  gap: 20px;
  background-color: var(--bg-main);
  padding: 12px 16px;
  border-radius: var(--border-radius-md);
  font-size: 13px;
  border: 1px solid var(--border-color);
}

.warnings-holder {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.warning-alert-count {
  font-size: 14px;
  font-weight: 700;
  color: var(--danger);
}

.warnings-scroll-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.safety-warning-card {
  border: 1px solid rgba(239, 68, 68, 0.2);
  background-color: var(--danger-bg);
  border-radius: var(--border-radius-md);
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  transition: opacity var(--transition-fast);
}

.safety-warning-card.acknowledged {
  opacity: 0.65;
  border-color: var(--border-color);
  background-color: var(--bg-main);
}

.warning-card-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.warning-tag {
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--danger);
  background-color: rgba(239, 68, 68, 0.08);
  padding: 2px 8px;
  border-radius: var(--border-radius-sm);
}

.acknowledged .warning-tag {
  color: var(--text-muted);
  background-color: #e2e8f0;
}

.severity-badge-high {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  background-color: var(--danger);
  color: white;
  padding: 2px 6px;
  border-radius: 4px;
}

.warning-msg {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
  line-height: 1.45;
}

.recommendation-box {
  background-color: white;
  border: 1px solid rgba(239, 68, 68, 0.1);
  padding: 10px 14px;
  border-radius: var(--border-radius-sm);
  font-size: 13px;
}

.recommendation-box strong {
  color: #b91c1c;
  display: block;
  margin-bottom: 4px;
}

.resolution-row {
  margin-top: 6px;
  border-top: 1px dashed rgba(239, 68, 68, 0.2);
  padding-top: 12px;
}

.ack-input-group {
  display: flex;
  gap: 10px;
}

.text-control-sm {
  padding: 8px 12px;
  font-size: 13px;
}

.ack-btn {
  background-color: var(--warning);
  color: white;
  border: none;
  padding: 0 16px;
  border-radius: var(--border-radius-md);
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  white-space: nowrap;
  transition: background var(--transition-fast);
}

.ack-btn:hover {
  background-color: #d97706;
}

.ack-done {
  font-size: 13px;
  color: var(--success);
  font-weight: 600;
}

.safety-success-message {
  flex-direction: column;
  text-align: center;
  padding: 40px 20px;
}

.success-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  max-width: 420px;
}

.success-tick-icon {
  width: 56px;
  height: 56px;
  color: var(--success);
}

.success-content h4 {
  font-size: 18px;
  font-weight: 800;
  color: var(--text-main);
}

.success-content p {
  font-size: 14px;
  color: var(--text-muted);
  line-height: 1.45;
}

.modal-footer {
  display: flex;
  justify-content: space-between;
  padding: 16px 24px;
  border-top: 1px solid var(--border-color);
  background-color: var(--bg-main);
}

/* Dashboard Premium Seeding Banner */
.system-seeding-banner {
  display: flex;
  align-items: center;
  gap: 16px;
  background-color: var(--primary-bg);
  border: 1px solid rgba(13, 148, 136, 0.15);
  border-radius: var(--border-radius-lg);
  padding: 16px 20px;
  margin-bottom: 24px;
  box-shadow: var(--shadow-sm);
}

.banner-status-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--border-radius-md);
  background-color: #ffffff;
  border: 1.5px solid var(--primary-medium);
  color: var(--primary-medium);
  flex-shrink: 0;
  box-shadow: 0 4px 6px rgba(13, 148, 136, 0.08);
}

.banner-svg {
  width: 22px;
  height: 22px;
}

.banner-text h4 {
  font-size: 14.5px;
  font-weight: 750;
  color: var(--primary);
  margin-bottom: 2px;
}

.badge-db-connected {
  font-size: 11px;
  font-weight: 800;
  color: var(--success);
  background-color: var(--success-bg);
  padding: 2px 8px;
  border-radius: var(--border-radius-sm);
  margin-left: 6px;
  border: 1px solid rgba(16, 185, 129, 0.2);
}

.banner-text p {
  font-size: 12.5px;
  color: var(--text-muted);
  font-weight: 550;
}

/* KPI icon background adjustments */
.stat-icon.success-bg {
  background-color: var(--primary-bg);
  color: var(--primary-medium);
}

.stat-icon.info-bg {
  background-color: var(--info-bg);
  color: var(--info);
}

.stat-icon.danger-bg {
  background-color: var(--danger-bg);
  color: var(--danger);
}

/* Enhanced Analytics grid layout */
.analytics-main-column {
  display: flex;
  flex-direction: column;
}

.analytics-side-column {
  display: flex;
  flex-direction: column;
}

/* Interactive SVG Chart styling */
.interactive-svg-container {
  width: 100%;
  overflow: visible;
}

.svg-chart-element {
  overflow: visible;
}

.interactive-bar:hover {
  opacity: 0.85;
}

.interactive-dot:hover {
  transform: scale(1.3);
}

/* Chart Tooltip Overlay */
.chart-tooltip-floating {
  pointer-events: none;
  animation: tooltipFade var(--transition-fast);
}

@keyframes tooltipFade {
  from {
    opacity: 0;
    transform: translateY(4px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Progress bar severity list */
.severity-progress-container {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.severity-item-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.indicator-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.indicator-dot.high-risk { background-color: var(--danger); }
.indicator-dot.medium-risk { background-color: var(--warning); }
.indicator-dot.low-risk { background-color: var(--success); }

/* Warning Feed details */
.recent-warnings-feed {
  padding: 20px;
}

.feed-item {
  border-left: 3px solid var(--danger);
  padding-left: 12px;
  margin-bottom: 12px;
}

.feed-item:last-child {
  margin-bottom: 0;
}

.feed-badge-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2px;
}

.feed-type-badge {
  font-size: 9.5px;
  font-weight: 800;
  background-color: var(--danger-bg);
  color: var(--danger);
  padding: 1px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}

.feed-time {
  font-size: 11px;
  color: var(--text-muted);
}

.feed-msg {
  font-size: 12px;
  color: var(--text-main);
  line-height: 1.45;
  font-weight: 600;
}

.feed-resolution {
  font-size: 11.5px;
  background-color: var(--bg-main);
  padding: 4px 8px;
  border-radius: 4px;
  margin-top: 4px;
}

/* Sidebar backdrop overlay for mobile view */
.sidebar-backdrop {
  display: none;
}

@media (max-width: 768px) {
  .sidebar-backdrop {
    display: block;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(15, 23, 42, 0.4);
    backdrop-filter: blur(4px);
    z-index: 95;
  }
}
</style>
