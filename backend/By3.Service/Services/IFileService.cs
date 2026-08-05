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

using Microsoft.AspNetCore.Http;
using By3.Service.DTOs;

namespace By3.Service.Services;

public interface IFileService
{
    Task<FileUploadResultDto> UploadAsync(IFormFile file, string category, string uploadMode, Guid? userId);
    Task<List<FileUploadResultDto>> UploadMultipleAsync(List<IFormFile> files, string category, Guid? userId);
    Task<FileRecordDto?> GetByIdAsync(Guid id);
    Task<PageResult<FileRecordDto>> GetListAsync(int page, int pageSize, string? keyword, string? category);
    Task<int> DeleteAsync(Guid id);
    Task<Stream?> GetFileStreamAsync(Guid id);
    Task<byte[]> ExportExcelAsync(string? category);
}
