<script setup lang="ts">
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()
</script>

<template>
  <div class="view-container">
    <!-- Sales logs table -->
    <div class="grid-card">
      <h3 class="section-title">Lịch sử Phiếu bán thuốc (Sales)</h3>
      <table class="data-table">
        <thead>
          <tr>
            <th>Mã HD</th>
            <th>Bệnh nhân</th>
            <th>Dược sĩ bán</th>
            <th>Ngày giao dịch</th>
            <th>Tổng tiền</th>
            <th>Duyệt an toàn</th>
            <th>Trạng thái</th>
            <th>Ghi chú</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="sale in store.sales.value" :key="sale.SaleId">
            <td>HD-00{{ sale.SaleId }}</td>
            <td><strong>{{ store.patients.value.find(p => p.PatientId === sale.PatientId)?.FullName }}</strong></td>
            <td>{{ store.users.value.find(u => u.UserId === sale.PharmacistId)?.FullName }}</td>
            <td>{{ sale.SaleDate }}</td>
            <td>{{ sale.TotalAmount.toLocaleString() }}đ</td>
            <td>
              <span :class="['status-tag', sale.FinalDecision === 'Approved' ? 'safe' : sale.FinalDecision === 'Denied' ? 'danger' : 'warning']">
                {{ sale.FinalDecision === 'Approved' ? 'Đã duyệt' : sale.FinalDecision === 'Denied' ? 'Từ chối' : 'Chờ duyệt' }}
              </span>
            </td>
            <td>
              <span :class="['status-tag', sale.Status === 'Completed' ? 'safe' : 'danger']">
                {{ sale.Status === 'Completed' ? 'Hoàn tất' : 'Hủy bỏ' }}
              </span>
            </td>
            <td><small>{{ sale.Note }}</small></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.view-container {
  display: flex;
  flex-direction: column;
}
.grid-card {
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 24px;
}
</style>
