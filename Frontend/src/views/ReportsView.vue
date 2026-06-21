<script setup lang="ts">
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()
</script>

<template>
  <div class="view-container">
    <!-- Summary Metrics -->
    <div class="metrics-grid">
      <div class="metric-card">
        <div class="metric-info">
          <span class="metric-label">Doanh thu tháng này</span>
          <span class="metric-value">148,250,000đ</span>
          <span class="metric-change positive">↑ 12% so với tháng trước</span>
        </div>
        <div class="metric-icon revenue">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
      </div>
      <div class="metric-card">
        <div class="metric-info">
          <span class="metric-label">Tổng hóa đơn đã bán</span>
          <span class="metric-value">1,240</span>
          <span class="metric-change positive">↑ 8% so với tháng trước</span>
        </div>
        <div class="metric-icon orders">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
      </div>
      <div class="metric-card">
        <div class="metric-info">
          <span class="metric-label">Tỷ lệ cảnh báo an toàn</span>
          <span class="metric-value">1.8%</span>
          <span class="metric-change negative">↓ 0.4% cải thiện an toàn</span>
        </div>
        <div class="metric-icon alerts">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
      </div>
    </div>

    <!-- Chart and Recent Activity -->
    <div class="reports-main-layout">
      <!-- SVG Chart Card -->
      <div class="grid-card chart-card">
        <div class="card-header-row">
          <h3 class="section-title">Xu hướng doanh thu & Tần suất Cảnh báo an toàn</h3>
          <div class="time-filter">
            <span class="filter-btn active">7 ngày qua</span>
            <span class="filter-btn">30 ngày qua</span>
          </div>
        </div>
        <div class="chart-container">
          <svg viewBox="0 0 500 200" class="svg-chart">
            <!-- Grid Lines -->
            <line x1="40" y1="20" x2="480" y2="20" stroke="var(--border-color)" stroke-dasharray="4 4" />
            <line x1="40" y1="70" x2="480" y2="70" stroke="var(--border-color)" stroke-dasharray="4 4" />
            <line x1="40" y1="120" x2="480" y2="120" stroke="var(--border-color)" stroke-dasharray="4 4" />
            <line x1="40" y1="170" x2="480" y2="170" stroke="var(--border-color)" />

            <!-- Revenue Line (Cyan) -->
            <path d="M 40 150 Q 110 110, 180 130 T 320 80 T 460 40" fill="none" stroke="#22d3ee" stroke-width="3" stroke-linecap="round" />
            
            <!-- Warnings Line (Red/Orange) -->
            <path d="M 40 160 Q 110 140, 180 150 T 320 120 T 460 145" fill="none" stroke="#f43f5e" stroke-width="2" stroke-dasharray="3 3" stroke-linecap="round" />

            <!-- Dots on Revenue -->
            <circle cx="180" cy="130" r="4" fill="#22d3ee" />
            <circle cx="320" cy="80" r="4" fill="#22d3ee" />
            <circle cx="460" cy="40" r="4" fill="#22d3ee" />

            <!-- Axis Labels -->
            <text x="40" y="190" class="chart-label">Thứ 2</text>
            <text x="110" y="190" class="chart-label">Thứ 3</text>
            <text x="180" y="190" class="chart-label">Thứ 4</text>
            <text x="250" y="190" class="chart-label">Thứ 5</text>
            <text x="320" y="190" class="chart-label">Thứ 6</text>
            <text x="390" y="190" class="chart-label">Thứ 7</text>
            <text x="460" y="190" class="chart-label">CN</text>

            <text x="15" y="25" class="chart-label y-axis">100M</text>
            <text x="15" y="75" class="chart-label y-axis">50M</text>
            <text x="15" y="125" class="chart-label y-axis">10M</text>
          </svg>
          <div class="chart-legend">
            <span class="legend-item"><span class="legend-color revenue"></span> Doanh thu bán hàng</span>
            <span class="legend-item"><span class="legend-color warnings"></span> Số ca phát hiện cảnh báo</span>
          </div>
        </div>
      </div>

      <!-- Reports List Card -->
      <div class="grid-card list-card">
        <h3 class="section-title">Danh sách Báo cáo định kỳ</h3>
        <div class="report-list">
          <div class="report-item">
            <div class="report-meta">
              <span class="report-name">Báo cáo an toàn thuốc & Cảnh báo lâm sàng</span>
              <span class="report-date">Cập nhật: 21/06/2026</span>
            </div>
            <button class="action-btn-sm text-btn">Tải PDF</button>
          </div>
          <div class="report-item">
            <div class="report-meta">
              <span class="report-name">Báo cáo doanh thu & Lợi nhuận nhà thuốc</span>
              <span class="report-date">Cập nhật: 15/06/2026</span>
            </div>
            <button class="action-btn-sm text-btn">Tải PDF</button>
          </div>
          <div class="report-item">
            <div class="report-meta">
              <span class="report-name">Báo cáo kiểm kê kho thuốc định kỳ</span>
              <span class="report-date">Cập nhật: 01/06/2026</span>
            </div>
            <button class="action-btn-sm text-btn">Tải PDF</button>
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
  gap: 24px;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
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
  gap: 4px;
}

.metric-label {
  font-size: 13px;
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

.metric-change {
  font-size: 12px;
  font-weight: 600;
}

.metric-change.positive {
  color: var(--primary-light);
}

.metric-change.negative {
  color: #f43f5e;
}

.metric-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 50%;
}

.metric-icon.revenue {
  background-color: rgba(34, 211, 238, 0.1);
  color: #22d3ee;
}

.metric-icon.orders {
  background-color: rgba(16, 185, 129, 0.1);
  color: #10b981;
}

.metric-icon.alerts {
  background-color: rgba(244, 63, 94, 0.1);
  color: #f43f5e;
}

.metric-icon svg {
  width: 24px;
  height: 24px;
}

.reports-main-layout {
  display: grid;
  grid-template-columns: 3fr 2fr;
  gap: 20px;
}

@media (max-width: 992px) {
  .reports-main-layout {
    grid-template-columns: 1fr;
  }
}

.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  flex-wrap: wrap;
  gap: 12px;
}

.time-filter {
  display: flex;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  padding: 2px;
}

.filter-btn {
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 600;
  border-radius: var(--border-radius-sm);
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
}

.filter-btn.active, .filter-btn:hover {
  background-color: var(--bg-card);
  color: var(--text-main);
  box-shadow: var(--shadow-sm);
}

.chart-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.svg-chart {
  width: 100%;
  height: auto;
}

.chart-label {
  font-size: 10px;
  fill: var(--text-muted);
  font-weight: 600;
  text-anchor: middle;
}

.chart-label.y-axis {
  text-anchor: end;
}

.chart-legend {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: var(--text-muted);
  justify-content: center;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 3px;
}

.legend-color.revenue {
  background-color: #22d3ee;
}

.legend-color.warnings {
  background: repeating-linear-gradient(45deg, #f43f5e, #f43f5e 2px, transparent 2px, transparent 4px);
  border: 1px solid #f43f5e;
}

.report-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.report-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  transition: border-color 0.2s;
}

.report-item:hover {
  border-color: var(--primary-medium);
}

.report-meta {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.report-name {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-main);
}

.report-date {
  font-size: 11px;
  color: var(--text-muted);
}

.text-btn {
  background: none;
  border: none;
  color: var(--primary-medium);
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: var(--border-radius-sm);
  transition: background-color 0.2s;
}

.text-btn:hover {
  background-color: rgba(16, 185, 129, 0.1);
}
</style>
