<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>文件管理</span>
          <div class="header-actions">
            <single-upload v-permission="'file:create'" button-text="单文件上传" category="general" @success="loadData" class="upload-btn" />
            <el-button v-permission="'file:create'" type="primary" @click="multiUploadVisible = true">多文件上传</el-button>
            <el-button v-permission="'file:list'" type="success" @click="exportExcel" :loading="exporting">导出 Excel</el-button>
          </div>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索文件名" clearable />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">搜索</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="originalFileName" label="文件名" />
        <el-table-column prop="fileSize" label="大小" width="120">
          <template #default="{ row }">
            {{ formatSize(row.fileSize) }}
          </template>
        </el-table-column>
        <el-table-column prop="contentType" label="类型" width="180" />
        <el-table-column prop="uploadMode" label="上传模式" width="100" />
        <el-table-column prop="createdAt" label="上传时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180">
          <template #default="{ row }">
            <el-button type="primary" size="small" @click="downloadFile(row)">下载</el-button>
            <el-button v-permission="'file:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, sizes, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog title="多文件上传" v-model="multiUploadVisible" width="500px">
      <multi-upload category="general" @success="onMultiUploadSuccess" />
    </el-dialog>

    <el-dialog title="导出进度" v-model="exportVisible" width="400px" :close-on-click-modal="false">
      <div class="export-progress">
        <el-progress :percentage="exportProgress" :status="exportStatus" />
        <p>{{ exportMessage }}</p>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { multiFileApi } from '@/api'
import { downloadFile as downloadUtil } from '@/utils/download'
import SingleUpload from '@/components/upload/SingleUpload.vue'
import MultiUpload from '@/components/upload/MultiUpload.vue'

const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const search = reactive({ page: 1, pageSize: 10, keyword: '' })
const multiUploadVisible = ref(false)

const exporting = ref(false)
const exportVisible = ref(false)
const exportProgress = ref(0)
const exportStatus = ref('')
const exportMessage = ref('准备导出...')

function formatSize(bytes: number) {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return (bytes / Math.pow(k, i)).toFixed(2) + ' ' + sizes[i]
}

function formatDate(value: string) {
  if (!value) return '-'
  const d = new Date(value)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-')
}

async function loadData() {
  loading.value = true
  const res = await multiFileApi.getList(search)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

function downloadFile(row: any) {
  downloadUtil(`/v1/multifiles/${row.id}/download`, undefined, row.originalFileName)
}

async function handleDelete(row: any) {
  try {
    await ElMessageBox.confirm('确认删除？', '提示', { type: 'warning' })
  } catch {
    return
  }
  await multiFileApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
}

function onMultiUploadSuccess() {
  multiUploadVisible.value = false
  loadData()
}

async function exportExcel() {
  exporting.value = true
  exportVisible.value = true
  exportProgress.value = 10
  exportMessage.value = '正在查询数据...'
  exportStatus.value = ''

  try {
    exportProgress.value = 50
    exportMessage.value = '正在生成 Excel...'
    await downloadUtil('/v1/multifiles/export', { category: 'general' }, `files_${new Date().toISOString().slice(0, 19).replace(/[T:]/g, '-')}.xlsx`)
    exportProgress.value = 100
    exportMessage.value = '导出成功'
    exportStatus.value = 'success'
    setTimeout(() => { exportVisible.value = false }, 1500)
  } catch (err) {
    exportProgress.value = 100
    exportMessage.value = '导出失败'
    exportStatus.value = 'exception'
    ElMessage.error('导出失败')
  } finally {
    exporting.value = false
  }
}

onMounted(() => { loadData() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.header-actions { display: flex; align-items: center; gap: 8px; }
.header-actions .upload-btn { display: inline-flex; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
.mr-2 { margin-right: 8px; }
.export-progress { padding: 20px; text-align: center; }
</style>
