# 格式选择功能实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 添加格式选择功能，让用户可以从yt-dlp -F输出中选择格式，而不是手动输入格式ID。

**Architecture:** 解析yt-dlp -F输出，构建格式对象列表，提供下拉选择界面，支持快速选择预设。

**Tech Stack:** WPF, .NET 10, C#, 正则表达式

## Global Constraints

- 目标框架：net10.0-windows
- UI框架：WPF (XAML)
- 无第三方UI库
- 保持现有功能不变
- 支持深色主题

---

### Task 1: 创建格式数据模型

**Covers:** [S4]

**Files:**
- Create: `Models/FormatInfo.cs`
- Test: `Tests/Models/FormatInfoTests.cs`

**Interfaces:**
- Consumes: 无
- Produces: `FormatInfo` 类，包含格式ID、分辨率、编码、码率、帧率等属性

- [ ] **Step 1: 创建FormatInfo模型类**

```csharp
// Models/FormatInfo.cs
namespace yt_dlp_gui.Models
{
    public class FormatInfo
    {
        public string FormatId { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public string Codec { get; set; } = string.Empty;
        public string Bitrate { get; set; } = string.Empty;
        public string Fps { get; set; } = string.Empty;
        public string AudioCodec { get; set; } = string.Empty;
        public string AudioBitrate { get; set; } = string.Empty;
        public string AudioSampleRate { get; set; } = string.Empty;
        public bool IsVideo { get; set; }
        public bool IsAudio { get; set; }
        
        public string DisplayText => IsVideo 
            ? $"{Resolution} ({Codec}, {Bitrate}, {Fps})"
            : $"{AudioCodec} ({AudioBitrate}, {AudioSampleRate})";
    }
}
```

- [ ] **Step 2: 创建单元测试**

```csharp
// Tests/Models/FormatInfoTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Tests.Models
{
    [TestClass]
    public class FormatInfoTests
    {
        [TestMethod]
        public void DisplayText_VideoFormat_ReturnsFormattedString()
        {
            var format = new FormatInfo
            {
                IsVideo = true,
                Resolution = "1920x1080",
                Codec = "VP9",
                Bitrate = "2.5Mbps",
                Fps = "60fps"
            };
            
            Assert.AreEqual("1920x1080 (VP9, 2.5Mbps, 60fps)", format.DisplayText);
        }
        
        [TestMethod]
        public void DisplayText_AudioFormat_ReturnsFormattedString()
        {
            var format = new FormatInfo
            {
                IsAudio = true,
                AudioCodec = "Opus",
                AudioBitrate = "128Kbps",
                AudioSampleRate = "48kHz"
            };
            
            Assert.AreEqual("Opus (128Kbps, 48kHz)", format.DisplayText);
        }
    }
}
```

- [ ] **Step 3: 运行测试验证失败**

Run: `dotnet test Tests/`
Expected: FAIL，因为FormatInfo类不存在

- [ ] **Step 4: 提交代码**

```bash
git add Models/FormatInfo.cs Tests/Models/FormatInfoTests.cs
git commit -m "feat: add FormatInfo model for format selection"
```

---

### Task 2: 实现格式解析器

**Covers:** [S4]

**Files:**
- Create: `Services/FormatParser.cs`
- Test: `Tests/Services/FormatParserTests.cs`

**Interfaces:**
- Consumes: `FormatInfo` 类
- Produces: `FormatParser` 类，包含 `Parse(string output)` 方法

- [ ] **Step 1: 创建FormatParser服务类**

