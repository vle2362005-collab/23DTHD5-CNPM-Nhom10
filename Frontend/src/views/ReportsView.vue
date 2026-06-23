<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { usePharmacyStore } from '../store/pharmacy'
import type { Sale, Warning } from '../store/pharmacy'
import { ApiService } from '../services/api'

const store = usePharmacyStore()

// Period selection state: '7days' | '30days' | 'all'
const selectedPeriod = ref<'7days' | '30days' | 'all'>('all')

// Statistics API Data Ref
interface StatisticsReport {
  TotalRevenue: number
  CompletedSalesCount: number
  CancelledSalesCount: number
  TotalWarningsCount: number
  WarningApprovalRate: number
  ChartDaysData: Array<{
    Date: string
    Label: string
    Revenue: number
    Warnings: number
  }>
  WarningCategoriesBreakdown: Array<{
    Type: string
    Color: string
    Count: number
    Percent: number
  }>
}

const stats = ref<StatisticsReport | null>(null)
const isLoading = ref(false)

const fetchStats = async () => {
  isLoading.value = true
  try {
    stats.value = await ApiService.getStatisticsReport(selectedPeriod.value)
  } catch (err) {
    console.error('Failed to fetch statistics:', err)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchStats()
})

watch(selectedPeriod, () => {
  fetchStats()
})

// Helper function to parse "yyyy-MM-dd HH:mm" date string
const parseSaleDate = (dateStr: string): Date => {
  try {
    const parts = dateStr.split(' ')
    const firstPart = parts[0]
    if (firstPart) {
      const dateParts = firstPart.split('-')
      if (dateParts.length === 3) {
        const year = parseInt(dateParts[0] || '0')
        const month = parseInt(dateParts[1] || '1') - 1
        const day = parseInt(dateParts[2] || '1')
        let hour = 0
        let minute = 0
        const secondPart = parts[1]
        if (secondPart) {
          const timeParts = secondPart.split(':')
          if (timeParts.length >= 2) {
            hour = parseInt(timeParts[0] || '0')
            minute = parseInt(timeParts[1] || '0')
          }
        }
        return new Date(year, month, day, hour, minute)
      }
    }
  } catch (err) {
    console.error('Failed parsing date:', dateStr, err)
  }
  return new Date(dateStr)
}

// Filtered Sales based on period (still needed for warnings audit estimation)
const filteredSales = computed<Sale[]>(() => {
  const allSales = store.sales.value
  if (selectedPeriod.value === 'all') return allSales

  const now = new Date(2026, 5, 23, 22, 0, 0) // Mock current date from metadata
  const daysLimit = selectedPeriod.value === '7days' ? 7 : 30
  const limitTime = now.getTime() - daysLimit * 24 * 60 * 60 * 1000

  return allSales.filter(s => {
    const sDate = parseSaleDate(s.SaleDate)
    return sDate.getTime() >= limitTime
  })
})

// Filtered Warnings based on period (still needed for warnings audit list)
const filteredWarnings = computed<Warning[]>(() => {
  const allWarnings = store.warnings.value
  
  // Find associated sales for warning date matching (warnings don't have CreatedAt directly in store model)
  const salesMap = new Map<number, string>()
  store.sales.value.forEach(s => {
    salesMap.set(s.SaleId, s.SaleDate)
  })

  if (selectedPeriod.value === 'all') return allWarnings

  const now = new Date(2026, 5, 23, 22, 0, 0)
  const daysLimit = selectedPeriod.value === '7days' ? 7 : 30
  const limitTime = now.getTime() - daysLimit * 24 * 60 * 60 * 1000

  return allWarnings.filter(w => {
    // If warning was acknowledged, it has a date
    if (w.AcknowledgedAt) {
      const ackDate = parseSaleDate(w.AcknowledgedAt)
      return ackDate.getTime() >= limitTime
    }
    // Fallback: check matching Patient's sales to estimate timeframe
    const patientSales = store.sales.value.filter(s => s.PatientId === w.PatientId)
    const firstSale = patientSales[0]
    if (firstSale) {
      const latestSaleDate = parseSaleDate(firstSale.SaleDate)
      return latestSaleDate.getTime() >= limitTime
    }
    return true
  })
})

// ==========================================
// 1. METRICS COMPUTATIONS (Adapted from API)
// ==========================================
const totalRevenue = computed(() => stats.value?.TotalRevenue ?? 0)
const completedSalesCount = computed(() => stats.value?.CompletedSalesCount ?? 0)
const cancelledSalesCount = computed(() => stats.value?.CancelledSalesCount ?? 0)
const totalWarningsCount = computed(() => stats.value?.TotalWarningsCount ?? 0)
const warningApprovalRate = computed(() => stats.value?.WarningApprovalRate ?? 100)

