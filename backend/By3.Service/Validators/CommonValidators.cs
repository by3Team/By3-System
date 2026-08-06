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

using FluentValidation;
using By3.Service.DTOs;

namespace By3.Service.Validators;

/// <summary>
/// 登录请求参数验证器。
/// </summary>
public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("用户名不能为空");
        RuleFor(x => x.Password).NotEmpty().WithMessage("密码不能为空");
    }
}

/// <summary>
/// 创建用户参数验证器。
/// </summary>
public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().Length(3, 20).WithMessage("用户名长度3-20位");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("密码最少8位")
            .Matches(@"[A-Z]").WithMessage("密码须包含至少一个大写字母")
            .Matches(@"[a-z]").WithMessage("密码须包含至少一个小写字母")
            .Matches(@"[0-9]").WithMessage("密码须包含至少一个数字");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("邮箱格式错误");
    }
}

/// <summary>
/// 更新用户参数验证器。
/// </summary>
public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("邮箱格式错误");
    }
}

/// <summary>
/// 重置密码参数验证器。
/// </summary>
public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).WithMessage("密码最少8位")
            .Matches(@"[A-Z]").WithMessage("密码须包含至少一个大写字母")
            .Matches(@"[a-z]").WithMessage("密码须包含至少一个小写字母")
            .Matches(@"[0-9]").WithMessage("密码须包含至少一个数字");
    }
}

/// <summary>
/// 修改密码参数验证器。
/// </summary>
public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("旧密码不能为空");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).WithMessage("新密码最少8位")
            .Matches(@"[A-Z]").WithMessage("新密码须包含至少一个大写字母")
            .Matches(@"[a-z]").WithMessage("新密码须包含至少一个小写字母")
            .Matches(@"[0-9]").WithMessage("新密码须包含至少一个数字");
        RuleFor(x => x.NewPassword).NotEqual(x => x.OldPassword).WithMessage("新密码不能与旧密码相同");
    }
}

/// <summary>
/// 创建角色参数验证器。
/// </summary>
public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(50).WithMessage("角色名称不能为空");
    }
}

/// <summary>
/// 更新角色参数验证器。
/// </summary>
public class UpdateRoleValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().When(x => !string.IsNullOrEmpty(x.RoleName)).MaximumLength(50).WithMessage("角色名称长度不能超过50");
    }
}

/// <summary>
/// 创建菜单参数验证器。
/// </summary>
public class CreateMenuValidator : AbstractValidator<CreateMenuDto>
{
    public CreateMenuValidator()
    {
        RuleFor(x => x.MenuName).NotEmpty().MaximumLength(50).WithMessage("菜单名称不能为空");
        RuleFor(x => x.MenuType).InclusiveBetween(1, 3).WithMessage("菜单类型只能是1-3");
    }
}

/// <summary>
/// 更新菜单参数验证器。
/// </summary>
public class UpdateMenuValidator : AbstractValidator<UpdateMenuDto>
{
    public UpdateMenuValidator()
    {
        RuleFor(x => x.MenuType).InclusiveBetween(1, 3).When(x => x.MenuType.HasValue).WithMessage("菜单类型只能是1-3");
    }
}
