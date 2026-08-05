<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>对外 API 接口管理</span>
          <el-button v-permission="'externalapi:create'" type="primary" @click="openDialog()">新增接口</el-button>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索接口名称/路径" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="search.status" placeholder="全部" clearable style="width: 120px">
            <el-option label="全部" value="" />
            <el-option label="启用" value="enabled" />
            <el-option label="停用" value="disabled" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">搜索</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="apiName" label="接口名称" />
        <el-table-column label="请求路径" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">
            <div class="route-cell">
              <span class="route-text">{{ row.route }}</span>
              <el-tooltip content="复制接口信息" placement="top">
                <el-icon class="copy-icon" @click="copyApiInfo(row)"><CopyDocument /></el-icon>
              </el-tooltip>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="method" label="方法" width="80" align="center" />
        <el-table-column prop="rateLimitPerSecond" label="限流(QPS)" width="100" align="center">
          <template #default="{ row }">{{ row.rateLimitPerSecond || '-' }}</template>
        </el-table-column>
        <el-table-column prop="requireIdempotency" label="需幂等" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="row.requireIdempotency ? 'success' : 'info'">{{ row.requireIdempotency ? '是' : '否' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="isEnabled" label="状态" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'danger'">{{ row.isEnabled ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="170">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="360" fixed="right">
          <template #default="{ row }">
            <el-button type="info" size="small" @click="openDetail(row)">详情</el-button>
            <el-button v-permission="'externalapi:update'" type="primary" size="small" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'externalapi:update'" :type="row.isEnabled ? 'danger' : 'success'" size="small" @click="handleToggle(row)">{{ row.isEnabled ? '停用' : '启用' }}</el-button>
            <el-button v-permission="'externalapi:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, sizes, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="600px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="120px">
        <el-form-item label="接口名称" prop="apiName">
          <el-input v-model="form.apiName" />
        </el-form-item>
        <el-form-item label="请求路径" prop="route">
          <el-input v-model="form.route" placeholder="例如 /external/v1/users" />
        </el-form-item>
        <el-form-item label="请求方法" prop="method">
          <el-select v-model="form.method" placeholder="请选择" style="width: 100%">
            <el-option label="GET" value="GET" />
            <el-option label="POST" value="POST" />
            <el-option label="PUT" value="PUT" />
            <el-option label="DELETE" value="DELETE" />
          </el-select>
        </el-form-item>
        <el-form-item label="限流(QPS)">
          <el-input-number v-model="form.rateLimitPerSecond" :min="0" :max="10000" controls-position="right" style="width: 100%" />
          <div class="form-tip">0 表示不限流</div>
        </el-form-item>
        <el-form-item label="需幂等校验">
          <el-switch v-model="form.requireIdempotency" />
          <div class="form-tip">开启后外部请求必须携带 Idempotency-Key 请求头</div>
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>

    <el-drawer title="接口详情" v-model="detailVisible" size="650px" :destroy-on-close="true" @opened="initChart">
      <div v-loading="detailLoading" class="detail-panel">
        <template v-if="stats">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="接口名称">{{ stats.apiName }}</el-descriptions-item>
            <el-descriptions-item label="请求路径">{{ stats.method }} {{ stats.route }}</el-descriptions-item>
            <el-descriptions-item label="限流(QPS)">{{ stats.rateLimitPerSecond || '不限流' }}</el-descriptions-item>
            <el-descriptions-item label="需幂等">{{ stats.requireIdempotency ? '是' : '否' }}</el-descriptions-item>
            <el-descriptions-item label="状态">
              <el-tag :type="stats.isEnabled ? 'success' : 'danger'">{{ stats.isEnabled ? '启用' : '停用' }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="最近调用">{{ formatDate(stats.lastCallAt) }}</el-descriptions-item>
          </el-descriptions>

          <el-row :gutter="16" class="stat-cards">
            <el-col :span="8">
              <el-card shadow="hover">
                <div class="stat-value">{{ stats.totalRequests }}</div>
                <div class="stat-label">近 30 天总请求</div>
              </el-card>
            </el-col>
            <el-col :span="8">
              <el-card shadow="hover">
                <div class="stat-value success">{{ stats.successCount }}</div>
                <div class="stat-label">成功</div>
              </el-card>
            </el-col>
            <el-col :span="8">
              <el-card shadow="hover">
                <div class="stat-value danger">{{ stats.failureCount }}</div>
                <div class="stat-label">失败</div>
              </el-card>
            </el-col>
          </el-row>

          <div class="chart-title">近 30 天请求量曲线</div>
          <div ref="chartRef" class="chart-container"></div>

          <div class="section-title">已授权应用（Token）</div>
          <el-table :data="stats.allowedTokens" size="small" border>
            <el-table-column prop="appName" label="应用名称" min-width="140" />
            <el-table-column prop="apiKey" label="ApiKey" min-width="240" show-overflow-tooltip />
            <el-table-column prop="isEnabled" label="状态" width="80" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isEnabled ? 'success' : 'danger'" size="small">{{ row.isEnabled ? '启用' : '停用' }}</el-tag>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!stats.allowedTokens || stats.allowedTokens.length === 0" description="暂无 Token 授权访问该接口" />
        </template>
      </div>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, nextTick, onMounted, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import * as echarts from 'echarts'
import { externalApiApi } from '@/api'

const loading = ref(false)
const tableData = ref<any[]>([])
const total = ref(0)
const search = reactive({ page: 1, pageSize: 10, keyword: '', status: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref<any>()
const form = reactive<any>({
  apiName: '',
  route: '',
  method: 'GET',
  description: '',
  rateLimitPerSecond: 0,
  requireIdempotency: true,
  isEnabled: true
})

const formRules = {
  apiName: [{ required: true, message: '必填', trigger: 'blur' }],
  route: [{ required: true, message: '必填', trigger: 'blur' }],
  method: [{ required: true, message: '必填', trigger: 'change' }]
}

const detailVisible = ref(false)
const detailLoading = ref(false)
const stats = ref<any>(null)
const chartRef = ref<HTMLElement>()
let chartInstance: echarts.ECharts | null = null

function formatDate(value: string | null | undefined) {
  if (!value) return '-'
  const d = new Date(value)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-')
}

function generateDemoParams(row: any): string {
  const method = (row.method || 'GET').toUpperCase()
  let route = row.route || ''

  // 替换路径参数为示例 UUID
  route = route.replace(/\{[^}]+\}/g, '550e8400-e29b-41d4-a716-446655440000')

  if (method === 'GET') {
    return `${method} ${route}?page=1&pageSize=10`
  }
  if (method === 'DELETE') {
    return `${method} ${route}`
  }
  return `${method} ${route}
Content-Type: application/json

{
  "demoField": "demoValue"
}`
}

async function copyApiInfo(row: any) {
  const text = `接口名称：${row.apiName || '-'}
请求方式：${row.method || '-'}
请求路径：${row.route || '-'}
请求示例：
${generateDemoParams(row)}
限流(QPS)：${row.rateLimitPerSecond || '不限流'}
需幂等：${row.requireIdempotency ? '是' : '否'}
状态：${row.isEnabled ? '启用' : '停用'}
描述：${row.description || '-'}`

  try {
    await navigator.clipboard.writeText(text)
    ElMessage.success('接口信息已复制')
  } catch {
    ElMessage.error('复制失败')
  }
}

async function loadData() {
  loading.value = true
  try {
    const params: any = { page: search.page, pageSize: search.pageSize, keyword: search.keyword }
    if (search.status) {
      params.isEnabled = search.status
    }
    const res = await externalApiApi.getList(params)
    tableData.value = res.items
    total.value = res.total
  } finally {
    loading.value = false
  }
}

function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑接口' : '新增接口'
  Object.assign(form, row || {
    apiName: '',
    route: '',
    method: 'GET',
    description: '',
    rateLimitPerSecond: 0,
    requireIdempotency: true,
    isEnabled: true
  })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  if (isEdit.value) {
    await externalApiApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await externalApiApi.create(form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function handleDelete(row: any) {
  let count = 0
  try {
    count = await externalApiApi.getAuthorizedTokenCount(row.id)
  } catch {
    count = 0
  }

  const message = count > 0
    ? `该接口当前有 ${count} 个 Token 已授权访问，删除后这些 Token 将无法继续调用该接口。确认删除吗？`
    : '确认删除该接口？'

  try {
    await ElMessageBox.confirm(message, '提示', { type: 'warning' })
  } catch {
    return
  }
  await externalApiApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
}

async function handleToggle(row: any) {
  let count = 0
  try {
    count = await externalApiApi.getAuthorizedTokenCount(row.id)
  } catch {
    count = 0
  }

  const action = row.isEnabled ? '停用' : '启用'
  const message = count > 0
    ? `该操作将影响 ${count} 个已授权 Token，${row.isEnabled ? '这些 Token 即将停止服务' : '这些 Token 即将恢复服务'}。确认${action}吗？`
    : `确认${action}该接口吗？`

  try {
    await ElMessageBox.confirm(message, '提示', { type: 'warning' })
  } catch {
    return
  }

  try {
    await externalApiApi.toggle(row.id)
    ElMessage.success(`${action}成功`)
    loadData()
  } catch (e: any) {
    ElMessage.error(e?.message || '操作失败')
  }
}

async function openDetail(row: any) {
  detailLoading.value = true
  stats.value = null
  try {
    stats.value = await externalApiApi.getStats(row.id)
    detailVisible.value = true
  } catch (e: any) {
    ElMessage.error(e?.message || '加载详情失败')
  } finally {
    detailLoading.value = false
  }
}

function initChart() {
  nextTick(() => {
    if (!chartRef.value || !stats.value || !stats.value.dailyStats) return
    if (chartInstance) {
      chartInstance.dispose()
      chartInstance = null
    }
    // 等待抽屉动画完成，避免容器宽度为 0
    setTimeout(() => {
      if (!chartRef.value) return
      chartInstance = echarts.init(chartRef.value)
      const dates = stats.value.dailyStats.map((x: any) => x.date)
      const counts = stats.value.dailyStats.map((x: any) => x.count)
      const success = stats.value.dailyStats.map((x: any) => x.successCount)
      const failure = stats.value.dailyStats.map((x: any) => x.failureCount)

      chartInstance.setOption({
        tooltip: { trigger: 'axis' },
        legend: { data: ['总请求', '成功', '失败'], bottom: 0 },
        grid: { left: '3%', right: '4%', bottom: '15%', top: '10%', containLabel: true },
        xAxis: { type: 'category', boundaryGap: false, data: dates },
        yAxis: { type: 'value', minInterval: 1 },
        series: [
          { name: '总请求', type: 'line', smooth: true, data: counts, areaStyle: { opacity: 0.1 } },
          { name: '成功', type: 'line', smooth: true, data: success },
          { name: '失败', type: 'line', smooth: true, data: failure }
        ]
      })
      chartInstance.resize()
    }, 300)
  })
}

function onDrawerResize() {
  if (chartInstance) chartInstance.resize()
}

onMounted(() => {
  loadData()
  window.addEventListener('resize', onDrawerResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', onDrawerResize)
  if (chartInstance) {
    chartInstance.dispose()
    chartInstance = null
  }
})
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
.form-tip { font-size: 12px; color: #909399; margin-top: 4px; }
.detail-panel { padding: 0 10px; }
.route-cell { display: flex; align-items: center; gap: 8px; }
.route-text { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.copy-icon { cursor: pointer; color: #409eff; font-size: 14px; flex-shrink: 0; }
.copy-icon:hover { color: #66b1ff; }
.stat-cards { margin-top: 16px; }
.stat-value { font-size: 24px; font-weight: bold; text-align: center; color: #409eff; }
.stat-value.success { color: #67c23a; }
.stat-value.danger { color: #f56c6c; }
.stat-label { font-size: 12px; color: #909399; text-align: center; margin-top: 4px; }
.chart-title { margin-top: 24px; margin-bottom: 10px; font-weight: 500; }
.chart-container { width: 100%; height: 300px; }
.section-title { margin-top: 24px; margin-bottom: 10px; font-weight: 500; }
</style>
