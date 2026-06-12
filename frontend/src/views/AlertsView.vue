<template>
  <div class="animate-fade-in">
    <div class="mb-6 flex items-center justify-between flex-wrap gap-4">
      <div>
        <h2 class="text-xl font-bold text-primary-dark">告警管理</h2>
        <p class="text-sm text-primary/60 mt-1">实时监测告警事件，及时处理盐害风险</p>
      </div>
      <div class="flex gap-2">
        <button @click="testDingTalk" :disabled="testing" class="btn-secondary">
          <span v-if="testing">测试中...</span>
          <span v-else>🔔 测试钉钉推送</span>
        </button>
        <button @click="showConfigModal = true" class="btn-primary">
          ⚙️ 告警阈值配置
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
      <div class="card p-4 flex items-center gap-3">
        <div class="w-12 h-12 rounded-full bg-danger/10 flex items-center justify-center text-2xl">🔴</div>
        <div>
          <p class="text-sm text-primary/60">紧急告警</p>
          <p class="text-2xl font-bold text-danger">{{ stats.critical }}</p>
        </div>
      </div>
      <div class="card p-4 flex items-center gap-3">
        <div class="w-12 h-12 rounded-full bg-warning/10 flex items-center justify-center text-2xl">🟡</div>
        <div>
          <p class="text-sm text-primary/60">预警</p>
          <p class="text-2xl font-bold text-warning">{{ stats.warning }}</p>
        </div>
      </div>
      <div class="card p-4 flex items-center gap-3">
        <div class="w-12 h-12 rounded-full bg-success/10 flex items-center justify-center text-2xl">✅</div>
        <div>
          <p class="text-sm text-primary/60">已处理</p>
          <p class="text-2xl font-bold text-success">{{ stats.acknowledged }}</p>
        </div>
      </div>
      <div class="card p-4 flex items-center gap-3">
        <div class="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center text-2xl">📊</div>
        <div>
          <p class="text-sm text-primary/60">今日告警</p>
          <p class="text-2xl font-bold text-primary-dark">{{ stats.today }}</p>
        </div>
      </div>
    </div>

    <div class="card mb-6">
      <div class="flex flex-wrap gap-4 mb-4">
        <select v-model="filters.severity" class="input w-40">
          <option value="">全部级别</option>
          <option value="Critical">紧急</option>
          <option value="High">高</option>
          <option value="Medium">中</option>
          <option value="Low">低</option>
        </select>
        <select v-model="filters.status" class="input w-40">
          <option value="">全部状态</option>
          <option value="Pending">待处理</option>
          <option value="Acknowledged">已处理</option>
        </select>
        <input type="text" v-model="filters.search" placeholder="搜索泥塑名称..." class="input flex-1 min-w-48" />
        <button @click="fetchAlerts" class="btn-primary">🔍 搜索</button>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-primary/10">
              <th class="text-left py-3 px-2 text-primary/70 font-medium">时间</th>
              <th class="text-left py-3 px-2 text-primary/70 font-medium">级别</th>
              <th class="text-left py-3 px-2 text-primary/70 font-medium">泥塑</th>
              <th class="text-left py-3 px-2 text-primary/70 font-medium">类型</th>
              <th class="text-left py-3 px-2 text-primary/70 font-medium">告警标题</th>
              <th class="text-center py-3 px-2 text-primary/70 font-medium">当前值/阈值</th>
              <th class="text-center py-3 px-2 text-primary/70 font-medium">状态</th>
              <th class="text-center py-3 px-2 text-primary/70 font-medium">操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="alert in alerts" :key="alert.id" 
                class="border-b border-primary/5 hover:bg-primary-cream/30 transition-colors">
              <td class="py-3 px-2 text-primary/60 whitespace-nowrap">{{ formatTime(alert.createdAt) }}</td>
              <td class="py-3 px-2">
                <span :class="`status-${severityToStatus(alert.severity)}`" class="whitespace-nowrap">
                  {{ severityLabels[alert.severity] }}
                </span>
              </td>
              <td class="py-3 px-2">
                <router-link :to="`/sculpture/${alert.sculptureId}`" 
                             class="text-primary hover:text-primary-dark underline font-medium">
                  {{ getSculptureName(alert.sculptureId) }}
                </router-link>
              </td>
              <td class="py-3 px-2 text-primary/70">{{ typeLabels[alert.alertType] || alert.alertType }}</td>
              <td class="py-3 px-2 text-primary-dark font-medium max-w-xs truncate" :title="alert.message">
                {{ alert.title }}
              </td>
              <td class="py-3 px-2 text-center">
                <span class="text-danger font-medium">{{ alert.actualValue?.toFixed(1) }}</span>
                <span class="text-primary/40"> / </span>
                <span class="text-primary/60">{{ alert.thresholdValue }}</span>
              </td>
              <td class="py-3 px-2 text-center">
                <span v-if="alert.acknowledged" class="text-success text-sm">✓ 已处理</span>
                <span v-else class="text-warning text-sm">⏳ 待处理</span>
              </td>
              <td class="py-3 px-2 text-center">
                <button v-if="!alert.acknowledged" 
                        @click="acknowledgeAlert(alert.id)"
                        class="text-xs px-2 py-1 rounded bg-primary/10 hover:bg-primary/20 text-primary transition-colors">
                  确认处理
                </button>
                <span v-else class="text-primary/30">-</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="alerts.length === 0" class="text-center py-12 text-primary/50">
        暂无告警记录
      </div>

      <div class="flex items-center justify-between mt-4 pt-4 border-t border-primary/10">
        <p class="text-sm text-primary/60">共 {{ total }} 条记录</p>
        <div class="flex gap-2">
          <button @click="page = Math.max(1, page - 1); fetchAlerts()" 
                  :disabled="page <= 1" class="btn-secondary text-sm">上一页</button>
          <span class="px-3 py-2 text-primary/70">{{ page }} / {{ Math.ceil(total / pageSize) }}</span>
          <button @click="page++; fetchAlerts()" 
                  :disabled="page * pageSize >= total" class="btn-secondary text-sm">下一页</button>
        </div>
      </div>
    </div>

    <div v-if="showConfigModal" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" @click.self="showConfigModal = false">
      <div class="bg-white rounded-lg p-6 w-full max-w-xl mx-4 animate-fade-in">
        <h3 class="text-lg font-bold text-primary-dark mb-4">告警阈值配置</h3>
        <div class="space-y-4">
          <div v-for="t in thresholds" :key="t.id" class="p-4 bg-primary-cream/30 rounded-lg">
            <div class="flex items-center justify-between mb-2">
              <span class="font-medium text-primary-dark">{{ t.description }}</span>
              <span class="text-sm text-primary/50">单位: {{ t.unit }}</span>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="text-sm text-primary/60 mb-1 block">预警阈值</label>
                <input type="number" v-model.number="t.warningThreshold" class="w-full input" step="0.1" />
              </div>
              <div>
                <label class="text-sm text-primary/60 mb-1 block">紧急阈值</label>
                <input type="number" v-model.number="t.criticalThreshold" class="w-full input" step="0.1" />
              </div>
            </div>
          </div>
        </div>
        <div class="flex justify-end gap-3 mt-6">
          <button @click="showConfigModal = false" class="btn-secondary">取消</button>
          <button @click="saveThresholds" :disabled="saving" class="btn-primary">
            <span v-if="saving">保存中...</span>
            <span v-else>保存配置</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSculptureStore } from '@/stores/sculptureStore'
