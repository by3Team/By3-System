<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>系统设置</span>
        </div>
      </template>
      <el-menu :default-active="activeMenu" mode="horizontal" @select="handleSelect">
        <el-menu-item index="/system/setting/email">邮件设置</el-menu-item>
      </el-menu>
      <div class="setting-content">
        <router-view />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const activeMenu = ref(route.path)

watch(() => route.path, (val) => {
  activeMenu.value = val
})

function handleSelect(path: string) {
  router.push(path)
}
</script>

<script lang="ts">
export default { name: 'SystemSettingView' }
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.setting-content { margin-top: 16px; }
</style>
