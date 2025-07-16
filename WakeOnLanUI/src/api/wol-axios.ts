import axios, { AxiosError, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth-store'
import { useToast } from 'vuestic-ui'

const wolAxios = axios.create({
  baseURL: import.meta.env.VITE_WOL_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

wolAxios.interceptors.request.use((config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
  const authStore = useAuthStore()
  const token = authStore.token

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

wolAxios.interceptors.response.use(
  (response: AxiosResponse): AxiosResponse => response,
  async (error: AxiosError): Promise<AxiosError> => {
    const authStore = useAuthStore()
    const { init } = useToast()

    if (error.response && error.response.status === 401 && error.config?.url?.toLocaleLowerCase() !== '/auth/login') {
      init({
        message: `Ошибка авторизации`,
        color: 'danger',
      })

      await authStore.logout()
    }

    return Promise.reject(error)
  },
)

export default wolAxios
