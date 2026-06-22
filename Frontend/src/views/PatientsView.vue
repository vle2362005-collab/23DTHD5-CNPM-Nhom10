<script setup lang="ts">
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()
</script>

<template>
  <div class="view-container">
    <div class="table-container">
      <div class="table-actions" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; gap: 16px;">
        <input type="text" placeholder="Tìm kiếm bệnh nhân..." class="search-input-small" style="width: 280px; padding: 8px 12px; background-color: var(--bg-main); border: 1px solid var(--border-color); border-radius: var(--border-radius-md); font-size: 14px; outline: none;" />
        <button class="primary-btn">+ Thêm hồ sơ (Patients)</button>
      </div>
      <table class="data-table">
        <thead>
          <tr>
            <th>Họ tên</th>
            <th>Ngày sinh</th>
            <th>Giới tính</th>
            <th>Cân nặng</th>
            <th>Trạng thái đặc biệt</th>
            <th>Dị ứng (Allergies)</th>
            <th>Bệnh nền (Diseases)</th>
            <th>Ghi chú</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in store.patients.value" :key="p.PatientId">
            <td><strong>{{ p.FullName }}</strong><br><small>{{ p.Phone }}</small></td>
            <td>{{ p.DateOfBirth }}</td>
            <td>{{ p.Gender }}</td>
            <td>{{ p.WeightKg ? p.WeightKg + ' kg' : '-' }}</td>
            <td>
              <div class="spec-cond-badges">
                <span v-if="p.IsPregnant" class="status-tag danger">🤰 Mang thai</span>
                <span v-if="p.IsBreastfeeding" class="status-tag warning">🍼 Con bú</span>
                <span v-if="!p.IsPregnant && !p.IsBreastfeeding">Bình thường</span>
              </div>
            </td>
            <td>
              <div v-if="store.patientAllergies.value.some(pa => pa.PatientId === p.PatientId)">
                <span v-for="pa in store.patientAllergies.value.filter(pa => pa.PatientId === p.PatientId)" :key="pa.AllergyId" class="tag danger inline-block" style="display: inline-block; margin-right: 4px; margin-bottom: 4px;">
                  {{ store.activeIngredients.value.find(ai => ai.IngredientId === pa.IngredientId)?.IngredientName }}
                </span>
              </div>
              <span v-else>-</span>
            </td>
            <td>
              <div v-if="store.patientDiseases.value.some(pd => pd.PatientId === p.PatientId)">
                <span v-for="pd in store.patientDiseases.value.filter(pd => pd.PatientId === p.PatientId)" :key="pd.PatientDiseaseId" class="tag warning inline-block" style="display: inline-block; margin-right: 4px; margin-bottom: 4px;">
                  {{ store.diseases.value.find(d => d.DiseaseId === pd.DiseaseId)?.DiseaseName }}
                </span>
              </div>
              <span v-else>-</span>
            </td>
            <td><small>{{ p.Note || '-' }}</small></td>
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
.spec-cond-badges {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
</style>
