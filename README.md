# FizzySteamworks - LAN模式支持

> 原始项目文档请见 [README_ORIGIN.md](README_ORIGIN.md)

## 目标

实现FizzySteamworks库的**LAN直连模式**，通过配置文件快速切换UDP直连和Steam P2P两种网络模式，用于开发和测试。

## 如何使用

### 1. 配置文件

运行应用时，库会在**应用运行目录**自动生成 `lan_config.json` 配置文件。编辑此文件来切换模式：

**默认配置（P2P模式）：**
```json
{
  "lan": false,
  "connect_ip": "",
  "connect_listen_ip": "",
  "listen_port": 0
}
```

**LAN模式配置示例：**
```json
{
  "lan": true,
  "connect_ip": "127.0.0.1",
  "connect_listen_ip": "0.0.0.0",
  "listen_port": 27015
}
```

### 2. 配置参数说明

| 参数 | 说明 |
|------|------|
| `lan` | `true` = LAN直连模式，`false` = P2P模式 |
| `connect_ip` | 客户端连接的服务器IP地址 |
| `connect_listen_ip` | 服务器监听的IP地址（`0.0.0.0` = 所有接口） |
| `listen_port` | 监听端口 |

## 做了什么更改

### NextCommon.cs
- **配置加载逻辑**：改为从应用运行目录读取 `lan_config.json`
- **自动创建配置**：如果文件不存在，自动生成默认配置文件（`lan=false`）
- 添加详细日志输出便于调试

### NextClient.cs
- **Connect() 方法**：根据 `config.lan` 智能选择连接模式
  - `lan=true`：使用 `ConnectByIPAddress()` 进行UDP直连
  - `lan=false`：使用 `ConnectP2P()` 进行Steam P2P连接

### NextServer.cs
- **Host() 方法**：根据 `config.lan` 智能选择监听模式
  - `lan=true`：使用 `CreateListenSocketIP()` 监听指定IP和端口
  - `lan=false`：使用 `CreateListenSocketP2P()` 创建P2P监听

## 快速开始

1. 运行应用，自动在运行目录生成 `lan_config.json`
2. 编辑配置文件，设置 `lan=true` 和网络参数
3. 重启应用，日志会显示 "Loaded LAN config from ..." 或 "Created default config file ..."
4. 完成！客户端和服务器现在使用相应模式连接
