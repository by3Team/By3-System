<template>
  <div v-if="settingsStore.settings.showTagsView" class="tags-view">
    <div ref="scrollWrapper" class="tags-wrapper" @wheel="handleWheel">
      <div
        v-for="tag in tagsStore.visitedViews"
        :key="tag.path"
        class="tag-item"
        :class="{ active: isActive(tag), affix: isAffix(tag) }"
        :data-path="tag.path"
        @click="goTo(tag)"
        @contextmenu.prevent="openContextMenu($event, tag)"
      >
        <span class="tag-title">{{ tag.title }}</span>
        <el-icon v-if="!isAffix(tag)" class="tag-close" @click.stop="closeTag(tag)"><Close /></el-icon>
      </div>
    </div>

    <ul
      v-show="contextMenu.visible"
      class="context-menu"
      :style="{ left: contextMenu.left + 'px', top: contextMenu.top + 'px' }"
    >
      <li @click="refreshSelected">刷新</li>
      <li v-if="!isAffix(selectedTag)" @click="closeSelected">关闭</li>
      <li @click="closeOthers">关闭其他</li>
      <li @click="closeLeft">关闭左侧</li>
      <li @click="closeRight">关闭右侧</li>
      <li @click="closeAll">关闭全部</li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSettingsStore } from '@/store/settings'
import { useTagsStore, type TagView } from '@/store/tags'
import { Close } from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const settingsStore = useSettingsStore()
const tagsStore = useTagsStore()

const scrollWrapper = ref<HTMLElement>()

const contextMenu = ref({ visible: false, left: 0, top: 0 })
const selectedTag = ref<TagView | null>(null)

function isActive(tag: TagView) {
  return tag.path === route.path
}

function isAffix(tag: TagView | null | undefined) {
  if (!tag) return false
  return tagsStore.isAffixPath(tag.path)
}

function goTo(tag: TagView) {
  if (tag.path === route.path) return
  router.push({ path: tag.fullPath, query: tag.query, params: tag.params })
}

function closeTag(tag: TagView) {
  if (isAffix(tag)) return
  const isCurrent = tag.path === route.path
  const idx = tagsStore.delView(tag)
  if (isCurrent && idx !== -1) {
    const next = tagsStore.visitedViews[idx] || tagsStore.visitedViews[idx - 1]
    if (next) {
      router.push({ path: next.fullPath, query: next.query, params: next.params })
    } else {
      router.push('/dashboard')
    }
  }
}

function openContextMenu(e: MouseEvent, tag: TagView) {
  selectedTag.value = tag
  contextMenu.value = {
    visible: true,
    left: e.clientX,
    top: e.clientY,
  }
  // 如果菜单位置超出视口右侧/下侧，则自动偏移
  nextTick(() => {
    const menuEl = document.querySelector('.context-menu') as HTMLElement
    if (menuEl) {
      const rect = menuEl.getBoundingClientRect()
      if (rect.right > window.innerWidth) {
        contextMenu.value.left = window.innerWidth - rect.width - 10
      }
      if (rect.bottom > window.innerHeight) {
        contextMenu.value.top = window.innerHeight - rect.height - 10
      }
    }
  })
}

function closeContextMenu(e?: MouseEvent) {
  if (e) {
    const menuEl = document.querySelector('.context-menu') as HTMLElement | null
    if (menuEl && menuEl.contains(e.target as Node)) return
  }
  contextMenu.value.visible = false
}

function refreshSelected() {
  if (!selectedTag.value) return
  const fullPath = selectedTag.value.fullPath
  router.replace({ path: '/redirect' + fullPath })
  closeContextMenu()
}

function closeSelected() {
  if (selectedTag.value) {
    closeTag(selectedTag.value)
  }
  closeContextMenu()
}

function closeOthers() {
  if (!selectedTag.value) return
  tagsStore.delOthers(selectedTag.value)
  if (route.path !== selectedTag.value.path) {
    router.push({ path: selectedTag.value.fullPath, query: selectedTag.value.query, params: selectedTag.value.params })
  }
  closeContextMenu()
}

