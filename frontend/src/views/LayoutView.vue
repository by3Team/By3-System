<template>
  <el-container class="layout-container" :class="{ dark: settingsStore.settings.isDark }">
    <el-aside :width="isCollapse ? '64px' : '220px'" class="layout-aside">
      <sidebar :menus="auth.menus" :is-collapse="isCollapse" />
    </el-aside>
    <el-container class="layout-main">
      <el-header class="layout-header">
        <app-header :is-collapse="isCollapse" @toggle-collapse="toggleCollapse" @open-theme="themeVisible = true" />
      </el-header>
      <tags-view />
      <el-main class="layout-content">
        <router-view v-slot="{ Component }">
          <transition name="fade-transform" mode="out-in">
            <component :is="Component" :key="route.path" />
          </transition>
        </router-view>
      </el-main>
      <el-footer class="layout-footer" height="40px">
        <div class="footer-content">
          <span>{{ APP_NAME }} {{ APP_VERSION }}</span>
          <span class="footer-divider">|</span>
          <span>{{ APP_COPYRIGHT }}</span>
        </div>
      </el-footer>
    </el-container>
    <theme-settings v-model="themeVisible" />
  </el-container>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/store/auth'
import { useSettingsStore } from '@/store/settings'
import { APP_NAME, APP_VERSION, APP_COPYRIGHT } from '@/constants/app'
import Sidebar from '@/components/Sidebar.vue'
import AppHeader from '@/components/AppHeader.vue'
import TagsView from '@/components/TagsView.vue'
import ThemeSettings from '@/components/ThemeSettings.vue'

const auth = useAuthStore()
const settingsStore = useSettingsStore()
const route = useRoute()

const isCollapse = ref(false)
const themeVisible = ref(false)

function toggleCollapse() {
  isCollapse.value = !isCollapse.value
}
</script>

<script lang="ts">
export default { name: 'LayoutView' }
</script>

<style scoped>
.layout-container {
  height: 100vh;
}
.layout-aside {
  transition: width 0.3s;
  overflow: hidden;
}
.layout-main {
  display: flex;
  flex-direction: column;
  background: #f5f7fa;
}
.layout-header {
  padding: 0;
  height: 64px;
}
.layout-content {
  padding: 16px;
  overflow: auto;
}
.layout-footer {
  background: #ffffff;
  border-top: 1px solid #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #909399;
  font-size: 13px;
}
.footer-content {
  display: flex;
  align-items: center;
  gap: 12px;
}
.footer-divider {
  color: #dcdfe6;
}

.fade-transform-enter-active,
.fade-transform-leave-active {
  transition: all 0.3s;
}
.fade-transform-enter-from {
  opacity: 0;
  transform: translateX(-20px);
}
.fade-transform-leave-to {
  opacity: 0;
  transform: translateX(20px);
}
</style>
