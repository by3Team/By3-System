import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  // 加载当前 mode 对应的环境变量（.env.development 或 .env.production）
  const env = loadEnv(mode, process.cwd(), '')

  // 开发环境反向代理目标，可通过 .env.development 中的 VITE_PROXY_TARGET 覆盖
  const proxyTarget = env.VITE_PROXY_TARGET || 'http://localhost:5000'

  const isDev = mode === 'development'

  return {
    plugins: [vue()],
    resolve: {
      alias: {
        '@': resolve(import.meta.dirname, 'src')
      }
    },
    build: {
      target: 'es2020',
      chunkSizeWarningLimit: 500,
      rollupOptions: {
        output: {
          manualChunks(id: string) {
            if (id.includes('node_modules/element-plus')) return 'element-plus'
            if (id.includes('node_modules/vue') || id.includes('node_modules/pinia')) return 'vue-vendor'
          }
        }
      }
    },
    server: {
      port: 5175,
      // 仅在开发环境启用代理；生产构建由 Nginx 处理 /api 反向代理
      proxy: isDev
        ? {
            '/api': {
              target: proxyTarget,
              changeOrigin: true
            }
          }
        : undefined
    }
  }
})
