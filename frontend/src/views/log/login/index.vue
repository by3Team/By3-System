<template>
  <div>
    <el-card>
      <template #header>
        <span>登录日志</span>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.userName" placeholder="用户" clearable />
        </el-form-item>
        <el-form-item>
          <el-select v-model="search.isSuccess" placeholder="结果" clearable style="width: 110px">
            <el-option label="成功" :value="true" />
            <el-option label="失败" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="消息/IP" clearable />
        </el-form-item>
        <el-form-item>
          <el-date-picker
            v-model="search.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DDTHH:mm:ss"
            clearable
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="userName" label="用户" width="120" />
        <el-table-column prop="isSuccess" label="结果" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isSuccess ? 'success' : 'danger'">{{ row.isSuccess ? '成功' : '失败' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="message" label="消息" />
        <el-table-column prop="ipAddress" label="IP" width="130" />
        <el-table-column label="时间" width="180">
          <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { loginLogApi } from '@/api'
import { formatDateTime } from '@/utils/format'

const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const search = reactive({
  page: 1,
  pageSize: 20,
  userName: '',
  isSuccess: undefined as boolean | undefined,
  keyword: '',
  dateRange: [] as string[]
})

async function loadData() {
  loading.value = true
  const params: any = {
    page: search.page,
    pageSize: search.pageSize,
    userName: search.userName || undefined,
    isSuccess: search.isSuccess,
    keyword: search.keyword || undefined,
    startTime: search.dateRange && search.dateRange.length === 2 ? search.dateRange[0] : undefined,
    endTime: search.dateRange && search.dateRange.length === 2 ? search.dateRange[1] : undefined
  }
  const res = await loginLogApi.getList(params)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

function handleSearch() {
  search.page = 1
  loadData()
}

function handleReset() {
  search.userName = ''
  search.isSuccess = undefined
  search.keyword = ''
  search.dateRange = []
  search.page = 1
  loadData()
}

onMounted(loadData)
</script>

<style scoped>
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
</style>
