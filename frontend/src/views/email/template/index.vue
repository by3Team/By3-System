<template>
  <div>
    <el-card>
      <template #header>
        <div class="card-header">
          <span>邮件模板管理</span>
          <el-button v-permission="'email:create'" type="primary" @click="openTemplateDialog()">新增模板</el-button>
        </div>
      </template>
      <el-table :data="tableData" v-loading="loading" row-key="id" default-expand-all>
        <el-table-column type="index" label="序号" width="70" align="center" />
        <el-table-column prop="templateCode" label="模板编码" />
        <el-table-column prop="templateName" label="模板名称" />
        <el-table-column prop="description" label="描述" />
        <el-table-column prop="isEnabled" label="状态">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'danger'">{{ dictStore.getDictLabel('sys_status', row.isEnabled ? 'enabled' : 'disabled') }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="360">
          <template #default="{ row }">
            <el-button v-permission="'email:update'" type="primary" size="small" @click="openTemplateDialog(row)">编辑</el-button>
            <el-button v-permission="'email:create'" type="success" size="small" @click="openVersionDialog(row.id)">新增版本</el-button>
            <el-button v-permission="'email:send'" type="warning" size="small" @click="openTestDialog(row.id)">测试</el-button>
            <el-button v-permission="'email:send'" type="info" size="small" @click="openSendDialog(row.id)">发送</el-button>
            <el-button v-permission="'email:delete'" type="danger" size="small" @click="handleDeleteTemplate(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="search.page" v-model:page-size="search.pageSize" :total="total" layout="total, prev, pager, next" @change="loadData" class="pagination" />
    </el-card>

    <el-dialog :title="templateDialogTitle" v-model="templateDialogVisible" width="500px">
      <el-form :model="templateForm" :rules="templateRules" ref="templateFormRef" label-width="100px">
        <el-form-item label="模板编码" prop="templateCode">
          <el-input v-model="templateForm.templateCode" :disabled="isEditTemplate" />
        </el-form-item>
        <el-form-item label="模板名称" prop="templateName">
          <el-input v-model="templateForm.templateName" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="templateForm.description" type="textarea" />
        </el-form-item>
        <el-form-item label="状态" v-if="isEditTemplate">
          <el-switch v-model="templateForm.isEnabled" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="templateDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmitTemplate">确定</el-button>
      </template>
    </el-dialog>

    <el-dialog title="版本管理" v-model="versionDialogVisible" width="700px">
      <el-button v-permission="'email:create'" type="primary" @click="openVersionFormDialog()">新增版本</el-button>
      <el-table :data="versions" class="version-table">
        <el-table-column prop="version" label="版本号" width="100" />
        <el-table-column prop="subject" label="主题" />
        <el-table-column prop="isEnabled" label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'danger'">{{ dictStore.getDictLabel('sys_status', row.isEnabled ? 'enabled' : 'disabled') }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180">
          <template #default="{ row }">
            <el-button v-permission="'email:update'" type="primary" size="small" @click="openVersionFormDialog(row)">编辑</el-button>
            <el-button v-permission="'email:delete'" type="danger" size="small" @click="handleDeleteVersion(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-dialog>

    <el-dialog :title="versionFormTitle" v-model="versionFormVisible" width="900px" top="5vh">
      <el-form :model="versionForm" :rules="versionRules" ref="versionFormRef" label-width="100px">
        <el-form-item label="版本号" v-if="isEditVersion">
          <el-input v-model="versionForm.version" disabled />
        </el-form-item>
        <el-form-item label="邮件主题" prop="subject">
          <el-input v-model="versionForm.subject" />
        </el-form-item>
        <el-form-item label="内容格式" prop="bodyFormat">
          <el-radio-group v-model="versionForm.bodyFormat">
            <el-radio-button label="html">HTML</el-radio-button>
            <el-radio-button label="plain">纯文本</el-radio-button>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="邮件内容" prop="body">
          <RichTextEditor v-if="versionForm.bodyFormat === 'html'" v-model="versionForm.body" :height="400" placeholder="编辑 HTML 邮件内容" />
          <el-input v-else v-model="versionForm.body" type="textarea" :rows="20" placeholder="纯文本内容" />
        </el-form-item>
        <el-form-item label="状态" v-if="isEditVersion">
          <el-switch v-model="versionForm.isEnabled" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="versionFormVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmitVersion">确定</el-button>
      </template>
    </el-dialog>

    <el-dialog title="发送测试邮件" v-model="testDialogVisible" width="560px">
      <el-form :model="testForm" :rules="testRules" ref="testFormRef" label-width="100px">
        <el-form-item label="收件人" prop="toAddress">
          <el-input v-model="testForm.toAddress" placeholder="test@example.com" />
        </el-form-item>
        <el-form-item label="抄送人" prop="ccAddresses">
          <el-input v-model="testForm.ccAddresses" type="textarea" :rows="3" placeholder="多个抄送人用逗号或换行分隔" />
        </el-form-item>
        <el-form-item label="版本号">
          <el-select v-model="testForm.version" placeholder="默认最新启用版本" clearable>
            <el-option v-for="v in currentVersions" :key="v.id" :label="v.version" :value="v.version" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="testDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleTest" :loading="sending">发送</el-button>
      </template>
    </el-dialog>

    <el-dialog title="批量发送邮件" v-model="sendDialogVisible" width="560px">
      <el-form :model="sendForm" :rules="sendRules" ref="sendFormRef" label-width="100px">
        <el-form-item label="收件人" prop="toAddresses">
          <el-input v-model="sendForm.toAddresses" type="textarea" rows="5" placeholder="多个邮箱用逗号或换行分隔" />
        </el-form-item>
        <el-form-item label="抄送人" prop="ccAddresses">
          <el-input v-model="sendForm.ccAddresses" type="textarea" :rows="3" placeholder="多个抄送人用逗号或换行分隔" />
        </el-form-item>
        <el-form-item label="版本号">
          <el-select v-model="sendForm.version" placeholder="默认最新启用版本" clearable>
            <el-option v-for="v in currentVersions" :key="v.id" :label="v.version" :value="v.version" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="sendDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSend" :loading="sending">发送</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { emailApi } from '@/api'
