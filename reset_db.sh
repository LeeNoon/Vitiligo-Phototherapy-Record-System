#!/bin/bash

set -e

echo "========================================================="
echo "  白癜风光疗记录系统 - 数据库修复工具 (macOS/Linux)"
echo "========================================================="
echo

echo "注意：此操作会清空所有现有数据，重置数据库！"
echo "请确保您已经关闭了正在运行的网站窗口。"
echo

cd VitiligoTracker

echo

echo "[1/3] 清理旧数据..."
rm -f vitiligo.db
rm -rf Migrations

echo

echo "[2/3] 重新生成数据库结构..."
dotnet ef migrations add InitialCreate

echo

echo "[3/3] 应用更新..."
dotnet ef database update

echo

echo "========================================================="
echo "修复完成！"
echo "现在您可以重新运行 start.sh 了。"
echo "========================================================="
