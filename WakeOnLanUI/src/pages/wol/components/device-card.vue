<template>
  <div>
    <VaCard :class="{ 'animate-click': isClicked }" @click="handleClick">
      <VaCardTitle>
        {{ device.name }}
      </VaCardTitle>

      <VaCardContent>
        <div><strong>IP адрес:</strong> {{ device.ipV4 }}</div>
        <div><strong>MAC адрес:</strong> {{ device.mac }}</div>
        <div>
          <strong>Статус:</strong>
          <VaBadge
            :color="device.online ? 'success' : 'danger'"
            :text="device.online ? 'Онлайн' : 'Оффлайн'"
            class="mr-2"
          />
        </div>
        <div>
          <strong>Последнее сканирование:</strong>
          {{ device.lastScan.toLocaleDateString(locale) }} {{ device.lastScan.toLocaleTimeString(locale) }}
        </div>
      </VaCardContent>
    </VaCard>
  </div>
</template>
<script setup lang="ts">
import { ref, PropType } from 'vue'
import { IDevice } from '@/types/network-manager'
import wolApi from '@/api/wol-api'

const locale = import.meta.env.VITE_FULL_LOCALE
const props = defineProps({
  device: {
    type: Object as PropType<IDevice>,
    required: true,
  },
})
const isClicked = ref(false)

const handleClick = async () => {
  isClicked.value = true

  await wolApi.networkManager.sendMagicPacket(props.device)

  setTimeout(() => {
    isClicked.value = false
  }, 300)
}
</script>

<style scoped>
.animate-click {
  animation: clickAnimation 0.3s ease;
}

@keyframes clickAnimation {
  0% {
    transform: scale(1);
  }
  50% {
    transform: scale(0.95);
  }
  100% {
    transform: scale(1);
  }
}
</style>
