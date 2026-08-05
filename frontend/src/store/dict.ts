import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { dictDataApi } from '@/api'

export interface DictItem {
  id: string
  dictLabel: string
  dictValue: string
  remark: string
  sortOrder: number
  isDefault: boolean
  isEnabled: boolean
}

const STORAGE_KEY = 'app_dict_cache'
const CACHE_VERSION_KEY = 'app_dict_cache_version'

function loadCache(): Record<string, DictItem[]> {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    return saved ? JSON.parse(saved) : {}
  } catch {
    return {}
  }
}

function saveCache(cache: Record<string, DictItem[]>) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(cache))
  localStorage.setItem(CACHE_VERSION_KEY, Date.now().toString())
}

export const useDictStore = defineStore('dict', () => {
  const dictCache = ref<Record<string, DictItem[]>>(loadCache())
  const loading = ref(false)
  const loaded = ref(false)

  function getDict(typeCode: string) {
    return dictCache.value[typeCode] || []
  }

  function getDictLabel(typeCode: string, value: string | number | boolean | undefined) {
    if (value === undefined || value === null || value === '') return '-'
    const items = dictCache.value[typeCode] || []
    const item = items.find((d) => d.dictValue === String(value))
    return item?.dictLabel || String(value)
  }

  function getDictValue(typeCode: string, label: string) {
    const items = dictCache.value[typeCode] || []
    const item = items.find((d) => d.dictLabel === label)
    return item?.dictValue
  }

  async function loadDict(typeCodes: string[]) {
    const results = await Promise.all(
      typeCodes.map(async (code) => {
        const items = await dictDataApi.getByTypeCode(code)
        return { code, items: items as DictItem[] }
      })
    )
    results.forEach(({ code, items }) => {
      dictCache.value[code] = items.filter((item) => item.isEnabled !== false)
    })
    saveCache(dictCache.value)
  }

  async function loadAll(force = false) {
    if (loaded.value && !force) return
    loading.value = true
    try {
      const systemDictCodes = [
        'sys_gender',
        'sys_status',
        'sys_menu_type',
        'sys_yes_no',
        'sys_file_category'
      ]
      await loadDict(systemDictCodes)
      loaded.value = true
    } finally {
      loading.value = false
    }
  }

  function refresh() {
    return loadAll(true)
  }

  function clearCache() {
    dictCache.value = {}
    localStorage.removeItem(STORAGE_KEY)
    localStorage.removeItem(CACHE_VERSION_KEY)
    loaded.value = false
  }

  return {
    dictCache,
    loading,
    loaded,
    getDict,
    getDictLabel,
    getDictValue,
    loadDict,
    loadAll,
    refresh,
    clearCache
  }
})
