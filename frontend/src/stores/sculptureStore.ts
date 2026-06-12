import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { sculptureApi, sensorApi, dashboardApi, analysisApi } from '@/api'
import type { Sculpture, Sensor, DashboardStats, TimeSeriesData, MaterialScore, RichardsParams, MigrationResult, HeatmapData } from '@/types'

export const useSculptureStore = defineStore('sculpture', () => {
  const sculptures = ref<Sculpture[]>([])
  const sensors = ref<Sensor[]>([])
  const currentSculpture = ref<Sculpture | null>(null)
  const timeSeriesData = ref<TimeSeriesData[]>([])
  const heatmapData = ref<HeatmapData[]>([])
  const materials = ref<MaterialScore[]>([])
  const migrationResult = ref<MigrationResult | null>(null)
  const loading = ref(false)
  const dashboardStats = ref<DashboardStats>({
    totalSculptures: 30,
    normalCount: 25,
    warningCount: 3,
    alertCount: 2,
    activeSensors: 68,
    offlineSensors: 2,
    recentAlerts: 15
  })

  const statusCounts = computed(() => ({
    normal: sculptures.value.filter(s => s.status === 'normal').length,
    warning: sculptures.value.filter(s => s.status === 'warning').length,
    alert: sculptures.value.filter(s => s.status === 'alert').length
  }))

  async function fetchDashboardStats() {
    try {
      dashboardStats.value = await dashboardApi.getStats()
    } catch (error) {
      console.error('Fetch dashboard stats failed:', error)
    }
  }

  async function fetchSculptures() {
    loading.value = true
    try {
      sculptures.value = await sculptureApi.getList()
    } catch (error) {
      console.error('Fetch sculptures failed:', error)
      sculptures.value = generateMockSculptures(30)
    } finally {
      loading.value = false
    }
  }

  async function fetchSculptureDetail(id: string) {
    loading.value = true
    try {
      currentSculpture.value = await sculptureApi.getDetail(id)
    } catch (error) {
      console.error('Fetch sculpture detail failed:', error)
      currentSculpture.value = generateMockSculpture(id)
    } finally {
      loading.value = false
    }
  }

  async function fetchTimeSeries(id: string, days: number = 7) {
    try {
      timeSeriesData.value = await sculptureApi.getTimeSeries(id, days)
    } catch (error) {
      console.error('Fetch time series failed:', error)
      timeSeriesData.value = generateMockTimeSeries(days)
    }
  }

  async function fetchHeatmap(id: string) {
    try {
      heatmapData.value = await sculptureApi.getHeatmap(id)
    } catch (error) {
      console.error('Fetch heatmap failed:', error)
      heatmapData.value = generateMockHeatmap()
    }
  }

  async function fetchSensors(sculptureId?: string) {
    try {
      sensors.value = await sensorApi.getList(sculptureId)
    } catch (error) {
      console.error('Fetch sensors failed:', error)
      sensors.value = generateMockSensors(70, sculptureId)
    }
  }

  async function fetchMaterials() {
    try {
      materials.value = await analysisApi.getMaterials()
    } catch (error) {
      console.error('Fetch materials failed:', error)
      materials.value = generateMockMaterials()
    }
  }

  async function runMigrationSimulation(params: RichardsParams) {
    loading.value = true
    try {
      migrationResult.value = await analysisApi.getMigrationSimulation(params)
    } catch (error) {
      console.error('Migration simulation failed:', error)
      migrationResult.value = generateMockMigrationResult(params)
    } finally {
      loading.value = false
    }
  }

  function generateMockSculptures(count: number): Sculpture[] {
    const names = ['释迦牟尼佛', '观音菩萨', '文殊菩萨', '普贤菩萨', '地藏菩萨', '弥勒菩萨',
                   '大势至菩萨', '日光菩萨', '月光菩萨', '药师佛', '阿弥陀佛', '不空成就佛',
                   '阿閦佛', '宝生佛', '毗卢遮那佛', '卢舍那佛', '燃灯佛', '多宝佛',
                   '弥勒佛', '观世音菩萨', '自在观音', '送子观音', '千手观音', '水月观音',
                   '地藏王菩萨', '伽蓝菩萨', '韦陀菩萨', '哼哈二将', '四大天王', '十八罗汉']
    const locations = ['大雄宝殿', '观音殿', '文殊殿', '普贤殿', '地藏殿', '弥勒殿']
    const eras = ['唐代', '宋代', '元代', '明代', '清代', '民国']
    const statuses: Sculpture['status'][] = ['normal', 'normal', 'normal', 'normal', 'normal', 'warning', 'alert']
    
    return Array.from({ length: count }, (_, i) => {
      const saltLevel = 0.1 + Math.random() * 0.9
      const status = saltLevel > 0.7 ? 'alert' : saltLevel > 0.4 ? 'warning' : 'normal'
      
      return {
        id: `sculpture-${i + 1}`,
        name: names[i],
        location: locations[i % locations.length],
        era: eras[i % eras.length],
        status: statuses[i % statuses.length],
        saltLevel: Math.round(saltLevel * 100) / 100,
        temperature: Math.round((18 + Math.random() * 15) * 10) / 10,
        humidity: Math.round((40 + Math.random() * 40) * 10) / 10,
        lastUpdate: new Date(Date.now() - Math.random() * 60 * 60 * 1000).toISOString(),
        sensorIds: Array.from({ length: 3 }, (_, j) => `sensor-${i * 3 + j + 1}`)
      }
    })
  }

  function generateMockSculpture(id: string): Sculpture {
    const idx = parseInt(id.replace('sculpture-', '')) - 1
    const names = ['释迦牟尼佛', '观音菩萨', '文殊菩萨', '普贤菩萨', '地藏菩萨', '弥勒菩萨']
    const mockSculptures = generateMockSculptures(30)
    return mockSculptures[idx] || {
      ...mockSculptures[0],
      id,
      name: names[idx % 6] || `泥塑 ${id}`
    }
  }

  function generateMockTimeSeries(days: number): TimeSeriesData[] {
    const data: TimeSeriesData[] = []
    const now = new Date()
    for (let i = days * 24; i >= 0; i--) {
      const timestamp = new Date(now.getTime() - i * 60 * 60 * 1000)
      data.push({
        timestamp: timestamp.toISOString(),
        saltLevel: Math.round((0.3 + Math.random() * 0.4 + Math.sin(i / 24) * 0.1) * 100) / 100,
        temperature: Math.round((22 + Math.random() * 8 + Math.sin(i / 6) * 3) * 10) / 10,
        humidity: Math.round((55 + Math.random() * 20 + Math.cos(i / 6) * 5) * 10) / 10
      })
    }
    return data
  }

  function generateMockHeatmap(): HeatmapData[] {
    const data: HeatmapData[] = []
    for (let x = 0; x < 20; x++) {
      for (let y = 0; y < 30; y++) {
        const centerX = 10
        const centerY = 15
        const dist = Math.sqrt(Math.pow(x - centerX, 2) + Math.pow(y - centerY, 2))
        const value = Math.max(0, 1 - dist / 20) * Math.random()
        data.push({ x, y, value: Math.round(value * 100) / 100 })
      }
    }
    return data
  }

  function generateMockSensors(count: number, sculptureId?: string): Sensor[] {
    const types: Sensor['type'][] = ['salt', 'temperature', 'humidity']
    const names = ['盐分传感器', '温度传感器', '湿度传感器']
    
    return Array.from({ length: count }, (_, i) => {
      const typeIdx = i % 3
      const value = typeIdx === 0 ? 0.3 + Math.random() * 0.5 :
                   typeIdx === 1 ? 20 + Math.random() * 15 :
                   45 + Math.random() * 35
      const threshold = typeIdx === 0 ? 0.5 : typeIdx === 1 ? 35 : 70
      const status = value > threshold ? (Math.random() > 0.5 ? 'warning' : 'alert') : 'normal'
      
      return {
        id: `sensor-${i + 1}`,
        name: `${names[typeIdx]} ${i + 1}`,
        type: types[typeIdx],
        status: i % 35 === 0 ? 'alert' : status,
        value: Math.round(value * 10) / 10,
        threshold,
        sculptureId: sculptureId || `sculpture-${Math.floor(i / 3) + 1}`,
        position: {
          x: Math.round(Math.random() * 100) / 100,
          y: Math.round(Math.random() * 100) / 100,
          z: Math.round(Math.random() * 100) / 100
        },
        lastUpdate: new Date(Date.now() - Math.random() * 30 * 60 * 1000).toISOString(),
        battery: Math.round((60 + Math.random() * 40) * 10) / 10
      }
    }).filter(s => !sculptureId || s.sculptureId === sculptureId)
  }

  function generateMockMaterials(): MaterialScore[] {
    return [
      {
        material: 'TEOS (正硅酸乙酯)',
        durability: 85,
        permeability: 75,
        compatibility: 70,
        reversibility: 60,
        cost: 55,
        aesthetics: 80,
        overall: 71,
        recommended: false,
        tags: ['无机材料', '耐久性好']
      },
      {
        material: '纳米石灰',
        durability: 75,
        permeability: 90,
        compatibility: 95,
        reversibility: 85,
        cost: 70,
        aesthetics: 85,
        overall: 83,
        recommended: true,
        tags: ['推荐材料', '兼容性好', '可逆']
      },
      {
        material: '丙烯酸树脂',
        durability: 90,
        permeability: 40,
        compatibility: 65,
        reversibility: 45,
        cost: 80,
        aesthetics: 75,
        overall: 66,
        recommended: false,
        tags: ['耐久性好', '透气性差']
      },
      {
        material: '硅烷偶联剂',
        durability: 80,
        permeability: 85,
        compatibility: 75,
        reversibility: 70,
        cost: 45,
        aesthetics: 70,
        overall: 71,
        recommended: false,
        tags: ['透气性好', '成本低']
      }
    ]
  }

  function generateMockMigrationResult(params: RichardsParams): MigrationResult {
    const times: number[] = []
    const depths: number[] = []
    const concentrations: number[][] = []
    
    for (let t = 0; t <= params.timeSteps; t++) {
      times.push(t * 0.1)
    }
    
    for (let d = 0; d <= params.depthSteps; d++) {
      depths.push(d * 0.5)
    }
    
    for (let t = 0; t <= params.timeSteps; t++) {
      const row: number[] = []
      for (let d = 0; d <= params.depthSteps; d++) {
        const diffusion = Math.exp(-Math.pow(d * 0.5, 2) / (4 * (t * 0.1 + 1) * params.Ks))
        const advection = Math.exp(-params.alpha * (d * 0.5 - 0.1 * t * 0.1))
        const conc = params.initialConcentration * diffusion * advection
        row.push(Math.min(params.initialConcentration, Math.max(0, conc)))
      }
      concentrations.push(row)
    }
    
    return { times, depths, concentrations }
  }

  return {
    sculptures,
    sensors,
    currentSculpture,
    timeSeriesData,
    heatmapData,
    materials,
    migrationResult,
    loading,
    dashboardStats,
    statusCounts,
    fetchDashboardStats,
    fetchSculptures,
    fetchSculptureDetail,
    fetchTimeSeries,
    fetchHeatmap,
    fetchSensors,
    fetchMaterials,
    runMigrationSimulation
  }
})
