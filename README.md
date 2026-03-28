# FizzySteamworks - LAN模式支持

> 原始项目文档请见 [README_ORIGIN.md](README_ORIGIN.md)

## 目标

实现FizzySteamworks库的**LAN直连模式**，通过配置文件快速切换UDP直连和Steam P2P两种网络模式，用于开发和测试。
目前代码目标游戏**Sephiria 赛菲莉娅**

## 配置

### 1. 配置文件

运行应用时，库会在**应用运行目录**自动生成 `lan_config.json` 配置文件。编辑此文件来切换模式：

**默认配置（P2P模式）：**
```json
{
  "lan": false,
  "connect_ip": "",
  "connect_port": 0,
  "listen_ip": "",
  "listen_port": 0
}
```

**LAN模式配置示例：**
```json
{
  "lan": true,
  "connect_ip": "127.0.0.1",
  "connect_port": 27015,
  "listen_ip": "0.0.0.0",
  "listen_port": 27015
}
```

### 2. 配置参数说明

| 参数 | 说明 |
|------|------|
| `lan` | `true` = LAN直连模式，`false` = P2P模式 |
| `connect_ip` | 客户端连接的服务器IP地址 |
| `connect_port` | 客户端连接目标端口 |
| `listen_ip` | 服务器监听的IP地址（`0.0.0.0` = 所有接口） |
| `listen_port` | 服务器监听端口 |

## 实现细节

编译与依赖引用请见 [BUILD.md](BUILD.md)。

### NextCommon.cs
- 仅负责公共收发逻辑与缓冲区管理
- 启动时调用 `Config.EnsureLoaded()` 触发配置加载
- 不新增 `config` 实例字段，避免破坏原有字段布局

### NextClient.cs
- `Connect()` 中通过 `Config.Instance` 读取配置（局部变量）
- `lan=true`：使用 `ConnectByIPAddress()` 进行 UDP 直连
- `lan=false`：使用 `ConnectP2P()` 进行 Steam P2P 连接

### NextServer.cs
- `Host()` 中通过 `Config.Instance` 读取配置（局部变量）
- `lan=true`：使用 `CreateListenSocketIP()` 监听指定 IP/端口
- `lan=false`：使用 `CreateListenSocketP2P()` 创建 P2P 监听

### Config.cs
- 保留 `Config` 类型，并以单例 `Config.Instance` 提供访问
- 运行目录存在 `lan_config.json` 时加载，不存在则生成默认文件
- 字段保持为：
  - `lan`
  - `connect_ip`
  - `connect_port`
  - `listen_ip`
  - `listen_port`

### Mirror 事件兼容
- 未新增 `OnServerConnectedWithAddress` 字段
- 服务端连接事件使用 Mirror 原生 `OnServerConnected`

