<template>
  <div 
    class="card mb-3 transition-all duration-300"
    :class="{
      'opacity-60': alert.resolved,
      'border-l-4 border-l-status-normal': alert.level === 'normal',
      'border-l-4 border-l-status-warning': alert.level === 'warning',
      'border-l-4 border-l-status-alert animate-pulse-slow': alert.level === 'alert' && !alert.resolved
    }"
  >
    <div class="flex items-start justify-between">
      <div class="flex-1">
        <div class="flex items-center space-x-2 mb-2">
          <span :class="`status-${alert.level}`">
            {{ levelLabels[alert.level] }}
          </span>
          <span class="text-xs bg-primary-cream text-primary px-2 py-0.5 rounded">
            {{ typeLabels[alert.type] }}
          </span>
          <span class="text-xs text-primary/50">
            {{ formatTime(alert.timestamp) }}
          </span>
        </div>
        <p class="text-primary-dark font-medium">{{ alert.message }}</p>
        <div class="mt-2 flex items-center space-x-4 text-sm text-primary/60">
          <span>当前值: <strong :class="alert.level !== 'normal' ? 'text-status-alert' : ''">{{ formatValue(alert.value) }}</strong></span>
          <span>阈值: {{ formatValue(alert.threshold) }}</span>
          <span v-if="alert.sensorId">传感器: {{ alert.sensorId }}</span>
        </div>
      </div>
      <div class="flex items-center space-x-2 ml-4">
        <button
          v-if="!alert.resolved"
          class="btn-secondary text-sm"
          @click="$emit('resolve', alert.id)"
        >
          处理
        </button>
        <span v-else class="text-xs text-status-normal bg-status-normal/10 px-2 py-1 rounded">
          已处理
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Alert } from '@/types'

defineProps<{
  alert: Alert
}>()

defineEmits<{
  (e: 'resolve', id: string): void
}>()

const levelLabels: Record<string, string> = {
  normal: '正常',
  warning: '预警',
  alert: '告警'
}

const typeLabels: Record<string, string> = {
  salt: '盐分',
  temperature: '温度',
  humidity: '湿度',
  sensor_offline: '传感器离线'
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

function formatValue(value: number): string {
  if (value < 10) return value.toFixed(2)
  if (value < 100) return value.toFixed(1)
  return value.toFixed(0)
}
</script>