// ==========================================
// 2. LINE CHART SVG COORDINATES GENERATOR (Last 7 Days, Adapted from API)
// ==========================================
const chartDaysData = computed(() => {
  if (!stats.value?.ChartDaysData) return []
  return stats.value.ChartDaysData.map(d => ({
    label: d.Label,
    revenue: d.Revenue,
    warnings: d.Warnings
  }))
})

// Generate SVG Path for Revenue Line
const revenuePath = computed(() => {
  const data = chartDaysData.value
  const maxRevenue = Math.max(...data.map(d => d.revenue), 100000) // avoid divide by zero
  
  const points = data.map((d, index) => {
    const x = 40 + index * 70
    // Chart height is 150, padding bottom 30, padding top 20
    const y = 170 - (d.revenue / maxRevenue) * 130
    return `${x},${y}`
  })
  
  return points.length > 0 ? `M ${points.join(' L ')}` : ''
})

// Generate SVG Path for Warnings Line
const warningsPath = computed(() => {
  const data = chartDaysData.value
  const maxWarnings = Math.max(...data.map(d => d.warnings), 3) // min scale of 3
  
  const points = data.map((d, index) => {
    const x = 40 + index * 70
    const y = 170 - (d.warnings / maxWarnings) * 110
    return `${x},${y}`
  })
  
  return points.length > 0 ? `M ${points.join(' L ')}` : ''
})

// Max values for chart rendering bounds
const maxRevenueValue = computed(() => {
  return Math.max(...chartDaysData.value.map(d => d.revenue), 100000)
})

// ==========================================
// 3. DONUT CHART COMPUTATIONS (Warning Types breakdown, Adapted from API)
// ==========================================
const warningCategoriesBreakdown = computed(() => {
  if (!stats.value?.WarningCategoriesBreakdown) return []
  const list = stats.value.WarningCategoriesBreakdown
  const total = stats.value.TotalWarningsCount
  
  let cumulativePercent = 0
  return list.map(cat => {
    const percent = total > 0 ? (cat.Count / total) : 0
    const strokeDasharray = `${percent * 251.2} 251.2`
    const strokeDashoffset = -cumulativePercent * 251.2
    cumulativePercent += percent
    
    return {
      type: cat.Type,
      color: cat.Color,
      count: cat.Count,
      percent: cat.Percent,
      strokeDasharray,
      strokeDashoffset
    }
  })
})

// ==========================================
// 4. AUDIT WARNING LIST DETAILS
// ==========================================
const warningsAuditLogs = computed(() => {
  return filteredWarnings.value.map(w => {
    const patientName = store.patients.value.find(p => p.PatientId === w.PatientId)?.FullName || `Bệnh nhân #${w.PatientId}`
    const medicineName = store.medicines.value.find(m => m.MedicineId === w.MedicineId)?.MedicineName || 'Biệt dược chung'
    const reviewerName = w.AcknowledgedBy ? (store.users.value.find(u => u.UserId === w.AcknowledgedBy)?.FullName || 'Dược sĩ') : null
    
    return {
      id: w.WarningId,
      patientName,
      medicineName,
      type: w.WarningType,
      severity: w.Severity,
      message: w.Message,
      recommendation: w.Recommendation,
      isAcknowledged: w.IsAcknowledged,
      acknowledgedAt: w.AcknowledgedAt,
      decision: w.Decision,
      reviewerName
    }
  })
})

const getSeverityClass = (severity: string) => {
  const s = severity.toLowerCase()
  if (s === 'nghiêm trọng' || s === 'high') return 'danger'
  if (s === 'trung bình' || s === 'medium') return 'warning'
  return 'info'
}
</script>

