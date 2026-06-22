<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()

// Autocomplete Patient search states
const patientSearchText = ref('')
const showPatientSearchDropdown = ref(false)

// Initialize search text with active patient name if present
watch(
  () => store.activePatient.value,
  (newPat) => {
    if (newPat && !patientSearchText.value) {
      patientSearchText.value = newPat.FullName
    }
  },
  { immediate: true }
)

const filteredPatientsList = computed(() => {
  const query = patientSearchText.value.toLowerCase().trim()
  if (!query) return store.patients.value
  return store.patients.value.filter(
    p => p.FullName.toLowerCase().includes(query) || (p.Phone && p.Phone.includes(query))
  )
})

const selectPatientFromSearch = (patientId: number) => {
  store.selectedPatientId.value = patientId
  const pat = store.patients.value.find(p => p.PatientId === patientId)
  if (pat) {
    patientSearchText.value = pat.FullName
  }
  showPatientSearchDropdown.value = false
}

// Autocomplete Medicine search states
const medicineSearchText = ref('')
const showMedicineSearchDropdown = ref(false)

const filteredMedicinesList = computed(() => {
  const query = medicineSearchText.value.toLowerCase().trim()
  const activeMeds = store.medicines.value.filter(m => m.IsActive)
  if (!query) return activeMeds
  return activeMeds.filter(
    m => m.MedicineName.toLowerCase().includes(query) || (m.Strength && m.Strength.toLowerCase().includes(query))
  )
})

const selectMedicineFromSearch = (medicineId: number) => {
  store.selectedMedicineId.value = medicineId
  const med = store.medicines.value.find(m => m.MedicineId === medicineId)
  if (med) {
    medicineSearchText.value = `${med.MedicineName} (${med.Strength})`
  }
  showMedicineSearchDropdown.value = false
}

// Real-time Pre-check Alert engine
const precheckAlerts = computed(() => {
  const alerts: { severity: string; message: string }[] = []
  const selectedMedId = store.selectedMedicineId.value
  const activePat = store.activePatient.value
  const currentMed = store.medicines.value.find(m => m.MedicineId === selectedMedId)

  if (!activePat || !currentMed) return alerts

  // 1. Check Allergies
  const patientAllergiesData = store.patientAllergies.value.filter(pa => pa.PatientId === activePat.PatientId)
  const medIngredients = store.medicineIngredients.value.filter(mi => mi.MedicineId === currentMed.MedicineId)

  medIngredients.forEach(mi => {
    const ing = store.activeIngredients.value.find(ai => ai.IngredientId === mi.IngredientId)
    if (ing) {
      const isAllergic = patientAllergiesData.find(pa => pa.IngredientId === mi.IngredientId)
      if (isAllergic) {
        alerts.push({
          severity: isAllergic.Severity || 'Nghiêm trọng',
          message: `⚠️ Cảnh báo sớm: Bệnh nhân dị ứng với hoạt chất [${ing.IngredientName}] có trong thuốc này!`
        })
      }
    }
  })

  // 2. Check Interactions with current cart items
  store.prescriptionCart.value.forEach(item => {
    const medIngredientsA = store.medicineIngredients.value.filter(mi => mi.MedicineId === currentMed.MedicineId)
    const medIngredientsB = store.medicineIngredients.value.filter(mi => mi.MedicineId === item.medicine.MedicineId)

    medIngredientsA.forEach(miA => {
      medIngredientsB.forEach(miB => {
        const interact = store.drugInteractions.value.find(di =>
          (di.IngredientAId === miA.IngredientId && di.IngredientBId === miB.IngredientId) ||
          (di.IngredientAId === miB.IngredientId && di.IngredientBId === miA.IngredientId)
        )
        if (interact) {
          alerts.push({
            severity: interact.Severity,
            message: `⚡ Tương tác sớm: Thuốc đang chọn tương tác [${interact.Severity}] với thuốc [${item.medicine.MedicineName}] trong giỏ hàng!`
          })
        }
      })
    })
  })

  return alerts
})

// OCR Scanning Simulator states
const isScanningOCR = ref(false)

