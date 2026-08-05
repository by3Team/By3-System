<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>字典管理</span>
          <el-button v-permission="'dict:create'" type="primary" @click="openDialog()">新增字典类型</el-button>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索字典名称/类型" clearable />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">搜索</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="dictName" label="字典名称" />
        <el-table-column prop="dictType" label="字典类型" />
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
        <el-table-column label="操作" width="260">
          <template #default="{ row }">
            <el-button v-permission="'dict:list'" type="success" size="small" @click="goData(row)">配置字典</el-button>
            <el-button v-permission="'dict:update'" type="primary" size="small" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'dict:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, sizes, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="500px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="100px">
        <el-form-item label="字典名称" prop="dictName">
          <el-input v-model="form.dictName" />
        </el-form-item>
        <el-form-item label="字典类型" prop="dictType">
          <el-input v-model="form.dictType" />
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
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { dictTypeApi } from '@/api'
import { useDictStore } from '@/store/dict'

const router = useRouter()
const dictStore = useDictStore()
const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const search = reactive({ page: 1, pageSize: 10, keyword: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref()
const form = reactive<any>({ dictName: '', dictType: '', isEnabled: true })
const formRules = {
  dictName: [{ required: true, message: '必填', trigger: 'blur' }],
  dictType: [{ required: true, message: '必填', trigger: 'blur' }]
}

function formatDate(value: string) {
  if (!value) return '-'
  const d = new Date(value)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-')
}

async function loadData() {
  loading.value = true
  const res = await dictTypeApi.getList(search)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑字典类型' : '新增字典类型'
  Object.assign(form, row || { dictName: '', dictType: '', isEnabled: true })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  if (isEdit.value) {
    await dictTypeApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await dictTypeApi.create(form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
  dictStore.refresh().catch(() => {})
}

async function handleDelete(row: any) {
  await ElMessageBox.confirm('确认删除？', '提示', { type: 'warning' })
  await dictTypeApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
  dictStore.refresh().catch(() => {})
}

function goData(row: any) {
  router.push(`/system/dict/data/${row.id}?typeName=${encodeURIComponent(row.dictName)}&typeCode=${encodeURIComponent(row.dictType)}`)
}

onMounted(() => { loadData() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
</style>