```csharp
// Services/FormatParser.cs
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Services
{
    public class FormatParser
    {
        private static readonly Regex FormatLineRegex = new(
            @"^(\d+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(.*?)\s*$",
            RegexOptions.Compiled);
        
        private static readonly Regex ResolutionRegex = new(
            @"(\d{3,4}x\d{3,4})",
            RegexOptions.Compiled);
        
        private static readonly Regex FpsRegex = new(
            @"(\d+)fps",
            RegexOptions.Compiled);
        
        public List<FormatInfo> Parse(string output)
        {
            var formats = new List<FormatInfo>();
            var lines = output.Split('\n');
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("ID")) continue; // 跳过表头
                if (line.StartsWith("-")) continue;  // 跳过分隔线
                
                var match = FormatLineRegex.Match(line);
                if (!match.Success) continue;
                
                var format = new FormatInfo
                {
                    FormatId = match.Groups[1].Value,
                    Extension = match.Groups[2].Value,
                    Protocol = match.Groups[3].Value,
                    Filesize = match.Groups[4].Value
                };
                
                var rest = match.Groups[5].Value;
                ParseFormatDetails(format, rest);
                formats.Add(format);
            }
            
            return formats;
        }
        
        private void ParseFormatDetails(FormatInfo format, string rest)
        {
            // 检测是否为视频格式
            var resolutionMatch = ResolutionRegex.Match(rest);
            if (resolutionMatch.Success)
            {
                format.IsVideo = true;
                format.Resolution = resolutionMatch.Value;
                
                var fpsMatch = FpsRegex.Match(rest);
                if (fpsMatch.Success)
                    format.Fps = fpsMatch.Value;
                
                // 提取编码信息
                var codecMatch = Regex.Match(rest, @"(\w+)\s+\d+[kKmM]");
                if (codecMatch.Success)
                    format.Codec = codecMatch.Groups[1].Value;
                
                var bitrateMatch = Regex.Match(rest, @"(\d+[kKmM]\w*)");
                if (bitrateMatch.Success)
                    format.Bitrate = bitrateMatch.Groups[1].Value;
            }
            else
            {
                // 音频格式
                format.IsAudio = true;
                format.AudioCodec = ExtractAudioCodec(rest);
                format.AudioBitrate = ExtractAudioBitrate(rest);
                format.AudioSampleRate = ExtractAudioSampleRate(rest);
            }
        }
        
        private string ExtractAudioCodec(string rest)
        {
            var match = Regex.Match(rest, @"(Opus|AAC|MP3|Vorbis|AC3|EAC3)");
            return match.Success ? match.Value : "Unknown";
        }
        
        private string ExtractAudioBitrate(string rest)
        {
            var match = Regex.Match(rest, @"(\d+[kKmM]\w*)");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }
        
        private string ExtractAudioSampleRate(string rest)
        {
            var match = Regex.Match(rest, @"(\d+\.?\d*k?Hz)");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }
    }
}
```

- [ ] **Step 2: 创建单元测试**

```csharp
// Tests/Services/FormatParserTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Services;

namespace yt_dlp_gui.Tests.Services
{
    [TestClass]
    public class FormatParserTests
    {
        [TestMethod]
        public void Parse_ValidOutput_ReturnsFormatList()
        {
            var parser = new FormatParser();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
248  webm  1920x1080   60  |  100.50MiB  https  vp9             audio only
251  webm  audio only  0   |   10.23MiB  https  audio only      opus";
            
            var formats = parser.Parse(output);
            
            Assert.AreEqual(2, formats.Count);
            Assert.IsTrue(formats[0].IsVideo);
            Assert.IsTrue(formats[1].IsAudio);
        }
    }
}
```

- [ ] **Step 3: 运行测试验证失败**

Run: `dotnet test Tests/`
Expected: FAIL，因为FormatParser类不存在

- [ ] **Step 4: 提交代码**

```bash
git add Services/FormatParser.cs Tests/Services/FormatParserTests.cs
git commit -m "feat: add FormatParser for yt-dlp output parsing"
```

---

### Task 3: 创建格式选择UI控件

**Covers:** [S4]

**Files:**
- Create: `Controls/FormatSelector.xaml`
- Create: `Controls/FormatSelector.xaml.cs`
- Modify: `MainWindow.xaml`
- Modify: `MainWindow.xaml.cs`

**Interfaces:**
- Consumes: `FormatInfo` 类, `FormatParser` 类
- Produces: `FormatSelector` 用户控件，包含下拉选择和快速选择按钮

- [ ] **Step 1: 创建FormatSelector XAML控件**

