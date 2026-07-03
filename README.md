# ChatAvalonia - 跨平台实时聊天系统

基于 Avalonia UI + Furion 框架构建的跨平台实时聊天应用，采用 WebSocket 实现消息实时推送，支持 Windows、Linux、macOS 多端运行。
前端使用 Ursa.Controls 组件库打造现代化界面，后端依托 Furion 提供稳定的服务端能力。

---

## ✨ 功能特性
- 用户注册与登录认证
- 一对一实时私聊
- 消息时间戳展示
- 收发消息气泡布局区分
- 跨平台桌面客户端支持
- 模块化分层架构，易于扩展

---

## 🛠️ 技术栈

### 前端（桌面客户端）
- **UI 框架**：Avalonia UI 跨平台 UI 框架
- **组件库**：Ursa.Controls
- **运行环境**：.NET 8+
- **通信协议**：WebSocket

### 后端（服务端）
- **主框架**：Furion + ASP.NET Core
- **ORM 框架**：SqlSugar
- **通信能力**：WebSocket 实时长连接
- **架构模式**：分层架构（Core / Application / Server）

---

## 📂 项目结构
```plaintext
ChatAvalonia/
├── 聊天.应用        # 应用服务层，业务逻辑实现
├── Chat.Core        # 核心层，实体模型、DTO、公共接口与通用类
├── 聊天.桌面        # Avalonia 跨平台桌面客户端
├── 聊天服务器       # Furion 后端服务端，提供 WebSocket 与 HTTP 接口
├── .gitignore
└── Chat.sln         # 解决方案文件


🚀 快速开始
环境要求
.NET 8.0 SDK 及以上
Visual Studio 2022 / Rider（推荐）
兼容 SqlSugar 的数据库（默认支持 SQLite / MySQL，可自行配置）
1. 启动后端服务
在 IDE 中打开 聊天服务器 项目
修改项目内 appsettings.json，配置数据库连接字符串、服务监听端口
编译并启动项目，服务将同时监听 HTTP 与 WebSocket 端口
2. 启动桌面客户端
打开 聊天.桌面 项目
修改 appsettings.json 中的 ServerIp、HttpPort、WebSocketPort，指向已启动的后端服务地址
编译并启动客户端，注册账号后即可登录进入聊天


⚙️ 关键配置说明
客户端配置（聊天.桌面/appsettings.json）
json
{
  "ServerSettings": {
    "ServerIp": "127.0.0.1",
    "HttpPort": 8081,
    "WebSocketPort": 8082
  }
}
服务端配置（聊天服务器/appsettings.json）
json
{
  "DbSettings": {
    "ConnectionString": "你的数据库连接字符串"
  },
  "ServerPort": {
    "Http": 8081,
    "WebSocket": 8082
  }
}


📝 后续规划
 群聊功能支持
 文件、图片消息传输
 消息已读状态回执
 历史消息云端漫游
 AI 对话助手能力集成
