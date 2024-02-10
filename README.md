# EasyControl
**通过C#实现的一个简单的远程控制程序**
<p>
  <img src="https://github.com/Mangofang/EasyControl/blob/master/image/QQ%E6%88%AA%E5%9B%BE20240210152601.png">
</p>

纯萌新，屎山代码，欢迎修改指正

目前功能有限，且仅能支持 `1对1` 远控 `无法群控`

**程序带有编译cs文件被控端功能，但推荐自行编译被控端程序**

**如果你不知道怎么做，不推荐使用该程序**

## 声明：
1. 文中所涉及的技术、思路和工具仅供以安全为目的的学习交流使用，任何人不得将其用于非法用途以及盈利等目的，否则后果自行承担！

## 使用指南

1. 修改EasyControlClient.cs中反连的IP和端口
2. 运行EasyControl.exe
3. 修改 `icon.ico` 图标文件（可选）
4. 键入 `generate [outputname] [x86/x64]` 编译被控端代码 或 `自行编译`
5. 键入 `listen [ip] [port]` 启动对ip和端口的监听
6. 启动 `被控端程序`
7. 建立连接

**注意：家用无公网ip网络需要使用网络穿透或使用云服务器，否则被控端无法正常反连**

## 更新
2024年02月10日
  1. 公开仓库

## 可能的更新
1. 目标键盘记录
2. 目标语音记录
3. 获取摄像头
4. Chrome存储数据获取
5. 群控支持
6. 代码优化

<p align="center"">
  <img src="https://github.com/Mangofang/EasyControl/blob/master/image/cc9272d3-9afc-47cf-a2e4-75b05e141324.gif">
</p>