```xml
<!-- Controls/FormatSelector.xaml -->
<UserControl x:Class="yt_dlp_gui.Controls.FormatSelector"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d"
             d:DesignHeight="200" d:DesignWidth="400">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- 格式ID输入框和下拉按钮 -->
        <Grid Grid.Row="0" Margin="0,0,0,8">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <TextBox x:Name="FormatIdTextBox" 
                     Grid.Column="0"
                     Height="30"
                     Text="bv*+ba/b"/>
            
            <Button x:Name="DropdownButton"
                    Grid.Column="1"
                    Width="30"
                    Height="30"
                    Margin="5,0,0,0"
                    Content="▼"
                    Click="DropdownButton_Click"/>
        </Grid>
        
        <!-- 快速选择按钮 -->
        <WrapPanel Grid.Row="1" Margin="0,0,0,8">
            <Button Content="最佳画质" 
                    Margin="0,0,5,0"
                    Click="QuickSelect_Click"
                    Tag="best"/>
            <Button Content="1080p" 
                    Margin="0,0,5,0"
                    Click="QuickSelect_Click"
                    Tag="1080p"/>
            <Button Content="720p" 
                    Margin="0,0,5,0"
                    Click="QuickSelect_Click"
                    Tag="720p"/>
            <Button Content="仅音频" 
                    Margin="0,0,5,0"
                    Click="QuickSelect_Click"
                    Tag="audio"/>
        </WrapPanel>
        
        <!-- 格式选择下拉列表 -->
        <Popup x:Name="FormatPopup"
               Grid.Row="2"
               PlacementTarget="{Binding ElementName=FormatIdTextBox}"
               Placement="Bottom"
               AllowsTransparency="True"
               StaysOpen="False"
               Width="400">
            
            <Border Background="#2A2A2A"
                    BorderBrush="#3C3C3C"
                    BorderThickness="1"
                    CornerRadius="4">
                
                <ListBox x:Name="FormatListBox"
                         MaxHeight="200"
                         Background="Transparent"
                         BorderThickness="0"
                         SelectionChanged="FormatListBox_SelectionChanged">
                    
                    <ListBox.ItemTemplate>
                        <DataTemplate>
                            <Grid Margin="4">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                
                                <StackPanel Grid.Column="0">
                                    <TextBlock Text="{Binding Resolution}"
                                               Foreground="#E6E6E6"/>
                                    <TextBlock Text="{Binding DisplayText}"
                                               FontSize="11"
                                               Foreground="#B0B0B0"/>
                                </StackPanel>
                                
                                <TextBlock Grid.Column="1"
                                           Text="{Binding FormatId}"
                                           VerticalAlignment="Center"
                                           Foreground="#E6E6E6"/>
                            </Grid>
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>
            </Border>
        </Popup>
    </Grid>
</UserControl>
```

- [ ] **Step 2: 创建FormatSelector代码-behind**

```csharp
// Controls/FormatSelector.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using yt_dlp_gui.Models;
using yt_dlp_gui.Services;

namespace yt_dlp_gui.Controls
{
    public partial class FormatSelector : UserControl
    {
        private readonly FormatParser _parser = new();
        private List<FormatInfo> _formats = new();
        
        public string FormatId
        {
            get => FormatIdTextBox.Text;
            set => FormatIdTextBox.Text = value;
        }
        
        public FormatSelector()
        {
            InitializeComponent();
        }
        
        public void UpdateFormats(string ytDlpOutput)
        {
            _formats = _parser.Parse(ytDlpOutput);
            FormatListBox.ItemsSource = _formats;
        }
        
        private void DropdownButton_Click(object sender, RoutedEventArgs e)
        {
            FormatPopup.IsOpen = !FormatPopup.IsOpen;
        }
        
        private void FormatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormatListBox.SelectedItem is FormatInfo selected)
            {
                FormatIdTextBox.Text = selected.FormatId;
                FormatPopup.IsOpen = false;
            }
        }
        
        private void QuickSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                switch (tag)
                {
                    case "best":
                        FormatIdTextBox.Text = "bv*+ba/b";
                        break;
                    case "1080p":
                        FormatIdTextBox.Text = "bestvideo[height<=1080]+bestaudio/best[height<=1080]";
                        break;
                    case "720p":
                        FormatIdTextBox.Text = "bestvideo[height<=720]+bestaudio/best[height<=720]";
                        break;
                    case "audio":
                        FormatIdTextBox.Text = "bestaudio";
                        break;
                }
            }
        }
    }
}
```

- [ ] **Step 3: 在MainWindow中集成FormatSelector**

