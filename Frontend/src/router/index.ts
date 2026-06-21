import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/DashboardView.vue'
import SellMedicineView from '../views/SellMedicineView.vue'
import MedicinesView from '../views/MedicinesView.vue'
import PatientsView from '../views/PatientsView.vue'
import SafetyAlertsView from '../views/SafetyAlertsView.vue'
import SalesHistoryView from '../views/SalesHistoryView.vue'
import ReportsView from '../views/ReportsView.vue'
import UsersView from '../views/UsersView.vue'
import SettingsView from '../views/SettingsView.vue'

const routes = [
  {
    path: '/',
    name: 'dashboard',
    component: DashboardView
  },
  {
    path: '/sell-medicine',
    name: 'sell-medicine',
    component: SellMedicineView
  },
  {
    path: '/medicines',
    name: 'medicines',
    component: MedicinesView
  },
  {
    path: '/patients',
    name: 'patients',
    component: PatientsView
  },
  {
    path: '/safety-alerts',
    name: 'safety-alerts',
    component: SafetyAlertsView
  },
  {
    path: '/sales-history',
    name: 'sales-history',
    component: SalesHistoryView
  },
  {
    path: '/reports',
    name: 'reports',
    component: ReportsView
  },
  {
    path: '/users',
    name: 'users',
    component: UsersView
  },
  {
    path: '/settings',
    name: 'settings',
    component: SettingsView
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