<template>
  <div class="view-container">
    <!-- Filter Bar -->
    <div class="filter-bar-row">
      <div class="tab-description">
        <p class="text-muted" style="margin: 0;">Báo cáo số liệu kinh doanh và phân tích cảnh báo an toàn lâm sàng định kỳ.</p>
      </div>
      <div class="time-filter">
        <span :class="['filter-btn', { 'active': selectedPeriod === '7days' }]" @click="selectedPeriod = '7days'">7 ngày qua</span>
        <span :class="['filter-btn', { 'active': selectedPeriod === '30days' }]" @click="selectedPeriod = '30days'">30 ngày qua</span>
        <span :class="['filter-btn', { 'active': selectedPeriod === 'all' }]" @click="selectedPeriod = 'all'">Tất cả thời gian</span>
      </div>
    </div>

    <!-- KPI Dashboard Cards -->
    <div class="metrics-grid">
      <div class="metric-card">
        <div class="metric-info">
          <span class="metric-label">Doanh thu kinh doanh</span>
          <span class="metric-value">{{ totalRevenue.toLocaleString() }}đ</span>
          <span class="metric-subtext">Từ {{ completedSalesCount }} giao dịch hoàn tất</span>
        </div>
        <div class="metric-icon revenue">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
      </div>

      <div class="metric-card">
        <div class="metric-info">
          <span class="metric-label">Phiếu bán thuốc</span>
          <span class="metric-value">{{ filteredSales.length }}</span>
          <div class="metric-subtext-group">
            <span class="status-dot success"></span>
            <span>{{ completedSalesCount }} thành công</span>
            <span class="status-separator">|</span>
            <span class="status-dot danger"></span>
            <span>{{ cancelledSalesCount }} bị hủy</span>
          </div>
        </div>
        <div class="metric-icon orders">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
      </div>

      <div class="metric-card">
        <div class="metric-info">
          <span class="metric-label">Cảnh báo lâm sàng</span>
          <span class="metric-value">{{ totalWarningsCount }}</span>
          <span class="metric-subtext">Tổng số phát hiện trong hệ thống</span>
        </div>
        <div class="metric-icon alerts">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
      </div>

      <div class="metric-card">
        <div class="metric-info">
          <span class="metric-label">Tỷ lệ xử lý duyệt</span>
          <span class="metric-value">{{ warningApprovalRate }}%</span>
          <span class="metric-subtext">Cảnh báo đã duyệt đè y khoa</span>
        </div>
        <div class="metric-icon rate">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12c0 1.268-.63 2.39-1.593 3.068a3.745 3.745 0 01-1.043 3.296" />
          </svg>
        </div>
      </div>
    </div>

    <!-- Graphs Layout Grid -->
    <div class="reports-main-layout">
      <!-- 1. Revenue Trends SVG Line Chart -->
      <div class="grid-card chart-card">
        <div class="card-header-row" style="margin-bottom: 24px;">
          <div class="header-title-block">
            <h3 class="section-title">Xu hướng doanh thu & Tần suất cảnh báo (7 ngày qua)</h3>
            <p class="text-muted" style="font-size: 12px; margin: 4px 0 0 0;">Cập nhật tự động dựa trên giao dịch thực tế.</p>
          </div>
        </div>

        <div class="chart-wrapper">
          <svg viewBox="0 0 500 200" class="svg-chart">
            <!-- Grid Lines -->
            <line x1="40" y1="20" x2="480" y2="20" stroke="var(--border-color)" stroke-width="1" stroke-dasharray="3 3" />
            <line x1="40" y1="70" x2="480" y2="70" stroke="var(--border-color)" stroke-width="1" stroke-dasharray="3 3" />
            <line x1="40" y1="120" x2="480" y2="120" stroke="var(--border-color)" stroke-width="1" stroke-dasharray="3 3" />
            <line x1="40" y1="170" x2="480" y2="170" stroke="var(--border-color)" stroke-width="1.5" />

            <!-- Revenue Trend Line -->
            <path :d="revenuePath" fill="none" stroke="var(--primary-light)" stroke-width="3" stroke-linecap="round" />
            
            <!-- Warnings Trend Line -->
            <path :d="warningsPath" fill="none" stroke="#ef4444" stroke-width="2" stroke-dasharray="4 4" stroke-linecap="round" />

            <!-- Dynamic Graph Nodes for Revenue -->
            <g v-for="(d, idx) in chartDaysData" :key="'rev-dot-' + idx">
              <circle 
                v-if="d.revenue > 0"
                :cx="40 + idx * 70" 
                :cy="170 - (d.revenue / maxRevenueValue) * 130" 
                r="5" 
                fill="#ffffff" 
                stroke="var(--primary-light)" 
                stroke-width="2.5" 
              />
            </g>

            <!-- X Axis Labels -->
            <text 
              v-for="(d, idx) in chartDaysData" 
              :key="'x-lbl-' + idx"
              :x="40 + idx * 70" 
              y="190" 
              class="chart-label"
            >
              {{ d.label }}
            </text>

            <!-- Y Axis Labels -->
            <text x="35" y="24" class="chart-label y-axis">{{ (maxRevenueValue).toLocaleString() }}</text>
            <text x="35" y="74" class="chart-label y-axis">{{ (maxRevenueValue * 0.5).toLocaleString() }}</text>
            <text x="35" y="124" class="chart-label y-axis">0</text>
          </svg>
          
          <div class="chart-legend">
            <span class="legend-item"><span class="legend-color revenue"></span> Doanh thu (VND)</span>
            <span class="legend-item"><span class="legend-color warnings"></span> Số ca Cảnh báo lâm sàng</span>
          </div>
        </div>
      </div>

      <!-- 2. Warning Types Distribution Donut Chart -->
      <div class="grid-card donut-card">
        <h3 class="section-title">Cấu trúc loại Cảnh báo lâm sàng</h3>
        <div class="donut-wrapper">
          <div class="donut-svg-container">
            <svg viewBox="0 0 100 100" class="donut-svg">
              <circle cx="50" cy="50" r="40" fill="transparent" stroke="var(--bg-main)" stroke-width="12" />
              <!-- Render circular slices -->
              <circle 
                v-for="(cat, idx) in warningCategoriesBreakdown" 
                :key="'slice-' + idx"
                cx="50" 
                cy="50" 
                r="40" 
                fill="transparent" 
                :stroke="cat.color" 
                stroke-width="12" 
                :stroke-dasharray="cat.strokeDasharray" 
                :stroke-dashoffset="cat.strokeDashoffset" 
                transform="rotate(-90 50 50)"
                class="donut-segment"
              />
              <!-- Donut inner center summary text -->
              <text x="50" y="47" class="donut-center-title">{{ totalWarningsCount }}</text>
              <text x="50" y="60" class="donut-center-subtitle">Cảnh báo</text>
            </svg>
          </div>

          <div class="donut-legend">
            <div 
              v-for="(cat, idx) in warningCategoriesBreakdown" 
              :key="'leg-' + idx" 
              class="donut-legend-row"
            >
              <span class="legend-bullet" :style="{ backgroundColor: cat.color }"></span>
              <span class="legend-text text-ellipsis">{{ cat.type }}</span>
              <strong class="legend-qty">{{ cat.count }} ({{ cat.percent }}%)</strong>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Audit logs section -->
    <div class="grid-card full-width" style="margin-top: 10px;">
      <h3 class="section-title" style="margin-bottom: 20px;">Lịch sử & Nhật ký Kiểm toán Cảnh báo Lâm sàng (Safety Warnings Audit Log)</h3>
      
      <div class="table-container" v-if="warningsAuditLogs.length > 0">
        <table class="dashboard-table shadow-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Khách hàng</th>
              <th>Thuốc liên quan</th>
              <th>Loại cảnh báo</th>
              <th>Mức độ</th>
              <th>Nội dung rủi ro</th>
              <th>Xử lý lâm sàng</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="log in warningsAuditLogs" :key="log.id">
              <td><strong>#{{ log.id }}</strong></td>
              <td>{{ log.patientName }}</td>
              <td><strong>{{ log.medicineName }}</strong></td>
              <td>
                <span class="type-badge">{{ log.type }}</span>
              </td>
              <td>
                <span :class="['severity-pill', getSeverityClass(log.severity)]">
                  {{ log.severity }}
                </span>
              </td>
              <td class="message-cell" :title="log.message">{{ log.message }}</td>
              <td>
                <div v-if="log.isAcknowledged" class="ack-detail">
                  <span class="status-tag success flex-center" style="font-size: 11px; width: fit-content; gap: 4px; padding: 4px 8px;">
                    ✓ Đã duyệt
                  </span>
                  <small class="decision-reason" :title="log.decision || undefined">Lý do: <em>"{{ log.decision }}"</em> (Bởi: {{ log.reviewerName }})</small>
                </div>
                <div v-else>
                  <span class="status-tag danger flex-center" style="font-size: 11px; width: fit-content; gap: 4px; padding: 4px 8px;">
                    ⚠️ Chưa xử lý
                  </span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else class="empty-warnings flex-center">
        <div class="empty-content">
          <svg viewBox="0 0 24 24" class="safety-icon" fill="none" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12c0 1.268-.63 2.39-1.593 3.068a3.745 3.745 0 01-1.043 3.296" />
          </svg>
          <p>Không có nhật ký cảnh báo lâm sàng nào được ghi nhận trong thời gian này.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.view-container {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Filter Bar styles */
.filter-bar-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}

