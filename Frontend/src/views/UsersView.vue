<script setup lang="ts">
import { usePharmacyStore } from '../store/pharmacy'

const store = usePharmacyStore()
</script>

<template>
  <div class="view-container">
    <div class="grid-card">
      <div class="table-actions" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; gap: 16px;">
        <input 
          type="text" 
          placeholder="Tìm tài khoản nhân viên..." 
          class="search-input-small" 
          style="width: 280px; padding: 8px 12px; background-color: var(--bg-main); border: 1px solid var(--border-color); border-radius: var(--border-radius-md); font-size: 14px; outline: none; color: var(--text-main);"
        />
        <button class="primary-btn" v-if="store.currentRole.value === 'admin'">+ Tạo tài khoản mới</button>
      </div>
      <table class="data-table">
        <thead>
          <tr>
            <th>Mã số</th>
            <th>Họ và tên</th>
            <th>Email liên hệ</th>
            <th>Điện thoại</th>
            <th>Vai trò (Role)</th>
            <th>Ngày tạo</th>
            <th>Trạng thái</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="u in store.users.value" :key="u.UserId">
            <td>USR-00{{ u.UserId }}</td>
            <td><strong>{{ u.FullName }}</strong></td>
            <td>{{ u.Email }}</td>
            <td>{{ u.Phone }}</td>
            <td>
              <span :class="['role-badge', u.RoleId === 1 ? 'admin' : u.RoleId === 3 ? 'manager' : 'pharmacist']">
                {{ u.RoleId === 1 ? 'Quản trị viên' : u.RoleId === 3 ? 'Quản lý' : 'Dược sĩ' }}
              </span>
            </td>
            <td>{{ u.CreatedAt }}</td>
            <td><span class="status-tag active">Hoạt động</span></td>
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
.role-badge {
  display: inline-block;
  padding: 4px 8px;
  font-size: 11px;
  font-weight: 700;
  border-radius: var(--border-radius-sm);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.role-badge.admin {
  background-color: rgba(239, 68, 68, 0.1);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.2);
}
.role-badge.manager {
  background-color: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
  border: 1px solid rgba(245, 158, 11, 0.2);
}
.role-badge.pharmacist {
  background-color: rgba(59, 130, 246, 0.1);
  color: #3b82f6;
  border: 1px solid rgba(59, 130, 246, 0.2);
}
</style>
