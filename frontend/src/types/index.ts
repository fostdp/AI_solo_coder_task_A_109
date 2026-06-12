export type StatusType = 'normal' | 'warning' | 'alert'

export interface Sculpture {
  id: string
  name: string
  location: string
  era: string
  status: StatusType
  saltLevel: number
  temperature: number
  humidity: number
  lastUpdate: string
  sensorIds: string[]
  image?: string
}

export interface Sensor {
  id: string
  name: string
  type: 'salt' | 'temperature' | 'humidity'
  status: StatusType
  value: number
  threshold: number
  sculptureId: string
  position: { x: number; y: number; z: number }
  lastUpdate: string
  battery: number
}

export interface Alert {
  id: string
  type: 'salt' | 'temperature' | 'humidity' | 'sensor_offline'
  level: StatusType
  message: string
  sculptureId: string
  sensorId?: string
  value: number
  threshold: number
  timestamp: string
  resolved: boolean
}

export interface SaltDataPoint {
  depth: number
  time: number
  concentration: number
}

export interface TimeSeriesData {
  timestamp: string
  saltLevel: number
  temperature: number
  humidity: number
}

export interface MaterialScore {
  material: string
  durability: number
  permeability: number
  compatibility: number
  reversibility: number
  cost: number
  aesthetics: number
  overall: number
  recommended: boolean
  tags: string[]
}

export interface RichardsParams {
  alpha: number
  n: number
  thetaR: number
  thetaS: number
  Ks: number
  initialConcentration: number
  timeSteps: number
  depthSteps: number
}

export interface MigrationResult {
  times: number[]
  depths: number[]
  concentrations: number[][]
}

export interface User {
  id: string
  username: string
  role: 'admin' | 'user'
}

export interface ThresholdConfig {
  saltWarning: number
  saltAlert: number
  tempWarning: number
  tempAlert: number
  humidityWarning: number
  humidityAlert: number
}

export interface WebhookConfig {
  enabled: boolean
  url: string
  secret: string
  alertTypes: string[]
}

export interface DashboardStats {
  totalSculptures: number
  normalCount: number
  warningCount: number
  alertCount: number
  activeSensors: number
  offlineSensors: number
  recentAlerts: number
}

export interface HeatmapData {
  x: number
  y: number
  value: number
}
