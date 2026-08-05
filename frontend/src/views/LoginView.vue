<template>
  <div class="login-page">
    <div class="login-bg">
      <div class="login-box">
        <div class="login-header">
          <img src="/favicon.svg" alt="logo" class="logo">
          <h2>{{ APP_NAME }} 管理系统</h2>
          <p>企业级后台管理框架</p>
          <el-tag type="success" size="small" class="version-tag">{{ APP_VERSION }}</el-tag>
        </div>
        <el-form :model="form" :rules="rules" ref="formRef" class="login-form">
          <el-form-item prop="userName">
            <el-input v-model="form.userName" placeholder="用户名" size="large" :prefix-icon="User" />
          </el-form-item>
          <el-form-item prop="password">
            <el-input v-model="form.password" type="password" placeholder="密码" size="large" :prefix-icon="Lock" show-password @keyup.enter="handleLogin" />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" size="large" @click="handleLogin" :loading="loading" style="width: 100%">登 录</el-button>
          </el-form-item>
        </el-form>
        <div class="login-tips">
          <p>默认账号：admin / Demo123!</p>
        </div>
      </div>
      <div class="login-footer">
        <p>{{ APP_COPYRIGHT }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import { useAuthStore } from '@/store/auth'
import { useDictStore } from '@/store/dict'
import { authApi } from '@/api'
import { addDynamicRoutes } from '@/router'
import { APP_NAME, APP_VERSION, APP_COPYRIGHT } from '@/constants/app'

const router = useRouter()
const auth = useAuthStore()
const dictStore = useDictStore()
const formRef = ref()
const loading = ref(false)

const form = reactive({ userName: 'admin', password: 'Demo123!' })
const rules = {
  userName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

async function handleLogin() {
  await formRef.value.validate()
  loading.value = true
  try {
    const res = await authApi.login(form)
    auth.setAuth(res)
    addDynamicRoutes(res.menus)
    await dictStore.loadAll()
    ElMessage.success('登录成功')
    router.push('/')
  } catch (e) {
    // 拦截器已处理错误提示
  } finally {
    loading.value = false
  }
}
</script>

<script lang="ts">
export default { name: 'LoginView' }
</script>

<style scoped>
.login-page {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #e0e7ff 0%, #f3e8ff 100%);
}
.login-bg {
  width: 420px;
  padding: 44px 40px 24px;
  background: #ffffff;
  border-radius: 24px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.08);
  display: flex;
  flex-direction: column;
  align-items: center;
}
.login-box {
  width: 100%;
}
.login-header {
  text-align: center;
  margin-bottom: 36px;
}
.login-header .logo {
  width: 68px;
  height: 68px;
  margin-bottom: 18px;
}
.login-header h2 {
  margin: 0 0 8px 0;
  font-size: 26px;
  color: #1f2937;
  font-weight: 700;
}
.login-header p {
  margin: 0 0 12px 0;
  color: #6b7280;
  font-size: 14px;
}
.version-tag {
  font-weight: 500;
}
.login-form {
  margin-bottom: 24px;
}
.login-form :deep(.el-input__wrapper) {
  padding: 4px 11px;
}
.login-tips {
  text-align: center;
  color: #9ca3af;
  font-size: 13px;
  margin-bottom: 24px;
}
.login-footer {
  text-align: center;
  color: #9ca3af;
  font-size: 12px;
  border-top: 1px solid #f1f5f9;
  padding-top: 16px;
  width: 100%;
}
.login-footer p {
  margin: 0;
}
</style>
