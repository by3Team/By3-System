<template>
  <div class="rich-text-editor">
    <Toolbar
      v-if="!disabled"
      :editor="editorRef"
      :defaultConfig="toolbarConfig"
      mode="default"
      style="border-bottom: 1px solid #ccc"
    />
    <Editor
      v-model="valueHtml"
      :defaultConfig="editorConfig"
      mode="default"
      :style="{ height: height + 'px', 'overflow-y': 'hidden' }"
      @onCreated="handleCreated"
      @onChange="handleChange"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch, shallowRef, onBeforeUnmount } from 'vue'
import { Editor, Toolbar } from '@wangeditor/editor-for-vue'
import '@wangeditor/editor/dist/css/style.css'
import type { IDomEditor, IToolbarConfig, IEditorConfig } from '@wangeditor/editor'
import { useAuthStore } from '@/store/auth'

const props = defineProps<{
  modelValue?: string
  height?: number
  placeholder?: string
  disabled?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const editorRef = shallowRef<IDomEditor>()
const valueHtml = ref(props.modelValue || '')

watch(() => props.modelValue, (val) => {
  if (val !== valueHtml.value) {
    valueHtml.value = val || ''
  }
})

const toolbarConfig: Partial<IToolbarConfig> = {
  excludeKeys: ['fullScreen', 'group-video', 'group-file']
}

const auth = useAuthStore()

const editorConfig: Partial<IEditorConfig> = {
  placeholder: props.placeholder || '请输入内容...',
  MENU_CONF: {
    uploadImage: {
      server: '/api/v1/singlefiles/upload',
      fieldName: 'file',
      maxFileSize: 10 * 1024 * 1024,
      allowedFileTypes: ['image/*'],
      meta: { category: 'image' },
      headers: {
        ...(auth.token ? { Authorization: `Bearer ${auth.token}` } : {}),
        'Idempotency-Key': typeof crypto !== 'undefined' && crypto.randomUUID ? crypto.randomUUID() : ''
      },
      customInsert(res: any, insertFn: (url: string, alt?: string, href?: string) => void) {
        const url = res.data?.downloadUrl || res.data?.url
        if (url) insertFn(url, '', url)
      }
    }
  }
}

function setEditorDisabled(disabled: boolean) {
  const editor = editorRef.value
  if (!editor) return
  if (disabled) {
    editor.disable()
  } else {
    editor.enable()
  }
}

function handleCreated(editor: IDomEditor) {
  editorRef.value = editor
  setEditorDisabled(props.disabled || false)
}

function handleChange(editor: IDomEditor) {
  emit('update:modelValue', editor.getHtml())
}

watch(() => props.disabled, (disabled) => {
  setEditorDisabled(disabled || false)
})

onBeforeUnmount(() => {
  const editor = editorRef.value
  if (editor) editor.destroy()
})
</script>

<style scoped>
.rich-text-editor {
  border: 1px solid #ccc;
  border-radius: 4px;
  z-index: 100;
}
:deep(.w-e-text-container) {
  z-index: 99 !important;
}
:deep(.w-e-toolbar) {
  z-index: 100 !important;
}
</style>
