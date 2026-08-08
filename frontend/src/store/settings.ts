import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

const STORAGE_KEY = 'app_settings'

function hexToRgb(hex: string): { r: number; g: number; b: number } {
  const clean = hex.replace('#', '')
  const bigint = parseInt(clean, 16)
  const r = (bigint >> 16) & 255
  const g = (bigint >> 8) & 255
  const b = bigint & 255
  return { r, g, b }
}

function rgbToHex(r: number, g: number, b: number): string {
  return '#' + [r, g, b].map((v) => {
    const hex = Math.max(0, Math.min(255, Math.round(v))).toString(16)
    return hex.length === 1 ? '0' + hex : hex
  }).join('')
}

function mixColor(color: string, mixColor: string, weight: number): string {
  const c1 = hexToRgb(color)
  const c2 = hexToRgb(mixColor)
  const w = Math.max(0, Math.min(100, weight)) / 100
  return rgbToHex(
    c1.r * (1 - w) + c2.r * w,
    c1.g * (1 - w) + c2.g * w,
    c1.b * (1 - w) + c2.b * w
  )
}

function generatePrimaryShades(color: string): Record<string, string> {
  return {
    '--el-color-primary': color,
    '--el-color-primary-light-3': mixColor(color, '#ffffff', 30),
    '--el-color-primary-light-5': mixColor(color, '#ffffff', 50),
    '--el-color-primary-light-7': mixColor(color, '#ffffff', 70),
    '--el-color-primary-light-8': mixColor(color, '#ffffff', 80),
    '--el-color-primary-light-9': mixColor(color, '#ffffff', 90),
    '--el-color-primary-dark-2': mixColor(color, '#000000', 20),
    // 兼容项目内旧的 light-0 ~ light-11 用法
    '--el-color-primary-light-0': mixColor(color, '#ffffff', 90),
    '--el-color-primary-light-1': mixColor(color, '#ffffff', 80),
    '--el-color-primary-light-2': mixColor(color, '#ffffff', 70),
    '--el-color-primary-light-4': mixColor(color, '#ffffff', 60),
    '--el-color-primary-light-6': mixColor(color, '#ffffff', 40),
    '--el-color-primary-light-10': mixColor(color, '#000000', 10),
    '--el-color-primary-light-11': mixColor(color, '#000000', 20)
  }
}

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
    const shades = generatePrimaryShades(color)
    Object.entries(shades).forEach(([key, value]) => {
      el.style.setProperty(key, value)
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
