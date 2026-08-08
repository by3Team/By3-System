<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>角色管理</span>
          <el-button v-permission="'role:create'" type="primary" @click="openDialog()">新增角色</el-button>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索角色名" clearable />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">搜索</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="roleName" label="角色名" />
        <el-table-column prop="description" label="描述" />
        <el-table-column prop="isEnabled" label="状态">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'danger'">{{ dictStore.getDictLabel('sys_status', row.isEnabled ? 'enabled' : 'disabled') }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button v-permission="'role:update'" type="primary" size="small" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'role:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="500px">
      <el-form :model="form" ref="formRef" label-width="80px">
        <el-form-item label="角色名">
          <el-input v-model="form.roleName" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" />
        </el-form-item>
        <el-form-item label="菜单权限">
          <el-tree ref="treeRef" :data="menuTree" show-checkbox node-key="id" :default-checked-keys="form.menuIds" :props="{ label: 'menuName', children: 'children' }" />
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
import { ref, reactive, onMounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { roleApi, menuApi } from '@/api'
import { useDictStore } from '@/store/dict'

const dictStore = useDictStore()
const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const menuTree = ref<any[]>([])
const search = reactive({ page: 1, pageSize: 10, keyword: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref()
const treeRef = ref()
const form = reactive<any>({ roleName: '', description: '', menuIds: [], isEnabled: true })

async function loadData() {
  loading.value = true
  const res = await roleApi.getList(search)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

async function loadMenus() {
  menuTree.value = await menuApi.getAll()
}

async function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑角色' : '新增角色'
  Object.assign(form, row || { roleName: '', description: '', menuIds: [], isEnabled: true })
  if (row) {
    form.menuIds = await roleApi.getMenus(row.id)
  }
  dialogVisible.value = true
  nextTick(() => treeRef.value?.setCheckedKeys(form.menuIds || []))
}

async function handleSubmit() {
  form.menuIds = treeRef.value.getCheckedKeys()
  if (isEdit.value) {
    await roleApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await roleApi.create(form)
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
    await roleApi.delete(row.id)
    ElMessage.success('删除成功')
    loadData()
  } catch {
    // 错误消息已由 request.ts 拦截器统一处理
  }
}

onMounted(() => { loadData(); loadMenus() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
</style>
