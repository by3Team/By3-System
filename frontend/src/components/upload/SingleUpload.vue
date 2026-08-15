<template>
  <el-upload
    :action="uploadAction"
    :headers="headers"
    :before-upload="beforeUpload"
    :on-success="onSuccess"
    :on-error="onError"
    :show-file-list="false"
    :data="{ category }"
    :accept="accept"
  >
    <el-button type="primary" :loading="uploading">{{ buttonText }}</el-button>
  </el-upload>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { v4 as uuidv4 } from 'uuid'
import { useAuthStore } from '@/store/auth'
import { useDictStore } from '@/store/dict'

const props = defineProps<{
  buttonText?: string
  category?: string
}>()

const emit = defineEmits<{
  (e: 'success', data: any): void
}>()

const auth = useAuthStore()
const dictStore = useDictStore()
const uploading = ref(false)

const uploadAction = computed(() => `${import.meta.env.VITE_API_BASE_URL || '/api'}/v1/singlefiles/upload`)
const headers = computed(() => {
  const h: Record<string, string> = {}
  if (auth.token) h.Authorization = `Bearer ${auth.token}`
  h['Idempotency-Key'] = typeof crypto !== 'undefined' && crypto.randomUUID ? crypto.randomUUID() : uuidv4()
  return h
})
const accept = computed(() => {
  const category = props.category || 'general'
  const item = dictStore.getDict('sys_file_category').find((d) => d.dictValue === category)
  const remark = item?.remark
  if (!remark || remark === '*') return ''
  return remark
})

function beforeUpload() {
  uploading.value = true
  return true
}

function onSuccess(response: any) {
  uploading.value = false
  ElMessage.success(response.message || '上传成功')
  emit('success', response.data)
}

function onError(err: any) {
  uploading.value = false
  const msg = err?.message || '上传失败'
  ElMessage.error(msg)
}
</script>
