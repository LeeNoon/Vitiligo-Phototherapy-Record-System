@echo off
chcp 65001
cls
echo ========================================================
echo       白癜风光疗记录系统 - 完全重置修复工具
echo ========================================================
echo.
echo 正在强制关闭可能占用的进程...
taskkill /F /IM VitiligoTracker.exe >nul 2>&1

cd VitiligoTracker

echo.
echo [1/6] 安装数据库工具...
dotnet tool install --global dotnet-ef >nul 2>&1

echo.
echo [2/6] 清理旧数据和缓存...
if exist vitiligo.db del vitiligo.db
if exist Migrations rmdir /s /q Migrations
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo.
echo [3/6] 重新编译项目...
dotnet build
if %errorlevel% neq 0 (
    echo.
    echo [错误] 项目编译失败！请检查代码是否有误。
    pause
    exit /b
)

echo.
echo [4/6] 生成新的数据库结构...
dotnet ef migrations add InitialCreate
if %errorlevel% neq 0 (
    echo.
    echo [错误] 无法生成数据库迁移。
    pause
    exit /b
)

echo.
echo [5/6] 创建新数据库...
dotnet ef database update
if %errorlevel% neq 0 (
    echo.
    echo [错误] 无法创建数据库文件。
    pause
    exit /b
)

echo.
echo [6/6] 启动系统...
echo --------------------------------------------------------
echo 请在浏览器访问: http://localhost:5000
echo --------------------------------------------------------
dotnet run
pause
