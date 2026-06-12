<template>
  <div ref="containerRef" class="radar-canvas-container" :style="{ width, height }">
    <canvas ref="canvasRef" :width="canvasWidth" :height="canvasHeight" @mousemove="onMouseMove" @mouseleave="onMouseLeave"></canvas>
    <div v-if="tooltip.visible" class="radar-tooltip" :style="{ left: tooltip.x + 'px', top: tooltip.y + 'px' }">
      <div class="font-medium text-primary-dark">{{ tooltip.title }}</div>
      <div v-for="(item, idx) in tooltip.items" :key="idx" class="text-xs text-primary/70 flex items-center gap-1">
        <span class="w-2 h-2 rounded-full" :style="{ background: item.color }"></span>
        {{ item.dim }}: {{ item.value }}
      </div>
      <div class="text-xs font-bold mt-1" :style="{ color: tooltip.totalColor }">
        总分: {{ tooltip.total }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch, nextTick, computed } from 'vue'

export interface RadarSeries {
  name: string
  values: number[]
  color: string
  total?: number
}

export interface RadarProps {
  dimensions: string[]
  series: RadarSeries[]
  maxValue?: number
  width?: string
  height?: string
  levels?: number
}

const props = withDefaults(defineProps<RadarProps>(), {
  maxValue: 100,
  width: '100%',
  height: '360px',
  levels: 5
})

const containerRef = ref<HTMLDivElement | null>(null)
const canvasRef = ref<HTMLCanvasElement | null>(null)
const canvasWidth = ref(600)
const canvasHeight = ref(360)
const dpr = typeof window !== 'undefined' ? (window.devicePixelRatio || 1) : 1

const tooltip = ref({
  visible: false,
  x: 0,
  y: 0,
  title: '',
  items: [] as { dim: string; value: number; color: string }[],
  total: '',
  totalColor: ''
})

const center = computed(() => ({
  x: canvasWidth.value / 2,
  y: canvasHeight.value / 2 - 10
}))

const radius = computed(() => Math.min(canvasWidth.value, canvasHeight.value) / 2 - 60)

function angleForDim(i: number): number {
  return -Math.PI / 2 + (2 * Math.PI * i) / props.dimensions.length
}

function resize() {
  if (!containerRef.value) return
  const rect = containerRef.value.getBoundingClientRect()
  canvasWidth.value = Math.max(400, Math.floor(rect.width))
  canvasHeight.value = Math.max(300, Math.floor(rect.height || 360))
  nextTick(() => draw())
}

function draw() {
  const canvas = canvasRef.value
  if (!canvas) return
  const ctx = canvas.getContext('2d')
  if (!ctx) return

  ctx.setTransform(dpr, 0, 0, dpr, 0, 0)
  ctx.clearRect(0, 0, canvasWidth.value, canvasHeight.value)

  drawGrid(ctx)
  drawAxes(ctx)
  drawDimensionLabels(ctx)
  drawSeries(ctx)
}

function drawGrid(ctx: CanvasRenderingContext2D) {
  const cx = center.value.x
  const cy = center.value.y
  const r = radius.value
  const n = props.dimensions.length

  for (let lv = props.levels; lv >= 1; lv--) {
    const ratio = lv / props.levels
    ctx.beginPath()
    for (let i = 0; i <= n; i++) {
      const a = angleForDim(i % n)
      const x = cx + Math.cos(a) * r * ratio
      const y = cy + Math.sin(a) * r * ratio
      if (i === 0) ctx.moveTo(x, y)
      else ctx.lineTo(x, y)
    }
    ctx.closePath()
    ctx.fillStyle = lv % 2 === 0 ? 'rgba(139,69,19,0.03)' : 'rgba(139,69,19,0.06)'
    ctx.fill()
    ctx.strokeStyle = 'rgba(139,69,19,0.15)'
    ctx.lineWidth = 1
    ctx.stroke()

    ctx.fillStyle = 'rgba(139,69,19,0.4)'
    ctx.font = '10px sans-serif'
    ctx.textAlign = 'right'
    ctx.fillText(String(Math.round(props.maxValue * ratio)), cx - 4, cy - r * ratio + 3)
  }
}

function drawAxes(ctx: CanvasRenderingContext2D) {
  const cx = center.value.x
  const cy = center.value.y
  const r = radius.value
  const n = props.dimensions.length

  ctx.strokeStyle = 'rgba(139,69,19,0.2)'
  ctx.lineWidth = 1
  for (let i = 0; i < n; i++) {
    const a = angleForDim(i)
    ctx.beginPath()
    ctx.moveTo(cx, cy)
    ctx.lineTo(cx + Math.cos(a) * r, cy + Math.sin(a) * r)
    ctx.stroke()
  }
}

function drawDimensionLabels(ctx: CanvasRenderingContext2D) {
  const cx = center.value.x
  const cy = center.value.y
  const r = radius.value
  const n = props.dimensions.length
  const labelR = r + 22

  ctx.font = 'bold 12px sans-serif'
  ctx.textAlign = 'center'
  ctx.textBaseline = 'middle'
  ctx.fillStyle = '#5D4037'

  for (let i = 0; i < n; i++) {
    const a = angleForDim(i)
    const x = cx + Math.cos(a) * labelR
    const y = cy + Math.sin(a) * labelR
    ctx.fillText(props.dimensions[i], x, y)
  }
}

