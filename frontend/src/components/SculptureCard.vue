<template>
  <router-link
    :to="`/sculpture/${sculpture.id}`"
    class="card block cursor-pointer group hover:scale-105 transition-all duration-300"
    :class="borderClass"
  >
    <div class="flex items-start justify-between mb-3">
      <div>
        <h3 class="font-medium text-primary-dark group-hover:text-primary transition-colors">
          {{ sculpture.name }}
        </h3>
        <p class="text-xs text-primary/50 mt-0.5">{{ sculpture.era }} · {{ sculpture.location }}</p>
      </div>
      <span :class="`status-${sculpture.status}`">
        {{ statusLabels[sculpture.status] }}
      </span>
    </div>
    <div class="w-full h-32 bg-primary-cream rounded-md flex items-center justify-center mb-3 overflow-hidden">
      <svg viewBox="0 0 100 120" class="w-16 h-20 text-primary/30">
        <ellipse cx="50" cy="110" rx="35" ry="6" fill="currentColor" opacity="0.3"/>
        <path d="M25 110 L28 50 Q30 35 40 30 Q50 25 60 30 Q70 35 72 50 L75 110" fill="currentColor" opacity="0.4"/>
        <ellipse cx="50" cy="28" rx="15" ry="18" fill="currentColor" opacity="0.5"/>
        <ellipse cx="50" cy="22" rx="18" ry="8" fill="currentColor" opacity="0.3"/>
        <circle cx="44" cy="28" r="2" fill="#8B4513"/>
        <circle cx="56" cy="28" r="2" fill="#8B4513"/>
        <path d="M45 34 Q50 38 55 34" stroke="#8B4513" stroke-width="1.5" fill="none"/>
        <path d="M38 45 L35 80 L45 78 L48 48" fill="currentColor" opacity="0.4"/>
        <path d="M62 45 L65 80 L55 78 L52 48" fill="currentColor" opacity="0.4"/>
      </svg>
    </div>
    <div class="grid grid-cols-3 gap-2 text-center">
      <div>
        <p class="text-lg font-semibold text-primary-dark number-scroll">{{ sculpture.saltLevel.toFixed(2) }}</p>
        <p class="text-xs text-primary/50">盐分</p>
      </div>
      <div>
        <p class="text-lg font-semibold text-primary-dark">{{ sculpture.temperature.toFixed(1) }}°</p>
        <p class="text-xs text-primary/50">温度</p>
      </div>
      <div>
        <p class="text-lg font-semibold text-primary-dark">{{ sculpture.humidity.toFixed(0) }}%</p>
        <p class="text-xs text-primary/50">湿度</p>
      </div>
    </div>
  </router-link>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Sculpture } from '@/types'

const props = defineProps<{
  sculpture: Sculpture
}>()

const statusLabels: Record<string, string> = {
  normal: '正常',
  warning: '预警',
  alert: '告警'
}

const borderClass = computed(() => ({
  'border-l-4 border-l-status-normal': props.sculpture.status === 'normal',
  'border-l-4 border-l-status-warning': props.sculpture.status === 'warning',
  'border-l-4 border-l-status-alert animate-pulse-slow': props.sculpture.status === 'alert'
}))
</script>
