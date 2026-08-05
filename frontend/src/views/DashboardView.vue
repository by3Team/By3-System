<template>
  <div class="dashboard">
    <!-- 系统简介 -->
    <el-row :gutter="16" class="mt-16">
      <el-col :span="24">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <span>欢迎使用 {{ APP_NAME }}</span>
              <el-tag type="success">{{ APP_VERSION }}</el-tag>
            </div>
          </template>
          <p class="description">{{ APP_DESCRIPTION }}</p>
        </el-card>
      </el-col>
    </el-row>

    <!-- 功能清单 + 更新日志 -->
    <el-row :gutter="16" class="mt-16">
      <el-col :xs="24" :md="16">
        <el-card shadow="hover" class="feature-card">
          <template #header>
            <span>系统功能</span>
          </template>
          <el-table :data="APP_FEATURES" stripe class="feature-table">
            <el-table-column type="index" label="序号" width="60" align="center" />
            <el-table-column prop="module" label="功能模块" width="120">
              <template #default="{ row }">
                <el-link type="primary" :href="`/feature-doc/${row.code}`" target="_blank">{{ row.module }}</el-link>
              </template>
            </el-table-column>
            <el-table-column prop="description" label="功能说明" min-width="200" />
            <el-table-column prop="tags" label="技术标签" width="260">
              <template #default="{ row }">
                <el-tag v-for="tag in row.tags" :key="tag" size="small" type="info" class="tag-item">{{ tag }}</el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
      <el-col :xs="24" :md="8">
        <el-card shadow="hover" class="changelog-card-wrapper">
          <template #header>
            <span>更新日志</span>
          </template>
          <el-timeline>
            <el-timeline-item v-for="log in CHANGELOG" :key="log.version" :timestamp="log.date" placement="top" type="primary">
              <el-card shadow="never" class="changelog-item">
                <div class="changelog-version">{{ log.version }}</div>
                <ul>
                  <li v-for="item in log.items" :key="item">{{ item }}</li>
                </ul>
              </el-card>
            </el-timeline-item>
          </el-timeline>
        </el-card>
      </el-col>
    </el-row>

    <!-- 系统依赖包 -->
    <el-row :gutter="16" class="mt-16">
      <el-col :span="24">
        <el-card shadow="hover" v-loading="packagesLoading">
          <template #header>
            <span>系统引入包</span>
          </template>
          <el-tabs v-model="activePackageTab" type="border-card">
            <el-tab-pane label="后端依赖" name="backend">
              <el-table :data="backendPackages" stripe height="420">
                <el-table-column type="index" label="序号" width="60" align="center" />
                <el-table-column prop="project" label="项目" width="180" />
                <el-table-column prop="name" label="包名" min-width="220" />
                <el-table-column prop="version" label="版本" width="140" />
                <el-table-column prop="license" label="协议" width="140" />
              </el-table>
            </el-tab-pane>
            <el-tab-pane label="前端依赖" name="frontend">
              <el-tabs v-model="activeFrontendTab" tab-position="left" class="frontend-tabs">
                <el-tab-pane label="生产依赖" name="dependencies">
                  <el-table :data="frontendDependencies" stripe height="380">
                    <el-table-column type="index" label="序号" width="60" align="center" />
                    <el-table-column prop="name" label="包名" min-width="220" />
                    <el-table-column prop="version" label="版本" width="140" />
                    <el-table-column prop="license" label="协议" width="140" />
                  </el-table>
                </el-tab-pane>
                <el-tab-pane label="开发依赖" name="devDependencies">
                  <el-table :data="frontendDevDependencies" stripe height="380">
                    <el-table-column type="index" label="序号" width="60" align="center" />
                    <el-table-column prop="name" label="包名" min-width="220" />
                    <el-table-column prop="version" label="版本" width="140" />
                    <el-table-column prop="license" label="协议" width="140" />
                  </el-table>
                </el-tab-pane>
              </el-tabs>
            </el-tab-pane>
          </el-tabs>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { APP_NAME, APP_VERSION, APP_DESCRIPTION, APP_FEATURES, CHANGELOG } from '@/constants/app'
import { systemInfoApi } from '@/api'

const activePackageTab = ref('backend')
const activeFrontendTab = ref('dependencies')
const packagesLoading = ref(false)
const backendPackages = ref<any[]>([])
const frontendDependencies = ref<any[]>([])
const frontendDevDependencies = ref<any[]>([])

async function loadPackages() {
  packagesLoading.value = true
  try {
    const res = await systemInfoApi.getPackages()
    const backend: any[] = []
    res.backend?.forEach((project: any) => {
      project.packages?.forEach((pkg: any) => {
        backend.push({ project: project.project, name: pkg.name, version: pkg.version, license: pkg.license })
      })
    })
    backendPackages.value = backend
    frontendDependencies.value = res.frontend?.dependencies || []
    frontendDevDependencies.value = res.frontend?.devDependencies || []
  } finally {
    packagesLoading.value = false
  }
}

onMounted(() => {
  loadPackages()
})
</script>

<script lang="ts">
export default { name: 'DashboardView' }
</script>

<style scoped>
.dashboard {
  padding-bottom: 20px;
}
.mt-16 {
  margin-top: 16px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.description {
  color: #606266;
  line-height: 1.8;
  margin: 0;
}
.feature-card,
.changelog-card-wrapper {
  height: 100%;
}
.feature-card :deep(.el-card__body) {
  padding: 0;
}
.feature-table {
  width: 100%;
}
.tag-item {
  margin-right: 6px;
  margin-bottom: 4px;
}
.changelog-card-wrapper :deep(.el-card__body) {
  max-height: 560px;
  overflow-y: auto;
}
.changelog-item {
  background: #f8fafc;
}
.changelog-version {
  font-weight: bold;
  color: #409EFF;
  margin-bottom: 8px;
}
.frontend-tabs :deep(.el-tabs__content) {
  padding: 8px 0;
}
</style>
