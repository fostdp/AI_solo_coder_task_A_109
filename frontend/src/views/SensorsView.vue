<template>
  <div class="animate-fade-in">
    <div class="mb-6 flex items-center justify-between flex-wrap gap-4">
      <div>
        <h2 class="text-xl font-bold text-primary-dark">传感器管理</h2>
        <p class="text-sm text-primary/60 mt-1">70台传感器实时状态监控与数据管理</p>
      </div>
      <button @click="activeTab = 'layout'" class="btn-secondary">
        🗺️ 查看布局
      </button>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-6 gap-4 mb-6">
      <div class="card p-4 text-center">
        <p class="text-sm text-primary/60">总传感器</p>
        <p class="text-2xl font-bold text-primary-dark">{{ sensorSummary.totalSensors }}</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-sm text-primary/60">离子传感器</p>
        <p class="text-2xl font-bold text-primary-dark">{{ sensorSummary.ionSensors }}</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-sm text-primary/60">环境传感器</p>
        <p class="text-2xl font-bold text-primary-dark">{{ sensorSummary.environmentSensors }}</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-sm text-primary/60">在线</p>
        <p class="text-2xl font-bold text-success">{{ sensorSummary.online }}</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-sm text-primary/60">离线</p>
        <p class="text-2xl font-bold text-danger">{{ sensorSummary.offline }}</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-sm text-primary/60">活跃</p>
        <p class="text-2xl font-bold text-primary">{{ sensorSummary.active }}</p>
      </div>
    </div>

    <div class="border-b border-primary/10 mb-6">
      <div class="flex gap-1">
        <button v-for="tab in tabs" :key="tab.key"
                @click="activeTab = tab.key"
                class="px-4 py-2 font-medium transition-colors"
                :class="{
                  'border-b-2 border-primary text-primary-dark': activeTab === tab.key,
                  'text-primary/50 hover:text-primary': activeTab !== tab.key
                }">
          {{ tab.icon }} {{ tab.label }}
        </button>
      </div>
    </div>

    <div v-show="activeTab === 'list'" class="card">
      <div class="flex flex-wrap gap-4 mb-4">
        <select v-model="filterType" class="input w-40">
          <option value="">全部类型</option>
          <option value="ion">离子传感器</option>
          <option value="environment">环境传感器</option>
        </select>
        <select v-model="filterStatus" class="input w-40">
          <option value="">全部状态</option>
          <option value="Active">活跃</option>
          <option value="Inactive">停用</option>
        </select>
        <select v-model="filterSculpture" class="input w-48">
          <option value="">全部泥塑</option>
          <option v-for="s in sculptures" :key="s.id" :value="s.id">
            #{{ s.id }} {{ s.name }}
          </option>
        </select>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-primary/10">
              <th class="text-left py-3 px-2 text-primary/70 font-medium">设备编号</th>
              <th class="text-left py-3 px-2 text-primary/70 font-medium">类型</th>
              <th class="text-left py-3 px-2 text-primary/70 font-medium">型号</th>
              <th class="text-left py-3 px-2 text-primary/70 font-medium">安装位置</th>
              <th class="text-left py-3 px-2 text-primary/70 font-medium">关联泥塑</th>
              <th class="text-center py-3 px-2 text-primary/70 font-medium">状态</th>
              <th class="text-center py-3 px-2 text-primary/70 font-medium">最后上报</th>
              <th class="text-center py-3 px-2 text-primary/70 font-medium">操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="sensor in filteredSensors" :key="sensor.id" 
                class="border-b border-primary/5 hover:bg-primary-cream/30 transition-colors">
              <td class="py-3 px-2 font-mono font-medium text-primary-dark">{{ sensor.sensorCode }}</td>
              <td class="py-3 px-2">
                <span v-if="sensor.sensorType === 'ion'" class="px-2 py-1 rounded-full bg-primary/10 text-primary text-xs">
                  ⚡ 离子
                </span>
                <span v-else class="px-2 py-1 rounded-full bg-blue-100 text-blue-700 text-xs">
                  🌡️ 环境
                </span>
              </td>
              <td class="py-3 px-2 text-primary/70">{{ sensor.model }}</td>
              <td class="py-3 px-2 text-primary/70">{{ sensor.installLocation }}</td>
              <td class="py-3 px-2">
                <router-link :to="`/sculpture/${sensor.sculptureId}`" 
                             class="text-primary hover:text-primary-dark underline">
                  {{ getSculptureName(sensor.sculptureId) }}
                </router-link>
              </td>
              <td class="py-3 px-2 text-center">
                <span :class="sensor.status === 'Active' ? 'text-success' : 'text-danger'"
                      class="font-medium">
                  {{ sensor.status === 'Active' ? '● 在线' : '● 离线' }}
                </span>
              </td>
              <td class="py-3 px-2 text-center text-primary/60 whitespace-nowrap">
                {{ sensor.lastCalibration ? formatTime(sensor.lastCalibration) : '-' }}
              </td>
              <td class="py-3 px-2 text-center">
                <button @click="viewLogs(sensor.sensorCode)" 
                        class="text-xs px-2 py-1 rounded bg-primary/10 hover:bg-primary/20 text-primary transition-colors">
                  查看日志
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-show="activeTab === 'layout'" class="card">
      <h3 class="font-medium text-primary-dark mb-4">
        <span class="mr-2">🗺️</span>传感器平面布局图
      </h3>
      <div class="relative bg-primary-cream/10 rounded-lg" style="height: 500px;">
        <svg class="w-full h-full">
          <defs>
            <pattern id="grid" width="50" height="50" patternUnits="userSpaceOnUse">
              <path d="M 50 0 L 0 0 0 50" fill="none" stroke="rgba(139,69,19,0.1)" stroke-width="1"/>
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#grid)"/>
          
          <g v-for="s in sculptures" :key="'sc-'+s.id">
            <rect 
              :x="s.xPosition + '%'" 
              :y="s.yPosition + '%'" 
              width="60" height="80" 
              rx="4"
              :fill="getStatusColor(s.status)"
              opacity="0.8"
              stroke="#8B4513"
              stroke-width="1"
            />
            <text 
              :x="(s.xPosition + 3) + '%'" 
              :y="(s.yPosition + 4) + '%'" 
              font-size="10"
              fill="#5D4037"
              font-weight="bold"
            >#{{ s.id }}</text>
            <text 
              :x="(s.xPosition + 1) + '%'" 
              :y="(s.yPosition + 8) + '%'" 
              font-size="9"
              fill="#8B4513"
            >{{ s.name.substring(0, 4) }}</text>
          </g>

          <g v-for="sensor in filteredSensors" :key="'sensor-'+sensor.id">
            <circle 
              :cx="getSensorX(sensor) + '%'" 
              :cy="getSensorY(sensor) + '%'" 
              r="5"
              :fill="sensor.status === 'Active' ? '#2E8B57' : '#DC143C'"
              stroke="white"
              stroke-width="1.5"
            >
              <title>{{ sensor.sensorCode }} - {{ sensor.model }}</title>
            </circle>
          </g>
        </svg>
        
        <div class="absolute bottom-2 right-2 bg-white/90 p-2 rounded text-xs space-y-1">
          <div class="flex items-center gap-2"><span class="w-3 h-3 rounded-full bg-success"></span>正常</div>
          <div class="flex items-center gap-2"><span class="w-3 h-3 rounded-full bg-warning"></span>预警</div>
          <div class="flex items-center gap-2"><span class="w-3 h-3 rounded-full bg-danger"></span>告警</div>
        </div>
      </div>
    </div>

    <div v-show="activeTab === 'logs'" class="card">
      <div class="mb-4 flex gap-4 items-center">
        <label class="text-sm text-primary/70">选择传感器:</label>
        <select v-model="selectedSensorCode" class="input w-64">
          <option v-for="s in filteredSensors" :key="s.id" :value="s.sensorCode">
            {{ s.sensorCode }} - {{ s.model }}
          </option>
        </select>
        <button @click="fetchLogs" class="btn-primary">查询日志</button>
      </div>

      <div v-if="logData" class="space-y-4">
        <div class="p-4 bg-primary-cream/30 rounded-lg">
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <p class="text-xs text-primary/50">总记录数</p>
              <p class="text-xl font-bold text-primary-dark">{{ logData.stats?.totalRecords || 0 }}</p>
            </div>
            <div>
              <p class="text-xs text-primary/50">平均温度</p>
              <p class="text-xl font-bold text-primary-dark">{{ logData.stats?.avgTemperature || 0 }}°C</p>
            </div>
            <div>
              <p class="text-xs text-primary/50">平均湿度</p>
              <p class="text-xl font-bold text-primary-dark">{{ logData.stats?.avgHumidity || 0 }}%</p>
            </div>
            <div>
              <p class="text-xs text-primary/50">丢包率</p>
              <p class="text-xl font-bold" :class="logData.stats?.packetLoss > 5 ? 'text-danger' : 'text-success'">
                {{ logData.stats?.packetLoss || 0 }}%
              </p>
            </div>
          </div>
        </div>

        <div class="overflow-x-auto max-h-96 overflow-y-auto">
          <table class="w-full text-xs">
            <thead class="sticky top-0 bg-white">
              <tr class="border-b border-primary/10">
                <th class="text-left py-2 px-1 text-primary/70 font-medium">时间</th>
                <th class="text-center py-2 px-1 text-primary/70 font-medium">温度</th>
                <th class="text-center py-2 px-1 text-primary/70 font-medium">湿度</th>
                <th class="text-center py-2 px-1 text-primary/70 font-medium">Na⁺</th>
                <th class="text-center py-2 px-1 text-primary/70 font-medium">K⁺</th>
                <th class="text-center py-2 px-1 text-primary/70 font-medium">Ca²⁺</th>
                <th class="text-center py-2 px-1 text-primary/70 font-medium">盐浓度</th>
                <th class="text-center py-2 px-1 text-primary/70 font-medium">覆盖率</th>
                <th class="text-center py-2 px-1 text-primary/70 font-medium">信号</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(log, i) in logData.logs" :key="i" class="border-b border-primary/5">
                <td class="py-2 px-1 text-primary/60 whitespace-nowrap">{{ formatTime(log.timestamp) }}</td>
                <td class="py-2 px-1 text-center">{{ log.temperature || '-' }}</td>
                <td class="py-2 px-1 text-center">{{ log.humidity || '-' }}</td>
                <td class="py-2 px-1 text-center" :class="log.sodiumIon > 500 ? 'text-danger font-medium' : ''">
                  {{ log.sodiumIon || '-' }}
                </td>
                <td class="py-2 px-1 text-center">{{ log.potassiumIon || '-' }}</td>
                <td class="py-2 px-1 text-center">{{ log.calciumIon || '-' }}</td>
                <td class="py-2 px-1 text-center">{{ log.saltConcentration || '-' }}</td>
                <td class="py-2 px-1 text-center" :class="log.crystalCoverage > 30 ? 'text-danger font-medium' : ''">
                  {{ log.crystalCoverage ? log.crystalCoverage + '%' : '-' }}
                </td>
                <td class="py-2 px-1 text-center">{{ log.signalStrength || '-' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-if="selectedSensorCode && activeTab === 'logs'" class="mt-6">
      <div class="card">
        <h3 class="font-medium text-primary-dark mb-4">
          <span class="mr-2">📈</span>数据趋势图
        </h3>
        <LineChart
          v-if="logData && logData.logs.length > 0"
          :x-data="logChartXData"
          :series="logChartSeries"
          height="250px"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import LineChart from '@/components/LineChart.vue'
import { useSculptureStore } from '@/stores/sculptureStore'
import { api } from '@/api'
import type { Sculpture } from '@/types'

const sculptureStore = useSculptureStore()
const sculptures = ref<Sculpture[]>([])
const sensors = ref<any[]>([])
const sensorSummary = ref({
  totalSensors: 70,
  ionSensors: 40,
  environmentSensors: 30,
  active: 68,
  inactive: 2,
  online: 67,
  offline: 3
})

const activeTab = ref('list')
const filterType = ref('')
const filterStatus = ref('')
const filterSculpture = ref<number | ''>('')
const selectedSensorCode = ref('')
const logData = ref<any>(null)

const tabs = [
  { key: 'list', label: '传感器列表', icon: '📋' },
  { key: 'layout', label: '布局图', icon: '🗺️' },
  { key: 'logs', label: '数据日志', icon: '📊' }
]

const filteredSensors = computed(() => {
  let result = [...sensors.value]
  if (filterType.value) {
    result = result.filter(s => s.sensorType === filterType.value)
  }
  if (filterStatus.value) {
    result = result.filter(s => s.status === filterStatus.value)
  }
  if (filterSculpture.value !== '') {
    result = result.filter(s => s.sculptureId === filterSculpture.value)
  }
  return result
})

const logChartXData = computed(() => {
  if (!logData.value?.logs) return []
  return logData.value.logs.map((l: any) => 
    new Date(l.timestamp).toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
  )
})

const logChartSeries = computed(() => {
  if (!logData.value?.logs || logData.value.logs.length === 0) return []
  const firstLog = logData.value.logs[0]
  const series: any[] = []
  
  if (firstLog.temperature !== undefined && firstLog.temperature !== null) {
    series.push({
      name: '温度(°C)',
      data: logData.value.logs.map((l: any) => l.temperature),
      type: 'line',
      smooth: true,
      yAxisIndex: 0,
      lineStyle: { color: '#DC143C' },
      itemStyle: { color: '#DC143C' }
    })
  }
  if (firstLog.humidity !== undefined && firstLog.humidity !== null) {
    series.push({
      name: '湿度(%)',
      data: logData.value.logs.map((l: any) => l.humidity),
      type: 'line',
      smooth: true,
      yAxisIndex: 0,
      lineStyle: { color: '#1E90FF' },
      itemStyle: { color: '#1E90FF' }
    })
  }
  if (firstLog.sodiumIon !== undefined && firstLog.sodiumIon !== null) {
    series.push({
      name: 'Na⁺(ppm)',
      data: logData.value.logs.map((l: any) => l.sodiumIon),
      type: 'line',
      smooth: true,
      yAxisIndex: 1,
      lineStyle: { color: '#FF8C00' },
      itemStyle: { color: '#FF8C00' }
    })
  }
  if (firstLog.saltConcentration !== undefined && firstLog.saltConcentration !== null) {
    series.push({
      name: '总盐(%)',
      data: logData.value.logs.map((l: any) => l.saltConcentration),
      type: 'line',
      smooth: true,
      yAxisIndex: 1,
      lineStyle: { color: '#2E8B57' },
      itemStyle: { color: '#2E8B57' }
    })
  }
  return series
})

function getSculptureName(id: number): string {
  const s = sculptures.value.find(sc => sc.id === id)
  return s ? s.name : `#${id}`
}

function getStatusColor(status: string): string {
  switch (status) {
    case 'Normal': return '#90EE90'
    case 'Warning': return '#FFD700'
    case 'Alert': return '#FF6347'
    default: return '#D2B48C'
  }
}

function getSensorX(sensor: any): number {
  const sculp = sculptures.value.find(s => s.id === sensor.sculptureId)
  if (!sculp) return 50
  const offsetX = sensor.sensorType === 'ion' ? -15 : 75
  return sculp.xPosition + offsetX + (sensor.id % 3) * 10
}

function getSensorY(sensor: any): number {
  const sculp = sculptures.value.find(s => s.id === sensor.sculptureId)
  if (!sculp) return 50
  const offsetY = sensor.sensorType === 'ion' ? 15 : 50
  return sculp.yPosition + offsetY + (sensor.id % 2) * 20
}

function formatTime(timeStr: string): string {
  if (!timeStr) return '-'
  return new Date(timeStr).toLocaleString('zh-CN', {
    month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit'
  })
}

function viewLogs(sensorCode: string) {
  selectedSensorCode.value = sensorCode
  activeTab.value = 'logs'
  fetchLogs()
}

async function fetchLogs() {
  if (!selectedSensorCode.value) return
  try {
    const result = await api.getSensorDataLogs(selectedSensorCode.value, 100)
    logData.value = result.data || mockLogData()
  } catch (e) {
    logData.value = mockLogData()
  }
}

function mockLogData() {
  const logs: any[] = []
  const now = new Date()
  const isIon = selectedSensorCode.value.startsWith('ION')
  
  for (let i = 50; i >= 0; i--) {
    const t = new Date(now.getTime() - i * 45 * 60 * 1000)
    const log: any = { timestamp: t.toISOString(), signalStrength: -50 + Math.random() * 30 }
    if (isIon) {
      log.sodiumIon = Math.round(300 + Math.random() * 300)
      log.potassiumIon = Math.round(50 + Math.random() * 80)
      log.calciumIon = Math.round(100 + Math.random() * 150)
      log.saltConcentration = (0.5 + Math.random() * 1.5).toFixed(2)
      log.crystalCoverage = Math.round(15 + Math.random() * 25)
    } else {
      log.temperature = (18 + Math.random() * 10).toFixed(1)
      log.humidity = (40 + Math.random() * 30).toFixed(1)
    }
    logs.push(log)
  }
  
  const tempLogs = logs.filter(l => l.temperature !== undefined)
  const humLogs = logs.filter(l => l.humidity !== undefined)
  
  return {
    logs,
    stats: {
      totalRecords: logs.length,
      avgTemperature: tempLogs.length ? (tempLogs.reduce((s, l) => s + parseFloat(l.temperature), 0) / tempLogs.length).toFixed(1) : 0,
      avgHumidity: humLogs.length ? (humLogs.reduce((s, l) => s + parseFloat(l.humidity), 0) / humLogs.length).toFixed(1) : 0,
      packetLoss: (Math.random() * 3).toFixed(1)
    }
  }
}

async function loadSensors() {
  try {
    const result = await api.getSensors()
    sensors.value = result.data || mockSensors()
  } catch (e) {
    sensors.value = mockSensors()
  }
}

function mockSensors(): any[] {
  const arr: any[] = []
  for (let i = 1; i <= 40; i++) {
    arr.push({
      id: i,
      sensorCode: `ION-${String(i).padStart(3, '0')}`,
      sensorType: 'ion',
      model: `ION-SENSOR-${100 + i}`,
      installLocation: ['底座左侧', '底座右侧', '腰部', '肩部', '头部'][i % 5],
      sculptureId: ((i - 1) % 30) + 1,
      status: i % 20 === 0 ? 'Inactive' : 'Active',
      lastCalibration: new Date(Date.now() - i * 3600000).toISOString()
    })
  }
  for (let i = 1; i <= 30; i++) {
    arr.push({
      id: 40 + i,
      sensorCode: `ENV-${String(i).padStart(3, '0')}`,
      sensorType: 'environment',
      model: `ENV-SENSOR-${200 + i}`,
      installLocation: ['近地面', '中部', '顶部'][i % 3],
      sculptureId: i,
      status: i % 15 === 0 ? 'Inactive' : 'Active',
      lastCalibration: new Date(Date.now() - (i + 40) * 3600000).toISOString()
    })
  }
  return arr
}

function loadSculptures() {
  if (sculptureStore.sculptures.length > 0) {
    sculptures.value = sculptureStore.sculptures
  } else {
    const mock: Sculpture[] = []
    for (let i = 1; i <= 30; i++) {
      const row = Math.floor((i - 1) / 10)
      const col = (i - 1) % 10
      mock.push({
        id: i,
        name: `泥塑${['一','二','三','四','五','六','七','八','九','十'][col]}${['','二','三'][row]}`,
        status: i % 7 === 0 ? 'Alert' : i % 5 === 0 ? 'Warning' : 'Normal',
        xPosition: 5 + col * 9,
        yPosition: 5 + row * 30,
        location: '',
        era: '',
        material: '',
        dimensions: '',
        description: ''
      })
    }
    sculptures.value = mock
  }
}

onMounted(() => {
  loadSculptures()
  loadSensors()
})

watch(selectedSensorCode, () => {
  if (selectedSensorCode.value && activeTab.value === 'logs') {
    fetchLogs()
  }
})
</script>
