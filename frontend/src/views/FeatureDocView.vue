<template>
  <div class="feature-doc-page">
    <div class="doc-header">
      <h1>{{ feature?.module || '功能文档' }}</h1>
      <p v-if="feature">{{ feature.description }}</p>
    </div>
    <el-card shadow="hover" class="doc-card" v-loading="loading">
      <!-- nosemgrep: javascript.vue.security.audit.xss.templates.avoid-v-html.avoid-v-html -->
      <div class="markdown-body" v-html="renderedHtml"></div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { marked } from 'marked'
import DOMPurify from 'dompurify'
import { APP_FEATURES } from '@/constants/app'

const route = useRoute()
const loading = ref(false)
const renderedHtml = ref('')

const code = computed(() => route.params.code as string)
const feature = computed(() => APP_FEATURES.find((f) => f.code === code.value))

async function loadDoc() {
  if (!code.value) return
  loading.value = true
  try {
    const res = await fetch(`/docs/features/${code.value}.md`)
    const text = res.ok ? await res.text() : '# 文档未找到\n\n该功能暂无详细文档。'
    // 虽然文档来自本地静态文件，仍使用 DOMPurify 净化，防止潜在 XSS
    const rawHtml = await marked.parse(text)
    renderedHtml.value = DOMPurify.sanitize(rawHtml)
  } catch {
    renderedHtml.value = DOMPurify.sanitize(await marked.parse('# 加载失败\n\n请刷新页面重试。'))
  } finally {
    loading.value = false
  }
}

onMounted(loadDoc)
watch(code, loadDoc)
</script>

<script lang="ts">
export default { name: 'FeatureDocView' }
</script>

<style scoped>
.feature-doc-page {
  max-width: 900px;
  margin: 0 auto;
  padding: 24px;
}
.doc-header {
  text-align: center;
  margin-bottom: 24px;
}
.doc-header h1 {
  margin: 0 0 12px;
  font-size: 28px;
  color: #1f2937;
}
.doc-header p {
  margin: 0;
  color: #6b7280;
  font-size: 15px;
}
.doc-card {
  border-radius: 16px;
}
.markdown-body {
  line-height: 1.8;
  color: #374151;
}
.markdown-body :deep(h1) {
  font-size: 24px;
  color: #1f2937;
  border-bottom: 1px solid #e5e7eb;
  padding-bottom: 12px;
  margin-bottom: 20px;
}
.markdown-body :deep(h2) {
  font-size: 20px;
  color: #1f2937;
  margin-top: 28px;
  margin-bottom: 12px;
}
.markdown-body :deep(p) {
  margin: 12px 0;
}
.markdown-body :deep(ul),
.markdown-body :deep(ol) {
  padding-left: 24px;
  margin: 12px 0;
}
.markdown-body :deep(li) {
  margin: 6px 0;
}
.markdown-body :deep(code) {
  background: #f3f4f6;
  padding: 2px 6px;
  border-radius: 4px;
  font-family: monospace;
}
.markdown-body :deep(pre) {
  background: #1f2937;
  color: #f9fafb;
  padding: 16px;
  border-radius: 8px;
  overflow-x: auto;
}
.markdown-body :deep(pre code) {
  background: transparent;
  color: inherit;
  padding: 0;
}
</style>
