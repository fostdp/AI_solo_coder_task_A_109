<template>
  <header class="bg-white border-b border-primary/20 px-6 py-4 flex items-center justify-between">
    <div>
      <h2 class="text-lg font-semibold text-primary-dark">{{ pageTitle }}</h2>
      <p class="text-sm text-primary/60 mt-0.5">{{ pageSubtitle }}</p>
    </div>
    <div class="flex items-center space-x-4">
      <div class="relative">
        <button 
          class="relative p-2 rounded-lg hover:bg-primary-cream transition-colors"
          @click="showAlerts = !showAlerts"
        >
          <span class="text-xl">🔔</span>
          <span 
            v-if="unresolvedCount > 0"
            class="absolute -top-1 -right-1 w-5 h-5 bg-status-alert text-white text-xs rounded-full flex items-center justify-center"
          >
            {{ unresolvedCount > 9 ? '9+' : unresolvedCount }}
          </span>
        </button>
        <div 
          v-if="showAlerts"
          class="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg border border-primary/20 py-2 z-50"
        >
          <div class="px-4 py-2 border-b border-primary/10">
            <h3 class="font-medium text-primary-dark">最新告警</h3>
          </div>
          <div class="max-h-64 overflow-y-auto">
            <div 
              v-for="alert in recentAlerts" 
              :key="alert.id"
              class="px-4 py-3 hover:bg-primary-cream/50 cursor-pointer border-b border-primary/5 last:border-b-0"
              @click="goToAlert(alert)"
            >
              <div class="flex items-center justify-between">
                <span :class="`status-${alert.level}`">{{ alertTypeLabels[alert.type] }}</span>
                <span class="text-xs text-primary/50">{{ formatTime(alert.timestamp) }}</span>
              </div>
              <p class="text-sm text-primary-dark mt-1">{{ alert.message }}</p>
            </div>
            <div v-if="recentAlerts.length === 0" class="px-4 py-8 text-center text-primary/50">
              暂无告警
            </div>
          </div>
        </div>
      </div>
      <div class="flex items-center space-x-3">
        <div class="text-right">
          <p class="text-sm font-medium text-primary-dark">{{ user?.username || '管理员' }}</p>
          <p class="text-xs text-primary/50">{{ user?.role === 'admin' ? '系统管理员' : '普通用户' }}</p>
        </div>
        <div class="w-10 h-10 bg-primary-pale rounded-full flex items-center justify-center text-primary-dark font-medium">
          {{ (user?.username || '管').charAt(0) }}
        </div>
        <button 
          class="ml-2 px-3 py-1.5 text-sm text-primary/70 hover:text-primary-dark hover:bg-primary-cream rounded-md transition-colors"
          @click="handleLogout"
        >
          退出
        </button>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useAlertStore } from '@/stores/alertStore'
import type { Alert } from '@/types'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const alertStore = useAlertStore()

const showAlerts = ref(false)

const user = computed(() => authStore.user)

const unresolvedCount = computed(() => alertStore.unresolvedAlerts.length)

const recentAlerts = computed(() => alertStore.unresolvedAlerts.slice(0, 5))

const alertTypeLabels: Record<string, string> = {
  salt: '盐分超标',
  temperature: '温度异常',
  humidity: '湿度异常',
  sensor_offline: '传感器离线'
}

const pageTitles: Record<string, { title: string; subtitle: string }> = {
  '/': { title: '监测总览', subtitle: '实时监控所有泥塑状态' },
  '/analysis/migration': { title: '盐分迁移分析', subtitle: 'Richards方程模拟分析' },
  '/analysis/materials': { title: '材料适配分析', subtitle: '修复材料多维度评估' },
  '/alerts': { title: '告警管理', subtitle: '告警记录与阈值配置' },
  '/sensors': { title: '传感器管理', subtitle: '传感器状态与位置管理' },
  '/settings': { title: '系统设置', subtitle: '参数配置与系统管理' }
}

const pageTitle = computed(() => {
  if (route.path.startsWith('/sculpture/')) {
    return '泥塑详情'
  }
  const key = Object.keys(pageTitles).find(k => 
    k === '/' ? route.path === '/' : route.path.startsWith(k)
  )
  return pageTitles[key || '/']?.title || '监测总览'
})

const pageSubtitle = computed(() => {
  if (route.path.startsWith('/sculpture/')) {
    return '详细监测数据与分析'
  }
  const key = Object.keys(pageTitles).find(k => 
    k === '/' ? route.path === '/' : route.path.startsWith(k)
  )
  return pageTitles[key || '/']?.subtitle || ''
})

function formatTime(timestamp: string): string {
  const date = new Date(timestamp)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  const minutes = Math.floor(diff / 60000)
  if (minutes < 60) return `${minutes}分钟前`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}小时前`
  return `${Math.floor(hours / 24)}天前`
}

function goToAlert(alert: Alert) {
  showAlerts.value = false
  router.push(`/sculpture/${alert.sculptureId}`)
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>
