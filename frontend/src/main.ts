import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus, { ElMessage } from 'element-plus'
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'
import 'element-plus/dist/index.css'
import '@/styles/element-override.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import App from './App.vue'
import router, { addDynamicRoutes } from './router'
import { useAuthStore } from './store/auth'
import { useDictStore } from './store/dict'
import { permissionDirective } from './directives/permission'
import { authApi } from './api'

const app = createApp(App)
app.use(createPinia())

// 刷新后恢复动态路由
const auth = useAuthStore()
if (auth.token && auth.menus.length > 0) {
  addDynamicRoutes(auth.menus)
}

// 已登录时刷新用户信息/菜单（保证后端菜单/权限变更后前端能及时同步）
if (auth.token) {
  authApi.getInfo()
    .then((res: any) => {
      auth.setAuth({
        token: auth.token,
        userId: res.userId,
        userName: res.userName,
        realName: res.realName,
        permissions: res.permissions,
        menus: res.menus
      })
      addDynamicRoutes(res.menus)
    })
    .catch(() => {
      // 获取失败不强制退出，避免网络波动导致必须重新登录
    })
}

// 已登录时加载字典缓存
if (auth.token) {
  const dictStore = useDictStore()
  dictStore.loadAll().catch(() => {})
}

app.use(router)
app.use(ElementPlus, { locale: zhCn })
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}
app.directive('permission', permissionDirective)

app.config.errorHandler = (err, vm, info) => {
  // Element Plus 组件的预期异常（如 ElMessageBox 取消）不跳转 404
  if (err instanceof Error && err.message === 'cancel') return
  if (String(err).includes('cancel')) return

  console.error('Vue error:', err, info)
  const currentPath = router.currentRoute.value.path
  if (currentPath !== '/404' && currentPath !== '/login') {
    router.replace('/404')
  } else {
    ElMessage.error('页面发生错误，请刷新重试')
  }
}

app.mount('#app')
