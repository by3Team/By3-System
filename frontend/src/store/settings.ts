import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

const STORAGE_KEY = 'app_settings'

export interface AppSettings {
  themeColor: string
  sidebarColor: 'dark' | 'light'
  layoutMode: 'vertical' | 'horizontal'
  showBreadcrumb: boolean
  showTagsView: boolean
  showLogo: boolean
  isDark: boolean
}

const defaultSettings: AppSettings = {
  themeColor: '#409EFF',
  sidebarColor: 'light',
  layoutMode: 'vertical',
  showBreadcrumb: true,
  showTagsView: true,
  showLogo: true,
  isDark: false
}

function loadSettings(): AppSettings {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    return saved ? { ...defaultSettings, ...JSON.parse(saved) } : { ...defaultSettings }
  } catch {
    return { ...defaultSettings }
  }
}

export const useSettingsStore = defineStore('settings', () => {
  const settings = ref<AppSettings>(loadSettings())

  function updateSetting<K extends keyof AppSettings>(key: K, value: AppSettings[K]) {
    settings.value[key] = value
  }

  function resetSettings() {
    settings.value = { ...defaultSettings }
  }

  function applyThemeColor(color: string) {
    const el = document.documentElement
    el.style.setProperty('--el-color-primary', color)
    // 生成一组主色阶
    const shades = ['#ecf5ff', '#d9ecff', '#c6e2ff', '#b3d8ff', '#a0cfff', '#8cc5ff', '#79bbff', '#66b1ff', '#53a8ff', '#409EFF', '#3a8ee6', '#337ecc']
    shades.forEach((c, i) => {
      el.style.setProperty(`--el-color-primary-light-${i}`, c)
    })
  }

  function toggleDarkMode() {
    settings.value.isDark = !settings.value.isDark
    if (settings.value.isDark) {
      document.documentElement.classList.add('dark')
    } else {
      document.documentElement.classList.remove('dark')
    }
  }

  watch(settings, (val) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(val))
    applyThemeColor(val.themeColor)
    if (val.isDark) {
      document.documentElement.classList.add('dark')
    } else {
      document.documentElement.classList.remove('dark')
    }
  }, { deep: true, immediate: true })

  return {
    settings,
    updateSetting,
    resetSettings,
    toggleDarkMode
  }
})
