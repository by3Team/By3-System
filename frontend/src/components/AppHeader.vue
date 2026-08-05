<template>
  <div class="app-header">
    <div class="left">
      <el-icon class="collapse-btn" @click="toggleCollapse">
        <Fold v-if="!isCollapse" />
        <Expand v-else />
      </el-icon>
      <app-breadcrumb v-if="settingsStore.settings.showBreadcrumb" />
    </div>
    <div class="right">
      <el-tooltip content="主题设置">
        <el-icon class="action-icon" @click="emit('openTheme')"><Setting /></el-icon>
      </el-tooltip>
      <el-tooltip :content="isFullscreen ? '退出全屏' : '全屏'">
        <el-icon class="action-icon" @click="toggleFullscreen">
          <FullScreen v-if="!isFullscreen" />
          <Close v-else />
        </el-icon>
      </el-tooltip>
      <el-dropdown @command="handleCommand">
        <div class="user-info">
          <el-avatar :size="28" :icon="UserFilled" />
          <span class="username">{{ auth.realName || auth.userName }}</span>
          <el-icon><ArrowDown /></el-icon>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="profile">个人中心</el-dropdown-item>
            <el-dropdown-item command="password">修改密码</el-dropdown-item>
            <el-dropdown-item divided command="logout">退出登录</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </div>

  <el-dialog title="修改密码" v-model="pwdVisible" width="400px">
    <el-form :model="pwdForm" :rules="pwdRules" ref="pwdFormRef" label-width="100px">
      <el-form-item label="新密码" prop="newPassword">
        <el-input v-model="pwdForm.newPassword" type="password" show-password />
      </el-form-item>
      <el-form-item label="确认密码" prop="confirmPassword">
        <el-input v-model="pwdForm.confirmPassword" type="password" show-password />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="pwdVisible = false">取消</el-button>
      <el-button type="primary" @click="handleChangePassword">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Fold, Expand, Setting, FullScreen, Close, ArrowDown, UserFilled } from '@element-plus/icons-vue'
import { useAuthStore } from '@/store/auth'
import { useSettingsStore } from '@/store/settings'
import { useTagsStore } from '@/store/tags'
import { userApi } from '@/api'
import AppBreadcrumb from './Breadcrumb.vue'

const props = defineProps<{
  isCollapse: boolean
}>()

const emit = defineEmits<{
  toggleCollapse: []
  openTheme: []
}>()

const router = useRouter()
const auth = useAuthStore()
const settingsStore = useSettingsStore()
const tagsStore = useTagsStore()

const isFullscreen = ref(false)

const pwdVisible = ref(false)
const pwdFormRef = ref()
const pwdForm = reactive({ newPassword: '', confirmPassword: '' })
const pwdRules = {
  newPassword: [{ required: true, message: '请输入新密码', trigger: 'blur' }],
  confirmPassword: [
    { required: true, message: '请确认新密码', trigger: 'blur' },
    {
      validator: (_: any, value: string, callback: (err?: Error) => void) => {
        if (value !== pwdForm.newPassword) callback(new Error('两次输入密码不一致'))
        else callback()
      },
      trigger: 'blur'
    }
  ]
}

function toggleCollapse() {
  emit('toggleCollapse')
}

function toggleFullscreen() {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen()
    isFullscreen.value = true
  } else {
    document.exitFullscreen()
    isFullscreen.value = false
  }
}

function handleCommand(command: string) {
  if (command === 'logout') {
    auth.clearAuth()
    tagsStore.reset()
    router.push('/login')
    ElMessage.success('已退出登录')
  } else if (command === 'profile') {
    router.push('/profile')
  } else if (command === 'password') {
    pwdForm.newPassword = ''
    pwdForm.confirmPassword = ''
    pwdVisible.value = true
  }
}

async function handleChangePassword() {
  await pwdFormRef.value.validate()
  if (!auth.userId) {
    ElMessage.error('用户信息不完整，请重新登录')
    return
  }
  await userApi.resetPassword(auth.userId, { newPassword: pwdForm.newPassword })
  ElMessage.success('密码修改成功')
  pwdVisible.value = false
}
</script>

<script lang="ts">
export default { name: 'AppHeader' }
</script>

<style scoped>
.app-header {
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  background: #ffffff;
  border-bottom: 1px solid #f1f5f9;
}
.left {
  display: flex;
  align-items: center;
  gap: 16px;
}
:deep(.collapse-btn) {
  font-size: 26px;
  cursor: pointer;
  color: #64748b;
  padding: 4px;
  border-radius: 8px;
  box-sizing: content-box;
  transition: background 0.2s, color 0.2s;
}
:deep(.collapse-btn:hover) {
  background: var(--naive-hover-bg);
  color: var(--naive-hover-text);
}
.right {
  display: flex;
  align-items: center;
  gap: 18px;
}
:deep(.action-icon) {
  font-size: 26px;
  cursor: pointer;
  color: #64748b;
  padding: 4px;
  border-radius: 8px;
  box-sizing: content-box;
  transition: background 0.2s, color 0.2s;
}
:deep(.action-icon:hover) {
  background: var(--naive-hover-bg);
  color: var(--naive-hover-primary-text);
}
.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 10px 4px 4px;
  border-radius: 20px;
  transition: background 0.2s, color 0.2s;
  color: #374151;
}
.user-info:hover {
  background: var(--naive-hover-bg);
  color: var(--naive-hover-text);
}
.username {
  font-size: 14px;
  font-weight: 500;
  color: inherit;
}
:deep(.user-info:hover .el-icon) {
  color: var(--naive-hover-text);
}
:deep(.user-info .el-icon) {
  font-size: 16px;
  color: #64748b;
}
</style>
