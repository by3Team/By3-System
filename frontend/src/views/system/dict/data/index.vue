<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>字典数据：{{ typeName }}（{{ typeCode }}）</span>
          <div>
            <el-button @click="goBack">返回</el-button>
            <el-button v-permission="'dict:create'" type="primary" @click="openDialog()">新增字典项</el-button>
          </div>
        </div>
      </template>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="dictLabel" label="字典标签" />
        <el-table-column prop="dictValue" label="字典值" />
        <el-table-column prop="sortOrder" label="排序" width="80" align="center" />
        <el-table-column prop="isDefault" label="是否默认" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isDefault ? 'success' : 'info'">{{ dictStore.getDictLabel('sys_yes_no', row.isDefault ? 'yes' : 'no') }}</el-tag>
          </template>
        </el-table-column>
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
            <el-button v-permission="'dict:update'" type="primary" size="small" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'dict:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, sizes, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="500px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="100px">
        <el-form-item label="字典标签" prop="dictLabel">
          <el-input v-model="form.dictLabel" />
        </el-form-item>
        <el-form-item label="字典值" prop="dictValue">
          <el-input v-model="form.dictValue" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="form.sortOrder" :min="0" />
        </el-form-item>
        <el-form-item label="是否默认">
          <el-switch v-model="form.isDefault" />
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
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { dictDataApi } from '@/api'
import { useDictStore } from '@/store/dict'

const route = useRoute()
const router = useRouter()
const dictStore = useDictStore()
const typeId = route.params.typeId as string
const typeName = ref(route.query.typeName as string || '')
const typeCode = ref(route.query.typeCode as string || '')

const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const search = reactive({ page: 1, pageSize: 10, dictTypeId: typeId })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref()
const form = reactive<any>({ dictTypeId: typeId, dictLabel: '', dictValue: '', sortOrder: 0, isDefault: false, isEnabled: true })
const formRules = {
  dictLabel: [{ required: true, message: '必填', trigger: 'blur' }],
  dictValue: [{ required: true, message: '必填', trigger: 'blur' }]
}

function formatDate(value: string) {
  if (!value) return '-'
  const d = new Date(value)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-')
}

async function loadData() {
  loading.value = true
  const res = await dictDataApi.getList(search)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑字典项' : '新增字典项'
  Object.assign(form, row || { dictTypeId: typeId, dictLabel: '', dictValue: '', sortOrder: 0, isDefault: false, isEnabled: true })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  if (isEdit.value) {
    await dictDataApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await dictDataApi.create(form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
  dictStore.refresh().catch(() => {})
}

async function handleDelete(row: any) {
  await ElMessageBox.confirm('确认删除？', '提示', { type: 'warning' })
  await dictDataApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
  dictStore.refresh().catch(() => {})
}

function goBack() {
  router.push('/system/dict')
}

onMounted(() => { loadData() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.pagination { margin-top: 15px; justify-content: flex-end; }
</style>
