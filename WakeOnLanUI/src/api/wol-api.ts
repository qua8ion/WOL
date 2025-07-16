import { IAuthResponse, TokenValidStateModel } from '@/types/auth'
import { networkManager, auth } from './wol-api-routes'
import wolAxios from './wol-axios'
import { IAuthModel } from '@/types/auth'
import { IDevice, IPingedDevice } from '@/types/network-manager'
import { useToast } from 'vuestic-ui'
import { AxiosError } from 'axios'

const wolApi = {
  networkManager: {
    base: networkManager.base,
    pathes: networkManager.pathes,

    async sendMagicPacket(device: IDevice): Promise<boolean> {
      const { init } = useToast()

      const response = await wolAxios.post(`/${this.base}/${this.pathes.sendMagicPacket}`, device)
      const result: boolean = response.data

      if (result) {
        init({
          message: 'Сигнал о пробуждении отправлен',
          color: 'success',
        })
      } else {
        init({
          message: 'Сигнал о пробуждении не отправлен',
          color: 'danger',
        })
      }

      return response.data
    },
    async getDevices(): Promise<IDevice[]> {
      const response = await wolAxios.get(`/${this.base}/${this.pathes.getDevices}`)
      return response.data.map((device: IDevice) => ({
        ...device,
        lastScan: new Date(device.lastScan),
      }))
    },
    async pingDevices(ipArray: string[]): Promise<IPingedDevice[]> {
      const response = await wolAxios.post(`/${this.base}/${this.pathes.pingDevices}`, ipArray)
      return response.data.map((device: IPingedDevice) => ({
        ...device,
        lastScan: new Date(device.lastScan),
      }))
    },
  },
  auth: {
    base: auth.base,
    pathes: auth.pathes,

    async login(model: IAuthModel): Promise<IAuthResponse> {
      const { init } = useToast()
      try {
        const response = await wolAxios.post(`/${this.base}/${this.pathes.login}`, model)
        const result: IAuthResponse = response.data

        init({
          message: `Добро пожаловать, ${result.username}`,
          color: `success`,
        })

        return result
      } catch (error) {
        if ((error as AxiosError).status === 401) {
          init({
            message: 'Неправльный логин или пароль',
            color: 'danger',
          })
        }

        throw error
      }
    },
    async validateToken(): Promise<TokenValidStateModel> {
      const response = await wolAxios.get(`/${this.base}/${this.pathes.validateToken}`)
      return response.data
    },
  },
}

export default wolApi
