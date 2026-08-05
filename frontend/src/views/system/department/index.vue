<template>
  <div class="dept-page">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>组织机构</span>
          <el-button v-permission="'dept:create'" type="primary" @click="openDialog()">新增部门</el-button>
        </div>
      </template>
      <el-tree
        :data="treeData"
        :props="{ label: 'deptName', children: 'children' }"
        node-key="id"
        default-expand-all
        :expand-on-click-node="false"
        v-loading="loading"
      >
        <template #default="{ node, data }">
          <div class="tree-node">
            <span class="node-label">
              <el-icon><OfficeBuilding /></el-icon>
              {{ data.deptName }}
              <el-tag v-if="data.deptCode" size="small" type="info">{{ data.deptCode }}</el-tag>
              <el-tag :type="data.isEnabled ? 'success' : 'danger'" size="small">{{ dictStore.getDictLabel('sys_status', data.isEnabled ? 'enabled' : 'disabled') }}</el-tag>
            </span>
            <span class="node-actions">
              <el-button v-permission="'dept:create'" type="primary" link size="small" @click="openDialog(undefined, data.id)">添加下级</el-button>
              <el-button v-permission="'dept:update'" type="primary" link size="small" @click="openDialog(data)">编辑</el-button>
              <el-button v-permission="'dept:delete'" type="danger" link size="small" @click="handleDelete(data)">删除</el-button>
            </span>
          </div>
        </template>
      </el-tree>
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="500px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="100px">
        <el-form-item label="部门名称" prop="deptName">
          <el-input v-model="form.deptName" />
        </el-form-item>
        <el-form-item label="部门编码">
          <el-input v-model="form.deptCode" />
        </el-form-item>
        <el-form-item label="上级部门">
          <el-tree-select
            v-model="form.parentId"
            :data="treeData"
            :props="{ label: 'deptName', value: 'id', children: 'children' }"
            check-strictly
            clearable
            placeholder="选择上级部门"
            style="width: 100%"
          />
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
import { OfficeBuilding } from '@element-plus/icons-vue'
import { departmentApi } from '@/api'
import { useDictStore } from '@/store/dict'

const dictStore = useDictStore()
const loading = ref(false)
const treeData = ref<any[]>([])

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref()
const form = reactive<any>({ id: '', deptName: '', deptCode: '', parentId: undefined, sortOrder: 0, isEnabled: true })
const formRules = {
  deptName: [{ required: true, message: '必填', trigger: 'blur' }]
}

async function loadData() {
  loading.value = true
  treeData.value = await departmentApi.getTree()
  loading.value = false
}

function openDialog(row?: any, parentId?: string) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑部门' : '新增部门'
  Object.assign(form, row || { deptName: '', deptCode: '', parentId: parentId || undefined, sortOrder: 0, isEnabled: true })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  if (isEdit.value) {
    await departmentApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await departmentApi.create(form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function handleDelete(row: any) {
  await ElMessageBox.confirm('确认删除该部门？', '提示', { type: 'warning' })
  await departmentApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
}

onMounted(() => { loadData() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.tree-node {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  padding: 4px 0;
}
.node-label {
  display: flex;
  align-items: center;
  gap: 8px;
}
.node-label .el-icon {
  color: var(--el-color-primary);
}
.node-actions {
  display: flex;
  gap: 8px;
}
</style>
