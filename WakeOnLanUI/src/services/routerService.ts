import { useRouter as useVueRouter } from 'vue-router'

let routerInstance = null as any

export function getRouter() {
  if (!routerInstance) {
    routerInstance = useVueRouter()
  }
  return routerInstance
}
