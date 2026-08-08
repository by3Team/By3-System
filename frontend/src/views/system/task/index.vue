<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>任务管理</span>
          <el-button v-permission="'job:create'" type="primary" @click="openDialog()">新增任务</el-button>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索任务名称" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="search.isEnabled" placeholder="全部" clearable style="width: 120px">
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
        <el-table-column prop="jobName" label="任务名称" />
        <el-table-column prop="jobGroup" label="分组" width="120" />
        <el-table-column prop="jobType" label="任务类型" width="140" />
        <el-table-column prop="cronExpression" label="Cron 表达式" width="160" />
        <el-table-column prop="isEnabled" label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'danger'">{{ row.isEnabled ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="nextFireTime" label="下次执行时间" width="170">
          <template #default="{ row }">
            {{ formatDate(row.nextFireTime) }}
          </template>
        </el-table-column>
        <el-table-column prop="description" label="描述" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="170">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="420" fixed="right">
          <template #default="{ row }">
            <el-button v-permission="'job:update'" type="primary" size="small" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'job:list'" type="info" size="small" @click="openLogs(row)">日志</el-button>
            <el-button v-permission="'job:trigger'" type="success" size="small" @click="handleTrigger(row)">执行</el-button>
            <el-button v-permission="'job:update'" :type="row.isEnabled ? 'warning' : 'success'" size="small" @click="handleToggle(row)">{{ row.isEnabled ? '停用' : '启用' }}</el-button>
            <el-button v-permission="'job:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, sizes, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="600px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="110px">
        <el-form-item label="任务名称" prop="jobName">
          <el-input v-model="form.jobName" />
        </el-form-item>
        <el-form-item label="任务分组">
          <el-input v-model="form.jobGroup" placeholder="DEFAULT" />
        </el-form-item>
        <el-form-item label="任务类型" prop="jobType">
          <el-select v-model="form.jobType" placeholder="请选择任务类型" style="width: 100%">
            <el-option label="人员数据插入" value="UserDataSeed" />
          </el-select>
        </el-form-item>
        <el-form-item label="Cron 表达式" prop="cronExpression">
          <el-input v-model="form.cronExpression" placeholder="例如：0 0/10 * * * ?" />
        </el-form-item>
        <el-form-item label="批量大小">
          <el-input-number v-model="config.batchSize" :min="1" :max="100" />
        </el-form-item>
        <el-form-item label="备份目录">
          <el-input v-model="config.backupDirectory" placeholder="./backups/users" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="2" />
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

    <el-drawer title="执行日志" v-model="logDrawerVisible" size="60%">
      <el-table :data="logData" v-loading="logLoading" height="calc(100vh - 200px)">
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Success' ? 'success' : 'danger'">{{ row.status === 'Success' ? '成功' : '失败' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="fireTime" label="触发时间" width="170">
          <template #default="{ row }">{{ formatDate(row.fireTime) }}</template>
        </el-table-column>
        <el-table-column prop="endTime" label="结束时间" width="170">
          <template #default="{ row }">{{ formatDate(row.endTime) }}</template>
        </el-table-column>
        <el-table-column prop="result" label="结果" show-overflow-tooltip />
        <el-table-column prop="exceptionMessage" label="异常信息" show-overflow-tooltip />
      </el-table>
      <el-pagination v-model:current-page="logSearch.page" v-model:page-size="logSearch.pageSize" :total="logTotal" layout="total, prev, pager, next" @change="loadLogs" class="pagination" />
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { jobApi } from '@/api'

const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const search = reactive({ page: 1, pageSize: 10, keyword: '', isEnabled: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref()
const form = reactive<any>({
  jobName: '',
  jobGroup: 'DEFAULT',
  jobType: 'UserDataSeed',
  cronExpression: '0 0/10 * * * ?',
  description: '',
  configJson: '',
  isEnabled: true
})
const config = reactive({ batchSize: 5, backupDirectory: './backups/users' })

const formRules = {
  jobName: [{ required: true, message: '必填', trigger: 'blur' }],
  jobType: [{ required: true, message: '必填', trigger: 'change' }],
  cronExpression: [{ required: true, message: '必填', trigger: 'blur' }]
}

const logDrawerVisible = ref(false)
const logLoading = ref(false)
const logData = ref([])
const logTotal = ref(0)
const logSearch = reactive({ page: 1, pageSize: 10 })
const currentJobId = ref('')

function formatDate(value: string) {
  if (!value) return '-'
  const d = new Date(value)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-')
}

async function loadData() {
  loading.value = true
  const params: any = { page: search.page, pageSize: search.pageSize, keyword: search.keyword }
  if (search.isEnabled) {
    params.isEnabled = search.isEnabled
  }
  const res = await jobApi.getList(params)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑任务' : '新增任务'
  if (row) {
    Object.assign(form, row)
    try {
      const cfg = JSON.parse(row.configJson || '{}')
      config.batchSize = cfg.batchSize ?? 5
      config.backupDirectory = cfg.backupDirectory ?? './backups/users'
    } catch {
      config.batchSize = 5
      config.backupDirectory = './backups/users'
    }
  } else {
    Object.assign(form, {
      jobName: '',
      jobGroup: 'DEFAULT',
      jobType: 'UserDataSeed',
      cronExpression: '0 0/10 * * * ?',
      description: '',
      configJson: '',
      isEnabled: true
    })
    config.batchSize = 5
    config.backupDirectory = './backups/users'
  }
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  form.configJson = JSON.stringify({ batchSize: config.batchSize, backupDirectory: config.backupDirectory })
  if (isEdit.value) {
    await jobApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await jobApi.create(form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function handleDelete(row: any) {
  try {
    await ElMessageBox.confirm('确认删除？', '提示', { type: 'warning' })
  } catch {
    return
  }
  await jobApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
}

async function handleTrigger(row: any) {
  try {
    await ElMessageBox.confirm('确认立即执行一次该任务？', '提示', { type: 'warning' })
  } catch {
    return
  }
  await jobApi.trigger(row.id)
  ElMessage.success('任务已触发')
  setTimeout(() => {
    currentJobId.value = row.id
    logDrawerVisible.value = true
    loadLogs()
  }, 1500)
}

function openLogs(row: any) {
  currentJobId.value = row.id
  logSearch.page = 1
  logDrawerVisible.value = true
  loadLogs()
}

async function handleToggle(row: any) {
  const isEnable = !row.isEnabled
  const nextTime = isEnable ? formatDate(row.nextFireTime) : ''
  const message = isEnable
    ? `确认启用任务「${row.jobName}」？\nCron：${row.cronExpression}${nextTime ? `\n预计下次执行：${nextTime}` : ''}`
    : `确认停用任务「${row.jobName}」？停用后将不再按 Cron 表达式自动执行。`

  try {
    await ElMessageBox.confirm(message, '提示', {
      type: isEnable ? 'success' : 'warning',
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      dangerouslyUseHTMLString: false
    })
  } catch {
    return
  }

  await jobApi.toggle(row.id)
  ElMessage.success(isEnable ? '任务已启用' : '任务已停用')
  loadData()
}

async function loadLogs() {
  if (!currentJobId.value) return
  logLoading.value = true
  const res = await jobApi.getLogs(currentJobId.value, logSearch)
  logData.value = res.items
  logTotal.value = res.total
  logLoading.value = false
}

onMounted(() => { loadData() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
</style>
