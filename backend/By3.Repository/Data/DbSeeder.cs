// Copyright 2026 By3 Team
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.EntityFrameworkCore;
using By3.Repository.Entities;

namespace By3.Repository.Data;

public static class DbSeeder
{
    public static async Task EnsureSeedDataAsync(this AppDbContext db, string? defaultPassword = null)
    {
        // 默认管理员
        if (!await db.Users.AnyAsync(u => u.UserName == "admin"))
        {
            var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var adminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            db.Users.Add(new SysUser
            {
                Id = adminId,
                UserName = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword ?? "Demo123!"),
                RealName = "超级管理员",
                Email = "admin@example.com",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            });

            db.Roles.Add(new SysRole
            {
                Id = adminRoleId,
                RoleName = "超级管理员",
                Description = "拥有所有权限",
                IsEnabled = true
            });

            db.UserRoles.Add(new SysUserRole
            {
                Id = Guid.NewGuid(),
                UserId = adminId,
                RoleId = adminRoleId,
                CreatedAt = DateTime.UtcNow
            });

            // 普通用户角色
            var userRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            db.Roles.Add(new SysRole
            {
                Id = userRoleId,
                RoleName = "普通用户",
                Description = "仅可查看和部分操作",
                IsEnabled = true
            });

            // 默认菜单
            var sysMgmtId = Guid.NewGuid();
            var fileMgmtId = Guid.NewGuid();
            var emailMgmtId = Guid.NewGuid();
            var logMgmtId = Guid.NewGuid();

            var menus = new List<SysMenu>
            {
                new() { Id = sysMgmtId, MenuName = "系统管理", MenuType = 1, Route = "/system", Icon = "Setting", SortOrder = 1 },
                new() { Id = Guid.NewGuid(), MenuName = "用户管理", MenuType = 2, Route = "/system/user", Component = "system/user/index", Permission = "user:list", SortOrder = 1, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "用户新增", MenuType = 3, Permission = "user:create", SortOrder = 1, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "用户编辑", MenuType = 3, Permission = "user:update", SortOrder = 2, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "用户删除", MenuType = 3, Permission = "user:delete", SortOrder = 3, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "角色管理", MenuType = 2, Route = "/system/role", Component = "system/role/index", Permission = "role:list", SortOrder = 2, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "角色新增", MenuType = 3, Permission = "role:create", SortOrder = 1, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "角色编辑", MenuType = 3, Permission = "role:update", SortOrder = 2, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "角色删除", MenuType = 3, Permission = "role:delete", SortOrder = 3, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "菜单管理", MenuType = 2, Route = "/system/menu", Component = "system/menu/index", Permission = "menu:list", SortOrder = 3, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "菜单新增", MenuType = 3, Permission = "menu:create", SortOrder = 1, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "菜单编辑", MenuType = 3, Permission = "menu:update", SortOrder = 2, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "菜单删除", MenuType = 3, Permission = "menu:delete", SortOrder = 3, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "组织机构", MenuType = 2, Route = "/system/department", Component = "system/department/index", Permission = "dept:list", SortOrder = 4, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "部门新增", MenuType = 3, Permission = "dept:create", SortOrder = 1, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "部门编辑", MenuType = 3, Permission = "dept:update", SortOrder = 2, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "部门删除", MenuType = 3, Permission = "dept:delete", SortOrder = 3, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "岗位管理", MenuType = 2, Route = "/system/position", Component = "system/position/index", Permission = "position:list", SortOrder = 5, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "岗位新增", MenuType = 3, Permission = "position:create", SortOrder = 1, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "岗位编辑", MenuType = 3, Permission = "position:update", SortOrder = 2, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "岗位删除", MenuType = 3, Permission = "position:delete", SortOrder = 3, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "字典管理", MenuType = 2, Route = "/system/dict", Component = "system/dict/type/index", Permission = "dict:list", SortOrder = 6, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "字典新增", MenuType = 3, Permission = "dict:create", SortOrder = 1, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "字典编辑", MenuType = 3, Permission = "dict:update", SortOrder = 2, ParentId = sysMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "字典删除", MenuType = 3, Permission = "dict:delete", SortOrder = 3, ParentId = sysMgmtId },
                new() { Id = fileMgmtId, MenuName = "文件管理", MenuType = 1, Route = "/file", Icon = "Folder", SortOrder = 3 },
                new() { Id = Guid.NewGuid(), MenuName = "文件列表", MenuType = 2, Route = "/file/list", Component = "file/list/index", Permission = "file:list", SortOrder = 1, ParentId = fileMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "文件上传", MenuType = 3, Permission = "file:create", SortOrder = 1, ParentId = fileMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "文件删除", MenuType = 3, Permission = "file:delete", SortOrder = 2, ParentId = fileMgmtId },
                new() { Id = emailMgmtId, MenuName = "邮件管理", MenuType = 1, Route = "/email", Icon = "Message", SortOrder = 4 },
                new() { Id = Guid.NewGuid(), MenuName = "邮件模板", MenuType = 2, Route = "/email/template", Component = "email/template/index", Permission = "email:list", SortOrder = 1, ParentId = emailMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "邮件日志", MenuType = 2, Route = "/email/log", Component = "email/log/index", Permission = "email:list", SortOrder = 2, ParentId = emailMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "邮件新增", MenuType = 3, Permission = "email:create", SortOrder = 1, ParentId = emailMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "邮件编辑", MenuType = 3, Permission = "email:update", SortOrder = 2, ParentId = emailMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "邮件删除", MenuType = 3, Permission = "email:delete", SortOrder = 3, ParentId = emailMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "邮件发送", MenuType = 3, Permission = "email:send", SortOrder = 4, ParentId = emailMgmtId },
                new() { Id = logMgmtId, MenuName = "日志管理", MenuType = 1, Route = "/log", Icon = "Document", SortOrder = 5 },
                new() { Id = Guid.NewGuid(), MenuName = "操作日志", MenuType = 2, Route = "/log/audit", Component = "log/audit/index", Permission = "audit:list", SortOrder = 1, ParentId = logMgmtId },
                new() { Id = Guid.NewGuid(), MenuName = "登录日志", MenuType = 2, Route = "/log/login", Component = "log/login/index", Permission = "loginlog:list", SortOrder = 2, ParentId = logMgmtId },
            };

            db.Menus.AddRange(menus);
            await db.SaveChangesAsync();

            // 给超级管理员角色绑定所有菜单
            var allMenus = await db.Menus.ToListAsync();
            foreach (var m in allMenus)
            {
                db.RoleMenus.Add(new SysRoleMenu
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRoleId,
                    MenuId = m.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 给普通用户角色绑定只读菜单（仅 list 权限）
            var readonlyPermissions = new[] { "user:list", "role:list", "menu:list", "dept:list", "position:list", "dict:list", "file:list", "email:list", "audit:list", "loginlog:list" };
            var readonlyMenus = allMenus.Where(m => m.Permission == null || readonlyPermissions.Contains(m.Permission)).ToList();
            foreach (var m in readonlyMenus)
            {
                db.RoleMenus.Add(new SysRoleMenu
                {
                    Id = Guid.NewGuid(),
                    RoleId = userRoleId,
                    MenuId = m.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
        }

        // 系统默认字典（增量添加）
        await EnsureDictTypeAsync(db, Guid.Parse("44444444-4444-4444-4444-444444444444"), "性别", "sys_gender", new[]
        {
            ("男", "male", "", 1, false),
            ("女", "female", "", 2, false),
            ("未知", "unknown", "", 3, true)
        });
        await EnsureDictTypeAsync(db, Guid.Parse("55555555-5555-5555-5555-555555555555"), "启用状态", "sys_status", new[]
        {
            ("启用", "enabled", "", 1, true),
            ("禁用", "disabled", "", 2, false)
        });
        await EnsureDictTypeAsync(db, Guid.Parse("66666666-6666-6666-6666-666666666666"), "菜单类型", "sys_menu_type", new[]
        {
            ("目录", "1", "", 1, false),
            ("菜单", "2", "", 2, false),
            ("按钮", "3", "", 3, false)
        });
        await EnsureDictTypeAsync(db, Guid.Parse("77777777-7777-7777-7777-777777777777"), "是否", "sys_yes_no", new[]
        {
            ("是", "yes", "", 1, false),
            ("否", "no", "", 2, false)
        });
        await EnsureDictTypeAsync(db, Guid.Parse("88888888-8888-8888-8888-888888888888"), "文件类型", "sys_file_category", new[]
        {
            ("通用", "general", "*", 1, true),
            ("文档", "document", ".doc,.docx,.pdf,.txt,.xls,.xlsx,.ppt,.pptx", 2, false),
            ("图片", "image", ".jpg,.jpeg,.png,.gif,.bmp,.webp,.svg", 3, false),
            ("视频", "video", ".mp4,.avi,.mov,.wmv,.flv,.mkv", 4, false),
            ("音频", "audio", ".mp3,.wav,.wma,.aac,.flac,.ogg", 5, false),
            ("压缩包", "archive", ".zip,.rar,.7z,.tar,.gz", 6, false)
        });

        // 系统设置菜单与默认邮件配置（增量添加）
        await EnsureSystemSettingMenuAsync(db);
        await EnsureEmailSettingAsync(db);

        // 任务管理菜单与默认任务（增量添加）
        await EnsureTaskMenuAsync(db);
        await EnsureDefaultJobAsync(db);

        // 对外 API 菜单（增量添加）
        await EnsureExternalApiMenuAsync(db);

        // 对外 API 接口注册数据（增量添加）
        await EnsureExternalApiRoutesAsync(db);

        // 部门与岗位测试数据（增量添加）
        await EnsureDepartmentAndPositionAsync(db);

        // Token 通知邮件模板（增量添加）
        await EnsureTokenNotifyTemplateAsync(db);
    }

    private static async Task EnsureDepartmentAndPositionAsync(AppDbContext db)
    {
        // 部门测试数据
        if (!await db.Departments.AnyAsync())
        {
            var techId = Guid.NewGuid();
            var opsId = Guid.NewGuid();
            var hrId = Guid.NewGuid();
            var salesId = Guid.NewGuid();

            var departments = new List<SysDepartment>
            {
                new() { Id = techId, DeptName = "技术部", DeptCode = "TECH", SortOrder = 1, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "前端组", DeptCode = "TECH-FE", ParentId = techId, SortOrder = 1, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "后端组", DeptCode = "TECH-BE", ParentId = techId, SortOrder = 2, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "测试组", DeptCode = "TECH-QA", ParentId = techId, SortOrder = 3, CreatedAt = DateTime.UtcNow },
                new() { Id = opsId, DeptName = "运维部", DeptCode = "OPS", SortOrder = 2, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "系统运维", DeptCode = "OPS-SYS", ParentId = opsId, SortOrder = 1, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "网络运维", DeptCode = "OPS-NET", ParentId = opsId, SortOrder = 2, CreatedAt = DateTime.UtcNow },
                new() { Id = hrId, DeptName = "人力资源部", DeptCode = "HR", SortOrder = 3, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "招聘组", DeptCode = "HR-REC", ParentId = hrId, SortOrder = 1, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "培训组", DeptCode = "HR-TRN", ParentId = hrId, SortOrder = 2, CreatedAt = DateTime.UtcNow },
                new() { Id = salesId, DeptName = "销售部", DeptCode = "SALES", SortOrder = 4, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "华东区", DeptCode = "SALES-EAST", ParentId = salesId, SortOrder = 1, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), DeptName = "华南区", DeptCode = "SALES-SOUTH", ParentId = salesId, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            };
            db.Departments.AddRange(departments);
            await db.SaveChangesAsync();
        }

        // 岗位测试数据
        if (!await db.Positions.AnyAsync())
        {
            var positions = new List<SysPosition>
            {
                new() { Id = Guid.NewGuid(), PositionName = "总经理", PositionCode = "GM", SortOrder = 1, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "部门经理", PositionCode = "DEPT_MGR", SortOrder = 2, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "项目经理", PositionCode = "PM", SortOrder = 3, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "高级工程师", PositionCode = "SR_ENG", SortOrder = 4, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "中级工程师", PositionCode = "MID_ENG", SortOrder = 5, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "初级工程师", PositionCode = "JR_ENG", SortOrder = 6, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "测试工程师", PositionCode = "QA_ENG", SortOrder = 7, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "运维工程师", PositionCode = "OPS_ENG", SortOrder = 8, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "产品经理", PositionCode = "PD", SortOrder = 9, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "UI 设计师", PositionCode = "UI", SortOrder = 10, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "销售代表", PositionCode = "SALES_REP", SortOrder = 11, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), PositionName = "人事专员", PositionCode = "HR_SPEC", SortOrder = 12, CreatedAt = DateTime.UtcNow },
            };
            db.Positions.AddRange(positions);
            await db.SaveChangesAsync();
        }

        // 给 admin 用户分配部门和岗位
        var admin = await db.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
        if (admin != null && admin.DepartmentId == null)
        {
            var techDept = await db.Departments.FirstOrDefaultAsync(d => d.DeptCode == "TECH");
            var gmPosition = await db.Positions.FirstOrDefaultAsync(p => p.PositionCode == "GM");
            if (techDept != null) admin.DepartmentId = techDept.Id;
            if (gmPosition != null) admin.PositionId = gmPosition.Id;
            admin.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 确保 Token 通知邮件模板存在（固定编码 external_api_token_notify）。
    /// </summary>
    public static readonly Guid TokenNotifyTemplateId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static async Task EnsureTokenNotifyTemplateAsync(AppDbContext db)
    {
        var templateCode = "external_api_token_notify";
        var template = await db.EmailTemplates.FirstOrDefaultAsync(t => t.TemplateCode == templateCode);
        if (template == null)
        {
            template = new SysEmailTemplate
            {
                Id = TokenNotifyTemplateId,
                TemplateCode = templateCode,
                TemplateName = "对外 API Token 通知",
                Description = "Token 重新生成、启用、禁用等变更通知邮件",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            };
            db.EmailTemplates.Add(template);
            await db.SaveChangesAsync();
        }

        var version = await db.EmailTemplateVersions.FirstOrDefaultAsync(v => v.TemplateId == template.Id && v.Version == "v1");
        if (version == null)
        {
            db.EmailTemplateVersions.Add(new SysEmailTemplateVersion
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                Version = "v1",
                Subject = "【By3】对外 API Token 已重新生成",
                Body = @"<div style=""font-family: 'Microsoft YaHei', Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
  <h2 style=""color: #333; border-bottom: 2px solid #409EFF; padding-bottom: 10px;"">对外 API Token 变更通知</h2>
  <p>您好，您的对外 API Token 已发生变更，详情如下：</p>
  <table style=""width: 100%; border-collapse: collapse; margin: 15px 0;"">
    <tr style=""background: #f5f7fa;"">
      <td style=""padding: 10px; border: 1px solid #ebeef5; font-weight: bold; width: 140px;"">应用名称</td>
      <td style=""padding: 10px; border: 1px solid #ebeef5;"">{AppName}</td>
    </tr>
    <tr>
      <td style=""padding: 10px; border: 1px solid #ebeef5; font-weight: bold;"">新 ApiKey</td>
      <td style=""padding: 10px; border: 1px solid #ebeef5; font-family: monospace; color: #409EFF;"">{ApiKey}</td>
    </tr>
    <tr style=""background: #f5f7fa;"">
      <td style=""padding: 10px; border: 1px solid #ebeef5; font-weight: bold;"">新 ApiSecret</td>
      <td style=""padding: 10px; border: 1px solid #ebeef5; font-family: monospace; color: #E6A23C;"">{ApiSecret}</td>
    </tr>
    <tr>
      <td style=""padding: 10px; border: 1px solid #ebeef5; font-weight: bold;"">有效期类型</td>
      <td style=""padding: 10px; border: 1px solid #ebeef5;"">{ExpireType} 天</td>
    </tr>
    <tr style=""background: #f5f7fa;"">
      <td style=""padding: 10px; border: 1px solid #ebeef5; font-weight: bold;"">旧 Key 状态</td>
      <td style=""padding: 10px; border: 1px solid #ebeef5;"">{GraceRemark}</td>
    </tr>
    <tr>
      <td style=""padding: 10px; border: 1px solid #ebeef5; font-weight: bold;"">操作时间</td>
      <td style=""padding: 10px; border: 1px solid #ebeef5;"">{OperateTime}</td>
    </tr>
  </table>
  <p style=""color: #F56C6C; font-size: 14px;"">请妥善保管新的 ApiKey 和 ApiSecret，切勿泄露给他人。</p>
  <p style=""color: #999; font-size: 12px; margin-top: 30px;"">此邮件由系统自动发送，请勿直接回复。</p>
</div>",
                BodyFormat = "html",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureDictTypeAsync(AppDbContext db, Guid id, string dictName, string dictType, (string Label, string Value, string Remark, int Sort, bool Default)[] items)
    {
        var type = await db.DictTypes.FirstOrDefaultAsync(t => t.DictType == dictType);
        if (type == null)
        {
            type = new SysDictType { Id = id, DictName = dictName, DictType = dictType };
            db.DictTypes.Add(type);
            await db.SaveChangesAsync();
        }

        foreach (var item in items)
        {
            if (!await db.DictData.AnyAsync(d => d.DictTypeId == type.Id && d.DictValue == item.Value))
            {
                db.DictData.Add(new SysDictData
                {
                    DictTypeId = type.Id,
                    DictLabel = item.Label,
                    DictValue = item.Value,
                    Remark = item.Remark,
                    SortOrder = item.Sort,
                    IsDefault = item.Default
                });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureSystemSettingMenuAsync(AppDbContext db)
    {
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "超级管理员");

        // 系统设置应作为一级目录菜单，若之前被放在系统管理下则迁移出来
        var settingMenu = await db.Menus.FirstOrDefaultAsync(m => m.Route == "/system/setting");
        if (settingMenu == null)
        {
            settingMenu = new SysMenu
            {
                Id = Guid.NewGuid(),
                MenuName = "系统设置",
                MenuType = 1,
                Route = "/system/setting",
                Icon = "Tools",
                Permission = "setting:list",
                SortOrder = 6,
                ParentId = null,
                IsEnabled = true
            };
            db.Menus.Add(settingMenu);
            await db.SaveChangesAsync();
        }
        else
        {
            if (settingMenu.MenuType != 1)
            {
                settingMenu.MenuType = 1;
                settingMenu.Component = null;
            }
            settingMenu.ParentId = null;
            settingMenu.Icon = "Tools";
            settingMenu.SortOrder = 6;
            await db.SaveChangesAsync();
        }

        var emailSettingMenu = await db.Menus.FirstOrDefaultAsync(m => m.Route == "/system/setting/email");
        if (emailSettingMenu == null)
        {
            emailSettingMenu = new SysMenu
            {
                Id = Guid.NewGuid(),
                MenuName = "邮件设置",
                MenuType = 2,
                Route = "/system/setting/email",
                Component = "system/setting/email/index",
                Permission = "email:update",
                SortOrder = 1,
                ParentId = settingMenu.Id,
                IsEnabled = true
            };
            db.Menus.Add(emailSettingMenu);
            await db.SaveChangesAsync();
        }
        else
        {
            emailSettingMenu.ParentId = settingMenu.Id;
            await db.SaveChangesAsync();
        }

        if (adminRole != null)
        {
            var menuIds = new[] { settingMenu.Id, emailSettingMenu.Id };
            foreach (var menuId in menuIds)
            {
                if (!await db.RoleMenus.AnyAsync(rm => rm.RoleId == adminRole.Id && rm.MenuId == menuId))
                {
                    db.RoleMenus.Add(new SysRoleMenu
                    {
                        Id = Guid.NewGuid(),
                        RoleId = adminRole.Id,
                        MenuId = menuId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureEmailSettingAsync(AppDbContext db)
    {
        if (!await db.EmailSettings.AnyAsync())
        {
            db.EmailSettings.Add(new SysEmailSetting
            {
                Id = Guid.NewGuid(),
                SmtpHost = "",
                SmtpPort = 587,
                Username = "",
                Password = "",
                FromName = "",
                FromAddress = "",
                EnableSsl = true,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureTaskMenuAsync(AppDbContext db)
    {
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "超级管理员");

        var taskMenu = await db.Menus.FirstOrDefaultAsync(m => m.Route == "/task");
        if (taskMenu == null)
        {
            taskMenu = new SysMenu
            {
                Id = Guid.NewGuid(),
                MenuName = "任务管理",
                MenuType = 1,
                Route = "/task",
                Icon = "Clock",
                Permission = "job:list",
                SortOrder = 7,
                ParentId = null,
                IsEnabled = true
            };
            db.Menus.Add(taskMenu);
            await db.SaveChangesAsync();
        }
        else
        {
            if (taskMenu.MenuType != 1)
            {
                taskMenu.MenuType = 1;
                taskMenu.Component = null;
            }
            taskMenu.ParentId = null;
            taskMenu.Icon = "Clock";
            taskMenu.SortOrder = 7;
            await db.SaveChangesAsync();
        }

        var taskListMenu = await db.Menus.FirstOrDefaultAsync(m => m.Route == "/task/list");
        if (taskListMenu == null)
        {
            taskListMenu = new SysMenu
            {
                Id = Guid.NewGuid(),
                MenuName = "任务列表",
                MenuType = 2,
                Route = "/task/list",
                Component = "system/task/index",
                Permission = "job:list",
                SortOrder = 1,
                ParentId = taskMenu.Id,
                IsEnabled = true
            };
            db.Menus.Add(taskListMenu);
            await db.SaveChangesAsync();
        }
        else
        {
            taskListMenu.ParentId = taskMenu.Id;
            taskListMenu.Component = "system/task/index";
            await db.SaveChangesAsync();
        }

        var buttonPermissions = new[]
        {
            ("任务新增", "job:create", 1),
            ("任务编辑", "job:update", 2),
            ("任务删除", "job:delete", 3),
            ("任务触发", "job:trigger", 4)
        };

        foreach (var (name, perm, sort) in buttonPermissions)
        {
            if (!await db.Menus.AnyAsync(m => m.Permission == perm && m.ParentId == taskMenu.Id))
            {
                db.Menus.Add(new SysMenu
                {
                    Id = Guid.NewGuid(),
                    MenuName = name,
                    MenuType = 3,
                    Permission = perm,
                    SortOrder = sort,
                    ParentId = taskMenu.Id,
                    IsEnabled = true
                });
            }
        }
        await db.SaveChangesAsync();

        if (adminRole != null)
        {
            var taskMenuIds = await db.Menus.Where(m => m.Route == "/task" || m.Route == "/task/list" || (m.ParentId == taskMenu.Id && m.MenuType == 3)).Select(m => m.Id).ToListAsync();
            foreach (var menuId in taskMenuIds)
            {
                if (!await db.RoleMenus.AnyAsync(rm => rm.RoleId == adminRole.Id && rm.MenuId == menuId))
                {
                    db.RoleMenus.Add(new SysRoleMenu
                    {
                        Id = Guid.NewGuid(),
                        RoleId = adminRole.Id,
                        MenuId = menuId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureDefaultJobAsync(AppDbContext db)
    {
        if (!await db.Jobs.AnyAsync(j => j.JobType == "UserDataSeed" && !j.IsDeleted))
        {
            db.Jobs.Add(new SysJob
            {
                Id = Guid.NewGuid(),
                JobName = "人员数据插入",
                JobGroup = "DEFAULT",
                JobType = "UserDataSeed",
                CronExpression = "0 0/10 * * * ?",
                Description = "每10分钟自动生成5条演示用户数据，并备份当前人员数据到CSV",
                ConfigJson = "{\"BatchSize\":5,\"BackupDirectory\":\"./backups/users\"}",
                IsEnabled = false,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureExternalApiMenuAsync(AppDbContext db)
    {
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "超级管理员");

        var externalApiMenu = await db.Menus.FirstOrDefaultAsync(m => m.Route == "/externalapi");
        if (externalApiMenu == null)
        {
            externalApiMenu = new SysMenu
            {
                Id = Guid.NewGuid(),
                MenuName = "对外 API",
                MenuType = 1,
                Route = "/externalapi",
                Icon = "Connection",
                Permission = "externalapi:list",
                SortOrder = 8,
                ParentId = null,
                IsEnabled = true
            };
            db.Menus.Add(externalApiMenu);
            await db.SaveChangesAsync();
        }
        else
        {
            if (externalApiMenu.MenuType != 1)
            {
                externalApiMenu.MenuType = 1;
                externalApiMenu.Component = null;
            }
            externalApiMenu.ParentId = null;
            externalApiMenu.Icon = "Connection";
            externalApiMenu.SortOrder = 8;
            await db.SaveChangesAsync();
        }

        var tokenMenu = await db.Menus.FirstOrDefaultAsync(m => m.Route == "/externalapi/token");
        if (tokenMenu == null)
        {
            tokenMenu = new SysMenu
            {
                Id = Guid.NewGuid(),
                MenuName = "Token 管理",
                MenuType = 2,
                Route = "/externalapi/token",
                Component = "system/externalapi/index",
                Permission = "externalapi:list",
                SortOrder = 1,
                ParentId = externalApiMenu.Id,
                IsEnabled = true
            };
            db.Menus.Add(tokenMenu);
            await db.SaveChangesAsync();
        }
        else
        {
            tokenMenu.ParentId = externalApiMenu.Id;
            tokenMenu.MenuName = "Token 管理";
            tokenMenu.Component = "system/externalapi/index";
            await db.SaveChangesAsync();
        }

        var apiMenu = await db.Menus.FirstOrDefaultAsync(m => m.Route == "/externalapi/api");
        if (apiMenu == null)
        {
            apiMenu = new SysMenu
            {
                Id = Guid.NewGuid(),
                MenuName = "接口管理",
                MenuType = 2,
                Route = "/externalapi/api",
                Component = "system/externalapi/api/index",
                Permission = "externalapi:list",
                SortOrder = 2,
                ParentId = externalApiMenu.Id,
                IsEnabled = true
            };
            db.Menus.Add(apiMenu);
            await db.SaveChangesAsync();
        }
        else
        {
            apiMenu.ParentId = externalApiMenu.Id;
            apiMenu.MenuName = "接口管理";
            apiMenu.Component = "system/externalapi/api/index";
            await db.SaveChangesAsync();
        }

        var buttonPermissions = new[]
        {
            ("Token 新增", "externalapi:create", 1),
            ("Token 编辑", "externalapi:update", 2),
            ("Token 删除", "externalapi:delete", 3)
        };

        foreach (var (name, perm, sort) in buttonPermissions)
        {
            if (!await db.Menus.AnyAsync(m => m.Permission == perm && m.ParentId == externalApiMenu.Id))
            {
                db.Menus.Add(new SysMenu
                {
                    Id = Guid.NewGuid(),
                    MenuName = name,
                    MenuType = 3,
                    Permission = perm,
                    SortOrder = sort,
                    ParentId = externalApiMenu.Id,
                    IsEnabled = true
                });
            }
        }
        await db.SaveChangesAsync();

        if (adminRole != null)
        {
            var menuIds = await db.Menus
                .Where(m => m.Route == "/externalapi" || m.Route == "/externalapi/token" || m.Route == "/externalapi/api" || (m.ParentId == externalApiMenu.Id && m.MenuType == 3))
                .Select(m => m.Id)
                .ToListAsync();

            foreach (var menuId in menuIds)
            {
                if (!await db.RoleMenus.AnyAsync(rm => rm.RoleId == adminRole.Id && rm.MenuId == menuId))
                {
                    db.RoleMenus.Add(new SysRoleMenu
                    {
                        Id = Guid.NewGuid(),
                        RoleId = adminRole.Id,
                        MenuId = menuId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 注册默认的对外 API 接口路由。
    /// </summary>
    private static async Task EnsureExternalApiRoutesAsync(AppDbContext db)
    {
        if (await db.ExternalApis.AnyAsync()) return;

        var routes = new List<SysExternalApi>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ApiName = "用户列表",
                Route = "/external/v1/users",
                Method = "GET",
                Description = "获取用户分页列表",
                RateLimitPerSecond = 10,
                RequireIdempotency = true,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ApiName = "系统包信息",
                Route = "/external/v1/systeminfo/packages",
                Method = "GET",
                Description = "获取系统前后端引入包信息",
                RateLimitPerSecond = 10,
                RequireIdempotency = true,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ApiName = "部门树",
                Route = "/external/v1/departments",
                Method = "GET",
                Description = "获取部门树形结构",
                RateLimitPerSecond = 10,
                RequireIdempotency = false,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ApiName = "部门详情",
                Route = "/external/v1/departments/{id}",
                Method = "GET",
                Description = "根据ID获取部门详情",
                RateLimitPerSecond = 10,
                RequireIdempotency = false,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ApiName = "岗位列表",
                Route = "/external/v1/positions",
                Method = "GET",
                Description = "获取岗位分页列表",
                RateLimitPerSecond = 10,
                RequireIdempotency = false,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ApiName = "岗位详情",
                Route = "/external/v1/positions/{id}",
                Method = "GET",
                Description = "根据ID获取岗位详情",
                RateLimitPerSecond = 10,
                RequireIdempotency = false,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        db.ExternalApis.AddRange(routes);
        await db.SaveChangesAsync();
    }
}

