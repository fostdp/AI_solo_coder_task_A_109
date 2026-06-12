<template>
  <div class="animate-fade-in">
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h2 class="text-xl font-bold text-primary-dark">{{ sculpture?.name || '加载中...' }}</h2>
        <p class="text-sm text-primary/60 mt-1">{{ sculpture?.location }} · {{ sculpture?.era }}</p>
      </div>
      <router-link to="/" class="btn-secondary text-sm">← 返回列表</router-link>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-1">
        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">🧂</span>盐分热点图
          </h3>
          <SaltCanvas
            :hotspots="sculptureStore.heatmapData"
            :width="400"
            :height="500"
          />
        </div>
      </div>

      <div class="lg:col-span-2 space-y-6">
        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">📈</span>时序数据监测
          </h3>
          <LineChart
            :x-data="chartXData"
            :series="chartSeries"
            :y-axis="chartYAxis"
            height="300px"
          />
        </div>

        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">🔍</span>当前状态
          </h3>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div class="p-4 rounded-lg bg-primary-cream/50">
              <p class="text-sm text-primary/60">盐分覆盖率</p>
              <p class="text-2xl font-bold text-primary-dark mt-1">{{ (sculpture?.saltLevel * 100).toFixed(1) }}%</p>
              <span :class="`status-${sculpture?.status}`" class="mt-2 inline-block">
                {{ statusLabels[sculpture?.status || 'normal'] }}
              </span>
            </div>
            <div class="p-4 rounded-lg bg-primary-cream/50">
              <p class="text-sm text-primary/60">温度</p>
              <p class="text-2xl font-bold text-primary-dark mt-1">{{ sculpture?.temperature }}°C</p>
            </div>
            <div class="p-4 rounded-lg bg-primary-cream/50">
              <p class="text-sm text-primary/60">湿度</p>
              <p class="text-2xl font-bold text-primary-dark mt-1">{{ sculpture?.humidity }}%</p>
            </div>
            <div class="p-4 rounded-lg bg-primary-cream/50">
              <p class="text-sm text-primary/60">传感器数量</p>
              <p class="text-2xl font-bold text-primary-dark mt-1">{{ sculpture?.sensorIds.length || 0 }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import SaltCanvas from '@/components/SaltCanvas.vue'
import LineChart from '@/components/LineChart.vue'
import { useSculptureStore } from '@/stores/sculptureStore'

const route = useRoute()
const sculptureStore = useSculptureStore()

const sculpture = computed(() => sculptureStore.currentSculpture)

const statusLabels: Record<string, string> = {
  normal: '正常',
  warning: '预警',
  alert: '告警'
}

const chartXData = computed(() =>
  sculptureStore.timeSeriesData.map(d => {
    const date = new Date(d.timestamp)
    return `${date.getMonth() + 1}/${date.getDate()} ${date.getHours()}:00`
  })
)

const chartSeries = computed(() => [
  {
    name: 'Na⁺/K⁺ 浓度',
    data: sculptureStore.timeSeriesData.map(d => d.saltLevel),
    color: '#8B4513',
    yAxisIndex: 0
  },
  {
    name: 'Ca²⁺ 浓度',
    data: sculptureStore.timeSeriesData.map(d => d.saltLevel * 0.6),
    color: '#D2691E',
    yAxisIndex: 0
  },
  {
    name: '温度 (°C)',
    data: sculptureStore.timeSeriesData.map(d => d.temperature),
    color: '#DC143C',
    yAxisIndex: 1
  },
  {
    name: '湿度 (%)',
    data: sculptureStore.timeSeriesData.map(d => d.humidity),
    color: '#2E8B57',
    yAxisIndex: 1
  }
])

const chartYAxis = computed(() => [
  { name: '浓度', min: 0, max: 1, position: 'left' as const },
  { name: '温湿度', min: 0, max: 100, position: 'right' as const }
])

onMounted(async () => {
  const id = route.params.id as string
  await Promise.all([
    sculptureStore.fetchSculptureDetail(id),
    sculptureStore.fetchHeatmap(id),
    sculptureStore.fetchTimeSeries(id)
  ])
})
</script>
