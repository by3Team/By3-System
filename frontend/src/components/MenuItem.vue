<template>
  <el-menu-item v-if="menu.menuType === 2" :index="menu.route">
    <el-icon v-if="iconComponent"><component :is="iconComponent" /></el-icon>
    <template #title>
      <span>{{ menu.menuName }}</span>
    </template>
  </el-menu-item>
  <el-sub-menu v-else-if="menu.menuType === 1" :index="menu.route || String(menu.id)">
    <template #title>
      <el-icon v-if="iconComponent"><component :is="iconComponent" /></el-icon>
      <span>{{ menu.menuName }}</span>
    </template>
    <menu-item v-for="child in menu.children" :key="child.id" :menu="child" />
  </el-sub-menu>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

const props = defineProps<{ menu: any }>()

const iconComponent = computed(() => {
  const iconName = props.menu.icon?.trim?.() || ''
  if (!iconName) return null
  const icon = (ElementPlusIconsVue as Record<string, any>)[iconName]
  if (icon) return icon
  console.warn(`[MenuItem] 菜单图标未找到: ${iconName}，已使用默认图标`)
  return ElementPlusIconsVue.SetUp
})
</script>

<script lang="ts">
export default { name: 'MenuItem' }
</script>