function closeLeft() {
  if (!selectedTag.value) return
  const currentIdx = tagsStore.visitedViews.findIndex((v) => v.path === route.path)
  const selectedIdx = tagsStore.visitedViews.findIndex((v) => v.path === selectedTag.value.path)
  tagsStore.delLeft(selectedTag.value)
  if (currentIdx !== -1 && currentIdx < selectedIdx) {
    router.push({ path: selectedTag.value.fullPath, query: selectedTag.value.query, params: selectedTag.value.params })
  }
  closeContextMenu()
}

function closeRight() {
  if (!selectedTag.value) return
  const currentIdx = tagsStore.visitedViews.findIndex((v) => v.path === route.path)
  const selectedIdx = tagsStore.visitedViews.findIndex((v) => v.path === selectedTag.value!.path)
  tagsStore.delRight(selectedTag.value)
  if (currentIdx !== -1 && currentIdx > selectedIdx) {
    router.push({ path: selectedTag.value.fullPath, query: selectedTag.value.query, params: selectedTag.value.params })
  }
  closeContextMenu()
}

function closeAll() {
  tagsStore.delAll()
  if (route.path !== '/dashboard') {
    router.push('/dashboard')
  }
  closeContextMenu()
}

function handleWheel(e: WheelEvent) {
  if (!scrollWrapper.value) return
  e.preventDefault()
  scrollWrapper.value.scrollLeft += e.deltaY
}

function moveToCurrentTag() {
  nextTick(() => {
    if (!scrollWrapper.value) return
    const currentEl = scrollWrapper.value.querySelector(`[data-path="${route.path}"]`) as HTMLElement
    if (currentEl) {
      currentEl.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' })
    }
  })
}

watch(
  () => route.path,
  () => {
    tagsStore.addView(route)
    moveToCurrentTag()
  },
  { immediate: true }
)

onMounted(() => {
  tagsStore.addView(route)
  document.addEventListener('click', closeContextMenu)
})

onUnmounted(() => {
  document.removeEventListener('click', closeContextMenu)
})
</script>

<script lang="ts">
export default { name: 'TagsView' }
</script>

<style scoped>
.tags-view {
  display: flex;
  align-items: center;
  background: #ffffff;
  border-bottom: 1px solid #f1f5f9;
  height: 44px;
  padding: 0 16px;
  position: relative;
}
.tags-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
  overflow-x: auto;
  overflow-y: hidden;
  white-space: nowrap;
  width: 100%;
  scroll-behavior: smooth;
}
.tags-wrapper::-webkit-scrollbar {
  display: none;
}
.tag-item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 13px;
  color: #6b7280;
  background: #f1f5f9;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid transparent;
  flex-shrink: 0;
  user-select: none;
}
.tag-item:hover {
  background: var(--naive-hover-bg);
  color: var(--naive-hover-text);
}
.tag-item.active {
  background: var(--el-color-primary);
  color: #ffffff;
  box-shadow: 0 4px 10px rgba(64, 158, 255, 0.25);
}
.tag-item.affix {
  padding-right: 14px;
}
.tag-title {
  max-width: 160px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.tag-close {
  font-size: 12px;
  border-radius: 50%;
  padding: 2px;
  transition: background 0.2s, color 0.2s;
}
.tag-close:hover {
  background: rgba(0, 0, 0, 0.12);
  color: var(--naive-hover-text);
}
.context-menu {
  position: fixed;
  z-index: 3000;
  background: #ffffff;
  border-radius: 8px;
  box-shadow: var(--naive-shadow-2);
  padding: 6px 0;
  margin: 0;
  list-style: none;
  min-width: 120px;
  border: 1px solid #f1f5f9;
}
.context-menu li {
  padding: 8px 16px;
  font-size: 13px;
  color: var(--naive-text);
  cursor: pointer;
  transition: background 0.2s;
}
.context-menu li:hover {
  background: var(--naive-hover-bg);
  color: var(--naive-hover-text);
}
</style>
