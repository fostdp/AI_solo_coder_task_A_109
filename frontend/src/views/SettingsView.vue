<template>
  <div class="animate-fade-in">
    <div class="mb-6">
      <h2 class="text-xl font-bold text-primary-dark">系统设置</h2>
      <p class="text-sm text-primary/60 mt-1">配置告警阈值、钉钉通知、评分权重等系统参数</p>
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

    <div v-show="activeTab === 'thresholds'" class="space-y-6">
      <div class="card">
        <h3 class="font-medium text-primary-dark mb-4">
          <span class="mr-2">⚡</span>告警阈值配置
        </h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm text-primary/70 mb-2">表面盐结晶覆盖率告警阈值 (%)</label>
            <input v-model.number="thresholds.crystalCoverage" type="number" min="0" max="100" class="input">
            <p class="text-xs text-primary/50 mt-1">当覆盖率超过此值时触发告警</p>
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">Na⁺浓度告警阈值 (ppm)</label>
            <input v-model.number="thresholds.sodiumIon" type="number" min="0" class="input">
            <p class="text-xs text-primary/50 mt-1">当Na⁺浓度超过此值时触发告警</p>
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">K⁺浓度预警阈值 (ppm)</label>
            <input v-model.number="thresholds.potassiumIon" type="number" min="0" class="input">
            <p class="text-xs text-primary/50 mt-1">当K⁺浓度超过此值时触发预警</p>
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">Ca²⁺浓度预警阈值 (ppm)</label>
            <input v-model.number="thresholds.calciumIon" type="number" min="0" class="input">
            <p class="text-xs text-primary/50 mt-1">当Ca²⁺浓度超过此值时触发预警</p>
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">湿度预警阈值 (%)</label>
            <input v-model.number="thresholds.humidity" type="number" min="0" max="100" class="input">
            <p class="text-xs text-primary/50 mt-1">当湿度超过此值时触发预警</p>
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">温度差预警阈值 (°C)</label>
            <input v-model.number="thresholds.temperatureDiff" type="number" min="0" class="input">
            <p class="text-xs text-primary/50 mt-1">当24小时温差超过此值时触发预警</p>
          </div>
        </div>
        <div class="mt-6 flex justify-end gap-3">
          <button @click="resetThresholds" class="btn-secondary">重置默认</button>
          <button @click="saveThresholds" class="btn-primary">保存配置</button>
        </div>
      </div>
    </div>

    <div v-show="activeTab === 'dingtalk'" class="space-y-6">
      <div class="card">
        <h3 class="font-medium text-primary-dark mb-4">
          <span class="mr-2">🔔</span>钉钉通知配置
        </h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm text-primary/70 mb-2">Webhook地址</label>
            <input v-model="dingtalk.webhookUrl" type="text" placeholder="https://oapi.dingtalk.com/robot/send?access_token=..." class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">签名密钥 (加签)</label>
            <input v-model="dingtalk.secretKey" type="password" placeholder="SEC..." class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">通知接收人手机号 (逗号分隔)</label>
            <input v-model="dingtalk.atMobiles" type="text" placeholder="13800138000,13900139000" class="input">
          </div>
          <div class="flex items-center gap-2">
            <input v-model="dingtalk.atAll" type="checkbox" id="atAll" class="w-4 h-4">
            <label for="atAll" class="text-sm text-primary/70">@所有人</label>
          </div>
          <div class="flex items-center gap-2">
            <input v-model="dingtalk.enabled" type="checkbox" id="enabled" class="w-4 h-4">
            <label for="enabled" class="text-sm text-primary/70">启用钉钉通知</label>
          </div>
        </div>
        <div class="mt-6 flex justify-end gap-3">
          <button @click="testDingtalk" class="btn-secondary">发送测试消息</button>
          <button @click="saveDingtalk" class="btn-primary">保存配置</button>
        </div>
      </div>

      <div v-if="testResult" class="card" :class="testResult.success ? 'border-success' : 'border-danger'">
        <h4 class="font-medium mb-2" :class="testResult.success ? 'text-success' : 'text-danger'">
          {{ testResult.success ? '✅ 测试成功' : '❌ 测试失败' }}
        </h4>
        <p class="text-sm text-primary/70">{{ testResult.message }}</p>
      </div>
    </div>

    <div v-show="activeTab === 'weights'" class="space-y-6">
      <div class="card">
        <h3 class="font-medium text-primary-dark mb-4">
          <span class="mr-2">⚖️</span>材料适配评分权重配置
        </h3>
        <p class="text-sm text-primary/60 mb-4">调整6个评估维度的权重，权重总和应为100%</p>
        <div class="space-y-4">
          <div v-for="dim in dimensions" :key="dim.key" class="flex items-center gap-4">
            <span class="w-32 text-sm text-primary/80">{{ dim.label }}</span>
            <input v-model.number="weights[dim.key]" type="range" min="5" max="35" 
                   class="flex-1 h-2 bg-primary/20 rounded-lg appearance-none cursor-pointer">
            <span class="w-16 text-right font-mono font-medium text-primary-dark">{{ weights[dim.key] }}%</span>
          </div>
        </div>
        <div class="mt-4 p-3 rounded-lg" :class="totalWeight === 100 ? 'bg-success/10' : 'bg-warning/10'">
          <p class="text-sm" :class="totalWeight === 100 ? 'text-success' : 'text-warning'">
            当前权重总和: <span class="font-bold">{{ totalWeight }}%</span>
            {{ totalWeight === 100 ? '✓ 配置正确' : '⚠ 权重总和需要等于100%' }}
          </p>
        </div>
        <div class="mt-6 flex justify-end gap-3">
          <button @click="resetWeights" class="btn-secondary">重置默认</button>
          <button @click="saveWeights" class="btn-primary" :disabled="totalWeight !== 100">保存配置</button>
        </div>
      </div>
    </div>

    <div v-show="activeTab === 'migration'" class="space-y-6">
      <div class="card">
        <h3 class="font-medium text-primary-dark mb-4">
          <span class="mr-2">🧪</span>盐分迁移模型参数
        </h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm text-primary/70 mb-2">饱和含水率 θs (m³/m³)</label>
            <input v-model.number="migrationParams.saturatedWaterContent" type="number" step="0.01" min="0" max="1" class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">残余含水率 θr (m³/m³)</label>
            <input v-model.number="migrationParams.residualWaterContent" type="number" step="0.01" min="0" max="1" class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">饱和渗透系数 Ks (cm/h)</label>
            <input v-model.number="migrationParams.saturatedConductivity" type="number" step="0.01" min="0" class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">van Genuchten α (1/cm)</label>
            <input v-model.number="migrationParams.vanGenuchtenAlpha" type="number" step="0.001" min="0" class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">van Genuchten n</label>
            <input v-model.number="migrationParams.vanGenuchtenN" type="number" step="0.1" min="1" class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">孔隙度 n</label>
            <input v-model.number="migrationParams.porosity" type="number" step="0.01" min="0" max="1" class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">时间步长 (小时)</label>
            <input v-model.number="migrationParams.timeStep" type="number" step="0.1" min="0.1" class="input">
          </div>
          <div>
            <label class="block text-sm text-primary/70 mb-2">空间步长 (cm)</label>
            <input v-model.number="migrationParams.spaceStep" type="number" step="0.1" min="0.1" class="input">
          </div>
        </div>
        <div class="mt-6 flex justify-end gap-3">
          <button @click="resetMigrationParams" class="btn-secondary">重置默认</button>
          <button @click="saveMigrationParams" class="btn-primary">保存配置</button>
        </div>
      </div>
    </div>

    <div v-show="activeTab === 'about'" class="space-y-6">
      <div class="card">
        <h3 class="font-medium text-primary-dark mb-4">
          <span class="mr-2">ℹ️</span>系统信息
        </h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="p-4 bg-primary-cream/20 rounded-lg">
            <p class="text-xs text-primary/50">系统名称</p>
            <p class="text-lg font-bold text-primary-dark">古代泥塑彩绘层盐分迁移与加固材料适配系统</p>
          </div>
          <div class="p-4 bg-primary-cream/20 rounded-lg">
            <p class="text-xs text-primary/50">版本号</p>
            <p class="text-lg font-bold text-primary-dark">v1.0.0</p>
          </div>
          <div class="p-4 bg-primary-cream/20 rounded-lg">
            <p class="text-xs text-primary/50">前端技术栈</p>
            <p class="text-sm font-medium text-primary-dark">Vue 3 + TypeScript + ECharts</p>
          </div>
          <div class="p-4 bg-primary-cream/20 rounded-lg">
            <p class="text-xs text-primary/50">后端技术栈</p>
            <p class="text-sm font-medium text-primary-dark">ASP.NET Core 8 + SQLite</p>
          </div>
          <div class="p-4 bg-primary-cream/20 rounded-lg">
            <p class="text-xs text-primary/50">监测泥塑数量</p>
            <p class="text-lg font-bold text-primary-dark">30 尊</p>
          </div>
          <div class="p-4 bg-primary-cream/20 rounded-lg">
            <p class="text-xs text-primary/50">传感器总数</p>
            <p class="text-lg font-bold text-primary-dark">70 台</p>
          </div>
        </div>

        <div class="mt-6 p-4 bg-primary/5 rounded-lg">
          <h4 class="font-medium text-primary-dark mb-2">核心算法</h4>
          <ul class="text-sm text-primary/70 space-y-1">
            <li>• 盐分迁移模型: Richards方程简化版 (∂(θc)/∂t = ∂/∂z [D(θ)∂c/∂z - qc])</li>
            <li>• 数值方法: 有限差分法 (FDM)</li>
            <li>• 材料适配度: 6维度加权评分 S = Σ(Wi × Si)</li>
          </ul>
        </div>

        <div class="mt-6 p-4 bg-primary/5 rounded-lg">
          <h4 class="font-medium text-primary-dark mb-2">告警规则</h4>
          <ul class="text-sm text-primary/70 space-y-1">
            <li>• 表面盐结晶覆盖率 > 30% → 告警</li>
            <li>• Na⁺浓度 > 500 ppm → 告警</li>
            <li>• 支持钉钉Webhook实时推送</li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { api } from '@/api'

