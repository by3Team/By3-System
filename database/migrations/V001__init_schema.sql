CREATE TABLE by3_sysauditlog (
    "Id" uuid NOT NULL,
    "UserId" uuid,
    "UserName" character varying(100) NOT NULL,
    "Action" character varying(200) NOT NULL,
    "Controller" character varying(200),
    "RequestPath" character varying(500),
    "RequestMethod" character varying(20),
    "RequestParams" text,
    "RequestBody" text,
    "RequestHeaders" text,
    "ResponseResult" text,
    "ResponseHeaders" text,
    "StatusCode" integer,
    "ExceptionMessage" text,
    "ElapsedMs" bigint NOT NULL,
    "IpAddress" character varying(100),
    "UserAgent" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysauditlog" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysdepartment (
    "Id" uuid NOT NULL,
    "DeptName" character varying(100) NOT NULL,
    "DeptCode" character varying(100),
    "ParentId" uuid,
    "SortOrder" integer NOT NULL,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysdepartment" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysdictdata (
    "Id" uuid NOT NULL,
    "DictTypeId" uuid NOT NULL,
    "DictLabel" character varying(100) NOT NULL,
    "DictValue" character varying(100) NOT NULL,
    "Remark" character varying(500),
    "SortOrder" integer NOT NULL,
    "IsDefault" boolean NOT NULL DEFAULT FALSE,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysdictdata" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysdicttype (
    "Id" uuid NOT NULL,
    "DictName" character varying(100) NOT NULL,
    "DictType" character varying(100) NOT NULL,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysdicttype" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysemaillog (
    "Id" uuid NOT NULL,
    "TemplateId" uuid,
    "TemplateVersionId" uuid,
    "ToAddresses" text NOT NULL,
    "CcAddresses" text,
    "Subject" character varying(200) NOT NULL,
    "Body" text NOT NULL,
    "Status" character varying(20) NOT NULL,
    "ErrorMessage" text,
    "SentAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysemaillog" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysemailsetting (
    "Id" uuid NOT NULL,
    "SmtpHost" character varying(200) NOT NULL,
    "SmtpPort" integer NOT NULL,
    "Username" character varying(200) NOT NULL,
    "Password" character varying(500) NOT NULL,
    "FromName" character varying(200) NOT NULL,
    "FromAddress" character varying(200) NOT NULL,
    "EnableSsl" boolean NOT NULL,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_by3_sysemailsetting" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysemailtemplate (
    "Id" uuid NOT NULL,
    "TemplateCode" character varying(100) NOT NULL,
    "TemplateName" character varying(100) NOT NULL,
    "Description" character varying(500),
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysemailtemplate" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysemailtemplateversion (
    "Id" uuid NOT NULL,
    "TemplateId" uuid NOT NULL,
    "Version" character varying(50) NOT NULL,
    "Subject" character varying(200) NOT NULL,
    "Body" text NOT NULL,
    "BodyFormat" character varying(20) NOT NULL,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysemailtemplateversion" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysexternalapi (
    "Id" uuid NOT NULL,
    "ApiName" character varying(100) NOT NULL,
    "Route" character varying(500) NOT NULL,
    "Method" character varying(20) NOT NULL,
    "Description" character varying(500),
    "RateLimitPerSecond" integer NOT NULL DEFAULT 0,
    "RequireIdempotency" boolean NOT NULL DEFAULT TRUE,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysexternalapi" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysexternalapiaccesslog (
    "Id" uuid NOT NULL,
    "ApiKey" character varying(100) NOT NULL,
    "RequestPath" character varying(500) NOT NULL,
    "RequestMethod" character varying(20) NOT NULL,
    "RequestParams" text,
    "IpAddress" character varying(100),
    "Status" character varying(20) NOT NULL,
    "ErrorMessage" text,
    "IdempotencyKey" character varying(100),
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysexternalapiaccesslog" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysexternalapitoken (
    "Id" uuid NOT NULL,
    "AppName" character varying(100) NOT NULL,
    "ApiKey" character varying(100) NOT NULL,
    "ApiSecret" character varying(200) NOT NULL,
    "Description" character varying(500),
    "ExpireTime" timestamp with time zone,
    "ExpireType" character varying(20) NOT NULL DEFAULT '30',
    "AllowedApiIds" text,
    "ContactEmail" character varying(500),
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "PreviousValidUntil" timestamp with time zone,
    "PreviousApiKey" character varying(100),
    "PreviousApiSecret" character varying(200),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysexternalapitoken" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysexternalapitokenlog (
    "Id" uuid NOT NULL,
    "TokenId" uuid NOT NULL,
    "Action" character varying(50) NOT NULL,
    "ApiKey" character varying(100) NOT NULL,
    "IpAddress" character varying(100),
    "OperatorId" uuid,
    "OperatorName" character varying(100),
    "Remark" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysexternalapitokenlog" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysexternalapitokenhistory (
    "Id" uuid NOT NULL,
    "TokenId" uuid NOT NULL,
    "AppName" character varying(100) NOT NULL,
    "ApiKey" character varying(100) NOT NULL,
    "ApiSecret" character varying(200) NOT NULL,
    "ExpireTime" timestamp with time zone,
    "ValidUntil" timestamp with time zone,
    "InvalidatedAt" timestamp with time zone,
    "InvalidatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysexternalapitokenhistory" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysfilerecord (
    "Id" uuid NOT NULL,
    "FileName" character varying(200) NOT NULL,
    "OriginalFileName" character varying(200) NOT NULL,
    "StoragePath" character varying(500) NOT NULL,
    "FileSize" bigint NOT NULL,
    "ContentType" character varying(100),
    "FileCategory" character varying(50) NOT NULL,
    "UploadMode" character varying(20) NOT NULL,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysfilerecord" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysjob (
    "Id" uuid NOT NULL,
    "JobName" character varying(100) NOT NULL,
    "JobGroup" character varying(100) NOT NULL,
    "JobType" character varying(100) NOT NULL,
    "CronExpression" character varying(100) NOT NULL,
    "Description" character varying(500),
    "ConfigJson" text,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysjob" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysjoblog (
    "Id" uuid NOT NULL,
    "JobId" uuid NOT NULL,
    "JobName" character varying(100) NOT NULL,
    "FireTime" timestamp with time zone NOT NULL,
    "EndTime" timestamp with time zone,
    "Status" character varying(20) NOT NULL,
    "Result" character varying(500),
    "ExceptionMessage" text,
    "NextFireTime" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysjoblog" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysloginlog (
    "Id" uuid NOT NULL,
    "UserId" uuid,
    "UserName" character varying(100) NOT NULL,
    "IsSuccess" boolean NOT NULL,
    "Message" character varying(500),
    "IpAddress" character varying(100),
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysloginlog" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysmenu (
    "Id" uuid NOT NULL,
    "MenuName" character varying(100) NOT NULL,
    "Permission" character varying(100),
    "Route" character varying(200),
    "Icon" character varying(50),
    "Component" character varying(200),
    "MenuType" integer NOT NULL,
    "SortOrder" integer NOT NULL,
    "ParentId" uuid,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysmenu" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysposition (
    "Id" uuid NOT NULL,
    "PositionName" character varying(100) NOT NULL,
    "PositionCode" character varying(100),
    "SortOrder" integer NOT NULL,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysposition" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysrole (
    "Id" uuid NOT NULL,
    "RoleName" character varying(100) NOT NULL,
    "Description" character varying(500),
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysrole" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysrolemenu (
    "Id" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    "MenuId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysrolemenu" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysuser (
    "Id" uuid NOT NULL,
    "UserName" character varying(100) NOT NULL,
    "PasswordHash" character varying(200) NOT NULL,
    "Email" character varying(200),
    "Phone" character varying(50),
    "RealName" character varying(100),
    "Gender" text,
    "DepartmentId" uuid,
    "PositionId" uuid,
    "IsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_by3_sysuser" PRIMARY KEY ("Id")
);


CREATE TABLE by3_sysuserrole (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_by3_sysuserrole" PRIMARY KEY ("Id")
);


CREATE INDEX "IX_by3_sysauditlog_CreatedAt" ON by3_sysauditlog ("CreatedAt");


CREATE INDEX "IX_by3_sysdepartment_ParentId" ON by3_sysdepartment ("ParentId");


CREATE INDEX "IX_by3_sysdictdata_DictTypeId" ON by3_sysdictdata ("DictTypeId");


CREATE UNIQUE INDEX "IX_by3_sysdicttype_DictType" ON by3_sysdicttype ("DictType");


CREATE INDEX "IX_by3_sysemaillog_CreatedAt" ON by3_sysemaillog ("CreatedAt");


CREATE INDEX "IX_by3_sysemaillog_Status" ON by3_sysemaillog ("Status");


CREATE UNIQUE INDEX "IX_by3_sysemailtemplate_TemplateCode" ON by3_sysemailtemplate ("TemplateCode");


CREATE UNIQUE INDEX "IX_by3_sysemailtemplateversion_TemplateId_Version" ON by3_sysemailtemplateversion ("TemplateId", "Version");


CREATE UNIQUE INDEX "IX_by3_sysexternalapi_Route_Method" ON by3_sysexternalapi ("Route", "Method");


CREATE INDEX "IX_by3_sysexternalapiaccesslog_ApiKey" ON by3_sysexternalapiaccesslog ("ApiKey");


CREATE INDEX "IX_by3_sysexternalapiaccesslog_CreatedAt" ON by3_sysexternalapiaccesslog ("CreatedAt");


CREATE UNIQUE INDEX "IX_by3_sysexternalapitoken_ApiKey" ON by3_sysexternalapitoken ("ApiKey");


CREATE INDEX "IX_by3_sysexternalapitoken_PreviousApiKey" ON by3_sysexternalapitoken ("PreviousApiKey");


CREATE INDEX "IX_by3_sysexternalapitokenlog_TokenId" ON by3_sysexternalapitokenlog ("TokenId");


CREATE INDEX "IX_by3_sysexternalapitokenlog_CreatedAt" ON by3_sysexternalapitokenlog ("CreatedAt");


CREATE INDEX "IX_by3_sysexternalapitokenhistory_TokenId" ON by3_sysexternalapitokenhistory ("TokenId");


CREATE INDEX "IX_by3_sysexternalapitokenhistory_ApiKey" ON by3_sysexternalapitokenhistory ("ApiKey");


CREATE INDEX "IX_by3_sysexternalapitokenhistory_CreatedAt" ON by3_sysexternalapitokenhistory ("CreatedAt");


CREATE INDEX "IX_by3_sysfilerecord_CreatedAt" ON by3_sysfilerecord ("CreatedAt");


CREATE INDEX "IX_by3_sysjoblog_CreatedAt" ON by3_sysjoblog ("CreatedAt");


CREATE INDEX "IX_by3_sysjoblog_JobId" ON by3_sysjoblog ("JobId");


CREATE INDEX "IX_by3_sysjoblog_Status" ON by3_sysjoblog ("Status");


CREATE INDEX "IX_by3_sysloginlog_CreatedAt" ON by3_sysloginlog ("CreatedAt");


CREATE INDEX "IX_by3_sysrolemenu_RoleId_MenuId" ON by3_sysrolemenu ("RoleId", "MenuId");


CREATE INDEX "IX_by3_sysuserrole_UserId_RoleId" ON by3_sysuserrole ("UserId", "RoleId");



-- 表注释
COMMENT ON TABLE by3_sysauditlog IS '操作审计日志表';
COMMENT ON TABLE by3_sysdepartment IS '部门表';
COMMENT ON TABLE by3_sysdictdata IS '字典数据表';
COMMENT ON TABLE by3_sysdicttype IS '字典类型表';
COMMENT ON TABLE by3_sysemaillog IS '邮件发送日志表';
COMMENT ON TABLE by3_sysemailsetting IS '邮件发送配置表';
COMMENT ON TABLE by3_sysemailtemplate IS '邮件模板表';
COMMENT ON TABLE by3_sysemailtemplateversion IS '邮件模板版本表';
COMMENT ON TABLE by3_sysexternalapi IS '对外API接口注册表';
COMMENT ON TABLE by3_sysexternalapiaccesslog IS '对外API访问日志表';
COMMENT ON TABLE by3_sysexternalapitoken IS '对外API Token表';
COMMENT ON TABLE by3_sysexternalapitokenlog IS '对外API Token操作日志表';
COMMENT ON TABLE by3_sysfilerecord IS '文件记录表';
COMMENT ON TABLE by3_sysjob IS '定时任务表';
COMMENT ON TABLE by3_sysjoblog IS '定时任务执行日志表';
COMMENT ON TABLE by3_sysloginlog IS '登录日志表';
COMMENT ON TABLE by3_sysmenu IS '菜单表';
COMMENT ON TABLE by3_sysposition IS '职位表';
COMMENT ON TABLE by3_sysrole IS '角色表';
COMMENT ON TABLE by3_sysrolemenu IS '角色菜单关联表';
COMMENT ON TABLE by3_sysuser IS '用户表';
COMMENT ON TABLE by3_sysuserrole IS '用户角色关联表';

-- 字段注释
COMMENT ON COLUMN by3_sysauditlog."Id" IS '主键';
COMMENT ON COLUMN by3_sysauditlog."UserId" IS '用户ID';
COMMENT ON COLUMN by3_sysauditlog."UserName" IS '用户名';
COMMENT ON COLUMN by3_sysauditlog."Action" IS '操作描述';
COMMENT ON COLUMN by3_sysauditlog."Controller" IS '控制器名称';
COMMENT ON COLUMN by3_sysauditlog."RequestPath" IS '请求路径';
COMMENT ON COLUMN by3_sysauditlog."RequestMethod" IS '请求方法';
COMMENT ON COLUMN by3_sysauditlog."RequestParams" IS '请求参数（已脱敏）';
COMMENT ON COLUMN by3_sysauditlog."RequestBody" IS '请求体（已脱敏）';
COMMENT ON COLUMN by3_sysauditlog."RequestHeaders" IS '请求头';
COMMENT ON COLUMN by3_sysauditlog."ResponseResult" IS '响应结果';
COMMENT ON COLUMN by3_sysauditlog."ResponseHeaders" IS '响应头';
COMMENT ON COLUMN by3_sysauditlog."StatusCode" IS 'HTTP状态码';
COMMENT ON COLUMN by3_sysauditlog."ExceptionMessage" IS '异常信息';
COMMENT ON COLUMN by3_sysauditlog."ElapsedMs" IS '执行耗时（毫秒）';
COMMENT ON COLUMN by3_sysauditlog."IpAddress" IS 'IP地址';
COMMENT ON COLUMN by3_sysauditlog."UserAgent" IS '浏览器UA';
COMMENT ON COLUMN by3_sysauditlog."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysdepartment."Id" IS '主键';
COMMENT ON COLUMN by3_sysdepartment."DeptName" IS '部门名称';
COMMENT ON COLUMN by3_sysdepartment."DeptCode" IS '部门编码';
COMMENT ON COLUMN by3_sysdepartment."ParentId" IS '父部门ID';
COMMENT ON COLUMN by3_sysdepartment."SortOrder" IS '排序';
COMMENT ON COLUMN by3_sysdepartment."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysdepartment."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysdepartment."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysdepartment."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysdepartment."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysdepartment."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysdictdata."Id" IS '主键';
COMMENT ON COLUMN by3_sysdictdata."DictTypeId" IS '字典类型ID';
COMMENT ON COLUMN by3_sysdictdata."DictLabel" IS '显示标签';
COMMENT ON COLUMN by3_sysdictdata."DictValue" IS '字典值';
COMMENT ON COLUMN by3_sysdictdata."Remark" IS '备注';
COMMENT ON COLUMN by3_sysdictdata."SortOrder" IS '排序';
COMMENT ON COLUMN by3_sysdictdata."IsDefault" IS '是否默认';
COMMENT ON COLUMN by3_sysdictdata."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysdictdata."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysdictdata."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysdictdata."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysdictdata."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysdictdata."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysdicttype."Id" IS '主键';
COMMENT ON COLUMN by3_sysdicttype."DictName" IS '字典名称';
COMMENT ON COLUMN by3_sysdicttype."DictType" IS '字典类型编码';
COMMENT ON COLUMN by3_sysdicttype."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysdicttype."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysdicttype."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysdicttype."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysdicttype."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysdicttype."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysemaillog."Id" IS '主键';
COMMENT ON COLUMN by3_sysemaillog."TemplateId" IS '邮件模板ID';
COMMENT ON COLUMN by3_sysemaillog."TemplateVersionId" IS '模板版本ID';
COMMENT ON COLUMN by3_sysemaillog."ToAddresses" IS '收件人地址，多个用逗号分隔';
COMMENT ON COLUMN by3_sysemaillog."CcAddresses" IS '抄送人地址，多个用逗号分隔';
COMMENT ON COLUMN by3_sysemaillog."Subject" IS '邮件主题';
COMMENT ON COLUMN by3_sysemaillog."Body" IS '邮件内容';
COMMENT ON COLUMN by3_sysemaillog."Status" IS '发送状态：pending/success/failed';
COMMENT ON COLUMN by3_sysemaillog."ErrorMessage" IS '错误信息';
COMMENT ON COLUMN by3_sysemaillog."SentAt" IS '发送时间';
COMMENT ON COLUMN by3_sysemaillog."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysemailsetting."Id" IS '主键';
COMMENT ON COLUMN by3_sysemailsetting."SmtpHost" IS 'SMTP服务器地址';
COMMENT ON COLUMN by3_sysemailsetting."SmtpPort" IS 'SMTP端口';
COMMENT ON COLUMN by3_sysemailsetting."Username" IS '登录账号';
COMMENT ON COLUMN by3_sysemailsetting."Password" IS '登录密码';
COMMENT ON COLUMN by3_sysemailsetting."FromName" IS '发件人名称';
COMMENT ON COLUMN by3_sysemailsetting."FromAddress" IS '发件人邮箱';
COMMENT ON COLUMN by3_sysemailsetting."EnableSsl" IS '是否启用SSL';
COMMENT ON COLUMN by3_sysemailsetting."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysemailsetting."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysemailsetting."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysemailtemplate."Id" IS '主键';
COMMENT ON COLUMN by3_sysemailtemplate."TemplateCode" IS '模板编码';
COMMENT ON COLUMN by3_sysemailtemplate."TemplateName" IS '模板名称';
COMMENT ON COLUMN by3_sysemailtemplate."Description" IS '描述';
COMMENT ON COLUMN by3_sysemailtemplate."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysemailtemplate."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysemailtemplate."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysemailtemplate."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysemailtemplate."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysemailtemplate."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysemailtemplateversion."Id" IS '主键';
COMMENT ON COLUMN by3_sysemailtemplateversion."TemplateId" IS '所属模板ID';
COMMENT ON COLUMN by3_sysemailtemplateversion."Version" IS '版本号';
COMMENT ON COLUMN by3_sysemailtemplateversion."Subject" IS '邮件主题';
COMMENT ON COLUMN by3_sysemailtemplateversion."Body" IS '邮件内容';
COMMENT ON COLUMN by3_sysemailtemplateversion."BodyFormat" IS '内容格式：html/text/markdown';
COMMENT ON COLUMN by3_sysemailtemplateversion."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysemailtemplateversion."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysemailtemplateversion."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysemailtemplateversion."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysemailtemplateversion."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysemailtemplateversion."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."Id" IS '主键';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."ApiKey" IS '请求使用的API Key';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."RequestPath" IS '请求路径';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."RequestMethod" IS '请求方法';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."RequestParams" IS '请求参数';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."IpAddress" IS 'IP地址';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."Status" IS '状态：success/failed';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."ErrorMessage" IS '错误信息';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."IdempotencyKey" IS '幂等性Key';
COMMENT ON COLUMN by3_sysexternalapiaccesslog."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysexternalapi."Id" IS '主键';
COMMENT ON COLUMN by3_sysexternalapi."ApiName" IS '接口名称';
COMMENT ON COLUMN by3_sysexternalapi."Route" IS '请求路径，例如/external/v1/users';
COMMENT ON COLUMN by3_sysexternalapi."Method" IS '请求方法：GET/POST/PUT/DELETE';
COMMENT ON COLUMN by3_sysexternalapi."Description" IS '接口描述';
COMMENT ON COLUMN by3_sysexternalapi."RateLimitPerSecond" IS '单个AK每秒最大请求数，0表示不限流';
COMMENT ON COLUMN by3_sysexternalapi."RequireIdempotency" IS '是否要求Idempotency-Key';
COMMENT ON COLUMN by3_sysexternalapi."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysexternalapi."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysexternalapi."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysexternalapi."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysexternalapi."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysexternalapi."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysexternalapitoken."Id" IS '主键';
COMMENT ON COLUMN by3_sysexternalapitoken."AppName" IS '应用名称';
COMMENT ON COLUMN by3_sysexternalapitoken."ApiKey" IS 'API Key';
COMMENT ON COLUMN by3_sysexternalapitoken."ApiSecret" IS 'API Secret（用于签名）';
COMMENT ON COLUMN by3_sysexternalapitoken."Description" IS '描述';
COMMENT ON COLUMN by3_sysexternalapitoken."ExpireTime" IS '过期时间';
COMMENT ON COLUMN by3_sysexternalapitoken."ExpireType" IS '有效期类型：30/60/90/custom';
COMMENT ON COLUMN by3_sysexternalapitoken."AllowedApiIds" IS '允许访问的对外接口ID列表，JSON数组，为空表示允许全部';
COMMENT ON COLUMN by3_sysexternalapitoken."ContactEmail" IS '负责人邮箱，多个邮箱用逗号分隔，用于接收Token变更通知';
COMMENT ON COLUMN by3_sysexternalapitoken."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysexternalapitoken."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysexternalapitoken."PreviousValidUntil" IS '旧ApiKey缓冲期截止时间，为空表示旧Key立即失效';
COMMENT ON COLUMN by3_sysexternalapitoken."PreviousApiKey" IS '重新生成前的旧ApiKey';
COMMENT ON COLUMN by3_sysexternalapitoken."PreviousApiSecret" IS '重新生成前的旧ApiSecret';
COMMENT ON COLUMN by3_sysexternalapitoken."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysexternalapitoken."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysexternalapitoken."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysexternalapitoken."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysexternalapitokenlog."Id" IS '主键';
COMMENT ON COLUMN by3_sysexternalapitokenlog."TokenId" IS '关联TokenId';
COMMENT ON COLUMN by3_sysexternalapitokenlog."Action" IS '操作类型：Create/Update/Delete/Regenerate/Enable/Disable';
COMMENT ON COLUMN by3_sysexternalapitokenlog."ApiKey" IS '操作时当前ApiKey';
COMMENT ON COLUMN by3_sysexternalapitokenlog."IpAddress" IS '操作人IP';
COMMENT ON COLUMN by3_sysexternalapitokenlog."OperatorId" IS '操作人用户Id';
COMMENT ON COLUMN by3_sysexternalapitokenlog."OperatorName" IS '操作人用户名';
COMMENT ON COLUMN by3_sysexternalapitokenlog."Remark" IS '操作备注';
COMMENT ON COLUMN by3_sysexternalapitokenlog."CreatedAt" IS '操作时间';
COMMENT ON TABLE by3_sysexternalapitokenhistory IS '对外API Token历史凭证表';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."Id" IS '主键';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."TokenId" IS '关联TokenId';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."AppName" IS '应用名称快照';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."ApiKey" IS '历史ApiKey';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."ApiSecret" IS '历史ApiSecret';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."ExpireTime" IS '该历史凭证的过期时间';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."ValidUntil" IS '旧Key缓冲截止时间，为空表示立即失效';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."InvalidatedAt" IS '手动作废时间';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."InvalidatedBy" IS '手动作废人Id';
COMMENT ON COLUMN by3_sysexternalapitokenhistory."CreatedAt" IS '归档时间';
COMMENT ON COLUMN by3_sysfilerecord."Id" IS '主键';
COMMENT ON COLUMN by3_sysfilerecord."FileName" IS '存储文件名';
COMMENT ON COLUMN by3_sysfilerecord."OriginalFileName" IS '原始文件名';
COMMENT ON COLUMN by3_sysfilerecord."StoragePath" IS '存储路径';
COMMENT ON COLUMN by3_sysfilerecord."FileSize" IS '文件大小（字节）';
COMMENT ON COLUMN by3_sysfilerecord."ContentType" IS 'MIME类型';
COMMENT ON COLUMN by3_sysfilerecord."FileCategory" IS '文件分类';
COMMENT ON COLUMN by3_sysfilerecord."UploadMode" IS '上传模式：single/multi';
COMMENT ON COLUMN by3_sysfilerecord."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysfilerecord."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysfilerecord."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysfilerecord."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysfilerecord."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysfilerecord."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysjob."Id" IS '主键';
COMMENT ON COLUMN by3_sysjob."JobName" IS '任务名称';
COMMENT ON COLUMN by3_sysjob."JobGroup" IS '任务分组';
COMMENT ON COLUMN by3_sysjob."JobType" IS '任务类型';
COMMENT ON COLUMN by3_sysjob."CronExpression" IS 'Cron表达式';
COMMENT ON COLUMN by3_sysjob."Description" IS '描述';
COMMENT ON COLUMN by3_sysjob."ConfigJson" IS '任务配置JSON';
COMMENT ON COLUMN by3_sysjob."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysjob."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysjob."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysjob."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysjob."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysjob."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysjoblog."Id" IS '主键';
COMMENT ON COLUMN by3_sysjoblog."JobId" IS '任务ID';
COMMENT ON COLUMN by3_sysjoblog."JobName" IS '任务名称';
COMMENT ON COLUMN by3_sysjoblog."FireTime" IS '触发时间';
COMMENT ON COLUMN by3_sysjoblog."EndTime" IS '结束时间';
COMMENT ON COLUMN by3_sysjoblog."Status" IS '状态：Success/Failed';
COMMENT ON COLUMN by3_sysjoblog."Result" IS '执行结果';
COMMENT ON COLUMN by3_sysjoblog."ExceptionMessage" IS '异常信息';
COMMENT ON COLUMN by3_sysjoblog."NextFireTime" IS '预计下次执行时间';
COMMENT ON COLUMN by3_sysjoblog."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysloginlog."Id" IS '主键';
COMMENT ON COLUMN by3_sysloginlog."UserId" IS '用户ID';
COMMENT ON COLUMN by3_sysloginlog."UserName" IS '用户名';
COMMENT ON COLUMN by3_sysloginlog."IsSuccess" IS '是否成功';
COMMENT ON COLUMN by3_sysloginlog."Message" IS '消息';
COMMENT ON COLUMN by3_sysloginlog."IpAddress" IS 'IP地址';
COMMENT ON COLUMN by3_sysloginlog."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysmenu."Id" IS '主键';
COMMENT ON COLUMN by3_sysmenu."MenuName" IS '菜单名称';
COMMENT ON COLUMN by3_sysmenu."Permission" IS '权限标识';
COMMENT ON COLUMN by3_sysmenu."Route" IS '路由';
COMMENT ON COLUMN by3_sysmenu."Icon" IS '图标';
COMMENT ON COLUMN by3_sysmenu."Component" IS '组件路径';
COMMENT ON COLUMN by3_sysmenu."MenuType" IS '菜单类型：1目录 2菜单 3按钮';
COMMENT ON COLUMN by3_sysmenu."SortOrder" IS '排序';
COMMENT ON COLUMN by3_sysmenu."ParentId" IS '父菜单ID';
COMMENT ON COLUMN by3_sysmenu."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysmenu."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysmenu."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysmenu."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysmenu."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysmenu."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysposition."Id" IS '主键';
COMMENT ON COLUMN by3_sysposition."PositionName" IS '职位名称';
COMMENT ON COLUMN by3_sysposition."PositionCode" IS '职位编码';
COMMENT ON COLUMN by3_sysposition."SortOrder" IS '排序';
COMMENT ON COLUMN by3_sysposition."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysposition."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysposition."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysposition."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysposition."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysposition."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysrole."Id" IS '主键';
COMMENT ON COLUMN by3_sysrole."RoleName" IS '角色名称';
COMMENT ON COLUMN by3_sysrole."Description" IS '描述';
COMMENT ON COLUMN by3_sysrole."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysrole."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysrole."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysrole."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysrole."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysrole."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysrolemenu."Id" IS '主键';
COMMENT ON COLUMN by3_sysrolemenu."RoleId" IS '角色ID';
COMMENT ON COLUMN by3_sysrolemenu."MenuId" IS '菜单ID';
COMMENT ON COLUMN by3_sysrolemenu."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysuser."Id" IS '主键';
COMMENT ON COLUMN by3_sysuser."UserName" IS '用户名';
COMMENT ON COLUMN by3_sysuser."PasswordHash" IS 'BCrypt密码哈希';
COMMENT ON COLUMN by3_sysuser."Email" IS '邮箱';
COMMENT ON COLUMN by3_sysuser."Phone" IS '手机号（加密存储）';
COMMENT ON COLUMN by3_sysuser."RealName" IS '真实姓名';
COMMENT ON COLUMN by3_sysuser."Gender" IS '性别';
COMMENT ON COLUMN by3_sysuser."DepartmentId" IS '所属部门ID';
COMMENT ON COLUMN by3_sysuser."PositionId" IS '职位ID';
COMMENT ON COLUMN by3_sysuser."IsEnabled" IS '是否启用';
COMMENT ON COLUMN by3_sysuser."IsDeleted" IS '软删除标记';
COMMENT ON COLUMN by3_sysuser."CreatedAt" IS '创建时间';
COMMENT ON COLUMN by3_sysuser."UpdatedAt" IS '更新时间';
COMMENT ON COLUMN by3_sysuser."CreatedBy" IS '创建人';
COMMENT ON COLUMN by3_sysuser."UpdatedBy" IS '更新人';
COMMENT ON COLUMN by3_sysuserrole."Id" IS '主键';
COMMENT ON COLUMN by3_sysuserrole."UserId" IS '用户ID';
COMMENT ON COLUMN by3_sysuserrole."RoleId" IS '角色ID';
COMMENT ON COLUMN by3_sysuserrole."CreatedAt" IS '创建时间';

-- 默认对外 API 接口注册数据
INSERT INTO by3_sysexternalapi ("Id", "ApiName", "Route", "Method", "Description", "RateLimitPerSecond", "RequireIdempotency", "IsEnabled", "IsDeleted", "CreatedAt")
VALUES
    (gen_random_uuid(), '用户列表', '/external/v1/users', 'GET', '获取用户分页列表', 10, true, true, false, NOW()),
    (gen_random_uuid(), '系统包信息', '/external/v1/systeminfo/packages', 'GET', '获取系统前后端引入包信息', 10, true, true, false, NOW()),
    (gen_random_uuid(), '部门树', '/external/v1/departments', 'GET', '获取部门树形结构', 10, false, true, false, NOW()),
    (gen_random_uuid(), '部门详情', '/external/v1/departments/{id}', 'GET', '根据ID获取部门详情', 10, false, true, false, NOW()),
    (gen_random_uuid(), '岗位列表', '/external/v1/positions', 'GET', '获取岗位分页列表', 10, false, true, false, NOW()),
    (gen_random_uuid(), '岗位详情', '/external/v1/positions/{id}', 'GET', '根据ID获取岗位详情', 10, false, true, false, NOW())
ON CONFLICT ("Route", "Method") DO NOTHING;
