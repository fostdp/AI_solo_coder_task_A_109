<template>
  <div class="flex min-h-screen bg-primary-paper">
    <Sidebar />
    <div class="flex-1 flex flex-col">
      <Header />
      <main class="flex-1 p-6 overflow-auto animate-fade-in">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import Sidebar from './Sidebar.vue'
import Header from './Header.vue'
import { useAlertStore } from '@/stores/alertStore'
import { useSculptureStore } from '@/stores/sculptureStore'
import { useAuthStore } from '@/stores/authStore'

const alertStore = useAlertStore()
const sculptureStore = useSculptureStore()
const authStore = useAuthStore()

onMounted(async () => {
  await authStore.fetchUserInfo()
  await Promise.all([
    alertStore.fetchAlerts(),
    sculptureStore.fetchSculptures(),
    sculptureStore.fetchSensors()
  ])
})
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
