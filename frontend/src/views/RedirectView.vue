<template>
  <div />
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

onMounted(() => {
  const { params, query } = route
  const path = Array.isArray(params.path) ? params.path.join('/') : params.path || ''
  router.replace({ path: '/' + path, query })
})
</script>

<script lang="ts">
export default { name: 'RedirectView' }
</script>