const simulateOCRScan = () => {
  isScanningOCR.value = true
  
  // Simulate API scanning delay (1.5s)
  setTimeout(() => {
    isScanningOCR.value = false
    
    // Auto populate cart with scanned items
    // First, clear existing cart
    store.prescriptionCart.value = []
    
    // Add mock scanned medicines
    // 1. Paracetamol 500mg
    const para = store.medicines.value.find(m => m.MedicineId === 1)
    if (para) {
      store.prescriptionCart.value.push({
        medicine: para,
        quantity: 10,
        dosageInstruction: 'Ngày uống 3 lần, mỗi lần 1 viên sau ăn khi sốt',
        timesPerDay: 3,
        duration: '3 ngày',
        adviceNote: 'Không uống lúc đói, giãn cách tối thiểu 4-6 tiếng'
      })
    }
    
    // 2. Amoxicillin 500mg
    const amox = store.medicines.value.find(m => m.MedicineId === 2)
    if (amox) {
      store.prescriptionCart.value.push({
        medicine: amox,
        quantity: 10,
        dosageInstruction: 'Ngày uống 2 lần, mỗi lần 1 viên sau ăn sáng/tối',
        timesPerDay: 2,
        duration: '5 ngày',
        adviceNote: 'Uống đúng giờ, uống đủ liều kháng sinh'
      })
    }

    // Reset safety checker status
    store.hasCheckedSafety.value = false
    store.safetyWarnings.value = []
    alert('✓ Đã quét đơn thuốc mẫu thành công! Hệ thống tự động thêm 2 loại thuốc vào giỏ hàng và thiết lập liều dùng.')
  }, 1500)
}
</script>

