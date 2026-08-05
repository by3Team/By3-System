<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>用户管理</span>
          <el-button v-permission="'user:create'" type="primary" @click="openDialog()">新增用户</el-button>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索用户名/姓名" clearable />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">搜索</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading" :row-class-name="() => 'naive-row'">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="userName" label="用户名" />
        <el-table-column prop="realName" label="姓名" />
        <el-table-column prop="email" label="邮箱" />
        <el-table-column prop="phone" label="电话" />
        <el-table-column prop="departmentName" label="部门" />
        <el-table-column prop="positionName" label="职位" />
        <el-table-column prop="gender" label="性别">
          <template #default="{ row }">
            {{ dictStore.getDictLabel('sys_gender', row.gender) }}
          </template>
        </el-table-column>
        <el-table-column prop="roleNames" label="角色">
          <template #default="{ row }">
            <el-tag v-for="r in row.roleNames" :key="r" size="small" type="info">{{ r }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="isEnabled" label="状态">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'danger'">{{ dictStore.getDictLabel('sys_status', row.isEnabled ? 'enabled' : 'disabled') }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="280">
          <template #default="{ row }">
            <el-button v-permission="'user:update'" type="primary" size="small" @click="openDialog(row)">编辑</el-button>
            <el-button v-permission="'user:update'" type="warning" size="small" @click="openResetPassword(row)">重置密码</el-button>
            <el-button v-permission="'user:delete'" type="danger" size="small" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="500px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="80px">
        <el-form-item label="用户名" prop="userName" v-if="!isEdit">
          <el-input v-model="form.userName" />
        </el-form-item>
        <el-form-item label="密码" prop="password" v-if="!isEdit">
          <el-input v-model="form.password" type="password" />
        </el-form-item>
        <el-form-item label="姓名">
          <el-input v-model="form.realName" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="form.email" />
        </el-form-item>
        <el-form-item label="电话">
          <el-input v-model="form.phone" />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="form.roleIds" multiple placeholder="选择角色">
            <el-option v-for="r in roles" :key="r.id" :label="r.roleName" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="部门">
          <el-tree-select v-model="form.departmentId" :data="departments" :props="{ label: 'deptName', value: 'id', children: 'children' }" clearable check-strictly placeholder="选择部门" />
        </el-form-item>
        <el-form-item label="职位">
          <el-select v-model="form.positionId" clearable placeholder="选择职位">
            <el-option v-for="p in positions" :key="p.id" :label="p.positionName" :value="p.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="性别">
          <el-select v-model="form.gender" clearable placeholder="选择性别">
            <el-option v-for="g in dictStore.getDict('sys_gender')" :key="g.dictValue" :label="g.dictLabel" :value="g.dictValue" />
          </el-select>
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

    <el-dialog title="重置密码" v-model="resetPwdVisible" width="400px">
      <el-form :model="resetPwdForm" :rules="resetPwdRules" ref="resetPwdFormRef" label-width="100px">
        <el-form-item label="新密码" prop="newPassword">
          <el-input v-model="resetPwdForm.newPassword" type="password" />
        </el-form-item>
        <el-form-item label="确认密码" prop="confirmPassword">
          <el-input v-model="resetPwdForm.confirmPassword" type="password" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="resetPwdVisible = false">取消</el-button>
        <el-button type="primary" @click="handleResetPassword">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { userApi, roleApi, departmentApi, positionApi } from '@/api'
import { useDictStore } from '@/store/dict'

const dictStore = useDictStore()
const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const roles = ref<any[]>([])
const departments = ref<any[]>([])
const positions = ref<any[]>([])
const search = reactive({ page: 1, pageSize: 10, keyword: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const formRef = ref()
const form = reactive<any>({ userName: '', password: '', realName: '', email: '', phone: '', roleIds: [], departmentId: null, positionId: null, gender: '', isEnabled: true })
const formRules = {
  userName: [{ required: true, message: '必填', trigger: 'blur' }],
  password: [{ required: true, message: '必填', trigger: 'blur' }]
}

const resetPwdVisible = ref(false)
const resetPwdFormRef = ref()
const resetPwdForm = reactive<any>({ userId: '', newPassword: '', confirmPassword: '' })
const resetPwdRules = {
  newPassword: [{ required: true, message: '必填', trigger: 'blur' }],
  confirmPassword: [
    { required: true, message: '必填', trigger: 'blur' },
    {
      validator: (_: any, value: string, callback: (err?: Error) => void) => {
        if (value !== resetPwdForm.newPassword) callback(new Error('两次输入密码不一致'))
        else callback()
      },
      trigger: 'blur'
    }
  ]
}

async function loadData() {
  loading.value = true
  const res = await userApi.getList(search)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

async function loadRoles() {
  roles.value = await roleApi.getAll()
}

async function loadDepartments() {
  departments.value = await departmentApi.getTree()
}

async function loadPositions() {
  const res = await positionApi.getList({ page: 1, pageSize: 1000 })
  positions.value = res.items
}

function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑用户' : '新增用户'
  Object.assign(form, row || { userName: '', password: '', realName: '', email: '', phone: '', roleIds: [], departmentId: null, positionId: null, gender: '', isEnabled: true })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  if (isEdit.value) {
    await userApi.update(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await userApi.create(form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function handleDelete(row: any) {
  await ElMessageBox.confirm('确认删除？', '提示', { type: 'warning' })
  await userApi.delete(row.id)
  ElMessage.success('删除成功')
  loadData()
}

function openResetPassword(row: any) {
  resetPwdForm.userId = row.id
  resetPwdForm.newPassword = ''
  resetPwdForm.confirmPassword = ''
  resetPwdVisible.value = true
}

async function handleResetPassword() {
  await resetPwdFormRef.value.validate()
  await userApi.resetPassword(resetPwdForm.userId, { newPassword: resetPwdForm.newPassword })
  ElMessage.success('密码重置成功')
  resetPwdVisible.value = false
}

onMounted(() => { loadData(); loadRoles(); loadDepartments(); loadPositions() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
</style>
