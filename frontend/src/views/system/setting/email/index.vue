<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>邮件发送端设置</span>
        </div>
      </template>
      <el-form :model="form" :rules="rules" ref="formRef" label-width="120px" v-loading="loading">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="SMTP 服务器" prop="smtpHost">
              <el-input v-model="form.smtpHost" placeholder="例如 smtp.example.com" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="SMTP 端口" prop="smtpPort">
              <el-input-number v-model="form.smtpPort" :min="1" :max="65535" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="发件账号" prop="username">
              <el-input v-model="form.username" placeholder="登录 SMTP 的账号" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="发件密码" prop="password">
              <el-input v-model="form.password" type="password" show-password placeholder="登录 SMTP 的密码或授权码" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="发件人名称" prop="fromName">
              <el-input v-model="form.fromName" placeholder="显示的发件人名称" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="发件人地址" prop="fromAddress">
              <el-input v-model="form.fromAddress" placeholder="例如 noreply@example.com" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="启用 SSL/TLS" prop="enableSsl">
          <el-switch v-model="form.enableSsl" active-text="开启" inactive-text="关闭" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSave" :loading="saving">保存设置</el-button>
          <el-button @click="handleTest" :loading="testing">测试连接</el-button>
          <el-button @click="loadData">刷新</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { emailSettingApi } from '@/api'

const loading = ref(false)
const saving = ref(false)
const testing = ref(false)
const formRef = ref()
const form = reactive<any>({
  id: '',
  smtpHost: '',
  smtpPort: 587,
  username: '',
  password: '',
  fromName: '',
  fromAddress: '',
  enableSsl: true,
  isEnabled: true
})

const rules = {
  smtpHost: [{ required: true, message: '请输入 SMTP 服务器', trigger: 'blur' }],
  smtpPort: [{ required: true, message: '请输入 SMTP 端口', trigger: 'blur' }],
  username: [{ required: true, message: '请输入发件账号', trigger: 'blur' }],
  password: [{ required: true, message: '请输入发件密码', trigger: 'blur' }],
  fromAddress: [{ required: true, message: '请输入发件人地址', trigger: 'blur' }]
}

async function loadData() {
  loading.value = true
  try {
    const res = await emailSettingApi.get()
    Object.assign(form, res)
  } finally {
    loading.value = false
  }
}

async function handleSave() {
  await formRef.value.validate()
  saving.value = true
  try {
    await emailSettingApi.update(form)
    ElMessage.success('保存成功')
    loadData()
  } finally {
    saving.value = false
  }
}

async function handleTest() {
  await formRef.value.validate()
  testing.value = true
  try {
    await emailSettingApi.test(form)
    ElMessage.success('连接成功')
  } finally {
    testing.value = false
  }
}

onMounted(() => { loadData() })
</script>

<script lang="ts">
export default { name: 'EmailSettingView' }
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