.time-filter {
  display: flex;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  padding: 4px;
  box-shadow: var(--shadow-sm);
}

.filter-btn {
  padding: 8px 16px;
  font-size: 12.5px;
  font-weight: 700;
  border-radius: var(--border-radius-sm);
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
}

.filter-btn.active, .filter-btn:hover {
  background-color: var(--primary-bg);
  color: var(--primary-medium);
}

/* KPI Cards layout */
.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 20px;
}

.metric-card {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-lg);
  padding: 24px;
  box-shadow: var(--shadow-sm);
  transition: transform 0.2s, box-shadow 0.2s;
}

.metric-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.metric-info {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.metric-label {
  font-size: 12.5px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.metric-value {
  font-size: 24px;
  font-weight: 800;
  color: var(--text-main);
}

.metric-subtext {
  font-size: 12px;
  color: var(--text-muted);
}

.metric-subtext-group {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--text-muted);
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}
.status-dot.success { background-color: var(--success); }
.status-dot.danger { background-color: var(--danger); }
.status-separator { color: var(--border-color); }

.metric-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 46px;
  height: 46px;
  border-radius: var(--border-radius-md);
}

.metric-icon.revenue { background-color: rgba(20, 184, 166, 0.1); color: var(--primary-light); }
.metric-icon.orders { background-color: rgba(16, 185, 129, 0.1); color: var(--success); }
.metric-icon.alerts { background-color: rgba(239, 68, 68, 0.1); color: var(--danger); }
.metric-icon.rate { background-color: rgba(139, 92, 246, 0.1); color: #8b5cf6; }

.metric-icon svg {
  width: 22px;
  height: 22px;
}

/* Charts layout grids */
.reports-main-layout {
  display: grid;
  grid-template-columns: 1.6fr 1fr;
  gap: 24px;
}

@media (max-width: 992px) {
  .reports-main-layout {
    grid-template-columns: 1fr;
  }
}

.grid-card {
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-lg);
  padding: 24px;
  box-shadow: var(--shadow-sm);
}

