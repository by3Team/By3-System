<template>
  <div class="sidebar" :class="{ collapsed: isCollapse, dark: settings.sidebarColor === 'dark' }">
    <div v-if="settings.showLogo" class="logo">
      <img v-if="!isCollapse" src="/favicon.svg" alt="logo" class="logo-img">
      <span class="logo-text">{{ isCollapse ? 'B' : 'By3系统' }}</span>
    </div>
    <el-scrollbar class="menu-scrollbar">
      <el-menu
        :default-active="activeMenu"
        :collapse="isCollapse"
        :collapse-transition="false"
        router
        class="sidebar-menu"
        :text-color="menuTextColor"
        :active-text-color="settingsStore.settings.sidebarColor === 'dark' ? '#79bbff' : 'var(--el-color-primary)'"
        :style="{ '--el-menu-text-color': menuTextColor }"
      >
        <menu-item v-for="menu in menus" :key="menu.id" :menu="menu" />
      </el-menu>
    </el-scrollbar>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useSettingsStore } from '@/store/settings'
import MenuItem from './MenuItem.vue'

const props = defineProps<{
  menus: any[]
  isCollapse: boolean
}>()

const route = useRoute()
const settingsStore = useSettingsStore()
const { settings } = storeToRefs(settingsStore)

const activeMenu = computed(() => route.path)

const menuTextColor = computed(() => settings.value.sidebarColor === 'dark' ? '#bfcbd9' : '#4b5563')
</script>

<script lang="ts">
export default { name: 'Sidebar' }
</script>

<style scoped>
.sidebar {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #ffffff;
  transition: width 0.3s;
  width: 220px;
  box-shadow: 2px 0 12px rgba(0, 0, 0, 0.04);
}
.sidebar.collapsed {
  width: 64px;
}
.sidebar.dark {
  background: #1e293b;
}
.logo {
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #1f2937;
  font-size: 18px;
  font-weight: 700;
  border-bottom: 1px solid #f1f5f9;
  gap: 10px;
  padding: 0 15px;
}
.sidebar.dark .logo {
  color: #f8fafc;
  border-bottom-color: rgba(255, 255, 255, 0.08);
}
.logo-img {
  width: 30px;
  height: 30px;
}
.logo-text {
  white-space: nowrap;
  overflow: hidden;
}
.menu-scrollbar {
  flex: 1;
  padding: 8px 0;
}
.sidebar-menu {
  border-right: none;
}
.sidebar-menu:not(.el-menu--collapse) {
  width: 220px;
}
</style>
