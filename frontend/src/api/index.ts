import api from './request'

export const authApi = {
  login: (data: any) => api.post('/v1/auth/login', data),
  refresh: (data: any) => api.post('/v1/auth/refresh', data),
  logout: () => api.post('/v1/auth/logout'),
  getInfo: () => api.get('/v1/auth/info')
}

export const userApi = {
  getList: (params: any) => api.get('/v1/users', { params }),
  getById: (id: string) => api.get(`/v1/users/${id}`),
  create: (data: any) => api.post('/v1/users', data),
  update: (id: string, data: any) => api.put(`/v1/users/${id}`, data),
  delete: (id: string) => api.delete(`/v1/users/${id}`),
  getRoles: (id: string) => api.get(`/v1/users/${id}/roles`),
  resetPassword: (id: string, data: any) => api.post(`/v1/users/${id}/reset-password`, data)
}

export const roleApi = {
  getList: (params: any) => api.get('/v1/roles', { params }),
  getAll: () => api.get('/v1/roles/all'),
  getById: (id: string) => api.get(`/v1/roles/${id}`),
  create: (data: any) => api.post('/v1/roles', data),
  update: (id: string, data: any) => api.put(`/v1/roles/${id}`, data),
  delete: (id: string) => api.delete(`/v1/roles/${id}`),
  getMenus: (id: string) => api.get(`/v1/roles/${id}/menus`)
}

export const menuApi = {
  getAll: () => api.get('/v1/menus'),
  getById: (id: string) => api.get(`/v1/menus/${id}`),
  create: (data: any) => api.post('/v1/menus', data),
  update: (id: string, data: any) => api.put(`/v1/menus/${id}`, data),
  delete: (id: string) => api.delete(`/v1/menus/${id}`)
}

export const auditLogApi = {
  getList: (params: any) => api.get('/v1/auditlogs', { params }),
  getById: (id: string) => api.get(`/v1/auditlogs/${id}`)
}

export const loginLogApi = {
  getList: (params: any) => api.get('/v1/loginlogs', { params })
}

export const departmentApi = {
  getTree: () => api.get('/v1/departments'),
  getById: (id: string) => api.get(`/v1/departments/${id}`),
  create: (data: any) => api.post('/v1/departments', data),
  update: (id: string, data: any) => api.put(`/v1/departments/${id}`, data),
  delete: (id: string) => api.delete(`/v1/departments/${id}`)
}

export const positionApi = {
  getList: (params: any) => api.get('/v1/positions', { params }),
  getById: (id: string) => api.get(`/v1/positions/${id}`),
  create: (data: any) => api.post('/v1/positions', data),
  update: (id: string, data: any) => api.put(`/v1/positions/${id}`, data),
  delete: (id: string) => api.delete(`/v1/positions/${id}`)
}

export const dictTypeApi = {
  getList: (params: any) => api.get('/v1/dicttypes', { params }),
  getById: (id: string) => api.get(`/v1/dicttypes/${id}`),
  create: (data: any) => api.post('/v1/dicttypes', data),
  update: (id: string, data: any) => api.put(`/v1/dicttypes/${id}`, data),
  delete: (id: string) => api.delete(`/v1/dicttypes/${id}`)
}

export const dictDataApi = {
  getList: (params: any) => api.get('/v1/dictdata', { params }),
  getByTypeId: (dictTypeId: string) => api.get(`/v1/dictdata/by-type/${dictTypeId}`),
  getByTypeCode: (dictTypeCode: string) => api.get(`/v1/dictdata/by-type-code/${dictTypeCode}`),
  getById: (id: string) => api.get(`/v1/dictdata/${id}`),
  create: (data: any) => api.post('/v1/dictdata', data),
  update: (id: string, data: any) => api.put(`/v1/dictdata/${id}`, data),
  delete: (id: string) => api.delete(`/v1/dictdata/${id}`)
}

export const singleFileApi = {
  upload: (data: FormData) => api.post('/v1/singlefiles/upload', data, { headers: { 'Content-Type': 'multipart/form-data' } }),
  download: (id: string) => `${import.meta.env.VITE_API_BASE_URL || '/api'}/v1/singlefiles/${id}/download`
}

