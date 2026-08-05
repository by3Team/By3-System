@echo off
chcp 65001 >nul
setlocal

rem 切换到本批处理文件所在目录（项目根目录）
cd /d "%~dp0"

rem 设置开发环境变量
set ASPNETCORE_ENVIRONMENT=Development
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=by3_dev;Username=postgres;Password=123456
set Jwt__Key=your-super-secret-key-at-least-32-bytes-long!
set DataProtection__EncryptionKey=By3DevPhoneEncryptionKey-ChangeInProd!
set FileStorage__UploadPath=./uploads
set Jobs__UserSeed__DefaultPassword=Demo123!
set TablePrefix=by3_

rem 进入后端 API 目录并启动
cd /d "%~dp0backend\By3.Api"
dotnet run

endlocal
