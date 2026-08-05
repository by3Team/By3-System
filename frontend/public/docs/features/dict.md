# 字典管理

## 功能说明

集中维护系统运行期所需的各类字典数据（如下拉选项、状态值等），支持前端缓存和刷新。避免硬编码，便于统一管理。

## 核心概念

| 概念 | 说明 | 示例 |
|------|------|------|
| 字典类型 | 按业务分组的字典类别 | `sys_gender`、`sys_status`、`sys_file_category` |
| 字典数据 | 字典类型下的具体选项 | 男/女、启用/禁用、图片/文档 |

## 内置字典

| 字典类型编码 | 说明 | 数据项 |
|-------------|------|--------|
| `sys_gender` | 性别 | 男、女 |
| `sys_status` | 启用状态 | 启用、禁用 |
| `sys_menu_type` | 菜单类型 | 目录、菜单、按钮 |
| `sys_yes_no` | 是否 | 是、否 |
| `sys_file_category` | 文件类型 | 通用、文档、图片、视频、音频、压缩包 |

## 接口说明

### 字典类型

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/v1/dicttypes` | GET | `dict:list` | 字典类型列表 |
| `/api/v1/dicttypes/{id}` | GET | `dict:list` | 字典类型详情 |
| `/api/v1/dicttypes` | POST | `dict:create` | 创建字典类型 |
| `/api/v1/dicttypes/{id}` | PUT | `dict:update` | 更新字典类型 |
| `/api/v1/dicttypes/{id}` | DELETE | `dict:delete` | 删除字典类型 |

### 字典数据

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/v1/dictdata` | GET | `dict:list` | 字典数据列表 |
| `/api/v1/dictdata/by-type/{typeId}` | GET | `dict:list` | 按类型 ID 查询数据 |
| `/api/v1/dictdata/by-type-code/{code}` | GET | `dict:list` | 按类型编码查询数据 |
| `/api/v1/dictdata` | POST | `dict:create` | 创建字典数据 |
| `/api/v1/dictdata/{id}` | PUT | `dict:update` | 更新字典数据 |
| `/api/v1/dictdata/{id}` | DELETE | `dict:delete` | 删除字典数据 |

## 前端使用

### 自动缓存

用户登录后，系统自动加载所有启用的字典数据到 `localStorage`，页面可直接读取，无需重复请求。

### 缓存刷新

字典数据增删改后，前端缓存自动刷新，无需手动清理。

### 代码中使用

```typescript
import { useDictStore } from '@/store/dict'

const dict = useDictStore()

// 获取字典数据列表
const genderOptions = dict.getDictData('sys_gender')

// 获取字典标签
const label = dict.getDictLabel('sys_gender', '1') // 返回 '男'
```

## 使用方式

1. 在【系统管理 → 字典管理】中创建字典类型（如 `sys_status`、`启用状态`）
2. 在字典类型下添加数据项（如 `1=启用`、`0=禁用`）
3. 前端通过字典 Store 读取数据，渲染下拉框、标签等
4. 文件上传的扩展名校验依赖 `sys_file_category` 字典配置