<template>
  <div class="view-container">
    <div class="form-container">
      <!-- Left Column: Patient Selector & Profile -->
      <div class="form-section patient-details">
        <div class="section-header-row">
          <h3 class="section-title">1. Chọn bệnh nhân & Tiền sử bệnh (Patients)</h3>
          <!-- OCR Scan simulation button -->
          <button class="ocr-scan-btn flex-center" @click="simulateOCRScan" :disabled="isScanningOCR">
            <svg viewBox="0 0 24 24" class="ocr-icon" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6.827 6.175A2.31 2.31 0 015.186 7.23c-.38.054-.757.112-1.134.175C2.999 7.58 2.25 8.507 2.25 9.574V18a2.25 2.25 0 002.25 2.25h15A2.25 2.25 0 0021.75 18V9.574c0-1.067-.75-1.994-1.802-2.169a47.865 47.865 0 00-1.134-.175 2.31 2.31 0 01-1.64-1.055l-.822-1.316a2.192 2.192 0 00-1.736-1.039 48.774 48.774 0 00-5.232 0 2.192 2.192 0 00-1.736 1.039l-.821 1.316z" />
              <circle cx="12" cy="13" r="3" />
            </svg>
            {{ isScanningOCR ? 'Đang quét đơn...' : 'Quét đơn mẫu (OCR)' }}
          </button>
        </div>
        
        <!-- OCR scan laser animation -->
        <div class="ocr-scanning-overlay flex-center" v-if="isScanningOCR">
          <div class="scanning-box">
            <div class="laser-line"></div>
            <div class="scanned-text">Đang phân tích hình ảnh đơn thuốc...</div>
          </div>
        </div>
        
        <!-- Search patient autocomplete -->
        <div class="patient-selector-wrapper" style="position: relative;">
          <label class="form-label">Tìm kiếm bệnh nhân:</label>
          <div class="search-input-wrapper">
            <input 
              type="text" 
              placeholder="Nhập tên hoặc số điện thoại..." 
              class="form-control"
              v-model="patientSearchText"
              @focus="showPatientSearchDropdown = true"
            />
            <button class="clear-search-btn" v-if="patientSearchText" @click="patientSearchText = ''; store.selectedPatientId.value = 0">×</button>
          </div>

          <!-- Dropdown suggestions -->
          <div class="autocomplete-dropdown" v-if="showPatientSearchDropdown && filteredPatientsList.length > 0">
            <div 
              v-for="p in filteredPatientsList" 
              :key="p.PatientId" 
              class="dropdown-item-p"
              @click="selectPatientFromSearch(p.PatientId)"
            >
              <strong>{{ p.FullName }}</strong> - <span>{{ p.Phone }}</span>
            </div>
          </div>
          <div class="autocomplete-dropdown" v-else-if="showPatientSearchDropdown && patientSearchText">
            <div class="dropdown-empty">Không tìm thấy bệnh nhân nào</div>
          </div>
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
              <span class="detail-value text-ellipsis" :title="store.activePatient.value.Address || undefined">{{ store.activePatient.value.Address }}</span>
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
          <div class="input-row" style="position: relative;">
            <div class="input-col">
              <label class="form-label">Chọn thuốc cần kê đơn:</label>
              <div class="search-input-wrapper">
                <input 
                  type="text" 
                  placeholder="Nhập tên thuốc để tìm..." 
                  class="form-control"
                  v-model="medicineSearchText"
                  @focus="showMedicineSearchDropdown = true"
                />
                <button class="clear-search-btn" v-if="medicineSearchText" @click="medicineSearchText = ''; store.selectedMedicineId.value = 0">×</button>
              </div>

              <!-- Medicine suggestions dropdown -->
              <div class="autocomplete-dropdown" v-if="showMedicineSearchDropdown && filteredMedicinesList.length > 0">
                <div 
                  v-for="m in filteredMedicinesList" 
                  :key="m.MedicineId" 
                  class="dropdown-item-m"
                  @click="selectMedicineFromSearch(m.MedicineId)"
                >
                  <div class="dropdown-med-name"><strong>{{ m.MedicineName }}</strong> ({{ m.Strength }})</div>
                  <div class="dropdown-med-meta">{{ store.drugGroups.value.find(dg => dg.DrugGroupId === m.DrugGroupId)?.GroupName || 'Default' }} - <strong style="color: var(--primary-medium);">{{ m.Price.toLocaleString() }}đ</strong></div>
                </div>
              </div>
              <div class="autocomplete-dropdown" v-else-if="showMedicineSearchDropdown && medicineSearchText">
                <div class="dropdown-empty">Không tìm thấy thuốc nào</div>
              </div>
            </div>
            
            <div class="input-col max-100">
              <label class="form-label">Số lượng:</label>
              <input type="number" v-model.number="store.qtyToAdd.value" class="form-control" min="1" />
            </div>
          </div>

          <!-- Real-time Pre-check Alert banner -->
          <div class="precheck-alerts-container" v-if="precheckAlerts.length > 0">
            <div v-for="(alert, idx) in precheckAlerts" :key="idx" :class="['precheck-alert-banner', alert.severity === 'Nghiêm trọng' || alert.severity === 'High' ? 'danger' : 'warning']">
              <span>{{ alert.message }}</span>
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
            <button class="primary-btn full-width" @click="store.addMedicineToCart" :disabled="!store.selectedMedicineId.value">Thêm vào giỏ hàng thuốc</button>
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
                <!-- Editable quantity directly in table -->
                <td>
                  <input type="number" v-model.number="item.quantity" class="form-control table-qty-input" min="1" />
                </td>
                <td>{{ item.medicine.Price.toLocaleString() }}đ</td>
                <!-- Editable dosage directly in table -->
                <td>
                  <input type="text" v-model="item.dosageInstruction" class="form-control table-dosage-input" />
                </td>
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
      SAFETY CHECK RESULTS & INVOICE PREVIEW MODAL
    ========================================== -->
    <div class="modal-overlay flex-center" v-if="store.showSafetyResultsModal.value">
      <div class="modal-card safety-results-modal">
        <div class="modal-header">
          <div class="modal-title-area" style="display: flex; align-items: center; gap: 12px;">
            <svg viewBox="0 0 24 24" class="safety-modal-icon" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 26px; height: 26px; color: var(--primary-medium);">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
            </svg>
            <h3>{{ store.finalDecision.value === 'Pending' ? 'Kết quả Kiểm tra An toàn Lâm sàng' : 'Xem trước Hóa đơn biên lai' }}</h3>
          </div>
          <button class="close-modal-btn" @click="store.showSafetyResultsModal.value = false" style="background: transparent; border: none; font-size: 28px; line-height: 1; color: var(--text-muted); cursor: pointer;">×</button>
        </div>
        
        <div class="modal-body scrollable-modal-body">
          <div class="patient-quick-summary" v-if="store.activePatient.value">
            <span>Bệnh nhân: <strong>{{ store.activePatient.value.FullName }}</strong></span>
            <span>Cân nặng: <strong>{{ store.activePatient.value.WeightKg }} kg</strong></span>
            <span v-if="store.activePatient.value.IsPregnant" class="status-tag danger">Mang thai</span>
          </div>

          <!-- 1. WARNINGS RESOLUTION STEP (If finalDecision is still Pending) -->
          <div class="warnings-resolution-area" v-if="store.finalDecision.value === 'Pending'">
            <div class="warnings-holder" v-if="store.safetyWarnings.value.length > 0">
              <p class="warning-alert-count">⚠️ Phát hiện <strong>{{ store.safetyWarnings.value.length }}</strong> rủi ro lâm sàng cần giải quyết:</p>
              
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
                        placeholder="Nhập lý do/chỉ định thay thế..." 
                        class="form-control text-control-sm"
                        v-model="store.warningDecisions.value[w.WarningId]"
                      />
                      <button 
                        class="ack-btn" 
                        @click="store.acknowledgeWarning(w.WarningId, store.warningDecisions.value[w.WarningId] || 'Đã xác nhận và điều chỉnh đơn thuốc')"
                      >
                        Duyệt cảnh báo
                      </button>
                    </div>
                    <div v-else class="ack-done">
                      <span>✓ Đã duyệt: <strong>{{ w.Decision }}</strong> (Bởi: Ds. Trần Thị Mai)</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 2. INVOICE PREVIEW STEP (If finalDecision is Approved) -->
          <div class="invoice-preview-area" v-else>
            <div class="receipt-box">
              <div class="receipt-header">
                <h3>NHÀ THUỐC SAFEPHARM</h3>
                <p>123 Đường An Toàn, Gia Lai - Điện thoại: 0988.888.888</p>
                <div class="receipt-title">HÓA ĐƠN BÁN LẺ DƯỢC PHẨM</div>
                <p class="receipt-date">Ngày bán: 22/06/2026 - Mã HD: HD-00{{ store.sales.value.length + 1 }}</p>
              </div>

              <div class="receipt-info-block">
                <div>Khách hàng: <strong>{{ store.activePatient.value?.FullName }}</strong></div>
                <div>Điện thoại: {{ store.activePatient.value?.Phone }}</div>
                <div>Địa chỉ: {{ store.activePatient.value?.Address || 'Không có' }}</div>
                <div>Dược sĩ cấp phát: <strong>Ds. Trần Thị Mai</strong></div>
              </div>

              <table class="receipt-table">
                <thead>
                  <tr>
                    <th>Tên thuốc / Hướng dẫn</th>
                    <th style="text-align: right;">SL</th>
                    <th style="text-align: right;">Đơn giá</th>
                    <th style="text-align: right;">Thành tiền</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(item, idx) in store.prescriptionCart.value" :key="idx">
                    <td>
                      <div><strong>{{ item.medicine.MedicineName }}</strong> ({{ item.medicine.Strength }})</div>
                      <div class="receipt-dosage-desc">Liều dùng: {{ item.dosageInstruction }} ({{ item.duration }})</div>
                    </td>
                    <td style="text-align: right;">{{ item.quantity }}</td>
                    <td style="text-align: right;">{{ item.medicine.Price.toLocaleString() }}đ</td>
                    <td style="text-align: right;"><strong>{{ (item.medicine.Price * item.quantity).toLocaleString() }}đ</strong></td>
                  </tr>
                </tbody>
              </table>

              <div class="receipt-total-row">
                <span>TỔNG TIỀN PHẢI THANH TOÁN:</span>
                <span class="total-price">{{ store.cartTotalAmount.value.toLocaleString() }}đ</span>
              </div>

              <div class="receipt-signatures">
                <div class="sig-col">
                  <span>Khách hàng ký tên</span>
                  <div class="sig-placeholder"></div>
                  <strong>{{ store.activePatient.value?.FullName }}</strong>
                </div>
                <div class="sig-col">
                  <span>Dược sĩ xác nhận</span>
                  <div class="sig-placeholder"></div>
                  <strong>Ds. Trần Thị Mai</strong>
                </div>
              </div>
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
  gap: 20px;
}

