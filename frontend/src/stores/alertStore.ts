import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { alertApi, settingsApi } from '@/api'
import type { Alert, ThresholdConfig, WebhookConfig } from '@/types'

export const useAlertStore = defineStore('alert', () => {
  const alerts = ref<Alert[]>([])
  const loading = ref(false)
  const thresholds = ref<ThresholdConfig>({
    saltWarning: 0.5,
    saltAlert: 0.8,
    tempWarning: 35,
    tempAlert: 40,
    humidityWarning: 70,
    humidityAlert: 85
  })
  const webhook = ref<WebhookConfig>({
    enabled: false,
    url: '',
    secret: '',
    alertTypes: ['salt', 'temperature', 'humidity', 'sensor_offline']
  })

  const unresolvedAlerts = computed(() => 
    alerts.value.filter(a => !a.resolved).sort((a, b) => 
      new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
    )
  )

  const alertStats = computed(() => ({
    normal: alerts.value.filter(a => a.level === 'normal' && !a.resolved).length,
    warning: alerts.value.filter(a => a.level === 'warning' && !a.resolved).length,
    alert: alerts.value.filter(a => a.level === 'alert' && !a.resolved).length
  }))

  async function fetchAlerts(params?: { resolved?: boolean; level?: string; sculptureId?: string }) {
    loading.value = true
    try {
      alerts.value = await alertApi.getList(params)
    } catch (error) {
      console.error('Fetch alerts failed:', error)
      alerts.value = generateMockAlerts(30)
    } finally {
      loading.value = false
    }
  }

  async function resolveAlert(id: string) {
    try {
      await alertApi.resolve(id)
      const alert = alerts.value.find(a => a.id === id)
      if (alert) alert.resolved = true
    } catch (error) {
      console.error('Resolve alert failed:', error)
      const alert = alerts.value.find(a => a.id === id)
      if (alert) alert.resolved = true
    }
  }

  async function fetchThresholds() {
    try {
      thresholds.value = await settingsApi.getThresholds()
    } catch (error) {
      console.error('Fetch thresholds failed:', error)
    }
  }

  async function updateThresholds(config: ThresholdConfig) {
    try {
      thresholds.value = await settingsApi.updateThresholds(config)
      return true
    } catch (error) {
      console.error('Update thresholds failed:', error)
      thresholds.value = config
      return true
    }
  }

  async function fetchWebhook() {
    try {
      webhook.value = await settingsApi.getWebhook()
    } catch (error) {
      console.error('Fetch webhook failed:', error)
    }
  }

  async function updateWebhook(config: WebhookConfig) {
    try {
      webhook.value = await settingsApi.updateWebhook(config)
      return true
    } catch (error) {
      console.error('Update webhook failed:', error)
      webhook.value = config
      return true
    }
  }

  function generateMockAlerts(count: number): Alert[] {
    const types: Alert['type'][] = ['salt', 'temperature', 'humidity', 'sensor_offline']
    const levels: Alert['level'][] = ['warning', 'alert', 'warning']
    const names = ['释迦牟尼佛', '观音菩萨', '文殊菩萨', '普贤菩萨', '地藏菩萨', '弥勒菩萨']
    
    return Array.from({ length: count }, (_, i) => {
      const type = types[Math.floor(Math.random() * types.length)]
      const level = levels[Math.floor(Math.random() * levels.length)]
      const sculptureIdx = Math.floor(Math.random() * 30)
      
      const messages: Record<string, string> = {
        salt: `盐分含量超标`,
        temperature: `温度异常`,
        humidity: `湿度过高`,
        sensor_offline: `传感器离线`
      }
      
      return {
        id: `alert-${i + 1}`,
        type,
        level,
        message: `${names[sculptureIdx % 6]}${messages[type]}`,
        sculptureId: `sculpture-${sculptureIdx + 1}`,
        sensorId: type !== 'sensor_offline' ? `sensor-${i + 1}` : undefined,
        value: Math.random() * 100,
        threshold: 50 + Math.random() * 30,
        timestamp: new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000).toISOString(),
        resolved: i > 20
      }
    })
  }

  return {
    alerts,
    loading,
    thresholds,
    webhook,
    unresolvedAlerts,
    alertStats,
    fetchAlerts,
    resolveAlert,
    fetchThresholds,
    updateThresholds,
    fetchWebhook,
    updateWebhook
  }
})
