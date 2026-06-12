<template>
  <div ref="chartRef" :style="{ width: width, height: height }"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted, nextTick } from 'vue'
import * as echarts from 'echarts'
import type { EChartsOption } from 'echarts'

const props = withDefaults(defineProps<{
  xData: string[]
  series: {
    name: string
    data: number[]
    color: string
    yAxisIndex?: number
  }[]
  yAxis?: {
    name: string
    min?: number
    max?: number
    position?: 'left' | 'right'
  }[]
  width?: string
  height?: string
}>(), {
  width: '100%',
  height: '350px',
  yAxis: () => [{ name: '' }]
})

const chartRef = ref<HTMLElement | null>(null)
let chartInstance: echarts.ECharts | null = null

function initChart() {
  if (!chartRef.value) return
  
  if (chartInstance) {
    chartInstance.dispose()
  }
  
  chartInstance = echarts.init(chartRef.value)
  
  const colors = ['#8B4513', '#D2691E', '#DEB887', '#2E8B57', '#DAA520', '#DC143C']
  
  const option: EChartsOption = {
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#DEB887',
      borderWidth: 1,
      textStyle: {
        color: '#5D2E0C'
      },
      axisPointer: {
        type: 'cross',
        crossStyle: {
          color: '#DEB887'
        }
      }
    },
    legend: {
      data: props.series.map(s => s.name),
      top: 10,
      textStyle: {
        color: '#5D2E0C'
      }
    },
    grid: {
      left: '3%',
      right: props.yAxis.length > 1 ? '10%' : '4%',
      bottom: '3%',
      top: '60px',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: props.xData,
      axisLine: {
        lineStyle: {
          color: 'rgba(139, 69, 19, 0.3)'
        }
      },
      axisLabel: {
        color: '#5D2E0C',
        fontSize: 11
      }
    },
    yAxis: props.yAxis.map((y, idx) => ({
      type: 'value',
      name: y.name,
      min: y.min,
      max: y.max,
      position: y.position || (idx === 0 ? 'left' : 'right'),
      offset: idx > 0 ? 60 : 0,
      axisLine: {
        show: true,
        lineStyle: {
          color: props.series[idx]?.color || colors[idx]
        }
      },
      axisLabel: {
        color: '#5D2E0C',
        fontSize: 11
      },
      splitLine: {
        lineStyle: {
          color: 'rgba(139, 69, 19, 0.1)'
        }
      }
    })),
    series: props.series.map((s, idx) => ({
      name: s.name,
      type: 'line',
      data: s.data,
      yAxisIndex: s.yAxisIndex || idx,
      smooth: true,
      symbol: 'circle',
      symbolSize: 6,
      showSymbol: false,
      lineStyle: {
        width: 2.5,
        color: s.color || colors[idx]
      },
      itemStyle: {
        color: s.color || colors[idx]
      },
      areaStyle: {
        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
          { offset: 0, color: s.color || colors[idx] + '40' },
          { offset: 1, color: s.color || colors[idx] + '05' }
        ])
      }
    }))
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

watch(() => [props.xData, props.series, props.yAxis], () => {
  initChart()
}, { deep: true })

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  chartInstance?.dispose()
})
</script>
