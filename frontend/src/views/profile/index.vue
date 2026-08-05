<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>个人中心</span>
        </div>
      </template>
      <div class="profile-content">
        <div class="avatar-section">
          <el-avatar :size="80" :icon="UserFilled" />
          <h3>{{ auth.realName || auth.userName }}</h3>
          <p class="username">{{ auth.userName }}</p>
        </div>
        <el-descriptions :column="2" border>
          <el-descriptions-item label="用户名">{{ auth.userName }}</el-descriptions-item>
          <el-descriptions-item label="姓名">{{ auth.realName || '-' }}</el-descriptions-item>
          <el-descriptions-item label="角色">{{ roleNamesText }}</el-descriptions-item>
          <el-descriptions-item label="权限数量">{{ auth.permissions.length }}</el-descriptions-item>
        </el-descriptions>
      </div>
    </el-card>

    <el-card class="mt-16">
      <template #header>
        <div class="card-header">
          <span>修改密码</span>
        </div>
      </template>
      <el-form :model="pwdForm" :rules="pwdRules" ref="pwdFormRef" label-width="100px" style="max-width: 400px;">
        <el-form-item label="新密码" prop="newPassword">
          <el-input v-model="pwdForm.newPassword" type="password" show-password />
        </el-form-item>
        <el-form-item label="确认密码" prop="confirmPassword">
          <el-input v-model="pwdForm.confirmPassword" type="password" show-password />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleChangePassword">保存</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { UserFilled } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '@/store/auth'
import { userApi } from '@/api'

const auth = useAuthStore()

const roleNamesText = computed(() => {
  // 角色信息未在 auth 中存储，显示占位文本
  return '-'
})

const pwdFormRef = ref()
const pwdForm = reactive({ newPassword: '', confirmPassword: '' })
const pwdRules = {
  newPassword: [{ required: true, message: '请输入新密码', trigger: 'blur' }],
  confirmPassword: [
    { required: true, message: '请确认新密码', trigger: 'blur' },
    {
      validator: (_: any, value: string, callback: Function) => {
        if (value !== pwdForm.newPassword) callback(new Error('两次输入密码不一致'))
        else callback()
      },
      trigger: 'blur'
    }
  ]
}

async function handleChangePassword() {
  await pwdFormRef.value.validate()
  if (!auth.userId) {
    ElMessage.error('用户信息不完整，请重新登录')
    return
  }
  await userApi.resetPassword(auth.userId, { newPassword: pwdForm.newPassword })
  ElMessage.success('密码修改成功')
  pwdForm.newPassword = ''
  pwdForm.confirmPassword = ''
}
</script>

<script lang="ts">
export default { name: 'ProfileView' }
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.profile-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}
.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}
.avatar-section h3 {
  margin: 0;
  font-size: 20px;
  color: #1f2937;
}
.avatar-section .username {
  margin: 0;
  color: #6b7280;
  font-size: 14px;
}
.mt-16 {
  margin-top: 16px;
}
</style>
