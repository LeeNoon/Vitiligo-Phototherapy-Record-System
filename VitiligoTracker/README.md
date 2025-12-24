# 白癜风光疗记录系统 (Vitiligo Phototherapy Record System)

这是一个基于 ASP.NET Core MVC (.NET 8) 的简单光疗记录系统。

## 功能
- 患者管理：新增、删除、查看患者。
- 记录管理：为患者添加每次光疗的日期、剂量、时长和反应。
- 数据存储：使用 SQLite 本地数据库，无需额外安装数据库服务器。

## 如何运行

### 前置条件
请确保已安装 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)。

### 步骤

1. **打开终端**
   在当前文件夹 (`VitiligoTracker`) 打开命令行或终端。

2. **安装 Entity Framework 工具 (如果尚未安装)**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. **创建数据库**
   运行以下命令以生成数据库文件 (`vitiligo.db`)：
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **运行项目**
   ```bash
   dotnet run
   ```

5. **访问网站**
   打开浏览器访问终端中显示的地址 (通常是 `http://localhost:5000` 或 `https://localhost:5001`)。

## 项目结构
- **Models**: 数据模型 (Patient, TreatmentRecord)
- **Data**: 数据库上下文 (ApplicationDbContext)
- **Controllers**: 业务逻辑 (PatientsController)
- **Views**: 页面展示 (使用 Bootstrap 5 样式)
