<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { usePharmacyStore } from '../store/pharmacy'

const router = useRouter()
const store = usePharmacyStore()

// Local states for Dashboard chart
const weeklyReport = ref([
  { day: 'T2', sales: 14000, warnings: 1 },
  { day: 'T3', sales: 25000, warnings: 2 },
  { day: 'T4', sales: 18500, warnings: 0 },
  { day: 'T5', sales: 32000, warnings: 3 },
  { day: 'T6', sales: 7000, warnings: 1 }, // HD-001 (7000đ, 1 warning)
  { day: 'T7', sales: 45000, warnings: 5 },
  { day: 'CN', sales: 22000, warnings: 0 }
])

const activeChartIndex = ref<number | null>(null)

// Computed properties for Dashboard statistics
const todaySalesSum = computed(() => {
  return store.sales.value.reduce((sum, s) => sum + s.TotalAmount, 0)
})

const totalInterventions = computed(() => {
  return store.warnings.value.filter(w => w.IsAcknowledged).length
})

const warningPathPoints = computed(() => {
  return weeklyReport.value.map((item, idx) => {
    const x = 40 + 10 + idx * 54
    const y = 160 - (item.warnings / 6) * 120
    return `${x},${y}`
  }).join(' ')
})

// Mockup Inventory levels for stock check F03/F12
const inventoryAlerts = ref([
  { name: 'Paracetamol 500mg', stock: 8, unit: 'Viên', status: 'Cảnh báo sắp hết' },
  { name: 'Amoxicillin 500mg', stock: 45, unit: 'Viên', status: 'An toàn' },
  { name: 'Ibuprofen 400mg', stock: 3, unit: 'Viên', status: 'Hết hàng nghiêm trọng' }
])

const navigateToTab = (tabId: string) => {
  router.push({ name: tabId })
}
</script>