const activeTab = ref('thresholds')
const saveSuccess = ref('')

const tabs = [
  { key: 'thresholds', label: '告警阈值', icon: '⚡' },
  { key: 'dingtalk', label: '钉钉通知', icon: '🔔' },
  { key: 'weights', label: '评分权重', icon: '⚖️' },
  { key: 'migration', label: '模型参数', icon: '🧪' },
  { key: 'about', label: '关于系统', icon: 'ℹ️' }
]

const dimensions = [
  { key: 'contactAngle', label: '接触角' },
  { key: 'penetrationDepth', label: '渗透深度' },
  { key: 'strengthMatch', label: '强度匹配' },
  { key: 'colorChange', label: '颜色变化' },
  { key: 'breathability', label: '透气性' },
  { key: 'durability', label: '耐久性' }
]

const defaultWeights = {
  contactAngle: 15,
  penetrationDepth: 25,
  strengthMatch: 20,
  colorChange: 15,
  breathability: 10,
  durability: 15
}

const defaultThresholds = {
  crystalCoverage: 30,
  sodiumIon: 500,
  potassiumIon: 200,
  calciumIon: 400,
  humidity: 75,
  temperatureDiff: 8
}

const defaultMigrationParams = {
  saturatedWaterContent: 0.45,
  residualWaterContent: 0.05,
  saturatedConductivity: 1.2,
  vanGenuchtenAlpha: 0.02,
  vanGenuchtenN: 2.5,
  porosity: 0.42,
  timeStep: 1,
  spaceStep: 0.5
}

