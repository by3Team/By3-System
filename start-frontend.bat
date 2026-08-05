@echo off
chcp 65001 >nul
setlocal

rem 切换到本批处理文件所在目录（项目根目录）
cd /d "%~dp0"

rem 进入前端目录并启动开发服务器
cd /d "%~dp0frontend"
npm run dev

endlocal
