<template>
  <div class="animate-fade-in">
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
      <div v-for="stat in statsCards" :key="stat.label" class="card">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-primary/60">{{ stat.label }}</p>
            <p class="text-3xl font-bold text-primary-dark mt-1 number-scroll">{{ stat.value }}</p>
          </div>
          <div 
            class="w-14 h-14 rounded-lg flex items-center justify-center text-2xl"
            :class="stat.bgClass"
          >
            {{ stat.icon }}
          </div>
        </div>
        <div class="mt-3 flex items-center text-xs">
          <span :class="stat.trendClass">
            {{ stat.trend > 0 ? '↑' : stat.trend < 0 ? '↓' : '→' }}
            {{ Math.abs(stat.trend) }}%
          </span>
          <span class="text-primary/50 ml-2">较昨日</span>
        </div>
      </div>
    </div>

    <div class="card mb-6 p-0 overflow-hidden">
      <div class="px-4 py-3 border-b border-primary/10 flex items-center justify-between bg-primary-cream/30">
        <h3 class="font-medium text-primary-dark">
          <span class="mr-2">🚨</span>实时告警滚动
        </h3>
        <router-link to="/alerts" class="text-sm text-primary hover:text-primary-dark">
          查看全部 →
        </router-link>
      </div>
      <div class="overflow-hidden h-32 relative">
        <div 
          ref="scrollContainer"
          class="absolute w-full"
          :style="{ transform: `translateY(${-scrollOffset}px)` }"
        >
          <div 
            v-for="alert in scrollAlerts" 
            :key="alert.id"
            class="px-4 py-3 border-b border-primary/5 flex items-center justify-between hover:bg-primary-cream/30 transition-colors"
          >
            <div class="flex items-center">
              <span :class="`status-${alert.level} mr-3`">
                {{ alertLevelLabels[alert.level] }}
              </span>
              <span class="text-primary-dark">{{ alert.message }}</span>
            </div>
            <span class="text-xs text-primary/50">{{ formatTime(alert.timestamp) }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="card p-0 overflow-hidden">
      <div class="px-4 py-3 border-b border-primary/10 bg-primary-cream/30">
        <h3 class="font-medium text-primary-dark">
          <span class="mr-2">🗿</span>泥塑监测列表
        </h3>
      </div>
      <div class="p-4">
        <div class="grid grid-cols-2 md:grid-cols-5 lg:grid-cols-10 gap-3">
          <SculptureCard 
            v-for="sculpture in sculptures" 
            :key="sculpture.id" 
            :sculpture="sculpture" 
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import SculptureCard from '@/components/SculptureCard.vue'
import { useSculptureStore } from '@/stores/sculptureStore'
import { useAlertStore } from '@/stores/alertStore'

const sculptureStore = useSculptureStore()
const alertStore = useAlertStore()

const scrollContainer = ref<HTMLDivElement | null>(null)
const scrollOffset = ref(0)
let scrollTimer: number | null = null

const sculptures = computed(() => sculptureStore.sculptures)

const statsCards = computed(() => [
  {
    label: '泥塑总数',
    value: sculptureStore.dashboardStats.totalSculptures,
    icon: '🗿',
    bgClass: 'bg-primary-pale/30',
    trend: 0
  },
  {
    label: '正常状态',
    value: sculptureStore.dashboardStats.normalCount,
    icon: '✅',
    bgClass: 'bg-status-normal/10',
    trend: -2
  },
  {
    label: '预警状态',
    value: sculptureStore.dashboardStats.warningCount,
    icon: '⚠️',
    bgClass: 'bg-status-warning/10',
    trend: 1
  },
  {
    label: '告警状态',
    value: sculptureStore.dashboardStats.alertCount,
    icon: '🚨',
    bgClass: 'bg-status-alert/10',
    trend: 0
  }
])

const scrollAlerts = computed(() => [
  ...alertStore.unresolvedAlerts,
  ...alertStore.unresolvedAlerts
])

const alertLevelLabels: Record<string, string> = {
  normal: '正常',
  warning: '预警',
  alert: '告警'
}

function formatTime(timestamp: string): string {
  const date = new Date(timestamp)
  return date.toLocaleString('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function startScroll() {
  scrollTimer = window.setInterval(() => {
    scrollOffset.value += 1
    const itemHeight = 52
    const maxOffset = alertStore.unresolvedAlerts.length * itemHeight
    if (scrollOffset.value >= maxOffset) {
      scrollOffset.value = 0
    }
  }, 60)
}

function stopScroll() {
  if (scrollTimer) {
    clearInterval(scrollTimer)
    scrollTimer = null
  }
}

onMounted(async () => {
  await sculptureStore.loadDashboardStats()
  await sculptureStore.loadSculptures()
  await alertStore.loadRecentAlerts()
  if (alertStore.unresolvedAlerts.length > 0) {
    startScroll()
  }
})

onUnmounted(() => {
  stopScroll()
})
</script>