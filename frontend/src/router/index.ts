import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/store/auth'

// 预加载所有业务视图组件，避免 Vite 动态导入模板字符串导致的静态/动态导入冲突警告
const viewModules = import.meta.glob('@/views/**/*.vue')

function loadComponent(componentPath: string) {
  const fullPath = `/src/views/${componentPath}.vue`
  const module = viewModules[fullPath]
  if (!module) {
    console.warn(`视图组件不存在: ${fullPath}`)
    return () => import('@/views/NotFoundView.vue')
  }
  return () =>
    (module as () => Promise<any>)().catch((err) => {
      console.error('加载视图组件失败: %s', fullPath, err)
      return import('@/views/NotFoundView.vue')
    })
}

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'Login', component: loadComponent('LoginView'), meta: { public: true, title: '登录' } },
    { path: '/feature-doc/:code', name: 'FeatureDoc', component: loadComponent('FeatureDocView'), meta: { public: true, title: '功能文档' } },
    {
      path: '/',
      name: 'Layout',
      component: loadComponent('LayoutView'),
      redirect: '/dashboard',
      children: [
        { path: 'dashboard', name: 'Dashboard', component: () => import('@/views/DashboardView.vue'), meta: { title: '首页' } },
        { path: 'profile', name: 'Profile', component: loadComponent('profile/index'), meta: { title: '个人中心' } },
        { path: 'system/dict/data/:typeId', name: 'DictData', component: loadComponent('system/dict/data/index'), meta: { title: '字典数据', permission: 'dict:list' } },
      ]
    },
    { path: '/redirect/:path(.*)', name: 'Redirect', component: () => import('@/views/RedirectView.vue'), meta: { public: true, title: '跳转中' } },
    { path: '/403', name: 'Forbidden', component: loadComponent('ForbiddenView'), meta: { public: true, title: '无权访问' } },
    { path: '/404', name: 'NotFound', component: loadComponent('NotFoundView'), meta: { public: true, title: '页面不存在' } },
    { path: '/:pathMatch(.*)*', name: 'NotFoundCatchAll', component: loadComponent('NotFoundView'), meta: { public: true, title: '页面不存在' } }
  ]
})

let dynamicRoutesAdded = false

router.beforeEach((to, from, next) => {
  const auth = useAuthStore()

  if (to.meta.public) {
    if (to.path === '/login' && auth.token) {
      next('/')
      return
    }
    next()
    return
  }

  if (!auth.token) {
    next('/login')
    return
  }

  if (!dynamicRoutesAdded && auth.menus.length > 0) {
    addDynamicRoutes(auth.menus)
    dynamicRoutesAdded = true
    next({ ...to, replace: true })
    return
  }

  const requiredPermission = to.meta.permission as string | undefined
  if (requiredPermission && !auth.hasPermission(requiredPermission)) {
    next('/403')
    return
  }

  next()
})

export function addDynamicRoutes(menus: any[]) {
  if (dynamicRoutesAdded) return
  const layout = router.getRoutes().find(r => r.path === '/')
  if (!layout) return
  menus.forEach(menu => addRouteRecursive(menu, layout))
  dynamicRoutesAdded = true
}

export function resetDynamicRoutes() {
  dynamicRoutesAdded = false
}

// 路由导航异常（如动态组件加载失败）统一跳转到 404
router.onError((err) => {
  console.error('Router navigation error:', err)
  if (!['/404', '/login'].includes(router.currentRoute.value.path)) {
    router.replace('/404')
  }
})

function addRouteRecursive(menu: any, parent: any) {
  if (menu.menuType === 2 && menu.component) {
    const routePath = menu.route.replace(/^\//, '')
    const route = {
      path: routePath,
      name: `Route_${menu.id}`,
      component: loadComponent(menu.component),
      meta: { title: menu.menuName, permission: menu.permission, keepAlive: true }
    }
    router.addRoute(parent.name as string, route)
  }
  if (menu.children) {
    menu.children.forEach((child: any) => addRouteRecursive(child, parent))
  }
}

export default router
