<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { usePharmacyStore } from '../store/pharmacy'

const router = useRouter()
const store = usePharmacyStore()

const email = ref('')
const pin = ref('')
const errorMessage = ref('')
const isLoading = ref(false)

const handleLogin = async () => {
  if (!email.value || !pin.value) {
    errorMessage.value = 'Vui lòng nhập đầy đủ Email và mã PIN.'
    return
  }
  
  isLoading.value = true
  errorMessage.value = ''
  
  // Artificial delay for premium loading experience
  setTimeout(async () => {
    try {
      const success = await store.login(email.value, pin.value)
      if (success) {
        router.push({ name: 'dashboard' })
      } else {
        errorMessage.value = 'Email hoặc mã PIN không chính xác, hoặc tài khoản đã bị khóa.'
      }
    } catch (err) {
      errorMessage.value = 'Có lỗi hệ thống xảy ra. Vui lòng thử lại.'
    } finally {
      isLoading.value = false
    }
  }, 600)
}

const quickLogin = (roleEmail: string) => {
  email.value = roleEmail
  pin.value = '123456' // Default test pin
  handleLogin()
}
</script>

<template>
  <div class="login-page">
    <div class="login-container">
      
      <!-- Left Side: Hero Brand Area -->
      <div class="brand-hero">
        <div class="brand-hero-content">
          <div class="hero-logo">
            <svg viewBox="0 0 24 24" class="hero-logo-icon" fill="none" stroke="currentColor" stroke-width="2.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
            </svg>
            <span class="hero-logo-text">Safe<span class="brand-accent">Pharm</span></span>
          </div>
          
          <h2 class="hero-title">Hệ Thống Quản Lý Dược Phẩm An Toàn Lâm Sàng</h2>
          <p class="hero-desc">Đảm bảo an toàn sức khỏe bệnh nhân thông qua hệ thống cảnh báo tương tác thuốc và tự động phát hiện chống chỉ định bệnh lý.</p>
          
          <div class="feature-bullets">
            <div class="bullet-item">
              <div class="bullet-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              </div>
              <div>
                <h4>Đánh Giá An Toàn Tự Động</h4>
                <p>Kiểm tra dị ứng hoạt chất và cảnh báo mức độ tương tác thời gian thực.</p>
              </div>
            </div>
            
            <div class="bullet-item">
              <div class="bullet-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
              </div>
              <div>
                <h4>Hồ Sơ Bệnh Án Điện Tử</h4>
                <p>Theo dõi tiền sử bệnh lý và đối tượng đặc biệt (thai kỳ, cho con bú).</p>
              </div>
            </div>
          </div>
        </div>
        <div class="hero-background-glow"></div>
      </div>
      
      <!-- Right Side: Form and Quick Login -->
      <div class="form-side">
        <div class="form-wrapper">
          <div class="form-header">
            <h3>Đăng nhập hệ thống</h3>
            <p>Vui lòng điền thông tin tài khoản được cấp để tiếp tục</p>
          </div>
          
          <!-- Error Alert Banner -->
          <Transition name="fade">
            <div class="error-banner" v-if="errorMessage">
              <svg viewBox="0 0 24 24" class="error-icon" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10"/>
                <line x1="12" y1="8" x2="12" y2="12"/>
                <line x1="12" y1="16" x2="12.01" y2="16"/>
              </svg>
              <span>{{ errorMessage }}</span>
            </div>
          </Transition>
          
          <form @submit.prevent="handleLogin" class="login-form">
            <div class="input-group">
              <label for="email" class="input-label">Địa chỉ Email</label>
              <div class="input-container">
                <svg viewBox="0 0 24 24" class="field-icon" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
                  <polyline points="22,6 12,13 2,6"/>
                </svg>
                <input 
                  type="email" 
                  id="email" 
                  v-model="email" 
                  placeholder="name@gmail.com" 
                  class="field-control"
                  required
                />
              </div>
            </div>
            
            <div class="input-group">
              <div class="label-row">
                <label for="pin" class="input-label">Mã PIN xác thực</label>
                <span class="forgot-pin">Quên mã PIN?</span>
              </div>
              <div class="input-container">
                <svg viewBox="0 0 24 24" class="field-icon" fill="none" stroke="currentColor" stroke-width="2">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                  <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                </svg>
                <input 
                  type="password" 
                  id="pin" 
                  v-model="pin" 
                  placeholder="••••••" 
                  class="field-control"
                  required
                />
              </div>
            </div>
            
            <button type="submit" class="submit-btn" :disabled="isLoading">
              <span v-if="isLoading" class="spinner"></span>
              <span v-else>Đăng nhập an toàn</span>
            </button>
          </form>
          
          <!-- Quick Login Section -->
          <div class="quick-login-section">
            <div class="section-divider">
              <span>HOẶC ĐĂNG NHẬP NHANH (MÔ PHỎNG)</span>
            </div>
            
            <div class="quick-profiles-grid">
              <!-- Pharmacist Profile Card -->
              <button class="profile-card" @click="quickLogin('duocsi@gmail.com')">
                <div class="profile-avatar pharmacist">TM</div>
                <div class="profile-meta">
                  <span class="profile-name">Ds. Trần Thị Mai</span>
                  <span class="profile-role pharmacist">Dược sĩ trực ca</span>
                </div>
                <svg viewBox="0 0 24 24" class="arrow-right" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="9 18 15 12 9 6" />
                </svg>
              </button>
              
              <!-- Manager Profile Card -->
              <button class="profile-card" @click="quickLogin('quanly@gmail.com')">
                <div class="profile-avatar manager">TS</div>
                <div class="profile-meta">
                  <span class="profile-name">Ds. Phạm Thanh Sơn</span>
                  <span class="profile-role manager">Quản lý</span>
                </div>
                <svg viewBox="0 0 24 24" class="arrow-right" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="9 18 15 12 9 6" />
                </svg>
              </button>
              
              <!-- Admin Profile Card -->
              <button class="profile-card" @click="quickLogin('admin@gmail.com')">
                <div class="profile-avatar admin">MQ</div>
                <div class="profile-meta">
                  <span class="profile-name">Nguyễn Minh Quân</span>
                  <span class="profile-role admin">Quản trị viên</span>
                </div>
                <svg viewBox="0 0 24 24" class="arrow-right" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="9 18 15 12 9 6" />
                </svg>
              </button>
            </div>
          </div>
          
        </div>
      </div>
      
    </div>
  </div>