function drawSeries(ctx: CanvasRenderingContext2D) {
  const cx = center.value.x
  const cy = center.value.y
  const r = radius.value
  const n = props.dimensions.length

  for (let sIdx = props.series.length - 1; sIdx >= 0; sIdx--) {
    const s = props.series[sIdx]

    ctx.beginPath()
    for (let i = 0; i <= n; i++) {
      const idx = i % n
      const a = angleForDim(idx)
      const val = Math.min(props.maxValue, Math.max(0, s.values[idx] || 0))
      const ratio = val / props.maxValue
      const x = cx + Math.cos(a) * r * ratio
      const y = cy + Math.sin(a) * r * ratio
      if (i === 0) ctx.moveTo(x, y)
      else ctx.lineTo(x, y)
    }
    ctx.closePath()

    ctx.fillStyle = hexToRgba(s.color, 0.18)
    ctx.fill()
    ctx.strokeStyle = s.color
    ctx.lineWidth = 2
    ctx.stroke()

    for (let i = 0; i < n; i++) {
      const a = angleForDim(i)
      const val = Math.min(props.maxValue, Math.max(0, s.values[i] || 0))
      const ratio = val / props.maxValue
      const x = cx + Math.cos(a) * r * ratio
      const y = cy + Math.sin(a) * r * ratio

      ctx.beginPath()
      ctx.arc(x, y, 3.5, 0, 2 * Math.PI)
      ctx.fillStyle = s.color
      ctx.fill()
      ctx.strokeStyle = '#fff'
      ctx.lineWidth = 1.5
      ctx.stroke()
    }
  }

  drawLegend(ctx)
}

function drawLegend(ctx: CanvasRenderingContext2D) {
  if (props.series.length <= 1) return

  const startX = 12
  let startY = 12
  ctx.font = '11px sans-serif'
  ctx.textBaseline = 'middle'

  for (const s of props.series) {
    ctx.fillStyle = s.color
    ctx.fillRect(startX, startY, 12, 12)
    ctx.strokeStyle = 'rgba(0,0,0,0.1)'
    ctx.strokeRect(startX, startY, 12, 12)

    ctx.fillStyle = '#5D4037'
    ctx.textAlign = 'left'
    const label = s.total !== undefined ? `${s.name} (${s.total})` : s.name
    ctx.fillText(label, startX + 18, startY + 6)
    startY += 18
  }
}

function hexToRgba(hex: string, alpha: number): string {
  const m = hex.replace('#', '')
  const bigint = parseInt(m.length === 3 ? m.split('').map(c => c + c).join('') : m, 16)
  const r = (bigint >> 16) & 255
  const g = (bigint >> 8) & 255
  const b = bigint & 255
  return `rgba(${r},${g},${b},${alpha})`
}

function onMouseMove(e: MouseEvent) {
  const canvas = canvasRef.value
  if (!canvas) return
  const rect = canvas.getBoundingClientRect()
  const mx = e.clientX - rect.left
  const my = e.clientY - rect.top

  const cx = center.value.x
  const cy = center.value.y
  const r = radius.value
  const n = props.dimensions.length

  let hitIdx = -1
  let hitDist = Infinity

  for (let sIdx = 0; sIdx < props.series.length; sIdx++) {
    for (let i = 0; i < n; i++) {
      const a = angleForDim(i)
      const val = Math.min(props.maxValue, Math.max(0, props.series[sIdx].values[i] || 0))
      const ratio = val / props.maxValue
      const x = cx + Math.cos(a) * r * ratio
      const y = cy + Math.sin(a) * r * ratio
      const d = Math.hypot(mx - x, my - y)
      if (d < 8 && d < hitDist) {
        hitDist = d
        hitIdx = sIdx
      }
    }
  }

  if (hitIdx >= 0) {
    const s = props.series[hitIdx]
    tooltip.value = {
      visible: true,
      x: e.clientX - rect.left + 12,
      y: e.clientY - rect.top + 12,
      title: s.name,
      items: props.dimensions.map((d, i) => ({
        dim: d,
        value: Math.round(s.values[i] || 0),
        color: s.color
      })),
      total: s.total !== undefined ? String(s.total) : '',
      totalColor: s.color
    }
  } else {
    tooltip.value.visible = false
  }
}

function onMouseLeave() {
  tooltip.value.visible = false
}

onMounted(() => {
  resize()
  window.addEventListener('resize', resize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', resize)
})

watch(() => [props.series, props.dimensions, props.maxValue], () => draw(), { deep: true })
</script>

<style scoped>
.radar-canvas-container {
  position: relative;
  background: rgba(250, 244, 232, 0.3);
  border-radius: 8px;
  overflow: hidden;
}

.radar-canvas-container canvas {
  display: block;
}

.radar-tooltip {
  position: absolute;
  background: rgba(255, 255, 255, 0.97);
  border: 1px solid rgba(139, 69, 19, 0.2);
  border-radius: 6px;
  padding: 8px 12px;
  box-shadow: 0 4px 12px rgba(139, 69, 19, 0.15);
  pointer-events: none;
  z-index: 10;
  min-width: 140px;
}
</style>
