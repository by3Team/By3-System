package com.by3.service.impl;

import com.by3.dto.request.CreateDepartmentDto;
import com.by3.dto.request.UpdateDepartmentDto;
import com.by3.dto.response.DepartmentTreeDto;
import com.by3.entity.SysDepartment;
import com.by3.exception.BusinessException;
import com.by3.repository.SysDepartmentRepository;
import com.by3.service.DepartmentService;
import com.baomidou.mybatisplus.core.toolkit.Wrappers;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.UUID;

/**
 * 部门服务实现类
 * <p>
 * 提供部门的增删改查功能，支持树形结构展示。
 * 删除部门前会检查是否存在关联用户。
 * </p>
 */
@Service
public class DepartmentServiceImpl implements DepartmentService {

    private final SysDepartmentRepository departmentRepository;

    /**
     * 构造方法，注入部门仓储
     *
     * @param departmentRepository 部门仓储
     */
    public DepartmentServiceImpl(SysDepartmentRepository departmentRepository) {
        this.departmentRepository = departmentRepository;
    }

    /**
     * 获取部门树形结构
     *
     * @return 部门树形结构列表
     */
    @Override
    public List<DepartmentTreeDto> getTree() {
        List<SysDepartment> all = departmentRepository.selectList(
                Wrappers.<SysDepartment>lambdaQuery()
                        .orderByAsc(SysDepartment::getSortOrder));
        List<DepartmentTreeDto> dtos = all.stream().map(this::toDto).toList();
        return buildTree(dtos, null);
    }

    /**
     * 根据ID获取部门详情
     *
     * @param id 部门ID
     * @return 部门DTO
     * @throws RuntimeException 部门不存在时抛出
     */
    @Override
    public DepartmentTreeDto getById(UUID id) {
        SysDepartment dept = departmentRepository.selectById(id);
        if (dept == null) {
            throw new RuntimeException("部门不存在");
        }
        return toDto(dept);
    }

    /**
     * 创建部门
     *
     * @param dto 部门创建参数
     * @return 新创建的部门ID
     */
    @Override
    @Transactional
    public UUID create(CreateDepartmentDto dto) {
        long count = departmentRepository.selectCount(
                Wrappers.<SysDepartment>lambdaQuery().eq(SysDepartment::getDeptCode, dto.getDeptCode()));
        if (count > 0) {
            throw new BusinessException("部门编码已存在");
        }
        SysDepartment dept = new SysDepartment();
        dept.setDeptName(dto.getDeptName());
        dept.setDeptCode(dto.getDeptCode());
        dept.setParentId(dto.getParentId());
        dept.setSortOrder(dto.getSortOrder());
        departmentRepository.insert(dept);
        return dept.getId();
    }

    /**
     * 更新部门信息
     *
     * @param id  部门ID
     * @param dto 部门更新参数
     * @return 更新的记录数
     * @throws RuntimeException 部门不存在时抛出
     */
    @Override
    @Transactional
    public int update(UUID id, UpdateDepartmentDto dto) {
        SysDepartment dept = departmentRepository.selectById(id);
        if (dept == null) {
            throw new RuntimeException("部门不存在");
        }
        if (dto.getDeptName() != null) dept.setDeptName(dto.getDeptName());
        if (dto.getDeptCode() != null) dept.setDeptCode(dto.getDeptCode());
        if (dto.getParentId() != null) dept.setParentId(dto.getParentId());
        if (dto.getSortOrder() != null) dept.setSortOrder(dto.getSortOrder());
        if (dto.getIsEnabled() != null) dept.setIsEnabled(dto.getIsEnabled());
        return departmentRepository.updateById(dept);
    }

    /**
     * 删除部门
     * <p>
     * 删除前检查部门下是否存在用户，存在则拒绝删除。
     * </p>
     *
     * @param id 部门ID
     * @return 操作结果消息
     * @throws RuntimeException 部门下存在用户时抛出
     */
    @Override
    @Transactional
    public String delete(UUID id) {
        long userCount = departmentRepository.countUsersByDepartmentId(id);
        if (userCount > 0) {
            throw new RuntimeException("该部门下存在用户，无法删除");
        }
        departmentRepository.deleteById(id);
        return "删除成功";
    }

    /**
     * 将部门实体转换为DTO
     *
     * @param dept 部门实体
     * @return 部门DTO
     */
    private DepartmentTreeDto toDto(SysDepartment dept) {
        DepartmentTreeDto dto = new DepartmentTreeDto();
        dto.setId(dept.getId());
        dto.setDeptName(dept.getDeptName());
        dto.setDeptCode(dept.getDeptCode());
        dto.setParentId(dept.getParentId());
        dto.setSortOrder(dept.getSortOrder());
        dto.setIsEnabled(dept.getIsEnabled());
        dto.setCreatedAt(dept.getCreatedAt());
        return dto;
    }

    /**
     * 递归构建部门树
     *
     * @param list     部门DTO列表
     * @param parentId 父部门ID
     * @return 树形结构部门列表
     */
    private List<DepartmentTreeDto> buildTree(List<DepartmentTreeDto> list, UUID parentId) {
        return list.stream()
                .filter(d -> {
                    if (parentId == null) return d.getParentId() == null;
                    return parentId.equals(d.getParentId());
                })
                .peek(d -> d.setChildren(buildTree(list, d.getId())))
                .toList();
    }
}
