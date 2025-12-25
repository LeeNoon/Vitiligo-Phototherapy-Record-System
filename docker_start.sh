#!/bin/bash

set -e

echo "========================================================="
echo "  白癜风光疗记录系统 - Docker 启动脚本 (macOS/Linux)"
echo "========================================================="
echo

echo "正在检查 Docker 环境..."
if ! command -v docker >/dev/null 2>&1; then
    echo
    echo "[错误] 未检测到 Docker！"
    echo "请先安装 Docker Desktop for Mac。"
    echo "下载地址: https://www.docker.com/products/docker-desktop"
    exit 1
fi

echo

echo "[1/2] 正在构建并启动容器..."
echo "(首次运行可能需要下载镜像，请耐心等待)"
docker-compose up -d --build

if [ $? -ne 0 ]; then
    echo
    echo "[错误] 启动失败！请检查 Docker 是否正在运行。"
    exit 1
fi

echo

echo "[2/2] 启动成功！"
echo "--------------------------------------------------------"
echo "请在浏览器访问: http://localhost"
echo "--------------------------------------------------------"
echo "数据将保存在当前目录下的 docker_data 文件夹中。"
echo
