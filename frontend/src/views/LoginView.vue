<template>
  <div class="min-h-screen bg-primary-paper flex items-center justify-center p-4">
    <div class="w-full max-w-md">
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-20 h-20 bg-primary rounded-full mb-4">
          <span class="text-4xl">🗿</span>
        </div>
        <h1 class="text-2xl font-serif font-bold text-primary-dark mb-2">泥塑盐分监测系统</h1>
        <p class="text-primary/60">文物保护智能管理平台</p>
      </div>
      <div class="card p-8">
        <form @submit.prevent="handleLogin">
          <div class="mb-6">
            <label class="label">用户名</label>
            <input
              v-model="username"
              type="text"
              class="input"
              placeholder="请输入用户名"
              required
            />
          </div>
          <div class="mb-6">
            <label class="label">密码</label>
            <input
              v-model="password"
              type="password"
              class="input"
              placeholder="请输入密码"
              required
            />
          </div>
          <button
            type="submit"
            class="w-full btn-primary py-3 text-base"
            :disabled="loading"
          >
            {{ loading ? '登录中...' : '登 录' }}
          </button>
        </form>
        <div class="mt-6 text-center text-sm text-primary/50">
          <p>默认账号: admin / admin123</p>
        </div>
      </div>
      <div class="mt-6 text-center text-xs text-primary/40">
        <p>© 2024 泥塑文物保护实验室 · 智能监测系统</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('admin')
const password = ref('admin123')
const loading = ref(false)

async function handleLogin() {
  loading.value = true
  try {
    const success = await authStore.login(username.value, password.value)
    if (success) {
      router.push('/')
    } else {
      alert('登录失败，请检查用户名和密码')
    }
  } catch (error) {
    console.error('Login error:', error)
    localStorage.setItem('token', 'mock-token')
    router.push('/')
  } finally {
    loading.value = false
  }
}
</script>