/* Page sections header row */
.section-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}
.ocr-scan-btn {
  background-color: rgba(34, 211, 238, 0.1);
  border: 1px solid rgba(34, 211, 238, 0.2);
  color: #22d3ee;
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 700;
  border-radius: var(--border-radius-md);
  cursor: pointer;
  transition: all 0.2s;
  gap: 6px;
}
.ocr-scan-btn:hover:not(:disabled) {
  background-color: #22d3ee;
  color: #0f172a;
}
.ocr-scan-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.ocr-icon {
  width: 16px;
  height: 16px;
}

/* OCR scanner overlay animation */
.ocr-scanning-overlay {
  background: rgba(15, 23, 42, 0.05);
  border: 1px dashed #22d3ee;
  border-radius: var(--border-radius-md);
  padding: 24px 16px;
  margin-bottom: 16px;
  position: relative;
  overflow: hidden;
}
.scanning-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}
.laser-line {
  position: absolute;
  left: 0;
  width: 100%;
  height: 2px;
  background-color: #22d3ee;
  box-shadow: 0 0 8px #22d3ee;
  animation: scanLaser 1.5s infinite ease-in-out;
}
.scanned-text {
  font-size: 13px;
  font-weight: 700;
  color: #22d3ee;
  animation: blinkText 1s infinite alternate;
}

