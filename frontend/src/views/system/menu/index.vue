<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>菜单管理</span>
          <el-button v-permission="'menu:create'" type="primary" @click="openDialog()">新增菜单</el-button>
        </div>
      </template>
      <el-table :data="tableData" row-key="id" default-expand-all v-loading="loading">
        <el-table-column type="index" label="序号" width="70" align="center" />
        <el-table-column prop="menuName" label="菜单名" />
        <el-table-column prop="route" label="路由" />
        <el-table-column prop="permission" label="权限标识" />
        <el-table-column prop="menuType" label="类型">
          <template #default="{ row }">
            <el-tag v-if="row.menuType === 1" type="primary">{{ dictStore.getDictLabel('sys_menu_type', '1') }}</el-tag>
            <el-tag v-else-if="row.menuType === 2" type="success">{{ dictStore.getDictLabel('sys_menu_type', '2') }}</el-tag>
            <el-tag v-else type="warning">{{ dictStore.getDictLabel('sys_menu_type', '3') }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="sortOrder" label="排序" />
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button v-permission="'menu:update'" type="primary" size="small" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'menu:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="500px">
      <el-form :model="form" ref="formRef" label-width="100px">
        <el-form-item label="菜单名">
          <el-input v-model="form.menuName" />
        </el-form-item>
        <el-form-item label="类型">
          <el-radio-group v-model="form.menuType">
            <el-radio :label="1">目录</el-radio>
            <el-radio :label="2">菜单</el-radio>
            <el-radio :label="3">按钮</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="父菜单" v-if="form.menuType !== 1">
          <el-tree-select v-model="form.parentId" :data="parentMenus" :props="{ label: 'menuName', value: 'id', children: 'children' }" clearable check-strictly />
        </el-form-item>
        <el-form-item label="路由" v-if="form.menuType === 2">
          <el-input v-model="form.route" />
        </el-form-item>
        <el-form-item label="组件路径" v-if="form.menuType === 2">
          <el-input v-model="form.component" placeholder="如: system/user/index" />
        </el-form-item>
        <el-form-item label="权限标识" v-if="form.menuType === 3">
          <el-input v-model="form.permission" placeholder="如: user:create" />
        </el-form-item>
        <el-form-item label="图标" v-if="form.menuType !== 3">
          <el-input v-model="form.icon" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="form.sortOrder" />
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
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { menuApi } from '@/api'
import { useDictStore } from '@/store/dict'

const dictStore = useDictStore()
const loading = ref(false)
const tableData = ref<any[]>([])

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const form = reactive<any>({ menuName: '', menuType: 2, route: '', component: '', permission: '', icon: '', sortOrder: 0, parentId: null })

const parentMenus = computed(() => tableData.value.filter(m => m.menuType !== 3))

async function loadData() {
  loading.value = true
  tableData.value = await menuApi.getAll()
  loading.value = false
}

function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑菜单' : '新增菜单'
  Object.assign(form, row || { menuName: '', menuType: 2, route: '', component: '', permission: '', icon: '', sortOrder: 0, parentId: null })
  dialogVisible.value = true
}

async function handleSubmit() {
  if (isEdit.value) {
    await menuApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await menuApi.create(form)
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
  await menuApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
}

onMounted(loadData)
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
