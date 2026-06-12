<template>
  <div class="animate-fade-in">
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h2 class="text-xl font-bold text-primary-dark">盐分迁移分析</h2>
        <p class="text-sm text-primary/60 mt-1">基于Richards方程简化版的毛细上升盐分迁移模型</p>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-4 gap-6">
      <div class="lg:col-span-1">
        <div class="card space-y-5">
          <h3 class="font-medium text-primary-dark">
            <span class="mr-2">⚙️</span>模型参数
          </h3>

          <div>
            <label class="block text-sm text-primary/70 mb-2">选择泥塑</label>
            <select v-model="selectedSculptureId" class="w-full input">
              <option v-for="s in sculptures" :key="s.id" :value="s.id">
                {{ s.name }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-sm text-primary/70 mb-2">
              孔隙率: {{ params.porosity.toFixed(2) }}%
            </label>
            <input type="range" v-model.number="params.porosity" 
                   min="0.1" max="0.6" step="0.01" class="w-full" />
          </div>

          <div>
            <label class="block text-sm text-primary/70 mb-2">
              饱和度: {{ params.saturation.toFixed(2) }}%
            </label>
            <input type="range" v-model.number="params.saturation" 
                   min="0.1" max="1.0" step="0.01" class="w-full" />
          </div>

          <div>
            <label class="block text-sm text-primary/70 mb-2">
              环境温度: {{ params.temperature.toFixed(1) }}°C
            </label>
            <input type="range" v-model.number="params.temperature" 
                   min="-10" max="50" step="1" class="w-full" />
          </div>

          <div>
            <label class="block text-sm text-primary/70 mb-2">
              相对湿度: {{ params.humidity.toFixed(0) }}% RH
            </label>
            <input type="range" v-model.number="params.humidity" 
                   min="10" max="100" step="1" class="w-full" />
          </div>

          <div>
            <label class="block text-sm text-primary/70 mb-2">
              预测时长: {{ params.hours }} 小时
            </label>
            <input type="range" v-model.number="params.hours" 
                   min="1" max="720" step="1" class="w-full" />
          </div>

          <button @click="runSimulation" :disabled="loading" class="w-full btn-primary">
            <span v-if="loading">模拟中...</span>
            <span v-else>▶ 运行迁移模拟</span>
          </button>
        </div>
      </div>

      <div class="lg:col-span-3 space-y-6">
        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">📊</span>盐分浓度随深度变化曲线
          </h3>
          <LineChart
            :x-data="depthChartXData"
            :series="depthChartSeries"
            height="300px"
            :y-axis="[{ name: '浓度 (ppm)', min: 0 }]"
          />
          <p class="text-xs text-primary/50 mt-2 text-center">
            X轴: 深度 (cm) | 不同曲线代表不同时间点
          </p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="card">
            <h3 class="font-medium text-primary-dark mb-4">
              <span class="mr-2">⏱️</span>迁移过程动态
            </h3>
            <LineChart
              :x-data="timeChartXData"
              :series="timeChartSeries"
              height="250px"
              :y-axis="[{ name: '浓度 (ppm)', min: 0 }]"
            />
            <p class="text-xs text-primary/50 mt-2 text-center">
              X轴: 时间 (小时) | 不同曲线代表不同深度
            </p>
          </div>

          <div class="card">
            <h3 class="font-medium text-primary-dark mb-4">
              <span class="mr-2">🧮</span>Richards方程参数
            </h3>
            <div class="space-y-3 text-sm">
              <div class="flex justify-between items-center p-2 bg-primary-cream/30 rounded">
                <span class="text-primary/70">方程形式</span>
                <span class="font-mono font-medium text-primary-dark">
                  ∂(θc)/∂t = ∂/∂z [D(θ)∂c/∂z - qc]
                </span>
              </div>
              <div class="flex justify-between items-center p-2 bg-primary-cream/30 rounded">
                <span class="text-primary/70">水分扩散系数 D(θ)</span>
                <span class="font-medium text-primary-dark">{{ diffusionCoeff.toFixed(4) }} cm²/h</span>
              </div>
              <div class="flex justify-between items-center p-2 bg-primary-cream/30 rounded">
                <span class="text-primary/70">达西流速 q</span>
                <span class="font-medium text-primary-dark">{{ darcyVelocity.toFixed(6) }} cm/h</span>
              </div>
              <div class="flex justify-between items-center p-2 bg-primary-cream/30 rounded">
                <span class="text-primary/70">毛细上升高度</span>
                <span class="font-medium text-primary-dark">{{ capillaryHeight.toFixed(2) }} cm</span>
              </div>
              <div class="flex justify-between items-center p-2 bg-primary-cream/30 rounded">
                <span class="text-primary/70">预测最大浓度</span>
                <span class="font-medium text-primary-dark">{{ maxConcentration.toFixed(2) }} ppm</span>
              </div>
              <div class="flex justify-between items-center p-2 bg-primary-cream/30 rounded">
                <span class="text-primary/70">盐害风险等级</span>
                <span :class="`status-${riskLevel}`" class="font-medium">
                  {{ riskLabels[riskLevel] }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">📈</span>盐分迁移热力图
          </h3>
          <div class="relative" style="height: 300px;">
            <canvas ref="heatmapCanvas" class="w-full h-full"></canvas>
          </div>
          <div class="flex justify-between mt-2 px-4">
            <span class="text-xs text-primary/50">时间 →</span>
            <span class="text-xs text-primary/50">深度 ↓</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import LineChart from '@/components/LineChart.vue'
import { useSculptureStore } from '@/stores/sculptureStore'
import { api } from '@/api'
import type { Sculpture } from '@/types'

const sculptureStore = useSculptureStore()
const sculptures = ref<Sculpture[]>([])
const selectedSculptureId = ref(1)
const loading = ref(false)
const heatmapCanvas = ref<HTMLCanvasElement>()

const params = ref({
  porosity: 0.35,
  saturation: 0.6,
  temperature: 20,
  humidity: 65,
  hours: 72
})

const simulationResult = ref<any>(null)

const diffusionCoeff = computed(() => 
  0.001 * Math.exp(-3.5 * (1 - params.value.saturation)) * params.value.porosity
)

const darcyVelocity = computed(() => 
  0.0001 * params.value.saturation * (params.value.humidity / 100)
)

const capillaryHeight = computed(() => 
  (14 / (params.value.porosity * 1000000)) / 0.01
)

const maxConcentration = computed(() => {
  if (!simulationResult.value) return 0
  return Math.max(...simulationResult.value.flatMap((p: any) => p.concentration))
})

const riskLevel = computed(() => {
  const maxC = maxConcentration.value
  if (maxC > 500) return 'alert'
  if (maxC > 300) return 'warning'
  return 'normal'
})

const riskLabels: Record<string, string> = {
  normal: '低风险',
  warning: '中风险',
  alert: '高风险'
}

const depthChartXData = computed(() => {
  if (!simulationResult.value) return []
  return [...new Set(simulationResult.value.map((p: any) => p.depthCm))]
})

const depthChartSeries = computed(() => {
  if (!simulationResult.value) return []
  const timePoints = [...new Set(simulationResult.value.map((p: any) => p.timeHour))]
  const colors = ['#8B4513', '#D2691E', '#CD853F', '#DEB887', '#F5DEB3']
  
  return timePoints.filter((_, i) => i % Math.max(1, Math.floor(timePoints.length / 5)) === 0)
    .map((t, i) => ({
      name: `${t}h`,
      data: depthChartXData.value.map(d => {
        const point = simulationResult.value.find((p: any) => p.timeHour === t && p.depthCm === d)
        return point?.concentration || 0
      }),
      color: colors[i % colors.length],
      type: 'line'
    }))
})

const timeChartXData = computed(() => {
  if (!simulationResult.value) return []
  return [...new Set(simulationResult.value.map((p: any) => p.timeHour))]
})

const timeChartSeries = computed(() => {
  if (!simulationResult.value) return []
  const depths = [...new Set(simulationResult.value.map((p: any) => p.depthCm))]
  const colors = ['#DC143C', '#FF8C00', '#DAA520', '#2E8B57', '#4682B4']
  
  return depths.filter((_, i) => i % Math.max(1, Math.floor(depths.length / 5)) === 0)
    .map((d, i) => ({
      name: `${d}cm`,
      data: timeChartXData.value.map(t => {
        const point = simulationResult.value.find((p: any) => p.timeHour === t && p.depthCm === d)
        return point?.concentration || 0
      }),
      color: colors[i % colors.length],
      type: 'line'
    }))
})

const runSimulation = async () => {
  loading.value = true
  try {
    const data: any = await api.get('/migration/simulate/' + selectedSculptureId.value, {
      params: {
        porosity: params.value.porosity,
        saturation: params.value.saturation,
        temperature: params.value.temperature,
        humidity: params.value.humidity,
        hours: params.value.hours
      }
    })
    simulationResult.value = data.points
    await nextTick()
    drawHeatmap()
  } catch (e) {
    console.error('Simulation failed:', e)
    const mockPoints: any[] = []
    for (let h = 0; h <= params.value.hours; h += 3) {
      for (let d = 0; d <= 50; d += 2) {
        const baseC = 1000 * Math.exp(-d / 10) * (1 - Math.exp(-h / 24))
        const variation = 0.9 + Math.random() * 0.2
        mockPoints.push({
          timeHour: h,
          depthCm: d,
          concentration: baseC * variation
        })
      }
    }
    simulationResult.value = mockPoints
    await nextTick()
    drawHeatmap()
  } finally {
    loading.value = false
  }
}

const drawHeatmap = () => {
  const canvas = heatmapCanvas.value
  if (!canvas || !simulationResult.value) return
  
  const ctx = canvas.getContext('2d')!
  const w = canvas.width = canvas.offsetWidth * window.devicePixelRatio
  const h = canvas.height = canvas.offsetHeight * window.devicePixelRatio
  ctx.scale(window.devicePixelRatio, window.devicePixelRatio)

  const timePoints = [...new Set(simulationResult.value.map((p: any) => p.timeHour))]
  const depths = [...new Set(simulationResult.value.map((p: any) => p.depthCm))]
  
  const cellW = canvas.offsetWidth / timePoints.length
  const cellH = canvas.offsetHeight / depths.length

  const maxC = maxConcentration.value

  timePoints.forEach((t, ti) => {
    depths.forEach((d, di) => {
      const point = simulationResult.value.find((p: any) => p.timeHour === t && p.depthCm === d)
      if (point) {
        const intensity = Math.min(point.concentration / maxC, 1)
        const r = Math.floor(255 * intensity)
        const g = Math.floor(140 * (1 - intensity))
        const b = Math.floor(50 * (1 - intensity))
        ctx.fillStyle = `rgb(${r},${g},${b})`
        ctx.fillRect(ti * cellW, di * cellH, cellW + 1, cellH + 1)
      }
    })
  })
}

onMounted(async () => {
  await sculptureStore.fetchSculptures()
  sculptures.value = sculptureStore.sculptures
  await runSimulation()
  window.addEventListener('resize', drawHeatmap)
})

watch(selectedSculptureId, () => runSimulation())
watch(() => params.value, () => runSimulation(), { deep: true })
</script>
