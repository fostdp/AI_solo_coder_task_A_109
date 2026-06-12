<template>
  <div class="animate-fade-in">
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h2 class="text-xl font-bold text-primary-dark">加固材料适配分析</h2>
        <p class="text-sm text-primary/60 mt-1">基于接触角、渗透深度、强度匹配的综合评分系统</p>
      </div>
      <button @click="calculateScores" :disabled="loading" class="btn-primary">
        <span v-if="loading">计算中...</span>
        <span v-else>重新计算评分</span>
      </button>
    </div>

    <div class="mb-4">
      <label class="block text-sm text-primary/70 mb-2">选择泥塑</label>
      <select v-model="selectedSculptureId" class="w-full max-w-md input" @change="fetchScores">
        <option v-for="s in sculptures" :key="s.id" :value="s.id">
          #{{ s.id }} {{ s.name }} - {{ s.location }}
        </option>
      </select>
    </div>

    <div v-if="scores.length > 0" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-1">
        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">⚖️</span>六维雷达图对比
          </h3>
          <RadarCanvas
            :dimensions="dimLabels"
            :series="radarCanvasSeries"
            height="350px"
          />
        </div>

        <div class="card mt-6">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">📋</span>评分权重说明
          </h3>
          <div class="space-y-3 text-sm">
            <div v-for="w in weights" :key="w.name" 
                 class="flex items-center justify-between p-2 bg-primary-cream/30 rounded">
              <span class="text-primary/70">{{ w.label }}</span>
              <span class="font-medium text-primary-dark">{{ (w.weight * 100).toFixed(0) }}%</span>
            </div>
          </div>
        </div>
      </div>

      <div class="lg:col-span-2 space-y-6">
        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">🏆</span>材料综合评分排名
          </h3>
          <div class="space-y-4">
            <div v-for="(score, index) in sortedScores" :key="score.materialId" 
                 class="p-4 rounded-lg border transition-all hover:shadow-md"
                 :class="{
                   'border-success bg-success/5': score.recommended,
                   'border-primary/20': !score.recommended
                 }">
              <div class="flex items-center justify-between mb-3">
                <div class="flex items-center gap-3">
                  <span class="w-8 h-8 flex items-center justify-center rounded-full bg-primary/10 font-bold text-primary">
                    {{ index + 1 }}
                  </span>
                  <div>
                    <h4 class="font-medium text-primary-dark flex items-center gap-2">
                      {{ score.materialName }}
                      <span v-if="score.recommended" 
                            class="px-2 py-0.5 text-xs rounded-full bg-success/20 text-success">
                        ✓ 推荐
                      </span>
                    </h4>
                    <p class="text-xs text-primary/50">{{ score.materialId }}</p>
                  </div>
                </div>
                <div class="text-right">
                  <p class="text-2xl font-bold text-primary-dark">{{ score.totalScore.toFixed(1) }}</p>
                  <p class="text-xs text-primary/50">综合评分</p>
                </div>
              </div>

              <div class="space-y-2">
                <div class="grid grid-cols-6 gap-2">
                  <div v-for="dim in scoreDimensions" :key="dim.key" class="text-center">
                    <div class="text-xs text-primary/50 mb-1">{{ dim.label }}</div>
                    <div class="relative h-2 bg-primary/10 rounded-full overflow-hidden">
                      <div class="absolute left-0 top-0 h-full rounded-full transition-all"
                           :style="{
                             width: (score[dim.key] || 0) + '%',
                             backgroundColor: dim.color
                           }"></div>
                    </div>
                    <div class="text-xs font-medium mt-1" :style="{ color: dim.color }">
                      {{ (score[dim.key] || 0).toFixed(0) }}
                    </div>
                  </div>
                </div>

                <div class="mt-3 p-3 bg-primary-cream/30 rounded text-sm text-primary/70">
                  <span class="font-medium text-primary-dark">分析建议: </span>
                  {{ score.recommendation }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">📐</span>材料基础参数对比
          </h3>
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-primary/10">
                  <th class="text-left py-3 px-2 text-primary/70 font-medium">材料</th>
                  <th class="text-center py-3 px-2 text-primary/70 font-medium">接触角 (°)</th>
                  <th class="text-center py-3 px-2 text-primary/70 font-medium">渗透深度 (mm)</th>
                  <th class="text-center py-3 px-2 text-primary/70 font-medium">强度 (MPa)</th>
                  <th class="text-center py-3 px-2 text-primary/70 font-medium">耐候性 (%)</th>
                  <th class="text-center py-3 px-2 text-primary/70 font-medium">可逆性</th>
                  <th class="text-center py-3 px-2 text-primary/70 font-medium">成本 (元/kg)</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="m in materials" :key="m.id" class="border-b border-primary/5">
                  <td class="py-3 px-2 font-medium text-primary-dark">{{ m.name }}</td>
                  <td class="py-3 px-2 text-center">{{ m.defaultContactAngle }}</td>
                  <td class="py-3 px-2 text-center">{{ m.defaultPenetrationDepth }}</td>
                  <td class="py-3 px-2 text-center">{{ m.defaultStrength }}</td>
                  <td class="py-3 px-2 text-center">{{ m.weatherResistance }}</td>
                  <td class="py-3 px-2 text-center">
                    <span v-if="m.reversibility >= 80" class="text-success">完全可逆</span>
                    <span v-else-if="m.reversibility >= 50" class="text-warning">部分可逆</span>
                    <span v-else class="text-danger">不可逆</span>
                  </td>
                  <td class="py-3 px-2 text-center">¥{{ m.costPerKg }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="card">
          <h3 class="font-medium text-primary-dark mb-4">
            <span class="mr-2">📜</span>历史计算记录
          </h3>
          <div v-if="history.length > 0" class="space-y-2 max-h-60 overflow-y-auto">
            <div v-for="h in history" :key="h.id" 
                 class="flex items-center justify-between p-3 bg-primary-cream/20 rounded">
              <div>
                <span class="font-medium text-primary-dark">{{ h.materialId }}</span>
                <span class="text-sm text-primary/50 ml-2">
                  综合分: <span class="font-medium">{{ h.totalScore.toFixed(1) }}</span>
                </span>
              </div>
              <span class="text-xs text-primary/50">{{ h.calculatedAt }}</span>
            </div>
          </div>
          <p v-else class="text-center text-primary/50 py-4">暂无历史记录</p>
        </div>
      </div>
    </div>

    <div v-else class="card text-center py-12">
      <p class="text-primary/50">加载中...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import RadarCanvas from '@/components/RadarCanvas.vue'
import { useSculptureStore } from '@/stores/sculptureStore'
import { api } from '@/api'
import type { Sculpture, MaterialScore } from '@/types'

const sculptureStore = useSculptureStore()
const sculptures = ref<Sculpture[]>([])
const selectedSculptureId = ref(1)
const loading = ref(false)
const scores = ref<MaterialScore[]>([])
const materials = ref<any[]>([])
const history = ref<any[]>([])

const scoreDimensions = [
  { key: 'contactAngle', label: '接触角', color: '#8B4513' },
  { key: 'penetrationDepth', label: '渗透深度', color: '#D2691E' },
  { key: 'strengthMatch', label: '强度匹配', color: '#CD853F' },
  { key: 'weatherResistance', label: '耐候性', color: '#DAA520' },
  { key: 'reversibility', label: '可逆性', color: '#2E8B57' },
  { key: 'costPerformance', label: '性价比', color: '#4682B4' }
]

const indicators = scoreDimensions.map(d => ({ name: d.label, max: 100 }))
const dimLabels = scoreDimensions.map(d => d.label)

const weights = [
  { name: 'contactAngle', label: '接触角', weight: 0.20 },
  { name: 'penetrationDepth', label: '渗透深度', weight: 0.25 },
  { name: 'strengthMatch', label: '强度匹配', weight: 0.20 },
  { name: 'weatherResistance', label: '耐候性', weight: 0.15 },
  { name: 'reversibility', label: '可逆性', weight: 0.10 },
  { name: 'costPerformance', label: '性价比', weight: 0.10 }
]

const colors = ['#8B4513', '#D2691E', '#CD853F', '#2E8B57']

const sortedScores = computed(() => 
  [...scores.value].sort((a, b) => b.totalScore - a.totalScore)
)

const radarSeries = computed(() => 
  scores.value.map((s, i) => ({
    name: s.materialName,
    data: [
      s.contactAngle,
      s.penetrationDepth,
      s.strengthMatch,
      s.weatherResistance,
      s.reversibility,
      s.costPerformance
    ],
    color: colors[i % colors.length]
  }))
)

const radarCanvasSeries = computed(() =>
  scores.value.map((s, i) => ({
    name: s.materialName,
    values: [
      s.contactAngle || 0,
      s.penetrationDepth || 0,
      s.strengthMatch || 0,
      s.weatherResistance || 0,
      s.reversibility || 0,
      s.costPerformance || 0
    ],
    color: colors[i % colors.length],
    total: s.totalScore !== undefined ? Math.round(s.totalScore * 10) / 10 : undefined
  }))
)

const fetchScores = async () => {
  try {
    const data: any = await api.get('/materials/adaptation/' + selectedSculptureId.value)
    scores.value = data
  } catch (e) {
    console.error('Failed to fetch scores:', e)
    scores.value = mockScores
  }
}

const fetchMaterials = async () => {
  try {
    const data: any = await api.get('/materials')
    materials.value = data
  } catch (e) {
    materials.value = mockMaterials
  }
}

const fetchHistory = async () => {
  try {
    const data: any = await api.get('/materials/history/' + selectedSculptureId.value)
    history.value = data
  } catch (e) {
    history.value = []
  }
}

const calculateScores = async () => {
  loading.value = true
  try {
    const data: any = await api.post('/materials/calculate/' + selectedSculptureId.value, {})
    scores.value = data
    await fetchHistory()
  } catch (e) {
    scores.value = mockScores
  } finally {
    loading.value = false
  }
}

const mockMaterials = [
  { id: 'TEOS-001', name: '正硅酸乙酯(TEOS)', defaultContactAngle: 65, defaultPenetrationDepth: 5, defaultStrength: 2.5, weatherResistance: 95, reversibility: 40, costPerKg: 350 },
  { id: 'NANO-LIME-001', name: '纳米石灰乳液', defaultContactAngle: 55, defaultPenetrationDepth: 8, defaultStrength: 1.8, weatherResistance: 88, reversibility: 90, costPerKg: 280 },
  { id: 'ACRYLIC-001', name: '丙烯酸树脂', defaultContactAngle: 75, defaultPenetrationDepth: 3, defaultStrength: 3.5, weatherResistance: 82, reversibility: 30, costPerKg: 180 },
  { id: 'SILANE-001', name: '硅烷偶联剂', defaultContactAngle: 85, defaultPenetrationDepth: 6, defaultStrength: 3.0, weatherResistance: 90, reversibility: 50, costPerKg: 420 }
]

const mockScores: MaterialScore[] = [
  { materialId: 'TEOS-001', materialName: '正硅酸乙酯(TEOS)', contactAngle: 85, penetrationDepth: 60, strengthMatch: 75, weatherResistance: 95, reversibility: 30, costPerformance: 65, totalScore: 70.5, recommended: false, recommendation: '耐候性优异，但渗透深度一般，适合中短期加固需求。' },
  { materialId: 'NANO-LIME-001', materialName: '纳米石灰乳液', contactAngle: 95, penetrationDepth: 90, strengthMatch: 90, weatherResistance: 85, reversibility: 95, costPerformance: 75, totalScore: 87.5, recommended: true, recommendation: '综合性能最优，与基体兼容性好，可逆性高，强烈推荐使用。' },
  { materialId: 'ACRYLIC-001', materialName: '丙烯酸树脂', contactAngle: 60, penetrationDepth: 30, strengthMatch: 55, weatherResistance: 78, reversibility: 25, costPerformance: 90, totalScore: 54.5, recommended: false, recommendation: '成本较低，但可逆性差，可能影响文物原真性，不建议使用。' },
  { materialId: 'SILANE-001', materialName: '硅烷偶联剂', contactAngle: 50, penetrationDepth: 75, strengthMatch: 70, weatherResistance: 90, reversibility: 50, costPerformance: 45, totalScore: 63.5, recommended: false, recommendation: '防水性好，但成本较高，可作为辅助防水材料使用。' }
]

onMounted(async () => {
  await sculptureStore.fetchSculptures()
  sculptures.value = sculptureStore.sculptures
  await Promise.all([fetchMaterials(), fetchScores(), fetchHistory()])
})
</script>
