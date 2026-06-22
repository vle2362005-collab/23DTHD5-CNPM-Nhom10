<script setup lang="ts">
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()
</script>

<template>
  <div class="view-container">
    <div class="form-container">
      <div class="grid-card">
        <h3 class="section-title">Cấu hình CSDL & Logic động cơ cảnh báo</h3>
        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-name">Kiểm tra dị ứng nghiêm trọng</span>
            <span class="setting-desc">Tự động đối chiếu giỏ hàng với danh mục dị ứng của Patients.</span>
          </div>
          <label class="switch">
            <input type="checkbox" checked />
            <span class="slider"></span>
          </label>
        </div>
        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-name">Cảnh báo tương tác chéo (Drug Interactions)</span>
            <span class="setting-desc">Quét các cặp hoạt chất tương tác từ bảng DrugInteractions trong đơn.</span>
          </div>
          <label class="switch">
            <input type="checkbox" checked />
            <span class="slider"></span>
          </label>
        </div>
        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-name">Cảnh báo đối tượng mang thai & cho con bú</span>
            <span class="setting-desc">Tự động phát hiện thuốc chống chỉ định cho bà bầu.</span>
          </div>
          <label class="switch">
            <input type="checkbox" checked />
            <span class="slider"></span>
          </label>
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
.form-container {
  max-width: 800px;
  width: 100%;
}
.setting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
  border-bottom: 1px solid var(--border-color);
}
.setting-item:last-child {
  border-bottom: none;
}
.setting-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.setting-name {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-main);
}
.setting-desc {
  font-size: 13px;
  color: var(--text-muted);
}
</style>
