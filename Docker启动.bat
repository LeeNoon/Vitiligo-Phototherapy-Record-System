@echo off
chcp 65001
cls
echo ========================================================
echo       白癜风光疗记录系统 - Docker 启动脚本
echo ========================================================
echo.
echo 正在检查 Docker 环境...
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo [错误] 未检测到 Docker！
    echo 请先安装 Docker Desktop for Windows。
    echo 下载地址: https://www.docker.com/products/docker-desktop
    pause
    exit /b
)

echo.
echo [1/2] 正在构建并启动容器...
echo (首次运行可能需要下载镜像，请耐心等待)
docker-compose up -d --build

if %errorlevel% neq 0 (
    echo.
    echo [错误] 启动失败！请检查 Docker 是否正在运行。
    pause
    exit /b
)

echo.
echo [2/2] 启动成功！
echo --------------------------------------------------------
echo 请在浏览器访问: http://localhost
echo --------------------------------------------------------
echo 数据将保存在当前目录下的 docker_data 文件夹中。
echo.
pause