export const multiFileApi = {
  upload: (data: FormData) => api.post('/v1/multifiles/upload', data, { headers: { 'Content-Type': 'multipart/form-data' } }),
  getList: (params: any) => api.get('/v1/multifiles', { params }),
  delete: (id: string) => api.delete(`/v1/multifiles/${id}`),
  export: (params: any) => api.get('/v1/multifiles/export', { params, responseType: 'blob' }),
  download: (id: string) => `${import.meta.env.VITE_API_BASE_URL || '/api'}/v1/multifiles/${id}/download`
}

export const emailApi = {
  getTemplateList: (params: any) => api.get('/v1/emailtemplates', { params }),
  getTemplateById: (id: string) => api.get(`/v1/emailtemplates/${id}`),
  createTemplate: (data: any) => api.post('/v1/emailtemplates', data),
  updateTemplate: (id: string, data: any) => api.put(`/v1/emailtemplates/${id}`, data),
  deleteTemplate: (id: string) => api.delete(`/v1/emailtemplates/${id}`),
  getVersions: (templateId: string) => api.get(`/v1/emailtemplates/${templateId}/versions`),
  createVersion: (data: any) => api.post('/v1/emailtemplates/versions', data),
  updateVersion: (id: string, data: any) => api.put(`/v1/emailtemplates/versions/${id}`, data),
  deleteVersion: (id: string) => api.delete(`/v1/emailtemplates/versions/${id}`),
  send: (data: any) => api.post('/v1/emailtemplates/send', data),
  test: (data: any) => api.post('/v1/emailtemplates/test', data),
  getLogs: (params: any) => api.get('/v1/emailtemplates/logs', { params })
}

export const emailSettingApi = {
  get: () => api.get('/v1/emailsettings'),
  update: (data: any) => api.put('/v1/emailsettings', data),
  test: (data: any) => api.post('/v1/emailsettings/test', data)
}

export const systemInfoApi = {
  getPackages: () => api.get('/v1/systeminfo/packages')
}

export const jobApi = {
  getList: (params: any) => api.get('/v1/jobs', { params }),
  getById: (id: string) => api.get(`/v1/jobs/${id}`),
  create: (data: any) => api.post('/v1/jobs', data),
  update: (id: string, data: any) => api.put(`/v1/jobs/${id}`, data),
  delete: (id: string) => api.delete(`/v1/jobs/${id}`),
  trigger: (id: string) => api.post(`/v1/jobs/${id}/trigger`),
  toggle: (id: string) => api.post(`/v1/jobs/${id}/toggle`),
  getLogs: (id: string, params: any) => api.get(`/v1/jobs/${id}/logs`, { params })
}

export const externalApiTokenApi = {
  getList: (params: any) => api.get('/v1/externalapitokens', { params }),
  exportCsv: (params: any) => api.get('/v1/externalapitokens/export', { params, responseType: 'blob' }),
  getById: (id: string) => api.get(`/v1/externalapitokens/${id}`),
  getLogs: (id: string, params: any) => api.get(`/v1/externalapitokens/${id}/logs`, { params }),
  getHistory: (id: string, params: any) => api.get(`/v1/externalapitokens/${id}/history`, { params }),
  invalidateHistory: (id: string, historyId: string) => api.post(`/v1/externalapitokens/${id}/history/${historyId}/invalidate`),
  create: (data: any) => api.post('/v1/externalapitokens', data),
  update: (id: string, data: any) => api.put(`/v1/externalapitokens/${id}`, data),
  delete: (id: string) => api.delete(`/v1/externalapitokens/${id}`),
  regenerate: (id: string, data: any) => api.post(`/v1/externalapitokens/${id}/regenerate`, data)
}

export const externalApiApi = {
  getList: (params: any) => api.get('/v1/externalapis', { params }),
  getById: (id: string) => api.get(`/v1/externalapis/${id}`),
  getStats: (id: string) => api.get(`/v1/externalapis/${id}/stats`),
  getAuthorizedTokenCount: (id: string) => api.get(`/v1/externalapis/${id}/authorized-token-count`),
  create: (data: any) => api.post('/v1/externalapis', data),
  update: (id: string, data: any) => api.put(`/v1/externalapis/${id}`, data),
  delete: (id: string) => api.delete(`/v1/externalapis/${id}`),
  toggle: (id: string) => api.post(`/v1/externalapis/${id}/toggle`)
}
