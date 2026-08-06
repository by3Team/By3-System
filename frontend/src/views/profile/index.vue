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
          <el-avatar :size="96" :icon="UserFilled" />
          <h3>{{ displayName }}</h3>
          <p class="username">{{ auth.userName }}</p>
          <el-tag type="success" size="small">超级管理员</el-tag>
        </div>
        <el-descriptions :column="2" border>
          <el-descriptions-item label="用户名">{{ auth.userName }}</el-descriptions-item>
          <el-descriptions-item label="姓名">{{ auth.realName || '-' }}</el-descriptions-item>
          <el-descriptions-item label="角色">超级管理员</el-descriptions-item>
          <el-descriptions-item label="权限数量">{{ auth.permissions.length }}</el-descriptions-item>
        </el-descriptions>
      </div>
    </el-card>

    <el-row :gutter="16" class="stat-row">
      <el-col :span="12">
        <el-card shadow="hover">
          <div class="stat-item">
            <el-icon :size="28" color="#409eff"><Lock /></el-icon>
            <div class="stat-info">
              <div class="stat-value">{{ auth.permissions.length }}</div>
              <div class="stat-label">权限数量</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card shadow="hover">
          <div class="stat-item">
            <el-icon :size="28" color="#67c23a"><Menu /></el-icon>
            <div class="stat-info">
              <div class="stat-value">{{ menuCount }}</div>
              <div class="stat-label">可用菜单</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { UserFilled, Lock, Menu } from '@element-plus/icons-vue'
import { useAuthStore } from '@/store/auth'

const auth = useAuthStore()

const displayName = computed(() => auth.realName || auth.userName || '-')

const menuCount = computed(() => {
  function count(menus: any[]): number {
    return menus.reduce((sum, m) => sum + 1 + (m.children ? count(m.children) : 0), 0)
  }
  return count(auth.menus)
})
</script>

<script lang="ts">
export default { name: 'ProfileView' }
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.profile-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 16px 0;
}

.avatar-section h3 {
  margin: 0;
  font-size: 22px;
  color: #1f2937;
}

.avatar-section .username {
  margin: 0;
  color: #6b7280;
  font-size: 14px;
}

.stat-row {
  margin-top: 16px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 8px 4px;
}

.stat-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-value {
  font-size: 24px;
  font-weight: 600;
  color: #1f2937;
  line-height: 1;
}

.stat-label {
  font-size: 13px;
  color: #6b7280;
}
</style>
