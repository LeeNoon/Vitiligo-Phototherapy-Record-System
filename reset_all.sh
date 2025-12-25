#!/bin/bash

set -e

echo "========================================================="
echo "  白癜风光疗记录系统 - 完全重置修复工具 (macOS/Linux)"
echo "========================================================="
echo

echo "正在强制关闭可能占用的进程..."
killall VitiligoTracker 2>/dev/null || true

cd VitiligoTracker

echo

echo "[1/6] 安装数据库工具..."
dotnet tool install --global dotnet-ef || true

echo

echo "[2/6] 清理旧数据和缓存..."
rm -f vitiligo.db
rm -rf Migrations
rm -rf bin
rm -rf obj

echo

echo "[3/6] 重新编译项目..."
dotnet build || { echo "[错误] 项目编译失败！请检查代码是否有误。"; exit 1; }

echo

echo "[4/6] 生成新的数据库结构..."
dotnet ef migrations add InitialCreate || { echo "[错误] 无法生成数据库迁移。"; exit 1; }

echo

echo "[5/6] 创建新数据库..."
dotnet ef database update || { echo "[错误] 无法创建数据库文件。"; exit 1; }

echo

echo "[6/6] 启动系统..."
echo "--------------------------------------------------------"
echo "请在浏览器访问: http://localhost:5000"
echo "--------------------------------------------------------"
dotnet run
