import { request } from './request'
import type { 
  Sculpture, Sensor, Alert, TimeSeriesData, 
  MaterialScore, MigrationResult, RichardsParams,
  DashboardStats, ThresholdConfig, WebhookConfig, User, HeatmapData
} from '@/types'

export const authApi = {
  login: (username: string, password: string) =>
    request<{ token: string; user: User }>({
      url: '/auth/login',
      method: 'post',
      data: { username, password }
    }),
  logout: () =>
    request({ url: '/auth/logout', method: 'post' }),
  getUserInfo: () =>
    request<User>({ url: '/auth/user', method: 'get' })
}

export const sculptureApi = {
  getList: () =>
    request<Sculpture[]>({ url: '/sculptures', method: 'get' }),
  getDetail: (id: string | number) =>
    request<Sculpture>({ url: `/sculptures/${id}`, method: 'get' }),
  getTimeSeries: (id: string | number, days: number = 7) =>
    request<TimeSeriesData[]>({ url: `/sculptures/${id}/timeseries`, method: 'get', params: { days } }),
  getHeatmap: (id: string | number) =>
    request<HeatmapData[]>({ url: `/sculptures/${id}/heatmap`, method: 'get' }),
  getDashboardStats: () =>
    request<DashboardStats>({ url: '/sculptures/dashboard/stats', method: 'get' })
}

export const sensorApi = {
  getList: (sculptureId?: string | number) =>
    request<Sensor[]>({ url: '/sensors', method: 'get', params: sculptureId !== undefined ? { sculptureId } : {} }),
  getDetail: (id: string) =>
    request<Sensor>({ url: `/sensors/${id}`, method: 'get' }),
  update: (id: string, data: Partial<Sensor>) =>
    request<Sensor>({ url: `/sensors/${id}`, method: 'put', data }),
  getLayout: () =>
    request<any>({ url: '/sensors/layout', method: 'get' }),
  getDataLogs: (sensorCode: string, limit: number = 100) =>
    request<any>({ url: '/sensors/data/logs', method: 'get', params: { sensorCode, limit } })
}

export const alertApi = {
  getList: (params?: { resolved?: boolean; level?: string; sculptureId?: string | number }) =>
    request<Alert[]>({ url: '/alerts', method: 'get', params }),
  resolve: (id: string) =>
    request<Alert>({ url: `/alerts/${id}/resolve`, method: 'post' }),
  acknowledge: (id: string) =>
    request<Alert>({ url: `/alerts/${id}/acknowledge`, method: 'post' }),
  getStats: () =>
    request<{ normal: number; warning: number; alert: number }>({ url: '/alerts/stats', method: 'get' }),
  getConfig: () =>
    request<any>({ url: '/alerts/config', method: 'get' }),
  updateConfig: (data: any) =>
    request<any>({ url: '/alerts/config', method: 'put', data }),
  testDingtalk: () =>
    request<any>({ url: '/alerts/test-dingtalk', method: 'post' })
}

export const materialApi = {
  getList: () =>
    request<any[]>({ url: '/materials', method: 'get' }),
  getAdaptation: (sculptureId: string | number) =>
    request<any>({ url: `/materials/adaptation/${sculptureId}`, method: 'get' }),
  getHistory: (sculptureId: string | number) =>
    request<any[]>({ url: `/materials/history/${sculptureId}`, method: 'get' }),
  calculate: (sculptureId: string | number, params: any) =>
    request<any>({ url: `/materials/calculate/${sculptureId}`, method: 'post', data: params }),
  getScoreWeights: () =>
    request<any>({ url: '/materials/weights', method: 'get' }),
  updateScoreWeights: (weights: any) =>
    request<any>({ url: '/materials/weights', method: 'put', data: weights })
}

export const migrationApi = {
  getSimulation: (sculptureId: string | number, params?: any) =>
    request<MigrationResult>({ url: `/migration/simulate/${sculptureId}`, method: 'get', params }),
  runSimulation: (params: RichardsParams) =>
    request<MigrationResult>({ url: '/migration/simulate', method: 'post', data: params })
}

export const dingtalkApi = {
  getConfig: () =>
    request<any>({ url: '/dingtalk/config', method: 'get' }),
  updateConfig: (config: any) =>
    request<any>({ url: '/dingtalk/config', method: 'put', data: config }),
  test: (config: any) =>
    request<any>({ url: '/dingtalk/test', method: 'post', data: config })
}

export const settingsApi = {
  getThresholds: () =>
    request<any>({ url: '/settings/thresholds', method: 'get' }),
  updateThresholds: (data: any) =>
    request<any>({ url: '/settings/thresholds', method: 'put', data }),
  getWebhook: () =>
    request<any>({ url: '/settings/webhook', method: 'get' }),
  updateWebhook: (data: any) =>
    request<any>({ url: '/settings/webhook', method: 'put', data })
}

export const dashboardApi = {
  getStats: () =>
    request<any>({ url: '/dashboard/stats', method: 'get' })
}

export const analysisApi = {
  getMigrationSimulation: (params?: any) =>
    request<any>({ url: '/analysis/migration', method: 'post', data: params }),
  getMaterials: () =>
    request<any>({ url: '/analysis/materials', method: 'get' })
}

export const api = {
  get: (url: string, params?: any) => request<any>({ url, method: 'get', params }),
  post: (url: string, data?: any) => request<any>({ url, method: 'post', data }),
  put: (url: string, data?: any) => request<any>({ url, method: 'put', data }),
  delete: (url: string) => request<any>({ url, method: 'delete' }),

  login: authApi.login,
  logout: authApi.logout,
  getUserInfo: authApi.getUserInfo,

  getSculptures: sculptureApi.getList,
  getSculptureDetail: sculptureApi.getDetail,
  getSculptureTimeSeries: sculptureApi.getTimeSeries,
  getSculptureHeatmap: sculptureApi.getHeatmap,
  getDashboardStats: sculptureApi.getDashboardStats,

  getSensors: sensorApi.getList,
  getSensorDetail: sensorApi.getDetail,
  updateSensor: sensorApi.update,
  getSensorLayout: sensorApi.getLayout,
  getSensorDataLogs: sensorApi.getDataLogs,

  getAlerts: alertApi.getList,
  resolveAlert: alertApi.resolve,
  acknowledgeAlert: alertApi.acknowledge,
  getAlertStats: alertApi.getStats,
  getAlertThresholds: alertApi.getConfig,
  updateAlertThresholds: alertApi.updateConfig,
  testAlertDingtalk: alertApi.testDingtalk,

  getMaterials: materialApi.getList,
  getMaterialAdaptation: materialApi.getAdaptation,
  getMaterialHistory: materialApi.getHistory,
  calculateMaterialScore: materialApi.calculate,
  getMaterialScoreWeights: materialApi.getScoreWeights,
  updateMaterialScoreWeights: materialApi.updateScoreWeights,

  getMigrationSimulation: migrationApi.getSimulation,
  runMigrationSimulation: migrationApi.runSimulation,

  getDingtalkConfig: dingtalkApi.getConfig,
  updateDingtalkConfig: dingtalkApi.updateConfig,
  testDingtalk: dingtalkApi.test
}
