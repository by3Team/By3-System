<template>
  <el-breadcrumb separator="/" class="breadcrumb">
    <el-breadcrumb-item :to="{ path: '/dashboard' }">首页</el-breadcrumb-item>
    <el-breadcrumb-item v-for="(item, index) in breadcrumbs" :key="index" :to="item.path ? { path: item.path } : undefined">
      {{ item.title }}
    </el-breadcrumb-item>
  </el-breadcrumb>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const breadcrumbs = computed(() => {
  const matched = route.matched.filter(r => r.meta && r.meta.title && r.path !== '/dashboard')
  return matched.map(r => ({
    title: r.meta.title as string,
    path: r.path
  }))
})
</script>

<script lang="ts">
export default { name: 'AppBreadcrumb' }
</script>

<style scoped>
.breadcrumb {
  line-height: 1;
}
</style>