@keyframes scanLaser {
  0% { top: 0%; }
  50% { top: 100%; }
  100% { top: 0%; }
}
@keyframes blinkText {
  0% { opacity: 0.5; }
  100% { opacity: 1; }
}

/* Autocomplete search inputs */
.search-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}
.search-input-wrapper input {
  padding-right: 32px;
}
.clear-search-btn {
  position: absolute;
  right: 10px;
  background: transparent;
  border: none;
  font-size: 20px;
  color: var(--text-muted);
  cursor: pointer;
  line-height: 1;
}
.clear-search-btn:hover {
  color: var(--danger);
}

.autocomplete-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  width: 100%;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  box-shadow: var(--shadow-lg);
  z-index: 100;
  max-height: 200px;
  overflow-y: auto;
  margin-top: 4px;
}
.dropdown-item-p {
  padding: 10px 14px;
  cursor: pointer;
  font-size: 13.5px;
  border-bottom: 1px solid var(--border-color);
  color: var(--text-main);
  transition: background-color 0.2s;
}
.dropdown-item-p:hover {
  background-color: var(--bg-main);
}
.dropdown-item-p strong {
  color: var(--text-main);
}
.dropdown-item-m {
  padding: 10px 14px;
  cursor: pointer;
  border-bottom: 1px solid var(--border-color);
  transition: background-color 0.2s;
}
.dropdown-item-m:hover {
  background-color: var(--bg-main);
}
.dropdown-med-name {
  font-size: 13.5px;
  color: var(--text-main);
}
.dropdown-med-meta {
  font-size: 11px;
  color: var(--text-muted);
  margin-top: 2px;
}
.dropdown-empty {
  padding: 14px;
  text-align: center;
  font-size: 13px;
  color: var(--text-muted);
  font-style: italic;
}

/* Pre-check Alerts style */
.precheck-alerts-container {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 12px;
}
.precheck-alert-banner {
  padding: 10px 14px;
  border-radius: var(--border-radius-md);
  font-size: 12.5px;
  font-weight: 700;
  border: 1px solid transparent;
}
.precheck-alert-banner.danger {
  background-color: var(--danger-bg);
  color: var(--danger);
  border-color: rgba(239, 68, 68, 0.15);
}
.precheck-alert-banner.warning {
  background-color: var(--warning-bg);
  color: var(--warning);
  border-color: rgba(245, 158, 11, 0.15);
}

/* Table editable inputs */
.table-qty-input {
  width: 70px;
  padding: 4px 8px;
  font-size: 13px;
  text-align: center;
}
.table-dosage-input {
  width: 100%;
  min-width: 180px;
  padding: 4px 8px;
  font-size: 12.5px;
}

/* Scrollable modal body */
.scrollable-modal-body {
  max-height: 60vh;
  overflow-y: auto;
  padding-right: 6px;
}

/* Invoice receipt styling */
.receipt-box {
  background-color: #fff;
  border: 2px solid #000;
  color: #000;
  padding: 24px;
  font-family: 'Courier New', Courier, monospace;
}
.receipt-header {
  text-align: center;
  border-bottom: 1px dashed #000;
  padding-bottom: 14px;
  margin-bottom: 14px;
}
.receipt-header h3 {
  font-size: 18px;
  font-weight: 800;
  margin: 0 0 4px 0;
}
.receipt-header p {
  font-size: 11px;
  margin: 0;
}
.receipt-title {
  font-size: 15px;
  font-weight: 800;
  margin: 12px 0 6px 0;
  letter-spacing: 0.5px;
}
.receipt-date {
  font-size: 12px !important;
}

.receipt-info-block {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px;
  font-size: 12px;
  margin-bottom: 14px;
}

.receipt-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 14px;
}
.receipt-table th {
  border-bottom: 1px solid #000;
  padding: 6px 4px;
  font-size: 12px;
  font-weight: 800;
}
.receipt-table td {
  padding: 8px 4px;
  font-size: 11px;
  border-bottom: 1px dashed #e2e8f0;
}
.receipt-dosage-desc {
  font-size: 10px;
  color: #475569;
  margin-top: 2px;
}

.receipt-total-row {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
  font-weight: 800;
  border-top: 1px solid #000;
  padding-top: 10px;
  margin-bottom: 24px;
}
.receipt-total-row .total-price {
  font-size: 16px;
}

.receipt-signatures {
  display: flex;
  justify-content: space-between;
  margin-top: 20px;
}
.sig-col {
  text-align: center;
  width: 45%;
  font-size: 12px;
}
.sig-placeholder {
  height: 60px;
}
</style>
