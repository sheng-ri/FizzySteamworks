# FizzySteamworks - LAN模式支持

> 原始项目文档请见 [README_ORIGIN.md](README_ORIGIN.md)

## 目标

实现FizzySteamworks库的**LAN直连模式**，通过配置文件快速切换UDP直连和Steam P2P两种网络模式，用于开发和测试。
目前代码目标游戏**Sephiria 赛菲莉娅**

## 编译 DLL

### 前置条件
- 安装 .NET SDK（`dotnet` 命令可用）
- 拥有 Heathen.Steamworks 库

### 编译步骤

**Linux/macOS：**
```bash
export DLL_PATH=/path/to/game/Plugins
./build.sh Release
```

**Windows：**
```cmd
set DLL_PATH=C:\path\to\game\Plugins
build.bat Release
```

编译生成的 DLL 文件位置：`build/output/FizzySteamworks.dll`

### 使用编译的 DLL
1. 找到你的游戏 `Plugins` 文件夹
2. 备份原始的 `FizzySteamworks.dll`
3. 将编译生成的 DLL 复制到该文件夹替换原文件
4. 确保 `lan_config.json` 在游戏运行目录（与 EXE 同级）

## 配置

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

## 实现细节

### NextCommon.cs
- **配置加载逻辑**：从应用运行目录读取 `lan_config.json`
- **自动创建配置**：文件不存在时自动生成默认配置（`lan=false`）
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

1. 设置 DLL 路径：
   ```bash
   export DLL_PATH=/path/to/game/Plugins  # 或 Windows: set DLL_PATH=...
   ```

2. 编译生成 DLL：
   ```bash
   ./build.sh Release  # 或 Windows: build.bat Release
   ```

3. 将 DLL 复制到游戏 Plugins 文件夹替换原文件

4. 运行游戏，自动在运行目录生成 `lan_config.json`

5. 编辑配置文件，设置 `lan=true` 和网络参数

6. 重启游戏，享受LAN直连模式！
