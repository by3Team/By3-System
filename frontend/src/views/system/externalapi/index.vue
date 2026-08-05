<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>对外 API Token 管理</span>
          <el-button v-permission="'externalapi:create'" type="primary" @click="openDialog()">新增 Token</el-button>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索应用名称/ApiKey" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="search.isEnabled" placeholder="全部" style="width: 120px" clearable>
            <el-option label="全部" value="" />
            <el-option label="启用" value="enabled" />
            <el-option label="停用" value="disabled" />
          </el-select>
        </el-form-item>
        <el-form-item label="删除状态">
          <el-select v-model="search.includeDeleted" placeholder="未删除" style="width: 120px" clearable>
            <el-option label="未删除" value="false" />
            <el-option label="已删除" value="true" />
            <el-option label="全部" value="" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">搜索</el-button>
          <el-button type="success" @click="handleExportCsv">导出 CSV</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="appName" label="应用名称" />
        <el-table-column label="负责人邮箱" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            <span>{{ formatShortText(row.contactEmail, 18) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="ApiKey" width="320" show-overflow-tooltip>
          <template #default="{ row }">
            <div class="apikey-cell">
              <span class="apikey-text">{{ row.apiKey }}</span>
              <el-tooltip content="复制 ApiKey" placement="top">
                <el-icon class="copy-icon" @click="copyToClipboard(row.apiKey)"><CopyDocument /></el-icon>
              </el-tooltip>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="可访问接口" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">{{ formatAllowedApis(row) }}</template>
        </el-table-column>
        <el-table-column label="有效期类型" width="100" align="center">
          <template #default="{ row }">{{ formatExpireType(row.expireType) }}</template>
        </el-table-column>
        <el-table-column prop="expireTime" label="有效期至" width="170">
          <template #default="{ row }">{{ formatDate(row.expireTime) }}</template>
        </el-table-column>
        <el-table-column prop="isDeleted" label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.isDeleted" type="danger">已删除</el-tag>
            <el-tag v-else :type="row.isEnabled ? 'success' : 'danger'">{{ row.isEnabled ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="170">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="420" fixed="right">
          <template #default="{ row }">
            <el-button v-permission="'externalapi:update'" type="primary" size="small" :disabled="row.isDeleted" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'externalapi:update'" type="warning" size="small" :disabled="row.isDeleted" @click="openRegenerateDialog(row)">重新生成</el-button>
            <el-button type="info" size="small" @click="openHistoryDrawer(row)">历史</el-button>
            <el-button v-permission="'externalapi:delete'" type="danger" size="small" :disabled="row.isDeleted" @click="handleDelete(row)">删除</el-button>
            <el-button type="info" size="small" @click="openLogDialog(row)">日志</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, sizes, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="900px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="100px">
        <el-form-item label="应用名称" prop="appName">
          <el-input v-model="form.appName" />
        </el-form-item>
        <el-form-item label="负责人邮箱" prop="contactEmail">
          <el-input v-model="form.contactEmail" placeholder="多个邮箱用逗号分隔，用于接收 Token 变更通知" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="有效期" prop="expireType">
          <el-select v-model="form.expireType" style="width: 100%">
            <el-option label="30 天" value="30" />
            <el-option label="60 天" value="60" />
            <el-option label="90 天" value="90" />
            <el-option label="自定义时间" value="custom" />
          </el-select>
        </el-form-item>
        <el-form-item v-if="form.expireType === 'custom'" label="自定义时间">
          <el-date-picker v-model="form.expireTime" type="datetime" placeholder="选择有效期" value-format="YYYY-MM-DD HH:mm:ss" :disabled-date="disabledCustomDate" style="width: 100%" />
        </el-form-item>
        <el-form-item label="可访问接口">
          <div class="api-select-panel">
            <el-input v-model="apiSearch.keyword" placeholder="搜索接口名称/路径" clearable @change="loadExternalApis" class="api-search" />
            <el-table :data="externalApiOptions" v-loading="apiLoading" size="small" border :row-class-name="apiRowClassName">
              <el-table-column width="60" align="center">
                <template #header>
                  <el-checkbox
                    :model-value="isAllSelected"
                    :indeterminate="isIndeterminate"
                    @update:model-value="toggleAll"
                  />
                </template>
                <template #default="{ row }">
                  <el-checkbox
                    :model-value="form.allowedApiIds.includes(row.id)"
                    :disabled="isApiDisabled(row)"
                    @update:model-value="(checked: boolean) => toggleApi(row.id, checked)"
                  />
                </template>
              </el-table-column>
              <el-table-column prop="apiName" label="接口名称" min-width="140" />
              <el-table-column label="状态" width="90" align="center">
                <template #default="{ row }">
                  <el-tag v-if="row.isDeleted" type="info" size="small">已删除</el-tag>
                  <el-tag v-else :type="row.isEnabled ? 'success' : 'danger'" size="small">{{ row.isEnabled ? '启用' : '停用' }}</el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="route" label="请求路径" min-width="180" show-overflow-tooltip />
              <el-table-column prop="method" label="方法" width="70" align="center" />
            </el-table>
            <el-pagination
              v-model:current-page="apiSearch.page"
              v-model:page-size="apiSearch.pageSize"
              :total="apiTotal"
              :page-sizes="[10, 20, 50]"
              layout="total, sizes, prev, pager, next"
              @change="loadExternalApis"
              class="api-pagination"
            />
            <div class="form-tip">不选择任何接口时，该 Token 可访问所有已启用的对外接口</div>
          </div>
        </el-form-item>
        <el-form-item label="启用状态">
          <el-switch v-model="form.isEnabled" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>

    <el-dialog title="重新生成 Key/Secret" v-model="regenerateDialogVisible" width="500px">
      <el-form :model="regenerateForm" ref="regenerateFormRef" label-width="120px">
        <el-form-item label="旧 Key 处理方式">
          <el-radio-group v-model="regenerateForm.oldKeyExpireType">
            <el-radio :label="0">立即失效</el-radio>
            <el-radio :label="1">指定时间后失效</el-radio>
          </el-radio-group>
          <div class="form-tip">
            重新生成后新 Key 立即生效。选择“立即失效”则旧 Key 立刻不可用；选择“指定时间后失效”则旧 Key 在缓冲期内仍可使用。同一应用最多同时存在两个有效 Key（当前 Key 与一个缓冲中的旧 Key），更早的 Key 将自动失效。
          </div>
        </el-form-item>
        <el-form-item v-if="regenerateForm.oldKeyExpireType === 1" label="缓冲期至">
          <el-date-picker v-model="regenerateForm.oldKeyExpireAt" type="datetime" placeholder="选择旧 Key 失效时间" value-format="YYYY-MM-DD HH:mm:ss" :disabled-date="disabledRegenerateDate" style="width: 100%" />
          <div class="form-tip">默认与当前 Token 有效期一致，仅可选择当前时间之后</div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="regenerateDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleRegenerate">确定</el-button>
      </template>
    </el-dialog>

    <el-dialog title="Token 操作日志" v-model="logDialogVisible" width="920px">
      <el-table :data="logData" v-loading="logLoading" max-height="400">
        <el-table-column prop="createdAt" label="操作时间" width="170">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column prop="action" label="操作类型" width="120">
          <template #default="{ row }">{{ formatAction(row.action) }}</template>
        </el-table-column>
        <el-table-column prop="operatorName" label="操作人" width="120" />
        <el-table-column prop="ipAddress" label="IP 地址" width="140" />
        <el-table-column prop="remark" label="备注" min-width="320" show-overflow-tooltip />
      </el-table>
      <el-pagination v-model:current-page="logSearch.page" v-model:page-size="logSearch.pageSize" :total="logTotal" layout="total, prev, pager, next" @change="loadLogs" class="pagination" />
    </el-dialog>

    <el-drawer title="历史 Secret Key" v-model="historyDrawerVisible" size="40%">
      <div v-loading="historyLoading" class="history-panel">
        <el-form :inline="true" class="search-form">
          <el-form-item label="状态">
            <el-select v-model="historySearch.status" placeholder="全部" style="width: 120px" clearable>
              <el-option label="全部" value="all" />
              <el-option label="有效" value="valid" />
              <el-option label="失效" value="invalid" />
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="loadHistory">查询</el-button>
          </el-form-item>
        </el-form>
        <el-table :data="historyData" size="small" border>
          <el-table-column prop="createdAt" label="生成时间" min-width="160">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column prop="expireTime" label="原 Token 过期时间" width="160">
            <template #default="{ row }">{{ formatDate(row.expireTime) }}</template>
          </el-table-column>
          <el-table-column prop="validUntil" label="缓冲截止时间" width="160">
            <template #default="{ row }">{{ formatDate(row.validUntil) }}</template>
          </el-table-column>
          <el-table-column label="状态" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="row.isValid ? 'success' : 'danger'" size="small">{{ row.isValid ? '有效' : '失效' }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="100" align="center" fixed="right">
            <template #default="{ row }">
              <el-button v-permission="'externalapi:update'" type="danger" size="small" :disabled="!row.isValid" @click="handleInvalidateHistory(row)">作废</el-button>
            </template>
          </el-table-column>
        </el-table>
        <el-pagination v-model:current-page="historySearch.page" v-model:page-size="historySearch.pageSize" :total="historyTotal" layout="total, prev, pager, next" @change="loadHistory" class="pagination" />
      </div>
    </el-drawer>

    <el-dialog title="Token 创建成功 - 请立即保存" v-model="resultVisible" width="650px" :close-on-click-modal="false">
      <el-alert type="warning" :closable="false" show-icon>
        <template #default>ApiSecret 仅在此弹窗显示一次，关闭后将无法再次查看，请及时复制保存。</template>
      </el-alert>
      <div class="result-info">
        <div class="result-row">
          <span class="result-label">ApiKey：</span>
          <el-input v-model="resultToken.apiKey" readonly class="result-input">
            <template #append>
              <el-button @click="copyToClipboard(resultToken.apiKey)">复制</el-button>
            </template>
          </el-input>
        </div>
        <div class="result-row">
          <span class="result-label">ApiSecret：</span>
          <el-input v-model="resultToken.apiSecret" readonly class="result-input" type="password" show-password>
            <template #append>
              <el-button @click="copyToClipboard(resultToken.apiSecret)">复制</el-button>
            </template>
          </el-input>
        </div>
      </div>
      <el-tabs v-model="resultDemoTab" class="result-tabs">
        <el-tab-pane label="C# 示例" name="csharp">
          <pre class="code-block">{{ csharpDemo }}</pre>
          <el-button type="primary" size="small" @click="copyToClipboard(csharpDemo)">复制 C# 代码</el-button>
        </el-tab-pane>
        <el-tab-pane label="Java 示例" name="java">
          <pre class="code-block">{{ javaDemo }}</pre>
          <el-button type="primary" size="small" @click="copyToClipboard(javaDemo)">复制 Java 代码</el-button>
        </el-tab-pane>
        <el-tab-pane label="JavaScript 示例" name="js">
          <pre class="code-block">{{ jsDemo }}</pre>
          <el-button type="primary" size="small" @click="copyToClipboard(jsDemo)">复制 JS 代码</el-button>
        </el-tab-pane>
      </el-tabs>
      <template #footer>
        <el-button type="primary" @click="resultVisible = false">我已保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>
<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { externalApiTokenApi, externalApiApi } from '@/api'

const loading = ref(false)
const tableData = ref<any[]>([])
const total = ref(0)
const search = reactive<any>({ page: 1, pageSize: 10, keyword: '', isEnabled: '', includeDeleted: 'false' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref<any>()
const form = reactive<any>({ appName: '', description: '', contactEmail: '', expireType: '30', expireTime: '', allowedApiIds: [], isEnabled: true })
const formRules = { appName: [{ required: true, message: '必填', trigger: 'blur' }] }

const externalApiOptions = ref<any[]>([])
const apiLoading = ref(false)
const apiSearch = reactive<any>({ page: 1, pageSize: 20, keyword: '' })
const apiTotal = ref(0)

const regenerateDialogVisible = ref(false)
const regenerateFormRef = ref<any>()
const regenerateForm = reactive<any>({ oldKeyExpireType: 0, oldKeyExpireAt: '' })
const regenerateTokenId = ref<string>('')
const regenerateRow = ref<any>(null)

const logDialogVisible = ref(false)
const logLoading = ref(false)
const logData = ref<any[]>([])
const logTotal = ref(0)
const logSearch = reactive<any>({ page: 1, pageSize: 10 })
const logTokenId = ref<string>('')

const historyDrawerVisible = ref(false)
const historyLoading = ref(false)
const historyData = ref<any[]>([])
const historyTotal = ref(0)
const historySearch = reactive<any>({ page: 1, pageSize: 10, status: 'all' })
const historyTokenId = ref<string>('')

const resultVisible = ref(false)
const resultDemoTab = ref('csharp')
const resultToken = reactive<any>({ apiKey: '', apiSecret: '' })

const apiBaseUrl = computed(() => import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api')

function formatDate(value: string) {
  if (!value) return '-'
  const d = new Date(value)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-')
}

function formatExpireType(value: string | null | undefined) {
  const map: Record<string, string> = { '30': '30 天', '60': '60 天', '90': '90 天', custom: '自定义' }
  return map[value || ''] || value || '-'
}

function addDays(days: number): string {
  const d = new Date(Date.now() + days * 24 * 60 * 60 * 1000)
  return toIsoLocalString(d)
}

function toIsoLocalString(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}

function normalizeExpireTime(value: string | null | undefined): string | null {
  if (!value) return null
  const normalized = value.replace(' ', 'T')
  const d = new Date(normalized)
  return isNaN(d.getTime()) ? value : toIsoLocalString(d)
}

function formatAllowedApis(row: any) {
  if (!row.allowedApiIds || row.allowedApiIds.length === 0) return '全部接口'
  const names = row.allowedApiIds
    .map((id: string) => externalApiOptions.value.find((api: any) => api.id === id)?.apiName)
    .filter(Boolean)
  return names.length ? names.join('、') : '全部接口'
}

function formatShortText(value: string | null | undefined, maxLength: number) {
  if (!value) return '-'
  return value.length > maxLength ? value.slice(0, maxLength) + '...' : value
}

function formatAction(action: string) {
  const map: Record<string, string> = {
    Create: '创建',
    Update: '更新',
    Delete: '删除',
    Regenerate: '重新生成',
    Enable: '启用',
    Disable: '停用'
  }
  return map[action] || action
}

function disabledCustomDate(date: Date) {
  return date.getTime() < Date.now()
}

function disabledRegenerateDate(date: Date) {
  return date.getTime() < Date.now()
}

async function loadExternalApis() {
  apiLoading.value = true
  try {
    const res = await externalApiApi.getList({ page: apiSearch.page, pageSize: apiSearch.pageSize, keyword: apiSearch.keyword })
    externalApiOptions.value = res.items || []
    apiTotal.value = res.total || 0
  } catch {
    externalApiOptions.value = []
    apiTotal.value = 0
  } finally {
    apiLoading.value = false
  }
}

function buildSearchParams() {
  const params: any = { page: search.page, pageSize: search.pageSize, keyword: search.keyword }
  if (search.isEnabled) {
    params.isEnabled = search.isEnabled
  }
  if (search.includeDeleted !== '') {
    params.includeDeleted = search.includeDeleted === 'true'
  }
  return params
}

async function handleExportCsv() {
  try {
    const blob = await externalApiTokenApi.exportCsv(buildSearchParams())
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `external-api-tokens-${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '-')}.csv`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
    ElMessage.success('导出成功')
  } catch {
    ElMessage.error('导出失败')
  }
}

async function loadData() {
  loading.value = true
  try {
    const res = await externalApiTokenApi.getList(buildSearchParams())
    tableData.value = res.items
    total.value = res.total
  } finally {
    loading.value = false
  }
}

function isApiAuthorized(api: any) {
  return form.allowedApiIds.includes(api.id)
}

function isApiDisabled(api: any) {
  const authorized = isApiAuthorized(api)
  // 已删除的接口不可再授权/取消；已禁用且未授权的接口不可新授权，但已授权的可移除
  return api.isDeleted || (!api.isEnabled && !authorized)
}

function apiRowClassName({ row }: { row: any }) {
  return isApiDisabled(row) ? 'disabled-row' : ''
}

function toggleApi(id: string, checked: boolean) {
  const api = externalApiOptions.value.find((a: any) => a.id === id)
  if (api && isApiDisabled(api)) return
  if (checked) {
    if (!form.allowedApiIds.includes(id)) {
      form.allowedApiIds.push(id)
    }
  } else {
    const idx = form.allowedApiIds.indexOf(id)
    if (idx > -1) {
      form.allowedApiIds.splice(idx, 1)
    }
  }
}

const selectableApis = computed(() => externalApiOptions.value.filter((api: any) => !isApiDisabled(api)))

const isAllSelected = computed(() => {
  if (selectableApis.value.length === 0) return false
  return selectableApis.value.every((api: any) => form.allowedApiIds.includes(api.id))
})

const isIndeterminate = computed(() => {
  if (selectableApis.value.length === 0) return false
  const selectedCount = selectableApis.value.filter((api: any) => form.allowedApiIds.includes(api.id)).length
  return selectedCount > 0 && selectedCount < selectableApis.value.length
})

function toggleAll(checked: boolean) {
  selectableApis.value.forEach((api: any) => toggleApi(api.id, checked))
}

function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑 Token' : '新增 Token'
  Object.assign(form, { appName: '', description: '', contactEmail: '', expireType: '30', expireTime: '', allowedApiIds: [], isEnabled: true })
  if (row) {
    Object.assign(form, row)
    form.expireType = row.expireType || '30'
    // 深拷贝 allowedApiIds，避免弹窗内勾选影响列表数据
    form.allowedApiIds = row.allowedApiIds ? [...row.allowedApiIds] : []
  }
  apiSearch.page = 1
  apiSearch.pageSize = 20
  apiSearch.keyword = ''
  loadExternalApis()
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  if (form.expireType === 'custom' && !form.expireTime) {
    ElMessage.warning('请选择自定义有效期')
    return
  }
  const payload: any = { ...form }
  payload.expireTime = form.expireType === 'custom' ? normalizeExpireTime(form.expireTime) : addDays(Number(form.expireType))

  let res: any
  if (isEdit.value) {
    await externalApiTokenApi.update(form.id, payload)
    ElMessage.success('更新成功')
  } else {
    res = await externalApiTokenApi.create(payload)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
  if (res && res.apiSecret) {
    showResult(res)
  }
}

async function handleDelete(row: any) {
  try {
    await ElMessageBox.confirm('确认删除该 Token？删除后可在“全部”状态中查看历史记录。', '提示', { type: 'warning' })
  } catch {
    return
  }
  await externalApiTokenApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
}

function openRegenerateDialog(row: any) {
  if (row.expireType === 'custom' && row.expireTime && new Date(row.expireTime) <= new Date()) {
    ElMessage.warning('自定义有效期已过期，无法重新生成 Key/Secret')
    return
  }
  regenerateRow.value = row
  regenerateTokenId.value = row.id
  regenerateForm.oldKeyExpireType = 0
  regenerateForm.oldKeyExpireAt = row.expireTime ? formatDate(row.expireTime) : ''
  regenerateDialogVisible.value = true
}

async function handleRegenerate() {
  if (regenerateForm.oldKeyExpireType === 1 && !regenerateForm.oldKeyExpireAt) {
    ElMessage.warning('请选择旧 Key 失效时间')
    return
  }
  const payload: any = { oldKeyExpireType: regenerateForm.oldKeyExpireType }
  if (regenerateForm.oldKeyExpireType === 1) {
    const d = new Date(regenerateForm.oldKeyExpireAt)
    payload.oldKeyExpireAt = isNaN(d.getTime()) ? '' : d.toISOString()
  }
  const res = await externalApiTokenApi.regenerate(regenerateTokenId.value, payload)
  ElMessage.success('已重新生成')
  regenerateDialogVisible.value = false
  loadData()
  if (res && res.apiSecret) {
    showResult(res)
  }
}

function openLogDialog(row: any) {
  logTokenId.value = row.id
  logSearch.page = 1
  logSearch.pageSize = 10
  logDialogVisible.value = true
  loadLogs()
}

async function loadLogs() {
  logLoading.value = true
  try {
    const res = await externalApiTokenApi.getLogs(logTokenId.value, { page: logSearch.page, pageSize: logSearch.pageSize })
    logData.value = res.items
    logTotal.value = res.total
  } finally {
    logLoading.value = false
  }
}

function openHistoryDrawer(row: any) {
  historyTokenId.value = row.id
  historySearch.page = 1
  historySearch.pageSize = 10
  historySearch.status = 'all'
  historyDrawerVisible.value = true
  loadHistory()
}

async function loadHistory() {
  historyLoading.value = true
  try {
    const res = await externalApiTokenApi.getHistory(historyTokenId.value, {
      page: historySearch.page,
      pageSize: historySearch.pageSize,
      status: historySearch.status
    })
    historyData.value = res.items
    historyTotal.value = res.total
  } finally {
    historyLoading.value = false
  }
}

async function handleInvalidateHistory(row: any) {
  try {
    await ElMessageBox.confirm('确认作废该历史 Key？作废后将无法继续使用。', '提示', { type: 'warning' })
  } catch {
    return
  }
  await externalApiTokenApi.invalidateHistory(historyTokenId.value, row.id)
  ElMessage.success('已作废')
  loadHistory()
}

function showResult(token: any) {
  resultToken.apiKey = token.apiKey
  resultToken.apiSecret = token.apiSecret
  resultDemoTab.value = 'csharp'
  resultVisible.value = true
}

async function copyToClipboard(text: string) {
  try {
    await navigator.clipboard.writeText(text)
    ElMessage.success('已复制到剪贴板')
  } catch {
    ElMessage.error('复制失败')
  }
}

const API_KEY_PLACEHOLDER = 'YOUR_API_KEY'
const API_SECRET_PLACEHOLDER = 'YOUR_API_SECRET'

const csharpDemo = computed(() => {
  return `using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

class ExternalApiClient
{
    private static readonly string ApiKey = "${API_KEY_PLACEHOLDER}";
    private static readonly string ApiSecret = "${API_SECRET_PLACEHOLDER}";
    private static readonly string BaseUrl = "${apiBaseUrl.value}";

    static async Task Main()
    {
        var result = await GetUsersAsync();
        Console.WriteLine(result);
    }

    static async Task<string> GetUsersAsync()
    {
        var path = "/external/v1/users";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonce = Guid.NewGuid().ToString("N");
        var parameters = new Dictionary<string, string?>
        {
            { "page", "1" },
            { "pageSize", "10" }
        };

        var signature = Sign("GET", path, timestamp, nonce, parameters);

        using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
        client.DefaultRequestHeaders.Add("X-Timestamp", timestamp.ToString());
        client.DefaultRequestHeaders.Add("X-Nonce", nonce);
        client.DefaultRequestHeaders.Add("X-Signature", signature);

        return await client.GetStringAsync(path + "?page=1&pageSize=10");
    }

    static string Sign(string method, string path, long timestamp, string nonce, Dictionary<string, string?> parameters)
    {
        var sorted = new List<string>();
        foreach (var item in parameters)
        {
            if (!string.IsNullOrEmpty(item.Value))
                sorted.Add($"{item.Key}={HttpUtility.UrlEncode(item.Value)}");
        }
        sorted.Sort(StringComparer.Ordinal);
        var paramString = string.Join("&", sorted);
        var signString = $"{method.ToUpper()}&{path}&{timestamp}&{nonce}&{paramString}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ApiSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signString));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}`
})

const javaDemo = computed(() => {
  return `import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.*;

public class ExternalApiClient {
    private static final String API_KEY = "${API_KEY_PLACEHOLDER}";
    private static final String API_SECRET = "${API_SECRET_PLACEHOLDER}";
    private static final String BASE_URL = "${apiBaseUrl.value}";

    public static void main(String[] args) throws Exception {
        String result = getUsers();
        System.out.println(result);
    }

    public static String getUsers() throws Exception {
        String path = "/external/v1/users";
        long timestamp = System.currentTimeMillis() / 1000;
        String nonce = UUID.randomUUID().toString().replace("-", "");
        Map<String, String> params = new LinkedHashMap<>();
        params.put("page", "1");
        params.put("pageSize", "10");

        String signature = sign("GET", path, timestamp, nonce, params);

        HttpClient client = HttpClient.newHttpClient();
        HttpRequest request = HttpRequest.newBuilder()
            .uri(URI.create(BASE_URL + path + "?page=1&pageSize=10"))
            .header("X-Api-Key", API_KEY)
            .header("X-Timestamp", String.valueOf(timestamp))
            .header("X-Nonce", nonce)
            .header("X-Signature", signature)
            .GET()
            .build();

        HttpResponse<String> response = client.send(request, HttpResponse.BodyHandlers.ofString());
        return response.body();
    }

    public static String sign(String method, String path, long timestamp, String nonce, Map<String, String> params) throws Exception {
        List<String> sorted = new ArrayList<>();
        for (Map.Entry<String, String> entry : params.entrySet()) {
            if (entry.getValue() != null && !entry.getValue().isEmpty()) {
                sorted.add(entry.getKey() + "=" + URLEncoder.encode(entry.getValue(), StandardCharsets.UTF_8));
            }
        }
        Collections.sort(sorted);
        String paramString = String.join("&", sorted);
        String signString = method.toUpperCase() + "&" + path + "&" + timestamp + "&" + nonce + "&" + paramString;

        Mac mac = Mac.getInstance("HmacSHA256");
        SecretKeySpec secretKeySpec = new SecretKeySpec(API_SECRET.getBytes(StandardCharsets.UTF_8), "HmacSHA256");
        mac.init(secretKeySpec);
        byte[] hash = mac.doFinal(signString.getBytes(StandardCharsets.UTF_8));

        StringBuilder hexString = new StringBuilder();
        for (byte b : hash) {
            String hex = Integer.toHexString(0xff & b);
            if (hex.length() == 1) hexString.append('0');
            hexString.append(hex);
        }
        return hexString.toString();
    }
}`
})

const jsDemo = computed(() => {
  return `import crypto from 'crypto'

const API_KEY = '${API_KEY_PLACEHOLDER}'
const API_SECRET = '${API_SECRET_PLACEHOLDER}'
const BASE_URL = '${apiBaseUrl.value}'

function sign(method, path, timestamp, nonce, params) {
  const sorted = Object.entries(params)
    .filter(([_, v]) => v !== undefined && v !== null && v !== '')
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([k, v]) => \`\${k}=\${encodeURIComponent(v)}\`)
    .join('&')
  const signString = \`\${method.toUpperCase()}&\${path}&\${timestamp}&\${nonce}&\${sorted}\`
  return crypto.createHmac('sha256', API_SECRET).update(signString).digest('hex')
}

async function getUsers() {
  const path = '/external/v1/users'
  const timestamp = Math.floor(Date.now() / 1000)
  const nonce = crypto.randomUUID().replace(/-/g, '')
  const params = { page: '1', pageSize: '10' }
  const signature = sign('GET', path, timestamp, nonce, params)

  const res = await fetch(\`\${BASE_URL}\${path}?page=1&pageSize=10\`, {
    headers: {
      'X-Api-Key': API_KEY,
      'X-Timestamp': String(timestamp),
      'X-Nonce': nonce,
      'X-Signature': signature
    }
  })
  return res.json()
}

getUsers().then(console.log).catch(console.error)`
})

onMounted(() => {
  loadExternalApis()
  loadData()
})
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
.form-tip { font-size: 12px; color: #909399; margin-top: 4px; }
.result-info { margin-top: 15px; }
.result-row { display: flex; align-items: center; margin-bottom: 12px; }
.result-label { width: 90px; flex-shrink: 0; font-weight: 500; }
.result-input { flex: 1; }
.result-tabs { margin-top: 10px; }
.code-block { background: #1e1e1e; color: #d4d4d4; padding: 12px; border-radius: 4px; overflow-x: auto; font-family: Consolas, monospace; font-size: 12px; line-height: 1.5; margin: 0 0 10px; }
.apikey-cell { display: flex; align-items: center; gap: 8px; }
.apikey-text { font-family: Consolas, monospace; word-break: break-all; }
.copy-icon { cursor: pointer; color: #409eff; font-size: 14px; flex-shrink: 0; }
.copy-icon:hover { color: #66b1ff; }
.history-panel { padding: 0 10px; min-width: 660px; }
.api-select-panel { width: 100%; }
.api-search { margin-bottom: 10px; }
.api-pagination { margin-top: 10px; justify-content: flex-end; }
.api-select-panel :deep(.el-table__row) { height: 36px; }
.api-select-panel :deep(.el-table .cell) { padding: 4px 8px; line-height: 1.4; }
.api-select-panel :deep(.el-table--small .el-table__cell) { padding: 4px 0; }
.api-select-panel :deep(.el-table__row.disabled-row) { background-color: #f5f7fa; color: #c0c4cc; }
.api-select-panel :deep(.el-table__row.disabled-row .el-checkbox__input.is-disabled .el-checkbox__inner) { background-color: #e4e7ed; border-color: #d3d6dd; }
</style>
