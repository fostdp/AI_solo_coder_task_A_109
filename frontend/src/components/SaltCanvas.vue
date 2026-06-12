<template>
  <div class="relative">
    <canvas 
      ref="canvasRef" 
      :width="width" 
      :height="height"
      class="w-full rounded-lg border border-primary/20"
    />
    <div class="absolute bottom-3 right-3 flex items-center space-x-2 text-xs text-primary/60">
      <div class="flex items-center">
        <div class="w-3 h-3 rounded-full bg-gradient-to-r from-blue-400 to-red-500 mr-1"></div>
        <span>盐分浓度</span>
      </div>
      <span>低 → 高</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted } from 'vue'
import type { HeatmapData } from '@/types'

const props = withDefaults(defineProps<{
  hotspots: HeatmapData[]
  width?: number
  height?: number
  showBuddha?: boolean
}>(), {
  width: 400,
  height: 500,
  showBuddha: true
})

const canvasRef = ref<HTMLCanvasElement | null>(null)
let animationFrame: number | null = null
let pulsePhase = 0

function drawBuddha(ctx: CanvasRenderingContext2D) {
  const w = props.width
  const h = props.height
  const cx = w / 2
  
  ctx.fillStyle = 'rgba(139, 69, 19, 0.08)'
  ctx.strokeStyle = 'rgba(139, 69, 19, 0.3)'
  ctx.lineWidth = 1
  
  ctx.beginPath()
  ctx.ellipse(cx, h * 0.92, w * 0.35, h * 0.05, 0, 0, Math.PI * 2)
  ctx.fill()
  ctx.stroke()
  
  ctx.beginPath()
  ctx.moveTo(w * 0.25, h * 0.92)
  ctx.quadraticCurveTo(w * 0.22, h * 0.5, w * 0.28, h * 0.35)
  ctx.quadraticCurveTo(cx, h * 0.28, w * 0.72, h * 0.35)
  ctx.quadraticCurveTo(w * 0.78, h * 0.5, w * 0.75, h * 0.92)
  ctx.closePath()
  ctx.fill()
  ctx.stroke()
  
  ctx.beginPath()
  ctx.ellipse(cx, h * 0.28, w * 0.12, h * 0.1, 0, 0, Math.PI * 2)
  ctx.fill()
  ctx.stroke()
  
  ctx.beginPath()
  ctx.ellipse(cx, h * 0.22, w * 0.16, h * 0.05, 0, 0, Math.PI * 2)
  ctx.fillStyle = 'rgba(139, 69, 19, 0.06)'
  ctx.fill()
  ctx.stroke()
  
  ctx.fillStyle = 'rgba(139, 69, 19, 0.4)'
  ctx.beginPath()
  ctx.arc(cx - w * 0.04, h * 0.27, w * 0.01, 0, Math.PI * 2)
  ctx.fill()
  ctx.beginPath()
  ctx.arc(cx + w * 0.04, h * 0.27, w * 0.01, 0, Math.PI * 2)
  ctx.fill()
  
  ctx.beginPath()
  ctx.moveTo(cx - w * 0.03, h * 0.32)
  ctx.quadraticCurveTo(cx, h * 0.36, cx + w * 0.03, h * 0.32)
  ctx.strokeStyle = 'rgba(139, 69, 19, 0.4)'
  ctx.lineWidth = 1.5
  ctx.stroke()
  
  ctx.fillStyle = 'rgba(139, 69, 19, 0.08)'
  ctx.strokeStyle = 'rgba(139, 69, 19, 0.3)'
  ctx.lineWidth = 1
  
  ctx.beginPath()
  ctx.moveTo(w * 0.32, h * 0.42)
  ctx.quadraticCurveTo(w * 0.28, h * 0.65, w * 0.3, h * 0.85)
  ctx.lineTo(w * 0.42, h * 0.82)
  ctx.lineTo(w * 0.4, h * 0.48)
  ctx.closePath()
  ctx.fill()
  ctx.stroke()
  
  ctx.beginPath()
  ctx.moveTo(w * 0.68, h * 0.42)
  ctx.quadraticCurveTo(w * 0.72, h * 0.65, w * 0.7, h * 0.85)
  ctx.lineTo(w * 0.58, h * 0.82)
  ctx.lineTo(w * 0.6, h * 0.48)
  ctx.closePath()
  ctx.fill()
  ctx.stroke()
}

function getColor(value: number, alpha: number = 1): string {
  const r = Math.round(30 + value * 200)
  const g = Math.round(100 - value * 80)
  const b = Math.round(200 - value * 180)
  return `rgba(${r}, ${g}, ${b}, ${alpha})`
}

function draw() {
  const canvas = canvasRef.value
  if (!canvas) return
  
  const ctx = canvas.getContext('2d')
  if (!ctx) return
  
  ctx.clearRect(0, 0, props.width, props.height)
  
  ctx.fillStyle = '#FDF8F0'
  ctx.fillRect(0, 0, props.width, props.height)
  
  if (props.showBuddha) {
    drawBuddha(ctx)
  }
  
  const pulse = (Math.sin(pulsePhase) + 1) / 2
  pulsePhase += 0.05
  
  const cellW = props.width / 20
  const cellH = props.height / 30
  
  props.hotspots.forEach((spot) => {
    if (spot.value > 0.1) {
      const x = spot.x * cellW + cellW / 2
      const y = spot.y * cellH + cellH / 2
      const radius = (0.5 + spot.value * 2) * Math.min(cellW, cellH)
      const glowRadius = radius * (1.5 + pulse * 0.5)
      
      const gradient = ctx.createRadialGradient(x, y, 0, x, y, glowRadius)
      gradient.addColorStop(0, getColor(spot.value, 0.6 + pulse * 0.3))
      gradient.addColorStop(0.5, getColor(spot.value, 0.3))
      gradient.addColorStop(1, 'rgba(255, 255, 255, 0)')
      
      ctx.fillStyle = gradient
      ctx.beginPath()
      ctx.arc(x, y, glowRadius, 0, Math.PI * 2)
      ctx.fill()
      
      if (spot.value > 0.5) {
        ctx.fillStyle = getColor(spot.value, 0.8 + pulse * 0.2)
        ctx.beginPath()
        ctx.arc(x, y, radius * 0.6, 0, Math.PI * 2)
        ctx.fill()
      }
    }
  })
  
  animationFrame = requestAnimationFrame(draw)
}

onMounted(() => {
  draw()
})

watch(() => props.hotspots, () => {
  draw()
}, { deep: true })

onUnmounted(() => {
  if (animationFrame) {
    cancelAnimationFrame(animationFrame)
  }
})
</script>
