@echo off
chcp 65001
cls
echo ========================================================
echo       白癜风光疗记录系统 - 启动脚本
echo ========================================================
echo.

echo [1/4] 检查 .NET 环境...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo [错误] 未检测到 .NET SDK！
    echo.
    echo 请务必先安装 .NET 8.0 SDK。
    echo 下载地址: https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo 安装完成后，请关闭此窗口并重新运行。
    pause
    exit /b
)
echo .NET 环境正常。

echo.
echo [2/4] 进入项目目录...
cd VitiligoTracker

echo.
echo [3/4] 初始化数据库...
dotnet tool install --global dotnet-ef >nul 2>&1
dotnet ef migrations add InitialCreate >nul 2>&1
dotnet ef database update
if %errorlevel% neq 0 (
    echo 数据库更新可能遇到问题，尝试继续...
)

echo.
echo [4/4] 启动网站...
echo 网站启动后，请在浏览器访问: http://localhost:5000
echo (按 Ctrl+C 可停止运行)
echo.
dotnet run
pause
