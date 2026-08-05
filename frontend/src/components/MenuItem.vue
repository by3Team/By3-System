<template>
  <el-menu-item v-if="menu.menuType === 2" :index="menu.route">
    <el-icon v-if="menu.icon"><component :is="resolveIcon(menu.icon)" /></el-icon>
    <template #title>
      <span>{{ menu.menuName }}</span>
    </template>
  </el-menu-item>
  <el-sub-menu v-else-if="menu.menuType === 1" :index="menu.route || String(menu.id)">
    <template #title>
      <el-icon v-if="menu.icon"><component :is="resolveIcon(menu.icon)" /></el-icon>
      <span>{{ menu.menuName }}</span>
    </template>
    <menu-item v-for="child in menu.children" :key="child.id" :menu="child" />
  </el-sub-menu>
</template>

<script setup lang="ts">
import { useSettingsStore } from '@/store/settings'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

const props = defineProps<{ menu: any }>()
const settings = useSettingsStore()

function resolveIcon(iconName: string) {
  // 通过图标映射直接返回组件对象，避免字符串动态解析异常；不存在时返回默认图标
  const icon = (ElementPlusIconsVue as Record<string, any>)[iconName]
  if (icon) return icon
  console.warn(`[MenuItem] 菜单图标未找到: ${iconName}，已使用默认图标`)
  return ElementPlusIconsVue.SetUp
}
</script>

<script lang="ts">
export default { name: 'MenuItem' }
</script>