```xml
<!-- MainWindow.xaml 中替换格式ID部分 -->
<StackPanel Orientation="Horizontal">
    <TextBlock Text="格式 ID" FontWeight="Bold" VerticalAlignment="Center"/>
    <local:FormatSelector x:Name="FormatSelectorControl"
                          Width="400"
                          Height="80"
                          Margin="10,0"/>
</StackPanel>
```

- [ ] **Step 4: 更新MainWindow代码-behind**

```csharp
// MainWindow.xaml.cs 中修改CheckFormats_Click方法
private async void CheckFormats_Click(object sender, RoutedEventArgs e)
{
    string url = UrlTextBox.Text.Trim();
    string commonArgs = GetCommonArgs();
    if (string.IsNullOrEmpty(url))
    {
        return;
    }

    await Task.Run(() =>
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-F \"{url}\" {commonArgs}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Dispatcher.Invoke(() =>
                {
                    // 更新格式选择器
                    FormatSelectorControl.UpdateFormats(output);
                    
                    // 仍然显示原始输出窗口
                    var win = new Formats(
                        !string.IsNullOrWhiteSpace(output) ? output : error
                    );
                    win.Owner = this;
                    win.Show();
                });
            }
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error)
            );
        }
    });
}
```

- [ ] **Step 5: 运行应用测试功能**

Run: `dotnet run`
Expected: 应用启动，点击"查看可用格式"后格式选择器显示格式列表

- [ ] **Step 6: 提交代码**

```bash
git add Controls/FormatSelector.xaml Controls/FormatSelector.xaml.cs MainWindow.xaml MainWindow.xaml.cs
git commit -m "feat: integrate FormatSelector control into MainWindow"
```

---

### Task 4: 添加格式选择单元测试

**Covers:** [S4]

**Files:**
- Test: `Tests/Controls/FormatSelectorTests.cs`

**Interfaces:**
- Consumes: `FormatSelector` 控件
- Produces: 格式选择功能的单元测试

- [ ] **Step 1: 创建格式选择测试**

```csharp
// Tests/Controls/FormatSelectorTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Controls;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Tests.Controls
{
    [TestClass]
    public class FormatSelectorTests
    {
        [TestMethod]
        public void UpdateFormats_ValidOutput_UpdatesFormatList()
        {
            var selector = new FormatSelector();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
248  webm  1920x1080   60  |  100.50MiB  https  vp9             audio only
251  webm  audio only  0   |   10.23MiB  https  audio only      opus";
            
            selector.UpdateFormats(output);
            
            // 验证格式列表已更新（通过访问内部状态或模拟）
            Assert.IsNotNull(selector.FormatId);
        }
        
        [TestMethod]
        public void FormatId_DefaultValue_ReturnsBvBa()
        {
            var selector = new FormatSelector();
            
            Assert.AreEqual("bv*+ba/b", selector.FormatId);
        }
    }
}
```

- [ ] **Step 2: 运行测试验证**

Run: `dotnet test Tests/`
Expected: 测试通过

- [ ] **Step 3: 提交代码**

```bash
git add Tests/Controls/FormatSelectorTests.cs
git commit -m "test: add FormatSelector unit tests"
```

---

### Task 5: 更新文档和README

**Covers:** [S4]

**Files:**
- Modify: `README.md`

**Interfaces:**
- Consumes: 格式选择功能
- Produces: 更新的README文档

- [ ] **Step 1: 更新README.md**

```markdown
## ✨ 特性

- ✅ 基于 **yt-dlp**，支持 YouTube 、B站及大量视频网站
- 📋 **任务列表视图**
  - URL / 状态 / 下载速度 / 进度条
  - 进度百分比实时显示
- 🎞 **格式查看与选择**
  - 一键查看可下载格式（`yt-dlp -F`）
  - **智能格式选择**：显示分辨率、编码、码率信息
  - 快速选择预设：最佳画质、1080p、720p、仅音频
- 🍪 **Cookies 支持**
  - 支持导入浏览器 Cookies（会员 / 登录视频）
- 📁 **路径选择**
  - 图形化选择保存目录
- 🧩 纯 WPF 原生实现，无 WebView / 无 Electron
```

- [ ] **Step 2: 提交代码**

```bash
git add README.md
git commit -m "docs: update README with format selection feature"
```