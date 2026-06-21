<script setup lang="ts">
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()
</script>

<template>
  <div class="view-container">
    <!-- Drug Interactions (Tương tác thuốc) -->
    <div class="grid-card text-section" style="margin-bottom: 24px;">
      <h3 class="section-title">Danh mục Tương tác Thuốc (Drug Interactions)</h3>
      <table class="data-table">
        <thead>
          <tr>
            <th>Hoạt chất A</th>
            <th>Hoạt chất B</th>
            <th>Mức độ</th>
            <th>Mô tả tác hại</th>
            <th>Khuyến cáo lâm sàng (Recommendation)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="di in store.drugInteractions.value" :key="di.InteractionId">
            <td><strong>{{ store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientAId)?.IngredientName }}</strong></td>
            <td><strong>{{ store.activeIngredients.value.find(ai => ai.IngredientId === di.IngredientBId)?.IngredientName }}</strong></td>
            <td><span class="status-tag danger">{{ di.Severity }}</span></td>
            <td><small>{{ di.Description }}</small></td>
            <td><small class="green" style="color: var(--success); font-weight: 600;">{{ di.Recommendation }}</small></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Contraindications (Chống chỉ định) -->
    <div class="grid-card text-section">
      <h3 class="section-title">Danh mục Chống chỉ định (Contraindications)</h3>
      <table class="data-table">
        <thead>
          <tr>
            <th>Thuốc / Hoạt chất</th>
            <th>Điều kiện chống chỉ định</th>
            <th>Phân loại</th>
            <th>Mức độ</th>
            <th>Mô tả tác hại</th>
            <th>Khuyến cáo lâm sàng</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in store.contraindications.value" :key="c.ContraindicationId">
            <td>
              <strong v-if="c.MedicineId">{{ store.medicines.value.find(m => m.MedicineId === c.MedicineId)?.MedicineName }}</strong>
              <strong v-else-if="c.IngredientId">{{ store.activeIngredients.value.find(ai => ai.IngredientId === c.IngredientId)?.IngredientName }}</strong>
            </td>
            <td>
              <span v-if="c.DiseaseId" class="tag warning" style="background-color: var(--warning-bg); color: var(--warning); padding: 2px 6px; border-radius: 4px; font-size: 12px;">{{ store.diseases.value.find(d => d.DiseaseId === c.DiseaseId)?.DiseaseName }}</span>
              <span v-else class="tag danger" style="background-color: var(--danger-bg); color: var(--danger); padding: 2px 6px; border-radius: 4px; font-size: 12px;">🤰 Phụ nữ mang thai</span>
            </td>
            <td><small>{{ c.ConditionType }}</small></td>
            <td><span class="status-tag danger">{{ c.Severity }}</span></td>
            <td><small>{{ c.Description }}</small></td>
            <td><small class="green" style="color: var(--success); font-weight: 600;">{{ c.Recommendation }}</small></td>
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
