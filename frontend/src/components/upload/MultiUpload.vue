<template>
  <el-upload
    :action="uploadAction"
    :headers="headers"
    :before-upload="beforeUpload"
    :on-success="onSuccess"
    :on-error="onError"
    :on-change="onChange"
    :on-remove="onRemove"
    :auto-upload="false"
    :data="{ category }"
    :accept="accept"
    multiple
    drag
    :file-list="fileList"
    ref="uploadRef"
    class="multi-upload"
  >
    <el-icon class="el-icon--upload" :size="48"><UploadFilled /></el-icon>
    <div class="el-upload__text">
      将文件拖到此处，或 <em>点击上传</em>
    </div>
    <template #tip>
      <div class="el-upload__tip">支持多文件同时上传，可拖拽文件到上方区域</div>
    </template>
  </el-upload>
  <div class="upload-actions">
    <el-button type="success" @click="submitUpload" :loading="uploading">开始上传</el-button>
    <el-button @click="clearFiles">清空列表</el-button>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { UploadFilled } from '@element-plus/icons-vue'
import { useAuthStore } from '@/store/auth'
import { useDictStore } from '@/store/dict'
import type { UploadFile, UploadFiles, UploadInstance } from 'element-plus'

const props = defineProps<{
  category?: string
}>()

const emit = defineEmits<{
  (e: 'success', data: any): void
}>()

const auth = useAuthStore()
const dictStore = useDictStore()
const uploading = ref(false)
const fileList = ref<UploadFiles>([])
const uploadRef = ref<UploadInstance>()

const uploadAction = computed(() => `${import.meta.env.VITE_API_BASE_URL || '/api'}/v1/multifiles/upload`)
const headers = computed(() => auth.token ? { Authorization: `Bearer ${auth.token}` } : {})
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

function submitUpload() {
  if (fileList.value.length === 0) {
    ElMessage.warning('请选择文件')
    return
  }
  uploadRef.value?.submit()
}

function onSuccess(response: any) {
  uploading.value = false
  ElMessage.success(response.message || '上传成功')
  fileList.value = []
  emit('success', response.data)
}

function onError(err: any) {
  uploading.value = false
  const msg = err?.message || '上传失败'
  ElMessage.error(msg)
}

function onChange(_file: UploadFile, files: UploadFiles) {
  fileList.value = files
}

function onRemove(_file: UploadFile, files: UploadFiles) {
  fileList.value = files
}

function clearFiles() {
  uploadRef.value?.clearFiles()
  fileList.value = []
}
</script>

<style scoped>
.multi-upload {
  width: 100%;
}
.upload-actions {
  margin-top: 16px;
  display: flex;
  justify-content: center;
  gap: 12px;
}
</style>
