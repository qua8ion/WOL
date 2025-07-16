import { defineStore } from 'pinia'
import { IAuthModel } from '@/types/auth'
import { router } from '@/main'
import wolApi from '@/api/wol-api'

const STORAGE_TOKEN_KEY = 'jwt-token'

export const useAuthStore = defineStore({
  id: 'auth',
  state: () => ({
    token: ((): string | null => {
      const value = localStorage.getItem(STORAGE_TOKEN_KEY)
      try {
        return JSON.parse(value!)
      } catch (error) {
        console.log(error)
        return null
      }
    })(),
    returnUrl: '/' as string,
  }),
  actions: {
    async login(username: string, password: string) {
      const model: IAuthModel = {
        username: username,
        password: password,
      }

      const response = await wolApi.auth.login(model)
      const token = response.accessToken

      if (token) {
        this.token = token

        localStorage.setItem(STORAGE_TOKEN_KEY, JSON.stringify(token))

        // redirect to previous url or default to home page
        router.push(this.returnUrl as string)
      }
    },
    async validateToken(): Promise<boolean> {
      if (this.token) {
        const response = await wolApi.auth.validateToken()
        return response.valid
      }
      this.clearToken()
      return false
    },
    logout() {
      this.clearToken()
      router.push('/auth/login')
    },
    clearToken() {
      this.token = null
      localStorage.removeItem(STORAGE_TOKEN_KEY)
    },
  },
})