import { useDictStore } from '@/store/dict'
import RichTextEditor from '@/components/RichTextEditor.vue'

const dictStore = useDictStore()
const loading = ref(false)
const tableData = ref([])
const total = ref(0)
const search = reactive({ page: 1, pageSize: 10, keyword: '' })

const templateDialogVisible = ref(false)
const templateDialogTitle = ref('')
const isEditTemplate = ref(false)
const templateFormRef = ref()
const templateForm = reactive<any>({ templateCode: '', templateName: '', description: '', isEnabled: true })
const templateRules = {
  templateCode: [{ required: true, message: '必填', trigger: 'blur' }],
  templateName: [{ required: true, message: '必填', trigger: 'blur' }]
}

const versionDialogVisible = ref(false)
const versionFormVisible = ref(false)
const versionFormTitle = ref('')
const isEditVersion = ref(false)
const versionFormRef = ref()
const versionForm = reactive<any>({ templateId: '', version: '', subject: '', body: '', bodyFormat: 'html', isEnabled: true })
const versionRules = {
  subject: [{ required: true, message: '必填', trigger: 'blur' }],
  body: [{ required: true, message: '必填', trigger: 'blur' }]
}
const versions = ref<any[]>([])
const currentTemplateId = ref('')
const currentVersions = ref<any[]>([])

const testDialogVisible = ref(false)
const testFormRef = ref()
const testForm = reactive<any>({ templateId: '', toAddress: '', ccAddresses: '', version: '' })
const testRules = {
  toAddress: [{ required: true, message: '必填', trigger: 'blur' }]
}

const sendDialogVisible = ref(false)
const sendFormRef = ref()
const sendForm = reactive<any>({ templateId: '', toAddresses: '', ccAddresses: '', version: '' })
const sendRules = {
  toAddresses: [{ required: true, message: '必填', trigger: 'blur' }]
}

const sending = ref(false)

async function loadData() {
  loading.value = true
  const res = await emailApi.getTemplateList(search)
  tableData.value = res.items
  total.value = res.total
  loading.value = false
}