</template>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100vw;
  height: 100vh;
  background-color: var(--bg-main);
  padding: 20px;
  overflow: hidden;
}

.login-container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  width: 100%;
  max-width: 1100px;
  height: 100%;
  max-height: 720px;
  background-color: var(--bg-card);
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-premium);
  overflow: hidden;
}

/* Left Hero Section */
.brand-hero {
  position: relative;
  background-color: var(--bg-sidebar);
  color: #ffffff;
  padding: 48px;
  display: flex;
  align-items: center;
  overflow: hidden;
}

.brand-hero-content {
  position: relative;
  z-index: 2;
  display: flex;
  flex-direction: column;
  gap: 28px;
}

.hero-logo {
  display: flex;
  align-items: center;
  gap: 12px;
}

.hero-logo-icon {
  width: 38px;
  height: 38px;
  background: linear-gradient(135deg, var(--primary-light), var(--primary));
  padding: 6px;
  border-radius: var(--border-radius-md);
  color: #ffffff;
  box-shadow: 0 4px 10px rgba(13, 148, 136, 0.3);
}

.hero-logo-text {
  font-size: 24px;
  font-weight: 800;
  color: #ffffff;
  letter-spacing: 0.5px;
}

.brand-accent {
  color: var(--primary-light);
}

.hero-title {
  font-size: 32px;
  font-weight: 800;
  line-height: 1.25;
  color: #ffffff;
}

.hero-desc {
  font-size: 15px;
  color: var(--text-sidebar-light);
  line-height: 1.6;
}

.feature-bullets {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-top: 10px;
}

.bullet-item {
  display: flex;
  gap: 16px;
  align-items: flex-start;
}

.bullet-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: var(--border-radius-sm);
  background-color: rgba(20, 184, 166, 0.1);
  color: var(--primary-light);
  flex-shrink: 0;
  margin-top: 2px;
}

.bullet-icon svg {
  width: 18px;
  height: 18px;
}

.bullet-item h4 {
  font-size: 14px;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 4px;
}

.bullet-item p {
  font-size: 13px;
  color: var(--text-sidebar-light);
  line-height: 1.4;
}

.hero-background-glow {
  position: absolute;
  top: -20%;
  left: -20%;
  width: 80%;
  height: 80%;
  background: radial-gradient(circle, rgba(13, 148, 136, 0.15) 0%, rgba(0, 0, 0, 0) 70%);
  z-index: 1;
}

