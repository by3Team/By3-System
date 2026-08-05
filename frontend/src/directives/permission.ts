import { useAuthStore } from '@/store/auth'

function checkPermission(el: HTMLElement, binding: any) {
  const auth = useAuthStore()
  const perm = binding.value
  if (!perm) return
  if (!auth.hasPermission(perm)) {
    el.remove()
  }
}

export const permissionDirective = {
  mounted(el: HTMLElement, binding: any) {
    checkPermission(el, binding)
  },
  updated(el: HTMLElement, binding: any) {
    checkPermission(el, binding)
  }
}
