<template>
  <div ref="chartRef" :style="{ width: width, height: height }"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted, nextTick } from 'vue'
import * as echarts from 'echarts'
import type { EChartsOption } from 'echarts'

const props = withDefaults(defineProps<{
  indicators: { name: string; max: number }[]
  data: { name: string; value: number[]; color: string }[]
  width?: string
  height?: string
}>(), {
  width: '100%',
  height: '400px'
})

const chartRef = ref<HTMLElement | null>(null)
let chartInstance: echarts.ECharts | null = null

function initChart() {
  if (!chartRef.value) return
  
  chartInstance = echarts.init(chartRef.value)
  
  const option: EChartsOption = {
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#DEB887',
      borderWidth: 1,
      textStyle: {
        color: '#5D2E0C'
      }
    },
    legend: {
      data: props.data.map(d => d.name),
      bottom: 10,
      textStyle: {
        color: '#5D2E0C'
      }
    },
    radar: {
      indicator: props.indicators,
      shape: 'polygon',
      splitNumber: 5,
      axisName: {
        color: '#5D2E0C',
        fontSize: 12
      },
      splitLine: {
        lineStyle: {
          color: 'rgba(139, 69, 19, 0.2)'
        }
      },
      splitArea: {
        show: true,
        areaStyle: {
          color: ['rgba(139, 69, 19, 0.02)', 'rgba(139, 69, 19, 0.05)']
        }
      },
      axisLine: {
        lineStyle: {
          color: 'rgba(139, 69, 19, 0.3)'
        }
      }
    },
    series: [{
      type: 'radar',
      data: props.data.map(d => ({
        name: d.name,
        value: d.value,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: {
          width: 2,
          color: d.color
        },
        areaStyle: {
          color: d.color,
          opacity: 0.15
        },
        itemStyle: {
          color: d.color
        }
      }))
    }]
  }
  
  chartInstance.setOption(option)
}

function handleResize() {
  chartInstance?.resize()
}

onMounted(async () => {
  await nextTick()
  initChart()
  window.addEventListener('resize', handleResize)
})

watch(() => [props.indicators, props.data], () => {
  initChart()
}, { deep: true })

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  chartInstance?.dispose()
})
</script>
