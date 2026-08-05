import { defineStore } from 'pinia'
import { ref } from 'vue'

const STORAGE_KEY_TOKEN = 'token'
const STORAGE_KEY_USER_ID = 'userId'
const STORAGE_KEY_USER_NAME = 'userName'
const STORAGE_KEY_REAL_NAME = 'realName'
const STORAGE_KEY_PERMISSIONS = 'permissions'
const STORAGE_KEY_MENUS = 'menus'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(sessionStorage.getItem(STORAGE_KEY_TOKEN) || '')
  const userId = ref(sessionStorage.getItem(STORAGE_KEY_USER_ID) || '')
  const userName = ref(sessionStorage.getItem(STORAGE_KEY_USER_NAME) || '')
  const realName = ref(sessionStorage.getItem(STORAGE_KEY_REAL_NAME) || '')
  const permissions = ref<string[]>(JSON.parse(sessionStorage.getItem(STORAGE_KEY_PERMISSIONS) || '[]'))
  const menus = ref<any[]>(JSON.parse(sessionStorage.getItem(STORAGE_KEY_MENUS) || '[]'))

  function setAuth(data: any) {
    token.value = data.token
    userId.value = data.userId || ''
    userName.value = data.userName
    realName.value = data.realName || ''
    permissions.value = data.permissions || []
    menus.value = data.menus || []
    sessionStorage.setItem(STORAGE_KEY_TOKEN, data.token)
    sessionStorage.setItem(STORAGE_KEY_USER_ID, data.userId || '')
    sessionStorage.setItem(STORAGE_KEY_USER_NAME, data.userName)
    sessionStorage.setItem(STORAGE_KEY_REAL_NAME, data.realName || '')
    sessionStorage.setItem(STORAGE_KEY_PERMISSIONS, JSON.stringify(data.permissions || []))
    sessionStorage.setItem(STORAGE_KEY_MENUS, JSON.stringify(data.menus || []))
  }

  function clearAuth() {
    token.value = ''
    userId.value = ''
    userName.value = ''
    realName.value = ''
    permissions.value = []
    menus.value = []
    sessionStorage.removeItem(STORAGE_KEY_TOKEN)
    sessionStorage.removeItem(STORAGE_KEY_USER_ID)
    sessionStorage.removeItem(STORAGE_KEY_USER_NAME)
    sessionStorage.removeItem(STORAGE_KEY_REAL_NAME)
    sessionStorage.removeItem(STORAGE_KEY_PERMISSIONS)
    sessionStorage.removeItem(STORAGE_KEY_MENUS)
  }

  function hasPermission(perm: string) {
    return permissions.value.includes(perm)
  }

  return { token, userId, userName, realName, permissions, menus, setAuth, clearAuth, hasPermission }
})