/* Right Form Section */
.form-side {
  padding: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow-y: auto;
}

.form-wrapper {
  width: 100%;
  max-width: 420px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-header h3 {
  font-size: 22px;
  font-weight: 800;
  color: var(--text-main);
  margin-bottom: 6px;
}

.form-header p {
  font-size: 13px;
  color: var(--text-muted);
}

/* Form Styles */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.label-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.input-label {
  font-size: 12px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.forgot-pin {
  font-size: 12px;
  font-weight: 600;
  color: var(--primary-medium);
  cursor: pointer;
}

.forgot-pin:hover {
  text-decoration: underline;
}

.input-container {
  position: relative;
  display: flex;
  align-items: center;
}

.field-icon {
  position: absolute;
  left: 14px;
  width: 18px;
  height: 18px;
  color: var(--text-muted);
  pointer-events: none;
}

.field-control {
  width: 100%;
  padding: 12px 14px 12px 42px;
  background-color: var(--bg-main);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  color: var(--text-main);
  outline: none;
  font-size: 14px;
  transition: all var(--transition-fast);
}

.field-control:focus {
  border-color: var(--border-focus);
  background-color: #ffffff;
  box-shadow: 0 0 0 3px var(--primary-glow);
}

.submit-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--primary-medium);
  color: #ffffff;
  border: none;
  padding: 13px 20px;
  border-radius: var(--border-radius-md);
  font-weight: 700;
  cursor: pointer;
  transition: all var(--transition-fast);
  box-shadow: 0 4px 6px rgba(13, 148, 136, 0.15);
  margin-top: 6px;
}

.submit-btn:hover {
  background-color: var(--primary);
  box-shadow: 0 6px 12px rgba(13, 148, 136, 0.25);
  transform: translateY(-1px);
}

.submit-btn:disabled {
  background-color: #cbd5e1;
  color: #94a3b8;
  cursor: not-allowed;
  box-shadow: none;
  transform: none;
}

/* Error Banner styling */
.error-banner {
  display: flex;
  align-items: center;
  gap: 10px;
  background-color: var(--danger-bg);
  border: 1px solid rgba(239, 68, 68, 0.15);
  border-radius: var(--border-radius-md);
  padding: 12px 16px;
  color: var(--danger);
  font-size: 13px;
  font-weight: 600;
  line-height: 1.4;
}

.error-icon {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
}

/* Quick Login */
.quick-login-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-top: 10px;
}

.section-divider {
  display: flex;
  align-items: center;
  text-align: center;
  font-size: 10px;
  font-weight: 800;
  color: var(--text-muted);
  letter-spacing: 0.8px;
}

.section-divider::before,
.section-divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid var(--border-color);
}

.section-divider::before {
  margin-right: 12px;
}

.section-divider::after {
  margin-left: 12px;
}

.quick-profiles-grid {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.profile-card {
  display: flex;
  align-items: center;
  padding: 10px 14px;
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius-md);
  cursor: pointer;
  text-align: left;
  transition: all var(--transition-fast);
}

.profile-card:hover {
  background-color: var(--bg-main);
  border-color: var(--primary-medium);
  box-shadow: var(--shadow-sm);
  transform: translateX(2px);
}

.profile-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  color: #ffffff;
  font-weight: 700;
  font-size: 12px;
  margin-right: 12px;
  flex-shrink: 0;
}

.profile-avatar.pharmacist {
  background: linear-gradient(135deg, #10b981dd, #10b981);
}

.profile-avatar.manager {
  background: linear-gradient(135deg, #3b82f6dd, #3b82f6);
}

.profile-avatar.admin {
  background: linear-gradient(135deg, #ef4444dd, #ef4444);
}

.profile-meta {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.profile-name {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-main);
}

.profile-role {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  margin-top: 1px;
}

.profile-role.pharmacist { color: var(--success); }
.profile-role.manager { color: var(--info); }
.profile-role.admin { color: var(--danger); }

.arrow-right {
  width: 16px;
  height: 16px;
  color: var(--text-muted);
  transition: transform var(--transition-fast);
}

.profile-card:hover .arrow-right {
  color: var(--primary-medium);
  transform: translateX(2px);
}

/* Spinner anim */
.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Responsive */
@media (max-width: 820px) {
  .login-container {
    grid-template-columns: 1fr;
    max-height: none;
    height: auto;
  }
  .brand-hero {
    display: none;
  }
  .form-side {
    padding: 36px 24px;
  }
}
</style>
