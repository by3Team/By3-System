package com.by3.dto.request;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import lombok.Data;

import java.util.UUID;

/**
 * 更新部门信息请求DTO，用于修改部门的基本信息。
 */
@Data
public class UpdateDepartmentDto {

    /** 部门ID，必填 */
    @NotNull(message = "部门ID不能为空")
    private UUID id;

    /** 部门名称 */
    @Size(max = 64, message = "部门名称长度不能超过64")
    private String deptName;

    /** 部门编码，必填 */
    @NotBlank(message = "部门编码不能为空")
    @Size(max = 64, message = "部门编码长度不能超过64")
    private String deptCode;

    /** 父部门ID */
    private UUID parentId;

    /** 排序序号 */
    private Integer sortOrder;

    /** 是否启用 */
    private Boolean isEnabled;
}
