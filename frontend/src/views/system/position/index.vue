<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>岗位管理</span>
          <el-button v-permission="'position:create'" type="primary" @click="openDialog()">新增岗位</el-button>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索岗位名称/编码" clearable />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">搜索</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="positionName" label="岗位名称" />
        <el-table-column prop="positionCode" label="岗位编码" />
        <el-table-column prop="sortOrder" label="排序" width="80" align="center" />
        <el-table-column prop="isEnabled" label="状态">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'danger'">{{ dictStore.getDictLabel('sys_status', row.isEnabled ? 'enabled' : 'disabled') }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button v-permission="'position:update'" type="primary" size="small" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'position:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, sizes, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="500px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="100px">
        <el-form-item label="岗位名称" prop="positionName">
          <el-input v-model="form.positionName" />
        </el-form-item>
        <el-form-item label="岗位编码">
          <el-input v-model="form.positionCode" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="form.sortOrder" :min="0" />
        </el-form-item>
        <el-form-item label="状态" v-if="isEdit">
          <el-switch v-model="form.isEnabled" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { positionApi } from '@/api'
import { useDictStore } from '@/store/dict'

const dictStore = useDictStore()
const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const search = reactive({ page: 1, pageSize: 10, keyword: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref()
const form = reactive<any>({ positionName: '', positionCode: '', sortOrder: 0, isEnabled: true })
const formRules = {
  positionName: [{ required: true, message: '必填', trigger: 'blur' }]
}

function formatDate(value: string) {
  if (!value) return '-'
  const d = new Date(value)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-')
}

async function loadData() {
  loading.value = true
  const res = await positionApi.getList(search)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑岗位' : '新增岗位'
  Object.assign(form, row || { positionName: '', positionCode: '', sortOrder: 0, isEnabled: true })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  if (isEdit.value) {
    await positionApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await positionApi.create(form)
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
  try {
    await positionApi.delete(row.id)
    ElMessage.success('删除成功')
    loadData()
  } catch {
    // 错误消息已由 request.ts 拦截器统一处理
  }
}

onMounted(() => { loadData() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
</style>
