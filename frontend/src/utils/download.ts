import axios from 'axios'
import { useAuthStore } from '@/store/auth'

export async function downloadFile(url: string, params?: any, filename?: string) {
  const auth = useAuthStore()
  const res = await axios.get(url, {
    baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
    params,
    headers: auth.token ? { Authorization: `Bearer ${auth.token}` } : {},
    responseType: 'blob'
  })

  const blob = new Blob([res.data])
  const downloadUrl = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = downloadUrl
  link.download = filename || getFilenameFromHeader(res.headers['content-disposition']) || 'download'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(downloadUrl)
}

function getFilenameFromHeader(header?: string): string | undefined {
  if (!header) return undefined
  const match = header.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
  if (match) {
    return decodeURIComponent(match[1].replace(/['"]/g, ''))
  }
  return undefined
}
