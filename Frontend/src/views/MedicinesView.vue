<script setup lang="ts">
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()
</script>

<template>
  <div class="view-container">
    <div class="table-container">
      <div class="table-actions" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; gap: 16px;">
        <input type="text" placeholder="Lọc theo tên thuốc, hoạt chất..." class="search-input-small" style="width: 280px; padding: 8px 12px; background-color: var(--bg-main); border: 1px solid var(--border-color); border-radius: var(--border-radius-md); font-size: 14px; outline: none;" />
        <button class="primary-btn" v-if="store.currentRole.value === 'admin'">+ Thêm thuốc (Medicines)</button>
      </div>
      <table class="data-table">
        <thead>
          <tr>
            <th>Mã thuốc</th>
            <th>Tên thuốc</th>
            <th>Nhóm thuốc (Drug Group)</th>
            <th>Hàm lượng (Strength)</th>
            <th>Dạng bào chế</th>
            <th>ĐVT</th>
            <th>Kê đơn</th>
            <th>Đơn giá</th>
            <th>Trạng thái</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="med in store.medicines.value" :key="med.MedicineId">
            <td>MED-00{{ med.MedicineId }}</td>
            <td><strong>{{ med.MedicineName }}</strong></td>
            <td>{{ store.drugGroups.value.find(dg => dg.DrugGroupId === med.DrugGroupId)?.GroupName || 'Mặc định' }}</td>
            <td>{{ med.Strength }}</td>
            <td>{{ med.DosageForm }}</td>
            <td>{{ med.Unit }}</td>
            <td>
              <span :class="['status-tag', med.RequiresPrescription ? 'danger' : 'safe']">
                {{ med.RequiresPrescription ? 'Yêu cầu đơn' : 'Không kê đơn' }}
              </span>
            </td>
            <td>{{ med.Price.toLocaleString() }}đ</td>
            <td>
              <span :class="['status-tag', med.IsActive ? 'safe' : 'danger']">
                {{ med.IsActive ? 'Hoạt động' : 'Tạm ngừng' }}
              </span>
            </td>
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
</style>
