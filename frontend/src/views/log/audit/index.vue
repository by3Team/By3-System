<template>
  <div>
    <el-card>
      <template #header>
        <span>操作日志</span>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.userName" placeholder="用户" clearable />
        </el-form-item>
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="操作/路径" clearable />
        </el-form-item>
        <el-form-item>
          <el-select v-model="search.requestMethod" placeholder="方法" clearable style="width: 100px">
            <el-option label="GET" value="GET" />
            <el-option label="POST" value="POST" />
            <el-option label="PUT" value="PUT" />
            <el-option label="DELETE" value="DELETE" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-input v-model="search.statusCode" placeholder="状态码" clearable style="width: 100px" />
        </el-form-item>
        <el-form-item>
          <el-date-picker
            v-model="search.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DDTHH:mm:ss"
            clearable
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="userName" label="用户" width="120" />
        <el-table-column prop="action" label="操作" width="200" />
        <el-table-column prop="requestPath" label="请求路径" />
        <el-table-column prop="requestMethod" label="方法" width="80" align="center" />
        <el-table-column prop="statusCode" label="状态码" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="statusType(row.statusCode)" size="small">{{ row.statusCode ?? '-' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="elapsedMs" label="耗时(ms)" width="100" align="center" />
        <el-table-column prop="ipAddress" label="IP" width="130" />
        <el-table-column label="时间" width="180">
          <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="100" align="center">
          <template #default="{ row }">
            <el-button link type="primary" size="small" @click="openDetail(row.id)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-drawer v-model="detailVisible" title="操作日志详情" size="600px" destroy-on-close>
      <el-skeleton v-if="detailLoading" :rows="10" animated />
      <template v-else-if="detail">
        <el-descriptions :column="2" border class="detail-overview">
          <el-descriptions-item label="用户">{{ detail.userName }}</el-descriptions-item>
          <el-descriptions-item label="操作">{{ detail.action }}</el-descriptions-item>
          <el-descriptions-item label="方法">{{ detail.requestMethod }}</el-descriptions-item>
          <el-descriptions-item label="路径">{{ detail.requestPath }}</el-descriptions-item>
          <el-descriptions-item label="状态码">
            <el-tag :type="statusType(detail.statusCode)" size="small">{{ detail.statusCode ?? '-' }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="耗时">{{ detail.elapsedMs }} ms</el-descriptions-item>
          <el-descriptions-item label="IP">{{ detail.ipAddress }}</el-descriptions-item>
          <el-descriptions-item label="时间">{{ formatDateTime(detail.createdAt) }}</el-descriptions-item>
        </el-descriptions>

        <el-tabs v-model="activeTab" type="border-card" class="detail-tabs">
          <el-tab-pane label="请求参数" name="params">
            <json-block :content="detail.requestParams" />
          </el-tab-pane>
          <el-tab-pane label="请求体" name="body">
            <json-block :content="detail.requestBody" />
          </el-tab-pane>
          <el-tab-pane label="请求头" name="headers">
            <json-block :content="detail.requestHeaders" />
          </el-tab-pane>
          <el-tab-pane label="响应结果" name="response">
            <json-block :content="detail.responseResult" />
          </el-tab-pane>
          <el-tab-pane label="响应头" name="respHeaders">
            <json-block :content="detail.responseHeaders" />
          </el-tab-pane>
          <el-tab-pane v-if="detail.exceptionMessage" label="异常" name="exception">
            <json-block :content="detail.exceptionMessage" />
          </el-tab-pane>
        </el-tabs>
      </template>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { auditLogApi } from '@/api'
import { formatDateTime } from '@/utils/format'
import JsonBlock from './JsonBlock.vue'

const loading = ref(false)
const tableData = ref<any[]>([])
const total = ref(0)
const search = reactive({
  page: 1,
  pageSize: 20,
  userName: '',
  keyword: '',
  requestMethod: '',
  statusCode: '',
  dateRange: [] as string[]
})

const detailVisible = ref(false)
const detailLoading = ref(false)
const detail = ref<any>(null)
const activeTab = ref('params')

async function loadData() {
  loading.value = true
  const params: any = {
    page: search.page,
    pageSize: search.pageSize,
    userName: search.userName || undefined,
    keyword: search.keyword || undefined,
    requestMethod: search.requestMethod || undefined,
    statusCode: search.statusCode ? Number(search.statusCode) : undefined,
    startTime: search.dateRange && search.dateRange.length === 2 ? search.dateRange[0] : undefined,
    endTime: search.dateRange && search.dateRange.length === 2 ? search.dateRange[1] : undefined
  }
  const res = await auditLogApi.getList(params)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

function handleSearch() {
  search.page = 1
  loadData()
}

function handleReset() {
  search.userName = ''
  search.keyword = ''
  search.requestMethod = ''
  search.statusCode = ''
  search.dateRange = []
  search.page = 1
  loadData()
}

async function openDetail(id: string) {
  detailVisible.value = true
  detailLoading.value = true
  activeTab.value = 'params'
  try {
    detail.value = await auditLogApi.getById(id)
  } finally {
    detailLoading.value = false
  }
}

function statusType(code: number | null | undefined) {
  if (!code) return 'info'
  if (code >= 200 && code < 300) return 'success'
  if (code >= 400 && code < 500) return 'warning'
  if (code >= 500) return 'danger'
  return 'info'
}

onMounted(loadData)
</script>

<style scoped>
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
.detail-overview { margin-bottom: 16px; }
.detail-tabs :deep(.el-tabs__content) {
  padding: 0;
}
</style>