<template>
  <div class="view-container">
    <!-- SQL DB Seeding Banner -->
    <div class="system-seeding-banner">
      <div class="banner-status-icon flex-center">
        <svg viewBox="0 0 24 24" class="banner-svg" fill="none" stroke="currentColor" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M20.25 6.375c0 2.278-3.694 4.125-8.25 4.125S3.75 8.653 3.75 6.375m16.5 0c0-2.278-3.694-4.125-8.25-4.125S3.75 4.097 3.75 6.375m16.5 0v11.25c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125V6.375m16.5 0v3.75m-16.5-3.75v3.75m16.5 0v3.75C20.25 16.153 16.556 18 12 18s-8.25-1.847-8.25-4.125v-3.75" />
        </svg>
      </div>
      <div class="banner-text">
        <h4>Kết nối Cơ sở dữ liệu: <span class="badge-db-connected">PharmacySafetyDB (Connected)</span></h4>
        <p>Khởi chạy động cơ kiểm tra an toàn với: 2 bệnh nhân, 3 danh mục thuốc, 1 tương tác hoạt chất chéo và 1 chống chỉ định lâm sàng.</p>
      </div>
    </div>

    <!-- KPI Cards Row -->
    <div class="stat-grid">
      <!-- KPI Card 1 -->
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Doanh thu trong ngày</span>
          <span class="stat-value">{{ todaySalesSum.toLocaleString() }}đ</span>
          <span class="stat-trend green">
            <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="3" style="display:inline; vertical-align:middle; margin-right:2px;"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18" /><polyline points="17 6 23 6 23 12" /></svg>
            +14.2% so với hôm qua
          </span>
        </div>
        <div class="stat-icon flex-center success-bg">
          <svg viewBox="0 0 24 24" class="teal-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 1v22M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
          </svg>
        </div>
      </div>
      <!-- KPI Card 2 -->
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Đơn hàng (Sales Checked)</span>
          <span class="stat-value">{{ store.sales.value.length }} đơn</span>
          <span class="stat-trend green">100% hoàn thành an toàn</span>
        </div>
        <div class="stat-icon flex-center info-bg">
          <svg viewBox="0 0 24 24" class="blue-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
      </div>
      <!-- KPI Card 3 -->
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Số cảnh báo phát hiện</span>
          <span class="stat-value">{{ store.warnings.value.length }} ca</span>
          <span class="stat-trend red">Chỉ số an toàn lâm sàng</span>
        </div>
        <div class="stat-icon flex-center danger-bg">
          <svg viewBox="0 0 24 24" class="red-icon" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
      </div>
      <!-- KPI Card 4 -->
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Số ca đã can thiệp</span>
          <span class="stat-value">{{ totalInterventions }} ca</span>
          <span class="stat-trend green">✓ Tất cả đã được xử lý</span>
        </div>
        <div class="stat-icon flex-center info-bg" style="background-color: var(--warning-bg);">
          <svg viewBox="0 0 24 24" class="blue-icon" stroke="var(--warning)" fill="none" stroke-width="2">
            <path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2M9 5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2" />
            <path d="M9 14l2 2 4-4" />
          </svg>
        </div>
      </div>
    </div>

    <!-- Dashboard Analytics grids -->
    <div class="dashboard-grid" style="margin-top: 24px;">
      <!-- Left Big Area: Charts & Main Sales -->
      <div class="analytics-main-column">
        <!-- Weekly sales & alerts chart -->
        <div class="grid-card chart-card-interactive" style="position: relative; margin-bottom: 24px;">
          <div class="chart-title-row" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
            <div>
              <h3 class="card-title" style="margin-bottom: 2px;">Phân tích Doanh thu & Cảnh báo trong tuần</h3>
              <span class="card-subtitle" style="font-size: 12px; color: var(--text-muted);">Trực quan hóa tương quan doanh số bán hàng và số ca phát hiện rủi ro</span>
            </div>
            <div class="chart-legend" style="display: flex; gap: 14px; font-size: 12px; font-weight: 600;">
              <span class="legend-item" style="display: flex; align-items: center; gap: 6px;"><span class="legend-color sales-color" style="width: 12px; height: 12px; background: linear-gradient(180deg, var(--primary-light), var(--primary)); border-radius: 3px; display: inline-block;"></span>Doanh thu (đ)</span>
              <span class="legend-item" style="display: flex; align-items: center; gap: 6px;"><span class="legend-color warning-color" style="width: 12px; height: 12px; background-color: var(--danger); border-radius: 50%; display: inline-block;"></span>Cảnh báo (ca)</span>
            </div>
          </div>

          <div class="interactive-svg-container" style="position: relative;">
            <!-- Tooltip floating overlay -->
            <div class="chart-tooltip-floating" v-if="activeChartIndex !== null && weeklyReport[activeChartIndex]" :style="{ left: (45 + activeChartIndex * 54) + 'px', top: '10px', position: 'absolute', backgroundColor: '#0f172a', color: '#fff', padding: '8px 12px', borderRadius: '8px', fontSize: '11px', boxShadow: 'var(--shadow-lg)', zIndex: 10, display: 'flex', flexDirection: 'column', gap: '2px', border: '1px solid rgba(255,255,255,0.1)' }">
              <div class="tooltip-title" style="font-weight: 700;">{{ weeklyReport[activeChartIndex]?.day }}</div>
              <div class="tooltip-body" style="display: flex; flex-direction: column;">
                <span>Doanh thu: <strong>{{ (weeklyReport[activeChartIndex]?.sales || 0).toLocaleString() }}đ</strong></span>
                <span style="color: var(--danger);">Cảnh báo: <strong>{{ weeklyReport[activeChartIndex]?.warnings }} ca</strong></span>
              </div>
            </div>

            <svg width="100%" height="200" viewBox="0 0 450 200" class="svg-chart-element">
              <!-- Grid lines -->
              <line x1="30" y1="20" x2="420" y2="20" stroke="#f1f5f9" stroke-width="1" />
              <line x1="30" y1="60" x2="420" y2="60" stroke="#f1f5f9" stroke-width="1" />
              <line x1="30" y1="100" x2="420" y2="100" stroke="#f1f5f9" stroke-width="1" />
              <line x1="30" y1="140" x2="420" y2="140" stroke="#f1f5f9" stroke-width="1" />
              <line x1="30" y1="160" x2="420" y2="160" stroke="#cbd5e1" stroke-width="1.5" />

              <!-- Y Axis Labels -->
              <text x="22" y="24" font-size="9" fill="#94a3b8" text-anchor="end">50k</text>
              <text x="22" y="64" font-size="9" fill="#94a3b8" text-anchor="end">30k</text>
              <text x="22" y="104" font-size="9" fill="#94a3b8" text-anchor="end">15k</text>
              <text x="22" y="144" font-size="9" fill="#94a3b8" text-anchor="end">5k</text>
              <text x="22" y="164" font-size="9" fill="#94a3b8" text-anchor="end">0đ</text>

              <!-- Sales Bars -->
              <g class="bars-group">
                <rect 
                  v-for="(item, idx) in weeklyReport" 
                  :key="'bar-'+idx"
                  :x="40 + idx * 54"
                  :y="160 - (item.sales / 50000) * 120"
                  width="20"
                  :height="(item.sales / 50000) * 120"
                  rx="3"
                  fill="url(#salesGrad)"
                  style="cursor: pointer; transition: opacity var(--transition-fast);"
                  @mouseenter="activeChartIndex = idx"
                  @mouseleave="activeChartIndex = null"
                />
              </g>

              <!-- Warning Line Plot -->
              <polyline 
                :points="warningPathPoints" 
                fill="none" 
                stroke="var(--danger)" 
                stroke-width="3" 
                stroke-linecap="round"
                stroke-linejoin="round"
                style="filter: drop-shadow(0px 2px 4px rgba(239, 68, 68, 0.3));"
              />

              <!-- Warning Dots -->
              <g class="dots-group">
                <circle 
                  v-for="(item, idx) in weeklyReport" 
                  :key="'dot-'+idx"
                  :cx="40 + 10 + idx * 54"
                  :cy="160 - (item.warnings / 6) * 120"
                  r="5"
                  fill="#ffffff"
                  stroke="var(--danger)"
                  stroke-width="3"
                  style="cursor: pointer; transition: transform 0.1s ease;"
                  @mouseenter="activeChartIndex = idx"
                  @mouseleave="activeChartIndex = null"
                />
              </g>

              <!-- X Axis Labels -->
              <text 
                v-for="(item, idx) in weeklyReport" 
                :key="'lbl-'+idx"
                :x="40 + 10 + idx * 54"
                y="180"
                font-size="10"
                font-weight="700"
                fill="#64748b"
                text-anchor="middle"
              >
                {{ item.day }}
              </text>

              <!-- Definitions for Gradients -->
              <defs>
                <linearGradient id="salesGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stop-color="var(--primary-light)" />
                  <stop offset="100%" stop-color="var(--primary)" />
                </linearGradient>
              </defs>
            </svg>
          </div>
        </div>

        <!-- Recent Sales Table -->
        <div class="grid-card">
          <div class="card-header">
            <h2 class="card-title">Hóa đơn bán thuốc gần đây (Sales)</h2>
            <button class="text-btn" @click="navigateToTab('sales-history')">Chi tiết lịch sử →</button>
          </div>
          <table class="dashboard-table">
            <thead>
              <tr>
                <th>Mã HD</th>
                <th>Bệnh nhân</th>
                <th>Thời gian</th>
                <th>Tổng tiền</th>
                <th>Duyệt an toàn</th>
                <th>Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="sale in store.sales.value.slice(0, 3)" :key="sale.SaleId">
                <td><strong>HD-00{{ sale.SaleId }}</strong></td>
                <td>{{ store.patients.value.find(p => p.PatientId === sale.PatientId)?.FullName }}</td>
                <td>{{ sale.SaleDate }}</td>
                <td><strong>{{ sale.TotalAmount.toLocaleString() }}đ</strong></td>
                <td>
                  <span :class="['status-tag', sale.FinalDecision === 'Approved' ? 'safe' : sale.FinalDecision === 'Denied' ? 'danger' : 'warning']">
                    {{ sale.FinalDecision === 'Approved' ? 'Đã duyệt' : sale.FinalDecision === 'Denied' ? 'Từ chối' : 'Chờ kiểm tra' }}
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
      </div>

      <!-- Right Column: Sidebar Alerts & Stock -->
      <div class="analytics-side-column">
        <!-- Severity breakdown progress list -->
        <div class="grid-card" style="margin-bottom: 24px;">
          <h3 class="card-title" style="margin-bottom: 16px;">Cơ cấu Mức độ Rủi ro (Warnings)</h3>
          
          <div class="severity-progress-container" style="display: flex; flex-direction: column; gap: 14px;">
            <div class="severity-section">
              <div class="severity-item-row" style="display: flex; justify-content: space-between; font-size: 13px; font-weight: 600; margin-bottom: 4px;">
                <span class="name" style="display: flex; align-items: center; gap: 6px;"><span style="width: 8px; height: 8px; border-radius: 50%; background-color: var(--danger); display: inline-block;"></span>Nghiêm trọng (High)</span>
                <span class="val">15%</span>
              </div>
              <div class="progress-bar-bg" style="width: 100%; height: 6px; background-color: #f1f5f9; border-radius: 3px; overflow: hidden;">
                <div class="progress-bar-fill high-risk" style="width: 15%; height: 100%; background-color: var(--danger);"></div>
              </div>
            </div>

            <div class="severity-section">
              <div class="severity-item-row" style="display: flex; justify-content: space-between; font-size: 13px; font-weight: 600; margin-bottom: 4px;">
                <span class="name" style="display: flex; align-items: center; gap: 6px;"><span style="width: 8px; height: 8px; border-radius: 50%; background-color: var(--warning); display: inline-block;"></span>Trung bình (Medium)</span>
                <span class="val">35%</span>
              </div>
              <div class="progress-bar-bg" style="width: 100%; height: 6px; background-color: #f1f5f9; border-radius: 3px; overflow: hidden;">
                <div class="progress-bar-fill medium-risk" style="width: 35%; height: 100%; background-color: var(--warning);"></div>
              </div>
            </div>

            <div class="severity-section">
              <div class="severity-item-row" style="display: flex; justify-content: space-between; font-size: 13px; font-weight: 600; margin-bottom: 4px;">
                <span class="name" style="display: flex; align-items: center; gap: 6px;"><span style="width: 8px; height: 8px; border-radius: 50%; background-color: var(--success); display: inline-block;"></span>Nhẹ (Low / Prescription)</span>
                <span class="val">50%</span>
              </div>
              <div class="progress-bar-bg" style="width: 100%; height: 6px; background-color: #f1f5f9; border-radius: 3px; overflow: hidden;">
                <div class="progress-bar-fill low-risk" style="width: 50%; height: 100%; background-color: var(--success);"></div>
              </div>
            </div>
          </div>
        </div>

        <!-- Stock warnings -->
        <div class="grid-card" style="margin-bottom: 24px;">
          <div class="card-header" style="margin-bottom: 14px;">
            <h3 class="card-title">Cảnh báo tồn kho tối thiểu (F03)</h3>
          </div>
          <div class="stock-alerts-list" style="display: flex; flex-direction: column; gap: 10px;">
            <div v-for="(item, idx) in inventoryAlerts" :key="idx" :style="{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 14px', borderRadius: '8px', border: '1px solid', borderColor: item.stock <= 3 ? 'rgba(239, 68, 68, 0.15)' : item.stock <= 10 ? 'rgba(245, 158, 11, 0.15)' : 'var(--border-color)', backgroundColor: item.stock <= 3 ? 'var(--danger-bg)' : item.stock <= 10 ? 'var(--warning-bg)' : '#fafbfd' }">
              <div class="stock-info-block" style="display: flex; flex-direction: column; gap: 2px;">
                <span class="med-name" style="font-size: 13.5px; font-weight: 700; color: var(--text-main);">{{ item.name }}</span>
                <span class="stock-status-text" :style="{ fontSize: '11px', fontWeight: '600', color: item.stock <= 3 ? 'var(--danger)' : item.stock <= 10 ? 'var(--warning)' : 'var(--success)' }">{{ item.status }}</span>
              </div>
              <div class="stock-count-block" style="display: flex; align-items: baseline; gap: 2px;">
                <span class="count" :style="{ fontSize: '18px', fontWeight: '800', color: item.stock <= 3 ? 'var(--danger)' : item.stock <= 10 ? 'var(--warning)' : 'var(--text-main)' }">{{ item.stock }}</span>
                <span class="unit" style="font-size: 11px; color: var(--text-muted); font-weight: 500;">{{ item.unit }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Warnings Feed -->
        <div class="grid-card recent-warnings-feed">
          <h3 class="card-title" style="margin-bottom: 14px;">Nhật ký can thiệp lâm sàng</h3>
          <div class="warnings-feed-container" v-if="store.warnings.value.length > 0" style="display: flex; flex-direction: column; gap: 14px;">
            <div v-for="w in store.warnings.value.slice(0, 2)" :key="w.WarningId" class="feed-item" style="border-left: 3px solid var(--danger); padding-left: 12px; display: flex; flex-direction: column; gap: 4px;">
              <div class="feed-badge-row" style="display: flex; justify-content: space-between; align-items: center;">
                <span class="feed-type-badge" style="font-size: 10px; font-weight: 800; background-color: var(--danger-bg); color: var(--danger); padding: 1px 6px; border-radius: 4px; text-transform: uppercase;">{{ w.WarningType }}</span>
                <span class="feed-time" style="font-size: 11px; color: var(--text-muted);">Hôm nay</span>
              </div>
              <p class="feed-msg" style="font-size: 12.5px; color: var(--text-main); line-height: 1.45; font-weight: 600;">{{ w.Message }}</p>
              <div class="feed-resolution" style="font-size: 11.5px; background-color: var(--bg-main); padding: 4px 8px; border-radius: 4px; margin-top: 2px;">
                <span style="color: var(--text-muted);">Quyết định: <strong style="color: var(--success);">{{ w.Decision }}</strong></span>
              </div>
            </div>
          </div>
          <div v-else class="empty-feed flex-center" style="min-height: 100px; color: var(--text-muted); font-style: italic;">
            <p>Chưa ghi nhận ca can thiệp lâm sàng nào.</p>
          </div>
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
</style>
