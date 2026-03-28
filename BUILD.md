# DLL 编译指南 (临时配置)

## 快速编译

### 前提条件
- .NET SDK 6.0 或更高版本
- 依赖库已在 `lib/` 目录中

### 编译步骤

**Linux/macOS：**
```bash
chmod +x build.sh
./build.sh Release
```

**Windows：**
```cmd
build.bat Release
```

输出：`build/output/FizzySteamworks.dll`

## 依赖库

依赖库直接使用项目中的 `lib/` 目录，无需额外配置。

## 编译故障排除

### 错误：缺少类型

```
error CS0246: The type or namespace name 'CSteamID' could not be found
```

**检查 lib 目录**：
```bash
ls -la lib/*.dll
```

**确保 lib 目录包含所需 DLL 文件**。

## 编译输出

成功编译后，DLL 位置：`build/output/FizzySteamworks.dll`

### 使用编译的 DLL

1. 备份原始 `FizzySteamworks.dll`
2. 将新 DLL 复制到游戏 Plugins 文件夹
3. 重启游戏，自动生成 `lan_config.json`

## 临时配置说明

此配置为临时 hack 方案，直接使用本地 lib 依赖，简化构建过程。如需正式部署，请参考原始指南配置完整依赖路径。

```
error CS0246: The type or namespace name 'CSteamID' could not be found
```

**原因**：未找到 Steamworks.NET.dll

**解决方案**：
```bash
# Linux/macOS
export STEAMWORKS_DLL_PATH=/path/to/steamworks/dll
./build.sh Release

# Windows
set STEAMWORKS_DLL_PATH=C:\path\to\steamworks\dll
build.bat Release
```

### 错误：缺少 Mirror 依赖

```
error CS0246: The type or namespace name 'Transport' could not be found
```

**原因**：未找到 Mirror.dll

**解决方案**：类似上面，设置 `MIRROR_DLL_PATH`

### 错误：缺少 UnityEngine 依赖

```
error CS0246: The type or namespace name 'Debug' could not be found
```

**原因**：未找到 UnityEngine.dll

**解决方案**：设置 `UNITY_DLL_PATH`

### 错误：dotnet 命令未找到

**安装 .NET SDK：**

- **Ubuntu/Debian：**
  ```bash
  sudo apt-get update
  sudo apt-get install dotnet-sdk-7.0
  ```

- **macOS (Homebrew)：**
  ```bash
  brew install dotnet
  ```

- **Windows：**
  从 [Microsoft .NET 官网](https://dotnet.microsoft.com/download) 下载安装

## 编译后的操作

### 1. 替换游戏 DLL

定位游戏的 Plugins 文件夹：
```
GameFolder/Plugins/FizzySteamworks.dll  ← 替换此文件
```

### 2. 首次使用

运行游戏，会在 `game.exe` 同级目录自动生成：
```
game.exe
lan_config.json  ← 首次生成，默认 P2P 模式
```

### 3. 切换模式

编辑 `lan_config.json`：
```json
{
  "lan": true,
  "connect_ip": "192.168.1.100",
  "connect_listen_ip": "0.0.0.0",
  "listen_port": 27015
}
```

重启游戏生效。

## 高级选项

### 调试编译

生成带调试符号的版本：
```bash
./build.sh Debug
```

### 清理构建文件

```bash
rm -rf build/
dotnet clean com.mirror.steamworks.net/
```

### 指定输出路径

修改 FizzySteamworks.csproj 中的：
```xml
<OutputPath>your/custom/path</OutputPath>
```

## 快速参考

| 任务 | 命令 |
|------|------|
| 标准编译 | `./build.sh Release` |
| 调试编译 | `./build.sh Debug` |
| 设置Linux依赖 | `export VAR=/path && ./build.sh` |
| 设置Windows依赖 | `set VAR=C:\path && build.bat` |
| 清理构建 | `dotnet clean com.mirror.steamworks.net/` |

