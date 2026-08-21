<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>邮件发送日志</span>
        </div>
      </template>
      <el-form :inline="true" class="search-form">
        <el-form-item>
          <el-input v-model="search.keyword" placeholder="搜索收件人/主题" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="search.status" placeholder="全部" clearable style="width: 120px">
            <el-option label="全部" value="" />
            <el-option label="待发送" value="pending" />
            <el-option label="成功" value="sent" />
            <el-option label="失败" value="failed" />
          </el-select>
        </el-form-item>
        <el-form-item label="开始时间">
          <el-date-picker v-model="search.startDate" type="datetime" placeholder="开始时间" clearable value-format="YYYY-MM-DDTHH:mm:ss" />
        </el-form-item>
        <el-form-item label="结束时间">
          <el-date-picker v-model="search.endDate" type="datetime" placeholder="结束时间" clearable value-format="YYYY-MM-DDTHH:mm:ss" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">搜索</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="tableData" v-loading="loading">
        <el-table-column type="index" :index="(i: number) => (search.page - 1) * search.pageSize + i + 1" label="序号" width="70" align="center" />
        <el-table-column prop="toAddresses" label="收件人" />
        <el-table-column prop="ccAddresses" label="抄送人" show-overflow-tooltip />
        <el-table-column prop="subject" label="主题" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'sent' ? 'success' : row.status === 'failed' ? 'danger' : 'info'">
              {{ row.status === 'sent' ? '成功' : row.status === 'failed' ? '失败' : '待发送' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="senderType" label="发送来源" width="100">
          <template #default="{ row }">
            {{ formatSenderType(row.senderType) }}
          </template>
        </el-table-column>
        <el-table-column prop="senderName" label="发送人" width="120" />
        <el-table-column prop="errorMessage" label="错误信息" show-overflow-tooltip />
        <el-table-column prop="sentAt" label="发送时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.sentAt) }}
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, sizes, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { emailApi } from '@/api'

const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const search = reactive({ page: 1, pageSize: 10, keyword: '', status: '', startDate: '', endDate: '' })

function formatDate(value: string) {
  if (!value) return '-'
  const d = new Date(value)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/\//g, '-')
}

function formatSenderType(value: string) {
  const map: Record<string, string> = {
    System: '系统',
    Manual: '后台',
    Scheduled: '定时任务',
    Api: '外部接口'
  }
  return map[value] || value || '-'
}

async function loadData() {
  loading.value = true
  const res = await emailApi.getLogs(search)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

onMounted(() => { loadData() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-form { margin-bottom: 15px; }
.pagination { margin-top: 15px; justify-content: flex-end; }
</style>
