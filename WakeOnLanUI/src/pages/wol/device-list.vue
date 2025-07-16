<template>
  <div>
    <h1 class="page-title font-bold">{{ t('global.device-list') }}</h1>
    <VaCard color="primary" gradient>
      <VaCardContent>
        <VaInnerLoading
          :loading="loading"
          color="backgroundPrimary"
          class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 min-h-[4rem]"
        >
          <DeviceCard v-for="(device, index) in devices" :key="index" :device="device" />
        </VaInnerLoading>
      </VaCardContent>
    </VaCard>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { ref, onMounted, onUnmounted } from 'vue'
import DeviceCard from './components/device-card.vue'
import wolApi from '@/api/wol-api'
import { IDevice } from '@/types/network-manager'

const { t } = useI18n()
const devices = ref<IDevice[]>([])
const loading = ref<boolean>()

const getDevices = async (): Promise<IDevice[]> => {
  loading.value = true

  const result = await wolApi.networkManager.getDevices()

  loading.value = false

  return result
}

const updateDevicesOnline = async () => {
  const ipArray = devices.value.map((d: IDevice) => d.ipV4)
  const result = await wolApi.networkManager.pingDevices(ipArray)

  devices.value = devices.value.map((device: IDevice) => {
    const pingedDevice = result.find((pDevice) => pDevice.ip === device.ipV4)!

    device.lastScan = pingedDevice.lastScan
    device.online = pingedDevice.online

    return device
  })
}

const pingTimeout = setInterval(async () => await updateDevicesOnline(), 3000)

onMounted(async () => {
  devices.value = await getDevices()
})

onUnmounted(() => {
  clearTimeout(pingTimeout)
})
</script>