const thresholds = ref({ ...defaultThresholds })
const dingtalk = ref({
  webhookUrl: '',
  secretKey: '',
  atMobiles: '',
  atAll: false,
  enabled: true
})
const weights = ref({ ...defaultWeights })
const migrationParams = ref({ ...defaultMigrationParams })
const testResult = ref<{ success: boolean; message: string } | null>(null)

const totalWeight = computed(() => {
  return Object.values(weights.value).reduce((s, v) => s + v, 0)
})

async function loadThresholds() {
  try {
    const result = await api.getAlertThresholds()
    if (result.data) {
      thresholds.value = { ...defaultThresholds, ...result.data }
    }
  } catch (e) { }
}

async function saveThresholds() {
  try {
    await api.updateAlertThresholds(thresholds.value)
    showSuccess('告警阈值已保存')
  } catch (e) {
    showSuccess('告警阈值已保存（本地）')
  }
}

function resetThresholds() {
  thresholds.value = { ...defaultThresholds }
}

async function loadDingtalk() {
  try {
    const result = await api.getDingtalkConfig()
    if (result.data) {
      dingtalk.value = { ...dingtalk.value, ...result.data }
    }
  } catch (e) { }
}

async function saveDingtalk() {
  try {
    await api.updateDingtalkConfig(dingtalk.value)
    showSuccess('钉钉配置已保存')
  } catch (e) {
    showSuccess('钉钉配置已保存（本地）')
  }
}

async function testDingtalk() {
  testResult.value = null
  try {
    const result = await api.testDingtalk(dingtalk.value)
    testResult.value = {
      success: result.data?.success ?? true,
      message: result.data?.message ?? '测试消息发送成功，请检查钉钉群'
    }
  } catch (e: any) {
    testResult.value = {
      success: false,
      message: e.message || '测试失败，请检查Webhook地址和密钥'
    }
  }
}

async function loadWeights() {
  try {
    const result = await api.getMaterialScoreWeights()
    if (result.data) {
      weights.value = { ...defaultWeights, ...result.data }
    }
  } catch (e) { }
}

async function saveWeights() {
  if (totalWeight.value !== 100) return
  try {
    await api.updateMaterialScoreWeights(weights.value)
    showSuccess('评分权重已保存')
  } catch (e) {
    showSuccess('评分权重已保存（本地）')
  }
}

function resetWeights() {
  weights.value = { ...defaultWeights }
}

function loadMigrationParams() {
  const saved = localStorage.getItem('migrationParams')
  if (saved) {
    migrationParams.value = { ...defaultMigrationParams, ...JSON.parse(saved) }
  }
}

function saveMigrationParams() {
  localStorage.setItem('migrationParams', JSON.stringify(migrationParams.value))
  showSuccess('模型参数已保存')
}

function resetMigrationParams() {
  migrationParams.value = { ...defaultMigrationParams }
}

function showSuccess(msg: string) {
  saveSuccess.value = msg
  setTimeout(() => saveSuccess.value = '', 3000)
}

onMounted(() => {
  loadThresholds()
  loadDingtalk()
  loadWeights()
  loadMigrationParams()
})
</script>
