<template>
  <div class="json-block-wrapper">
    <div class="json-toolbar">
      <span class="json-tip">{{ isJson ? 'JSON' : '文本' }} · {{ charCount }} 字符</span>
      <el-button link type="primary" size="small" @click="copy">复制</el-button>
    </div>
    <pre class="json-block"><code>{{ displayed }}</code></pre>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ElMessage } from 'element-plus'

const props = defineProps<{
  content?: string | null
}>()

const displayed = computed(() => {
  if (!props.content || props.content.trim() === '') return '（无内容）'
  return props.content
})

const isJson = computed(() => {
  if (!props.content) return false
  const trimmed = props.content.trim()
  return (trimmed.startsWith('{') && trimmed.endsWith('}')) || (trimmed.startsWith('[') && trimmed.endsWith(']'))
})

const charCount = computed(() => props.content?.length ?? 0)

async function copy() {
  try {
    await navigator.clipboard.writeText(displayed.value)
    ElMessage.success('已复制')
  } catch {
    ElMessage.error('复制失败')
  }
}
</script>

<style scoped>
.json-block-wrapper {
  border: 1px solid #f1f5f9;
  border-radius: 10px;
  overflow: hidden;
}
.json-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}
.json-tip {
  font-size: 12px;
  color: #6b7280;
}
.json-block {
  margin: 0;
  padding: 12px;
  max-height: 420px;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-all;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 13px;
  line-height: 1.6;
  color: #1f2937;
  background: #ffffff;
}
</style>
