import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/LoginView.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: () => import('@/components/Layout/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Dashboard',
        component: () => import('@/views/DashboardView.vue')
      },
      {
        path: 'sculpture/:id',
        name: 'SculptureDetail',
        component: () => import('@/views/SculptureDetailView.vue'),
        props: true
      },
      {
        path: 'analysis/migration',
        name: 'MigrationAnalysis',
        component: () => import('@/views/MigrationAnalysisView.vue')
      },
      {
        path: 'analysis/materials',
        name: 'MaterialsAnalysis',
        component: () => import('@/views/MaterialsAnalysisView.vue')
      },
      {
        path: 'alerts',
        name: 'Alerts',
        component: () => import('@/views/AlertsView.vue')
      },
      {
        path: 'sensors',
        name: 'Sensors',
        component: () => import('@/views/SensorsView.vue')
      },
      {
        path: 'settings',
        name: 'Settings',
        component: () => import('@/views/SettingsView.vue')
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.meta.requiresAuth !== false

  if (requiresAuth && !authStore.isLoggedIn) {
    next('/login')
  } else if (to.path === '/login' && authStore.isLoggedIn) {
    next('/')
  } else {
    next()
  }
})

export default router
