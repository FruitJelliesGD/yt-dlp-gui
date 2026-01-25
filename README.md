# yt-dlp-gui (WPF)

一个基于 **WPF (.NET 10)** 的 `yt-dlp` 图形界面工具，专注于  
**简洁、稳定** 的下载体验。

> 🎯 目标：开箱即用的 yt-dlp GUI

---

## ✨ 特性

- ✅ 基于 **yt-dlp**，支持 YouTube 、B站及大量视频网站
- 📋 **任务列表视图**
  - URL / 状态 / 下载速度 / 进度条
  - 进度百分比实时显示
- 🎞 **格式查看**
  - 一键查看可下载格式（`yt-dlp -F`）
- 🍪 **Cookies 支持**
  - 支持导入浏览器 Cookies（会员 / 登录视频）
- 📁 **路径选择**
  - 图形化选择保存目录
- 🧩 纯 WPF 原生实现，无 WebView / 无 Electron

---

## 🖥 系统要求

- Windows 10 / 11
- .NET 10 Runtime
- 已安装 `yt-dlp`（或放置在程序同目录）

---

## 🚀 使用方法

1. 下载或自行编译本项目
2. 确保 `yt-dlp.exe` 可被程序调用  
   （推荐：放在 exe 同目录）
3. 启动程序
4. 粘贴视频 URL
5. 选择保存路径
6. （可选）查看格式 / 填写 Format ID
7. 点击 **开始下载**

---

## 📸 界面预览

> 深色模式 + 原生控件 + 清晰进度展示

<img width="1762" height="1091" alt="image" src="https://github.com/user-attachments/assets/1bacfcc3-30f7-4705-bd55-841b0382be4e" />


---

## 🛠 技术栈

- UI：WPF (XAML)
- 运行时：.NET 10
- 外部工具：yt-dlp
- 系统集成：
  - `DwmSetWindowAttribute`
  - Windows Immersive Dark Mode

---