import { useAlertStore } from '@/stores/alertStore'
import { api } from '@/api'
import type { Alert } from '@/types'

const sculptureStore = useSculptureStore()
const alertStore = useAlertStore()

const alerts = ref<Alert[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const showConfigModal = ref(false)
const testing = ref(false)
const saving = ref(false)
const thresholds = ref<any[]>([])

const filters = ref({
  severity: '',
  status: '',
  search: ''
})

const stats = ref({
  critical: 0,
  warning: 0,
  acknowledged: 0,
  today: 0
})

const severityLabels: Record<string, string> = {
  Critical: '紧急',
  High: '高',
  Medium: '中',
  Low: '低'
}

const typeLabels: Record<string, string> = {
  salt_coverage: '盐结晶覆盖率',
  na_high: 'Na⁺超标',
  k_high: 'K⁺超标',
  ca_high: 'Ca²⁺超标',
  sensor_offline: '传感器离线'
}

const severityToStatus = (s: string) => {
  if (s === 'Critical' || s === 'High') return 'alert'
  if (s === 'Medium') return 'warning'
  return 'normal'
}

const formatTime = (t: string) => {
  return new Date(t).toLocaleString('zh-CN')
}

const getSculptureName = (id: number) => {
  return sculptureStore.sculptures.find(s => s.id === id)?.name || `#${id}`
}

const fetchAlerts = async () => {
  try {
    const data: any = await api.get('/alerts', {
      params: {
        severity: filters.value.severity || undefined,
        page: page.value,
        pageSize: pageSize.value
      }
    })
    alerts.value = data.alerts || data
    total.value = data.total || alerts.value.length
    updateStats()
  } catch (e) {
    alerts.value = mockAlerts
    total.value = mockAlerts.length
    updateStats()
  }
}

const fetchThresholds = async () => {
  try {
    const data: any = await api.get('/alerts/config')
    thresholds.value = data
  } catch (e) {
    thresholds.value = mockThresholds
  }
}

const saveThresholds = async () => {
  saving.value = true
  try {
    await api.put('/alerts/config', thresholds.value)
    showConfigModal.value = false
  } catch (e) {
    console.error('Failed to save thresholds:', e)
  } finally {
    saving.value = false
  }
}

const acknowledgeAlert = async (id: string) => {
  try {
    await api.post(`/alerts/${id}/acknowledge`)
    await fetchAlerts()
    await alertStore.fetchAlerts()
  } catch (e) {
    const alert = alerts.value.find(a => a.id === id)
    if (alert) alert.acknowledged = true
  }
}

const testDingTalk = async () => {
  testing.value = true
  try {
    await api.post('/alerts/test-dingtalk')
    alert('钉钉测试推送已发送，请检查手机端')
  } catch (e) {
    alert('测试推送失败，请检查钉钉配置')
  } finally {
    testing.value = false
  }
}

const updateStats = () => {
  stats.value = {
    critical: alerts.value.filter(a => a.severity === 'Critical' && !a.acknowledged).length,
    warning: alerts.value.filter(a => (a.severity === 'High' || a.severity === 'Medium') && !a.acknowledged).length,
    acknowledged: alerts.value.filter(a => a.acknowledged).length,
    today: alerts.value.filter(a => {
      const today = new Date()
      const alertDate = new Date(a.createdAt)
      return alertDate.toDateString() === today.toDateString()
    }).length
  }
}

const mockThresholds = [
  { id: 1, parameterName: 'CrystalCoverage', description: '表面盐结晶覆盖率', unit: '%', warningThreshold: 20, criticalThreshold: 30 },
  { id: 2, parameterName: 'SodiumIon', description: '钠离子浓度', unit: 'ppm', warningThreshold: 300, criticalThreshold: 500 },
  { id: 3, parameterName: 'PotassiumIon', description: '钾离子浓度', unit: 'ppm', warningThreshold: 300, criticalThreshold: 500 },
  { id: 4, parameterName: 'CalciumIon', description: '钙离子浓度', unit: 'ppm', warningThreshold: 300, criticalThreshold: 500 },
  { id: 5, parameterName: 'Offline', description: '传感器离线时长', unit: 'minutes', warningThreshold: 90, criticalThreshold: 180 }
]

const mockAlerts: Alert[] = [
  { id: 'a1', sculptureId: 6, alertType: 'salt_coverage', severity: 'Critical', title: '盐结晶覆盖率超标', message: '弥勒菩萨表面盐结晶覆盖率达到35.6%，超过30%阈值', actualValue: 35.6, thresholdValue: 30, createdAt: '2026-06-12T14:30:00', acknowledged: false },
  { id: 'a2', sculptureId: 21, alertType: 'na_high', severity: 'Critical', title: 'Na⁺浓度严重超标', message: '阿那律尊者处Na⁺浓度达到612ppm，超过500ppm阈值', actualValue: 612, thresholdValue: 500, createdAt: '2026-06-12T13:45:00', acknowledged: false },
  { id: 'a3', sculptureId: 3, alertType: 'salt_coverage', severity: 'Medium', title: '盐结晶覆盖率预警', message: '普贤菩萨表面盐结晶覆盖率达到22.5%，接近20%预警阈值', actualValue: 22.5, thresholdValue: 20, createdAt: '2026-06-12T12:15:00', acknowledged: false },
  { id: 'a4', sculptureId: 9, alertType: 'k_high', severity: 'Medium', title: 'K⁺浓度预警', message: '大势至菩萨处K⁺浓度达到328ppm，超过300ppm预警阈值', actualValue: 328, thresholdValue: 300, createdAt: '2026-06-12T11:30:00', acknowledged: true },
  { id: 'a5', sculptureId: 15, alertType: 'ca_high', severity: 'Medium', title: 'Ca²⁺浓度预警', message: '目犍连尊者处Ca²⁺浓度达到315ppm，超过300ppm预警阈值', actualValue: 315, thresholdValue: 300, createdAt: '2026-06-12T10:45:00', acknowledged: true },
  { id: 'a6', sculptureId: 24, alertType: 'salt_coverage', severity: 'Medium', title: '盐结晶覆盖率预警', message: '摩诃男处盐结晶覆盖率达到27.4%，接近30%阈值', actualValue: 27.4, thresholdValue: 30, createdAt: '2026-06-12T09:00:00', acknowledged: false }
]

onMounted(async () => {
  await sculptureStore.fetchSculptures()
  await Promise.all([fetchAlerts(), fetchThresholds(), alertStore.fetchAlerts()])
})
</script>