function openTemplateDialog(row?: any) {
  isEditTemplate.value = !!row
  templateDialogTitle.value = row ? '编辑模板' : '新增模板'
  Object.assign(templateForm, row || { templateCode: '', templateName: '', description: '', isEnabled: true })
  templateDialogVisible.value = true
}

async function handleSubmitTemplate() {
  await templateFormRef.value.validate()
  if (isEditTemplate.value) {
    await emailApi.updateTemplate(templateForm.id, templateForm)
    ElMessage.success('更新成功')
  } else {
    await emailApi.createTemplate(templateForm)
    ElMessage.success('创建成功')
  }
  templateDialogVisible.value = false
  loadData()
}

async function handleDeleteTemplate(row: any) {
  await ElMessageBox.confirm('确认删除？', '提示', { type: 'warning' })
  await emailApi.deleteTemplate(row.id)
  ElMessage.success('删除成功')
  loadData()
}

async function openVersionDialog(templateId: string) {
  currentTemplateId.value = templateId
  versions.value = await emailApi.getVersions(templateId)
  versionDialogVisible.value = true
}

function openVersionFormDialog(row?: any) {
  isEditVersion.value = !!row
  versionFormTitle.value = row ? '编辑版本' : '新增版本'
  Object.assign(versionForm, row || { templateId: currentTemplateId.value, version: '', subject: '', body: '', bodyFormat: 'html', isEnabled: true })
  versionFormVisible.value = true
}

async function handleSubmitVersion() {
  await versionFormRef.value.validate()
  if (isEditVersion.value) {
    await emailApi.updateVersion(versionForm.id, versionForm)
    ElMessage.success('更新成功')
  } else {
    const { version, ...rest } = versionForm
    await emailApi.createVersion(rest)
    ElMessage.success('创建成功')
  }
  versionFormVisible.value = false
  versions.value = await emailApi.getVersions(currentTemplateId.value)
  loadData()
}

async function handleDeleteVersion(row: any) {
  await ElMessageBox.confirm('确认删除？', '提示', { type: 'warning' })
  await emailApi.deleteVersion(row.id)
  ElMessage.success('删除成功')
  versions.value = await emailApi.getVersions(currentTemplateId.value)
}

async function openTestDialog(templateId: string) {
  currentTemplateId.value = templateId
  currentVersions.value = await emailApi.getVersions(templateId)
  testForm.templateId = templateId
  testForm.toAddress = ''
  testForm.ccAddresses = ''
  testForm.version = ''
  testDialogVisible.value = true
}

async function handleTest() {
  await testFormRef.value.validate()
  sending.value = true
  const ccList = testForm.ccAddresses.split(/[,\n]+/).map((a: string) => a.trim()).filter((a: string) => a)
  await emailApi.test({
    templateId: testForm.templateId,
    toAddress: testForm.toAddress,
    ccAddresses: ccList,
    version: testForm.version || undefined
  })
  sending.value = false
  ElMessage.success('测试邮件已发送')
  testDialogVisible.value = false
}

async function openSendDialog(templateId: string) {
  currentTemplateId.value = templateId
  currentVersions.value = await emailApi.getVersions(templateId)
  sendForm.templateId = templateId
  sendForm.toAddresses = ''
  sendForm.ccAddresses = ''
  sendForm.version = ''
  sendDialogVisible.value = true
}

async function handleSend() {
  await sendFormRef.value.validate()
  sending.value = true
  const addresses = sendForm.toAddresses.split(/[,\n]+/).map((a: string) => a.trim()).filter((a: string) => a)
  const ccList = sendForm.ccAddresses.split(/[,\n]+/).map((a: string) => a.trim()).filter((a: string) => a)
  await emailApi.send({
    templateId: sendForm.templateId,
    version: sendForm.version || undefined,
    toAddresses: addresses,
    ccAddresses: ccList
  })
  sending.value = false
  ElMessage.success('邮件已加入发送队列')
  sendDialogVisible.value = false
}

onMounted(() => { loadData() })
</script>

<style scoped>
.card-header { display: flex; justify-content: space-between; align-items: center; }
.pagination { margin-top: 15px; justify-content: flex-end; }
.version-table { margin-top: 15px; }
</style>
