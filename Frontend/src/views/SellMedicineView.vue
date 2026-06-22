<script setup lang="ts">
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()
</script>

<template>
  <div class="view-container">
    <div class="form-container">
      <!-- Left Column: Patient Selector & Profile -->
      <div class="form-section patient-details">
        <h3 class="section-title">1. Chọn bệnh nhân & Tiền sử bệnh (Patients)</h3>
        
        <div class="patient-selector-wrapper">
          <label class="form-label">Tên bệnh nhân đăng ký:</label>
          <select v-model="store.selectedPatientId.value" class="form-control select-control">
            <option v-for="p in store.patients.value" :key="p.PatientId" :value="p.PatientId">
              {{ p.FullName }} ({{ p.Phone }})
            </option>
          </select>
        </div>

        <div class="patient-card-demo" v-if="store.activePatient.value">
          <div class="patient-header">
            <h4>{{ store.activePatient.value.FullName }}</h4>
            <span class="gender-age">{{ store.activePatient.value.Gender }} - {{ store.calculateAge(store.activePatient.value.DateOfBirth) }} tuổi</span>
          </div>
          
          <div class="patient-details-grid">
            <div class="detail-row">
              <span class="detail-label">Số điện thoại:</span>
              <span class="detail-value">{{ store.activePatient.value.Phone }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Cân nặng:</span>
              <span class="detail-value">{{ store.activePatient.value.WeightKg ? store.activePatient.value.WeightKg + ' kg' : 'Chưa nhập' }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Địa chỉ:</span>
              <span class="detail-value text-ellipsis" :title="store.activePatient.value.Address">{{ store.activePatient.value.Address }}</span>
            </div>
          </div>

          <!-- Special conditions flags -->
          <div class="condition-toggles">
            <span :class="['cond-badge', { 'active': store.activePatient.value.IsPregnant }]">
              🤰 Mang thai: {{ store.activePatient.value.IsPregnant ? 'CÓ' : 'KHÔNG' }}
            </span>
            <span :class="['cond-badge', { 'active': store.activePatient.value.IsBreastfeeding }]">
              🍼 Cho con bú: {{ store.activePatient.value.IsBreastfeeding ? 'CÓ' : 'KHÔNG' }}
            </span>
          </div>

          <!-- Allergies (PatientAllergies) -->
          <div class="allergy-tags">
            <span class="tag-title">Tiền sử Dị ứng:</span>
            <div class="tag-list" v-if="store.activePatientAllergiesList.value.length > 0">
              <span v-for="(alg, idx) in store.activePatientAllergiesList.value" :key="idx" class="tag danger">
                {{ alg.target }} ({{ alg.severity }})
              </span>
            </div>
            <span v-else class="empty-text">Không ghi nhận dị ứng</span>
          </div>

          <!-- Diseases (PatientDiseases) -->
          <div class="allergy-tags">
            <span class="tag-title">Bệnh nền ghi nhận:</span>
            <div class="tag-list" v-if="store.activePatientDiseasesList.value.length > 0">
              <span v-for="(d, idx) in store.activePatientDiseasesList.value" :key="idx" class="tag warning" :title="d.note">
                {{ d.name }}
              </span>
            </div>
            <span v-else class="empty-text">Không có bệnh nền</span>
          </div>
        </div>
      </div>

      <!-- Right Column: Cart builder -->
      <div class="form-section prescription-builder">
        <h3 class="section-title">2. Xây dựng giỏ hàng thuốc (Sale Details)</h3>
        
        <div class="builder-inputs">
          <div class="input-row">
            <div class="input-col">
              <label class="form-label">Chọn thuốc:</label>
              <select v-model="store.selectedMedicineId.value" class="form-control select-control">
                <option v-for="m in store.medicines.value.filter(med => med.IsActive)" :key="m.MedicineId" :value="m.MedicineId">
                  {{ m.MedicineName }} ({{ m.Strength }}) - {{ m.Price }}đ
                </option>
              </select>
            </div>
            <div class="input-col max-100">
              <label class="form-label">Số lượng:</label>
              <input type="number" v-model.number="store.qtyToAdd.value" class="form-control" min="1" />
            </div>
          </div>

          <div class="input-row">
            <div class="input-col">
              <label class="form-label">Hướng dẫn liều dùng (Dosage Instruction):</label>
              <input type="text" v-model="store.dosageText.value" class="form-control" />
            </div>
          </div>

          <div class="input-row">
            <div class="input-col">
              <label class="form-label">Số lần/ngày:</label>
              <input type="number" v-model.number="store.timesPerDayInput.value" class="form-control" min="1" />
            </div>
            <div class="input-col">
              <label class="form-label">Thời gian dùng:</label>
              <input type="text" v-model="store.durationInput.value" class="form-control" />
            </div>
          </div>

          <div class="input-row">
            <button class="primary-btn full-width" @click="store.addMedicineToCart">Thêm vào giỏ hàng thuốc</button>
          </div>
        </div>

        <div class="divider" style="margin: 20px 0; border-top: 1px solid var(--border-color);"></div>

        <!-- Prescription Cart Table -->
        <div class="cart-table-wrapper" v-if="store.prescriptionCart.value.length > 0">
          <table class="dashboard-table">
            <thead>
              <tr>
                <th>Tên thuốc</th>
                <th>Hàm lượng</th>
                <th>SL</th>
                <th>Đơn giá</th>
                <th>Liều dùng</th>
                <th>Xóa</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, idx) in store.prescriptionCart.value" :key="idx">
                <td><strong>{{ item.medicine.MedicineName }}</strong></td>
                <td>{{ item.medicine.Strength }}</td>
                <td>{{ item.quantity }}</td>
                <td>{{ item.medicine.Price }}đ</td>
                <td><small>{{ item.dosageInstruction }} ({{ item.duration }})</small></td>
                <td>
                  <button class="delete-btn" @click="store.removeFromCart(idx)" title="Xóa khỏi đơn">×</button>
                </td>
              </tr>
            </tbody>
          </table>
          
          <div class="cart-total-row">
            <span>Tổng tiền thuốc: <strong>{{ store.cartTotalAmount.value.toLocaleString() }}đ</strong></span>
          </div>

          <div class="cart-actions-row" style="margin-top: 12px; display: flex; justify-content: flex-end;">
            <button class="safety-btn flex-center" @click="store.runSafetyCheck">
              <svg viewBox="0 0 24 24" class="safety-icon-btn" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 18px; height: 18px; margin-right: 8px;">
                <path d="M9 12.75L11.25 15 15 9.75M21 12c0 1.268-.63 2.39-1.593 3.068a3.745 3.745 0 01-1.043 3.296" />
              </svg>
              Kiểm tra An toàn Bán thuốc (Safety Check)
            </button>
          </div>
        </div>

        <div v-else class="cart-placeholder flex-center">
          <div class="placeholder-content">
            <svg viewBox="0 0 24 24" class="cart-icon" fill="none" stroke="currentColor" stroke-width="1.5">
              <circle cx="9" cy="21" r="1" />
              <circle cx="20" cy="21" r="1" />
              <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6" />
            </svg>
            <p>Giỏ hàng thuốc đang trống. Hãy thêm các thuốc được chỉ định vào đơn.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- ==========================================
      SAFETY CHECK RESULTS MODAL DIALOG
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="store.showSafetyResultsModal.value">
      <div class="modal-card">
        <div class="modal-header">
          <div class="modal-title-area" style="display: flex; align-items: center; gap: 12px;">
            <svg viewBox="0 0 24 24" class="safety-modal-icon" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 26px; height: 26px; color: var(--primary-medium);">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
            </svg>
            <h3>Kết quả Kiểm tra An toàn Bán thuốc</h3>
          </div>
          <button class="close-modal-btn" @click="store.showSafetyResultsModal.value = false" style="background: transparent; border: none; font-size: 28px; line-height: 1; color: var(--text-muted); cursor: pointer;">×</button>
        </div>
        
        <div class="modal-body">
          <div class="patient-quick-summary" v-if="store.activePatient.value">
            <span>Bệnh nhân: <strong>{{ store.activePatient.value.FullName }}</strong></span>
            <span>Cân nặng: <strong>{{ store.activePatient.value.WeightKg }} kg</strong></span>
            <span v-if="store.activePatient.value.IsPregnant" class="status-tag danger">Mang thai</span>
          </div>

          <!-- Warnings List -->
          <div class="warnings-holder" v-if="store.safetyWarnings.value.length > 0">
            <p class="warning-alert-count">⚠️ Hệ thống phát hiện <strong>{{ store.safetyWarnings.value.length }}</strong> mối nguy hại nguy hiểm!</p>
            
            <div class="warnings-scroll-list">
              <div v-for="w in store.safetyWarnings.value" :key="w.WarningId" :class="['safety-warning-card', { 'acknowledged': w.IsAcknowledged }]">
                <div class="warning-card-head">
                  <span class="warning-tag">{{ w.WarningType }}</span>
                  <span class="severity-badge-high">{{ w.Severity }}</span>
                </div>
                <p class="warning-msg">{{ w.Message }}</p>
                
                <div class="recommendation-box">
                  <strong>Khuyến cáo y khoa:</strong>
                  <p>{{ w.Recommendation }}</p>
                </div>

                <!-- Resolution Area -->
                <div class="resolution-row">
                  <div v-if="!w.IsAcknowledged" class="ack-input-group">
                    <input 
                      type="text" 
                      placeholder="Nhập lý do/quyết định (ví dụ: Thay đổi thuốc sang Paracetamol)..." 
                      class="form-control text-control-sm"
                      v-model="store.warningDecisions.value[w.WarningId]"
                    />
                    <button 
                      class="ack-btn" 
                      @click="store.acknowledgeWarning(w.WarningId, store.warningDecisions.value[w.WarningId] || 'Đã xác nhận và điều chỉnh đơn thuốc')"
                    >
                      Xác nhận đã xử lý
                    </button>
                  </div>
                  <div v-else class="ack-done">
                    <span>✓ Đã xác nhận xử lý: <strong>{{ w.Decision }}</strong> (Bởi: Ds. Mai)</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="safety-success-message flex-center" v-else>
            <div class="success-content">
              <svg viewBox="0 0 24 24" class="success-tick-icon" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 56px; height: 56px; color: var(--success);">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <h4>Không phát hiện rủi ro!</h4>
              <p>Đơn thuốc an toàn với các dữ liệu dị ứng, tương tác thuốc và chống chỉ định bệnh nền hiện tại của bệnh nhân.</p>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="secondary-btn" @click="store.cancelPrescription">Hủy bán thuốc (Hủy giao dịch)</button>
          
          <button 
            class="primary-btn" 
            :disabled="store.finalDecision.value === 'Pending'"
            @click="store.completePrescriptionSales"
          >
            {{ store.finalDecision.value === 'Pending' ? 'Cần xác nhận các cảnh báo để bán' : 'Hoàn tất & Xuất hóa đơn' }}
          </button>
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
