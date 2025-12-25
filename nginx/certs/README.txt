请将您的 SSL 证书文件放入此目录：

1. 证书文件命名为: server.crt (如果是 .pem 文件，请重命名或修改 nginx/conf/default.conf)
2. 私钥文件命名为: server.key

如果没有证书，可以使用 openssl 生成自签名证书用于测试：
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout server.key -out server.crt
