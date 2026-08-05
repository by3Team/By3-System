import axios, { type AxiosResponse, type InternalAxiosRequestConfig, type AxiosError } from 'axios'
import { ElMessage } from 'element-plus'
import { v4 as uuidv4 } from 'uuid'
import { useAuthStore } from '@/store/auth'
import router from '@/router'

export interface ApiResponse<T = any> {
  code: number
  message: string
  data: T
}

interface ApiInstance {
  get<T = any>(url: string, config?: any): Promise<T>
  post<T = any>(url: string, data?: any, config?: any): Promise<T>
  put<T = any>(url: string, data?: any, config?: any): Promise<T>
  delete<T = any>(url: string, config?: any): Promise<T>
}

const instance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 30000
})

let isRedirectingToLogin = false

function handleUnauthorized() {
  if (isRedirectingToLogin) return
  isRedirectingToLogin = true

  const auth = useAuthStore()
  auth.clearAuth()
  ElMessage.error('登录已过期，请重新登录')

  if (router.currentRoute.value.path !== '/login') {
    router.replace('/login')
  }

  // 防止某些极端情况下标志未重置，导致后续 401 无法跳转
  setTimeout(() => { isRedirectingToLogin = false }, 3000)
}

function generateIdempotencyKey(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    try {
      return crypto.randomUUID()
    } catch {
      // fallback
    }
  }
  return uuidv4()
}

instance.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const auth = useAuthStore()
  if (auth.token) {
    config.headers.Authorization = `Bearer ${auth.token}`
  }
  if (config.method !== 'get' && config.method !== 'head') {
    config.headers['Idempotency-Key'] = generateIdempotencyKey()
  }
  return config
})

instance.interceptors.response.use(
  (res: AxiosResponse<ApiResponse>) => {
    // blob / arraybuffer 等二进制响应直接返回，不做统一 JSON 解析
    if (res.config.responseType === 'blob' || res.config.responseType === 'arraybuffer') {
      return res.data
    }

    const data = res.data
    if (data.code === 401) {
      handleUnauthorized()
      return Promise.reject(data)
    }
    if (data.code !== 200) {
      ElMessage.error(data.message || '请求失败')
      return Promise.reject(data)
    }
    return data.data
  },
  (err: AxiosError<ApiResponse>) => {
    const status = err.response?.status
    const message = err.response?.data?.message

    if (status === 401) {
      handleUnauthorized()
    } else if (status === 403) {
      ElMessage.error(message || '暂无权限执行此操作')
    } else if (status === 429) {
      ElMessage.error('请求过于频繁，请稍后再试')
    } else if (status && status >= 500) {
      ElMessage.error(message || '服务器内部错误')
    } else if (err.message === 'Network Error') {
      ElMessage.error('网络连接失败，请检查网络')
    } else {
      ElMessage.error(message || '网络错误')
    }

    return Promise.reject(err)
  }
)

const api = instance as ApiInstance
export default api
