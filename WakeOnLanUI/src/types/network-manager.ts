export interface IDevice {
  ipV4: string
  mac: string
  online: boolean
  description: string | null
  lastScan: Date
  name: string | null
}

export interface IPingedDevice {
  ip: string
  online: boolean
  lastScan: Date
}
