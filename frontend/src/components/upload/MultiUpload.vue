<template>
  <el-upload
    :http-request="customUpload"
    :before-upload="beforeUpload"
    :on-change="onChange"
    :on-remove="onRemove"
    :auto-upload="false"
    :data="{ category }"
    :accept="accept"
    name="files"
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
import api from '@/api/request'
import { useDictStore } from '@/store/dict'
import type { UploadFile, UploadFiles, UploadInstance, UploadRequestOptions } from 'element-plus'

const props = defineProps<{
  category?: string
}>()

const emit = defineEmits<{
  (e: 'success', data: any): void
}>()

const dictStore = useDictStore()
const uploading = ref(false)
const fileList = ref<UploadFiles>([])
const uploadRef = ref<UploadInstance>()
let completedCount = 0
let totalCount = 0

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

async function customUpload(options: UploadRequestOptions) {
  const formData = new FormData()
  formData.append('files', options.file)
  if (props.category) formData.append('category', props.category)

  try {
    const res = await api.post('/v1/multifiles/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    completedCount++
    ElMessage.success('上传成功')
    if (completedCount >= totalCount) {
      uploading.value = false
      fileList.value = []
      emit('success', res)
    }
    options.onSuccess(res)
  } catch (err: any) {
    completedCount++
    ElMessage.error(err?.message || '上传失败')
    if (completedCount >= totalCount) {
      uploading.value = false
      fileList.value = []
    }
    options.onError(err)
  }
}

function submitUpload() {
  if (fileList.value.length === 0) {
    ElMessage.warning('请选择文件')
    return
  }
  completedCount = 0
  totalCount = fileList.value.length
  uploading.value = true
  uploadRef.value?.submit()
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
