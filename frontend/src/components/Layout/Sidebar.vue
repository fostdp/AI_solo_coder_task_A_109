<template>
  <aside class="w-64 bg-primary-dark text-white min-h-screen flex flex-col">
    <div class="p-6 border-b border-primary/30">
      <h1 class="text-xl font-serif font-bold text-primary-pale">泥塑盐分监测</h1>
      <p class="text-xs text-primary-pale/60 mt-1">文物保护智能系统</p>
    </div>
    <nav class="flex-1 py-4">
      <ul class="space-y-1 px-3">
        <li v-for="item in menuItems" :key="item.path">
          <router-link
            :to="item.path"
            class="flex items-center px-4 py-3 rounded-lg text-sm transition-all duration-200"
            :class="{
              'bg-primary text-white': isActive(item.path),
              'text-primary-pale/70 hover:bg-primary/50 hover:text-white': !isActive(item.path)
            }"
          >
            <span class="mr-3 text-lg">{{ item.icon }}</span>
            <span>{{ item.name }}</span>
          </router-link>
        </li>
      </ul>
    </nav>
    <div class="p-4 border-t border-primary/30">
      <div class="text-xs text-primary-pale/50">
        <p>版本 v1.0.0</p>
        <p class="mt-1">{{ currentTime }}</p>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const currentTime = ref('')

const menuItems = [
  { name: '监测总览', path: '/', icon: '📊' },
  { name: '盐分迁移分析', path: '/analysis/migration', icon: '🧪' },
  { name: '材料适配分析', path: '/analysis/materials', icon: '🧱' },
  { name: '告警管理', path: '/alerts', icon: '🚨' },
  { name: '传感器管理', path: '/sensors', icon: '📡' },
  { name: '系统设置', path: '/settings', icon: '⚙️' }
]

function isActive(path: string): boolean {
  if (path === '/') {
    return route.path === '/'
  }
  return route.path.startsWith(path)
}

let timer: number | null = null

function updateTime() {
  const now = new Date()
  currentTime.value = now.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(() => {
  updateTime()
  timer = window.setInterval(updateTime, 60000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>
