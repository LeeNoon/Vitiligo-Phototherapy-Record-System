@echo off
chcp 65001
cls
echo ========================================================
echo       白癜风光疗记录系统 - 项目发布工具
echo ========================================================
echo.
echo 正在发布为 Windows 独立运行包 (无需安装 .NET)...
echo 这可能需要几分钟时间，请耐心等待...
echo.

cd VitiligoTracker
dotnet publish -c Release -r win-x64 --self-contained true -o ../PublishOutput

if %errorlevel% neq 0 (
    echo.
    echo [错误] 发布失败！请检查代码是否有误。
    pause
    exit /b
)

echo.
echo ========================================================
echo 发布成功！
echo.
echo 1. 发布文件已生成在桌面的 [PublishOutput] 文件夹中。
echo 2. 该文件夹包含了运行所需的所有文件。
echo.
echo [如何部署到服务器]
echo 1. 将整个 PublishOutput 文件夹复制到服务器。
echo 2. 双击运行 VitiligoTracker.exe 即可。
echo.
echo 提示：生成的包是"独立运行版"，服务器不需要安装 .NET 环境。
echo ========================================================
pause