.chart-card {
  display: flex;
  flex-direction: column;
}

.chart-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.svg-chart {
  width: 100%;
  overflow: visible;
}

.chart-label {
  font-size: 10px;
  fill: var(--text-muted);
  font-weight: 700;
  text-anchor: middle;
}

.chart-label.y-axis {
  text-anchor: end;
}

.chart-legend {
  display: flex;
  justify-content: center;
  gap: 20px;
  margin-top: 14px;
  font-size: 12.5px;
  font-weight: 600;
  color: var(--text-muted);
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 3px;
}

.legend-color.revenue { background-color: var(--primary-light); }
.legend-color.warnings { background-color: #ef4444; border: 1.5px dashed #ef4444; }

/* Donut Chart styles */
.donut-card {
  display: flex;
  flex-direction: column;
}

.donut-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
  margin-top: 12px;
}

.donut-svg-container {
  width: 150px;
  height: 150px;
  position: relative;
}

.donut-svg {
  width: 100%;
  height: 100%;
}

.donut-segment {
  transition: stroke-width 0.3s;
}
.donut-segment:hover {
  stroke-width: 14;
}

.donut-center-title {
  font-size: 16px;
  font-weight: 800;
  fill: var(--text-main);
  text-anchor: middle;
}

.donut-center-subtitle {
  font-size: 8px;
  font-weight: 700;
  fill: var(--text-muted);
  text-anchor: middle;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.donut-legend {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.donut-legend-row {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 13px;
}

.legend-bullet {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
}

.legend-text {
  flex: 1;
  color: var(--text-muted);
  font-weight: 500;
}

.legend-qty {
  color: var(--text-main);
  font-weight: 700;
  flex-shrink: 0;
}

/* Table audit styles */
.table-container {
  width: 100%;
  overflow-x: auto;
}

.shadow-table {
  border-radius: var(--border-radius-md);
  overflow: hidden;
  border: 1px solid var(--border-color);
}

.message-cell {
  max-width: 250px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: var(--text-muted);
}

.type-badge {
  background-color: var(--bg-main);
  color: var(--text-main);
  font-size: 11px;
  font-weight: 700;
  padding: 4px 8px;
  border-radius: var(--border-radius-sm);
  border: 1px solid var(--border-color);
}

.severity-pill {
  font-size: 11px;
  font-weight: 800;
  padding: 4px 10px;
  border-radius: var(--border-radius-full);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.severity-pill.danger { background-color: var(--danger-bg); color: var(--danger); }
.severity-pill.warning { background-color: var(--warning-bg); color: var(--warning); }
.severity-pill.info { background-color: var(--info-bg); color: var(--info); }

.ack-detail {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.decision-reason {
  font-size: 11px;
  color: var(--text-muted);
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Empty warnings state */
.empty-warnings {
  padding: 48px;
  text-align: center;
  color: var(--text-muted);
}

.empty-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.safety-icon {
  width: 48px;
  height: 48px;
  color: var(--text-muted);
}
</style>
