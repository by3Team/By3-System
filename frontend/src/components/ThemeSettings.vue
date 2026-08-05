<template>
  <el-drawer v-model="visible" title="主题设置" size="300px">
    <div class="setting-group">
      <h4>主题色</h4>
      <div class="color-list">
        <div
          v-for="color in themeColors"
          :key="color"
          class="color-item"
          :style="{ background: color }"
          :class="{ active: settingsStore.settings.themeColor === color }"
          @click="settingsStore.updateSetting('themeColor', color)"
        />
      </div>
    </div>

    <div class="setting-group">
      <h4>侧边栏风格</h4>
      <el-radio-group v-model="settingsStore.settings.sidebarColor">
        <el-radio-button label="dark">深色</el-radio-button>
        <el-radio-button label="light">浅色</el-radio-button>
      </el-radio-group>
    </div>

    <div class="setting-group">
      <h4>界面显示</h4>
      <div class="setting-item">
        <span>显示面包屑</span>
        <el-switch v-model="settingsStore.settings.showBreadcrumb" />
      </div>
      <div class="setting-item">
        <span>显示标签页</span>
        <el-switch v-model="settingsStore.settings.showTagsView" />
      </div>
      <div class="setting-item">
        <span>显示 Logo</span>
        <el-switch v-model="settingsStore.settings.showLogo" />
      </div>
    </div>

    <div class="setting-group">
      <h4>暗黑模式</h4>
      <el-switch v-model="settingsStore.settings.isDark" active-text="开启" inactive-text="关闭" />
    </div>

    <template #footer>
      <el-button @click="settingsStore.resetSettings">恢复默认</el-button>
      <el-button type="primary" @click="visible = false">确定</el-button>
    </template>
  </el-drawer>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useSettingsStore } from '@/store/settings'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const settingsStore = useSettingsStore()

const visible = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const themeColors = ['#409EFF', '#67C23A', '#E6A23C', '#F56C6C', '#909399', '#8E44AD']
</script>

<script lang="ts">
export default { name: 'ThemeSettings' }
</script>

<style scoped>
.setting-group {
  margin-bottom: 24px;
}
.setting-group h4 {
  margin: 0 0 12px 0;
  font-size: 14px;
  color: #606266;
}
.color-list {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}
.color-item {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  cursor: pointer;
  border: 2px solid transparent;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.12);
  transition: transform 0.2s;
}
.color-item:hover {
  transform: scale(1.1);
}
.color-item.active {
  border-color: var(--el-color-primary);
  box-shadow: 0 0 0 3px rgba(64, 158, 255, 0.25);
  transform: scale(1.1);
}
.setting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  font-size: 14px;
  color: #606266;
}
</style>
