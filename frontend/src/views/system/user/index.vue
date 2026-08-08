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
        <el-form-item label="姓名" prop="realName">
          <el-input v-model="form.realName" />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="form.email" />
        </el-form-item>
        <el-form-item label="电话" prop="phone">
          <el-input v-model="form.phone" @focus="onPhoneFocus" @blur="onPhoneBlur" @input="onPhoneInput" placeholder="未修改时显示掩码，点击后输入完整手机号" />
        </el-form-item>
        <el-form-item label="角色" prop="roleIds">
          <el-select v-model="form.roleIds" multiple placeholder="选择角色">
            <el-option v-for="r in roles" :key="r.id" :label="r.roleName" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="部门" prop="departmentId">
          <el-tree-select v-model="form.departmentId" :data="departments" :props="{ label: 'deptName', value: 'id', children: 'children' }" clearable check-strictly placeholder="选择部门" />
        </el-form-item>
        <el-form-item label="职位" prop="positionId">
          <el-select v-model="form.positionId" clearable placeholder="选择职位">
            <el-option v-for="p in positions" :key="p.id" :label="p.positionName" :value="p.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="性别" prop="gender">
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
const originalPhone = ref('')
const isPhoneModified = ref(false)

const formRules = {
  userName: [
    { required: true, message: '用户名不能为空', trigger: 'blur' },
    { min: 3, max: 20, message: '用户名长度3-20位', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '密码不能为空', trigger: 'blur' },
    { min: 8, message: '密码最少8位', trigger: 'blur' },
    { pattern: /[A-Z]/, message: '密码须包含至少一个大写字母', trigger: 'blur' },
    { pattern: /[a-z]/, message: '密码须包含至少一个小写字母', trigger: 'blur' },
    { pattern: /[0-9]/, message: '密码须包含至少一个数字', trigger: 'blur' }
  ],
  realName: [
    { required: true, message: '姓名不能为空', trigger: 'blur' },
    { min: 1, message: '姓名至少1个字符', trigger: 'blur' }
  ],
  email: [
    { type: 'email', message: '邮箱格式错误', trigger: 'blur' }
  ],
  phone: [
    {
      validator: (_: any, value: string, callback: (err?: Error) => void) => {
        // 未修改时（回显的是掩码值），跳过格式校验
        if (!isPhoneModified.value) {
          callback()
          return
        }
        if (!value || /^1[3-9]\d{9}$/.test(value)) callback()
        else callback(new Error('电话格式错误'))
      },
      trigger: 'blur'
    }
  ],
  roleIds: [
    { required: true, type: 'array', min: 1, message: '请选择角色', trigger: 'change' }
  ],
  departmentId: [
    { required: true, message: '请选择部门', trigger: 'change' }
  ],
  positionId: [
    { required: true, message: '请选择职位', trigger: 'change' }
  ],
  gender: [
    { required: true, message: '请选择性别', trigger: 'change' }
  ]
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

function resetForm() {
  Object.assign(form, { userName: '', password: '', realName: '', email: '', phone: '', roleIds: [], departmentId: null, positionId: null, gender: '', isEnabled: true })
  originalPhone.value = ''
  isPhoneModified.value = false
}

function onPhoneFocus() {
  if (form.phone.includes('*') && !isPhoneModified.value) {
    form.phone = ''
    isPhoneModified.value = true
  }
}

function onPhoneInput() {
  if (!isPhoneModified.value) {
    isPhoneModified.value = true
  }
}

function onPhoneBlur() {
  if (isPhoneModified.value && !form.phone && originalPhone.value) {
    form.phone = originalPhone.value
    isPhoneModified.value = false
  }
}

async function openDialog(row?: any) {
  isEdit.value = !!row
  dialogTitle.value = row ? '编辑用户' : '新增用户'
  resetForm()

  if (row?.id) {
    try {
      const detail = await userApi.getById(row.id)
      form.id = detail.id
      form.userName = detail.userName || ''
      form.realName = detail.realName || ''
      form.email = detail.email || ''
      form.phone = detail.phone || ''
      originalPhone.value = detail.phone || ''
      isPhoneModified.value = false
      form.gender = detail.gender || ''
      form.departmentId = detail.departmentId || null
      form.positionId = detail.positionId || null
      form.isEnabled = detail.isEnabled ?? true
      form.roleIds = detail.roleIds || []
    } catch {
      ElMessage.error('获取用户详情失败')
      return
    }
  }

  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) {
    ElMessage.warning('请正确填写表单后再提交')
    return
  }

  const submitForm = { ...form }
  if (isEdit.value) {
    // 未修改手机号，或用户误清空后未输入，都使用原始手机号
    if (!isPhoneModified.value || !submitForm.phone) {
      submitForm.phone = originalPhone.value
    }
  }

  try {
    if (isEdit.value) {
      await userApi.update(form.id, submitForm)
      ElMessage.success('更新成功')
    } else {
      await userApi.create(submitForm)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch (err: any) {
    const message = err?.message || err?.data?.message || '操作失败'
    ElMessage.error(message)
  }
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
