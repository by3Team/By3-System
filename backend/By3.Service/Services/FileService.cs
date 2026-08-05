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

using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class FileService : IFileService
{
    private readonly FileRecordRepository _repo;
    private readonly DictDataRepository _dictRepo;
    private readonly string _uploadRoot;

    public FileService(FileRecordRepository repo, DictDataRepository dictRepo, IConfiguration configuration)
    {
        _repo = repo;
        _dictRepo = dictRepo;
        _uploadRoot = configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    }

    public async Task<FileUploadResultDto> UploadAsync(IFormFile file, string category, string uploadMode, Guid? userId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("文件不能为空");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        await ValidateFileExtensionAsync(category, ext);

        var fileCategory = await ResolveFileCategoryAsync(category, ext);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var relativePath = Path.Combine(fileCategory, DateTime.UtcNow.ToString("yyyyMM"), fileName);
        var fullPath = GetFullPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var record = new SysFileRecord
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            OriginalFileName = file.FileName,
            StoragePath = relativePath,
            FileSize = file.Length,
            ContentType = file.ContentType ?? "application/octet-stream",
            FileCategory = fileCategory,
            UploadMode = uploadMode,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await _repo.CreateAsync(record);

        return new FileUploadResultDto
        {
            Id = record.Id,
            OriginalFileName = record.OriginalFileName,
            FileSize = record.FileSize,
            DownloadUrl = $"/api/v1/files/{record.Id}/download"
        };
    }

    public async Task<List<FileUploadResultDto>> UploadMultipleAsync(List<IFormFile> files, string category, Guid? userId)
    {
        var results = new List<FileUploadResultDto>();
        foreach (var file in files)
        {
            results.Add(await UploadAsync(file, category, "multiple", userId));
        }
        return results;
    }

    public async Task<FileRecordDto?> GetByIdAsync(Guid id)
    {
        var record = await _repo.GetByIdAsync(id);
        return record == null ? null : MapToDto(record);
    }

    public async Task<PageResult<FileRecordDto>> GetListAsync(int page, int pageSize, string? keyword, string? category)
    {
        var items = await _repo.GetListAsync(page, pageSize, keyword, category);
        var total = await _repo.GetCountAsync(keyword, category);
        return new PageResult<FileRecordDto>
        {
            Total = total,
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        var record = await _repo.GetByIdAsync(id);
        if (record != null)
        {
            var fullPath = GetFullPath(record.StoragePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        return await _repo.DeleteAsync(id);
    }

    public async Task<Stream?> GetFileStreamAsync(Guid id)
    {
        var record = await _repo.GetByIdAsync(id);
        if (record == null) return null;
        var fullPath = GetFullPath(record.StoragePath);
        if (!File.Exists(fullPath)) return null;
        return File.OpenRead(fullPath);
    }

    public async Task<byte[]> ExportExcelAsync(string? category)
    {
        var records = await _repo.GetAllAsync(category);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Files");

        var headers = new[] { "ID", "Original File Name", "File Size (Bytes)", "Content Type", "Category", "Upload Mode", "Created At" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int rowNum = 2;
        foreach (var record in records)
        {
            worksheet.Cell(rowNum, 1).Value = record.Id.ToString();
            worksheet.Cell(rowNum, 2).Value = record.OriginalFileName;
            worksheet.Cell(rowNum, 3).Value = record.FileSize;
            worksheet.Cell(rowNum, 4).Value = record.ContentType;
            worksheet.Cell(rowNum, 5).Value = record.FileCategory;
            worksheet.Cell(rowNum, 6).Value = record.UploadMode;
            worksheet.Cell(rowNum, 7).Value = record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            rowNum++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task ValidateFileExtensionAsync(string category, string ext)
    {
        var dictItems = await _dictRepo.GetByTypeCodeAsync("sys_file_category");
        var item = dictItems.FirstOrDefault(d => d.DictValue == category && d.IsEnabled);
        if (item == null) return; // 未配置的字典分类不拦截

        var allowedExts = (item.Remark ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant())
            .ToList();

        if (allowedExts.Count == 0 || (allowedExts.Count == 1 && allowedExts[0] == "*"))
            return;

        if (!allowedExts.Contains(ext))
            throw new ArgumentException($"文件类型 {ext} 不允许上传，当前分类支持：{item.Remark}");
    }

    private async Task<string> ResolveFileCategoryAsync(string category, string ext)
    {
        var dictItems = await _dictRepo.GetByTypeCodeAsync("sys_file_category");
        // 优先按传入的 category 匹配
        var item = dictItems.FirstOrDefault(d => d.DictValue == category && d.IsEnabled);
        if (item != null) return item.DictValue;

        // 否则按扩展名自动匹配分类
        var matched = dictItems.FirstOrDefault(d =>
            d.IsEnabled &&
            (d.Remark ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().ToLowerInvariant())
                .Contains(ext));

        return matched?.DictValue ?? "general";
    }

    private string GetFullPath(string relativePath)
        => Path.Combine(_uploadRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static FileRecordDto MapToDto(SysFileRecord record) => new()
    {
        Id = record.Id,
        FileName = record.FileName,
        OriginalFileName = record.OriginalFileName,
        FileSize = record.FileSize,
        ContentType = record.ContentType,
        FileCategory = record.FileCategory,
        UploadMode = record.UploadMode,
        CreatedAt = record.CreatedAt
    };
}
