import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import type { RouteLocationNormalized } from 'vue-router'

export interface TagView {
  path: string
  fullPath: string
  title: string
  name: string | symbol | undefined
  query?: Record<string, any>
  params?: Record<string, any>
}

const STORAGE_KEY = 'visited_tags'

function isAffixTag(route: RouteLocationNormalized): boolean {
  return !!route.meta?.affix || route.path === '/dashboard'
}

export const useTagsStore = defineStore('tags', () => {
  const visitedViews = ref<TagView[]>([])

  function loadFromStorage() {
    try {
      const saved = localStorage.getItem(STORAGE_KEY)
      if (saved) {
        const parsed = JSON.parse(saved) as TagView[]
        // 仅保留 affix 标签，query/params 不持久化
        visitedViews.value = parsed.filter((t) => t.path === '/dashboard')
      }
    } catch {
      visitedViews.value = []
    }
  }

  function addView(view: RouteLocationNormalized) {
    if (!view.meta?.title) return
    const exists = visitedViews.value.find((v) => v.path === view.path)
    if (exists) {
      exists.fullPath = view.fullPath
      exists.query = view.query
      exists.params = view.params
      return
    }
    visitedViews.value.push({
      path: view.path,
      fullPath: view.fullPath,
      title: (view.meta.title as string) || '未命名',
      name: view.name,
      query: { ...view.query },
      params: { ...view.params }
    })
  }

  function delView(view: TagView) {
    const idx = visitedViews.value.findIndex((v) => v.path === view.path)
    if (idx === -1) return -1
    visitedViews.value.splice(idx, 1)
    return idx
  }

  function delOthers(view: TagView) {
    visitedViews.value = visitedViews.value.filter((v) => v.path === view.path || isAffixPath(v.path))
  }

  function delLeft(view: TagView) {
    const idx = visitedViews.value.findIndex((v) => v.path === view.path)
    if (idx === -1) return
    visitedViews.value = [
      ...visitedViews.value.filter((v, i) => i >= idx || isAffixPath(v.path)),
    ]
  }

  function delRight(view: TagView) {
    const idx = visitedViews.value.findIndex((v) => v.path === view.path)
    if (idx === -1) return
    visitedViews.value = [
      ...visitedViews.value.filter((v, i) => i <= idx || isAffixPath(v.path)),
    ]
  }

  function delAll() {
    visitedViews.value = visitedViews.value.filter((v) => isAffixPath(v.path))
  }

  function isAffixPath(path: string): boolean {
    return path === '/dashboard'
  }

  function reset() {
    visitedViews.value = []
    localStorage.removeItem(STORAGE_KEY)
  }

  function saveToStorage() {
    try {
      const storable = visitedViews.value.map((v) => ({
        path: v.path,
        fullPath: v.fullPath,
        title: v.title,
        name: v.name,
      }))
      localStorage.setItem(STORAGE_KEY, JSON.stringify(storable))
    } catch {
      // ignore
    }
  }

  watch(
    visitedViews,
    () => {
      saveToStorage()
    },
    { deep: true }
  )

  loadFromStorage()

  return {
    visitedViews,
    addView,
    delView,
    delOthers,
    delLeft,
    delRight,
    delAll,
    isAffixPath,
    reset,
  }
})
