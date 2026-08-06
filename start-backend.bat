@echo off
chcp 65001 >nul
setlocal

rem 切换到本批处理文件所在目录（项目根目录）
cd /d "%~dp0"

rem 进入后端 API 目录并启动（配置项在 appsettings.json 中）
cd /d "%~dp0backend\By3.Api"
dotnet run

endlocal
