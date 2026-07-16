# UI现代化重设计实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 采用VS Code风格重新设计UI，统一所有视觉元素

**Architecture:** 通过更新全局资源和样式，统一所有组件的视觉风格

**Tech Stack:** WPF, XAML, C#

## Global Constraints

- 目标框架：net10.0-windows
- UI框架：WPF (XAML)
- 无第三方UI库
- 只使用深色模式

---

### Task 1: 更新全局颜色资源

**Covers:** [S3]

**Files:**
- Modify: `MainWindow.xaml:14-39`

**Interfaces:**
- Consumes: 无
- Produces: 更新后的颜色资源

- [ ] **Step 1: 更新颜色资源**

```xml
<!-- 文本 -->
<SolidColorBrush x:Key="TextBrush" Color="#CCCCCC"/>
<SolidColorBrush x:Key="SubTextBrush" Color="#858585"/>
<SolidColorBrush x:Key="LinkBrush" Color="#3794FF"/>

<!-- 输入框 -->
<SolidColorBrush x:Key="InputBg" Color="#3C3C3C"/>
<SolidColorBrush x:Key="InputBorder" Color="#555555"/>
<SolidColorBrush x:Key="InputBorderHover" Color="#6E6E6E"/>

<!-- 按钮 -->
<SolidColorBrush x:Key="BtnBg" Color="#0E639C"/>
<SolidColorBrush x:Key="BtnHover" Color="#1177BB"/>
<SolidColorBrush x:Key="BtnPressed" Color="#094771"/>
<SolidColorBrush x:Key="BtnBorder" Color="#007ACC"/>
<SolidColorBrush x:Key="BtnSecondaryBg" Color="#3C3C3C"/>
<SolidColorBrush x:Key="BtnSecondaryHover" Color="#4A4A4A"/>

<!-- ListView -->
<SolidColorBrush x:Key="ListBg" Color="#252526"/>
<SolidColorBrush x:Key="ListBorder" Color="#3C3C3C"/>
<SolidColorBrush x:Key="ListItemHover" Color="#2A2D2E"/>
<SolidColorBrush x:Key="ListItemSelected" Color="#094771"/>

<!-- 表头 -->
<SolidColorBrush x:Key="HeaderBg" Color="#252526"/>
<SolidColorBrush x:Key="HeaderBorder" Color="#3C3C3C"/>
```

- [ ] **Step 2: 提交代码**

```bash
git add MainWindow.xaml
git commit -m "feat: update global color resources for VS Code style"
```

---

### Task 2: 更新全局样式

**Covers:** [S3]

**Files:**
- Modify: `MainWindow.xaml:41-131`

**Interfaces:**
- Consumes: 颜色资源
- Produces: 更新后的全局样式

- [ ] **Step 1: 更新TextBlock样式**

```xml
<!-- TextBlock -->
<Style TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
    <Setter Property="FontSize" Value="13"/>
</Style>
```

- [ ] **Step 2: 更新TextBox样式**

```xml
<!-- TextBox -->
<Style TargetType="TextBox">
    <Setter Property="Background" Value="{StaticResource InputBg}"/>
    <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource InputBorder}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="8,6"/>
    <Setter Property="Height" Value="32"/>
    <Setter Property="FontSize" Value="13"/>
    <Setter Property="VerticalContentAlignment" Value="Center"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="BorderBrush" Value="{StaticResource InputBorderHover}"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

- [ ] **Step 3: 更新Button样式**

```xml
<!-- Button -->
<Style TargetType="Button">
    <Setter Property="Background" Value="{StaticResource BtnBg}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="BorderBrush" Value="{StaticResource BtnBorder}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Height" Value="32"/>
    <Setter Property="Padding" Value="12,0"/>
    <Setter Property="FontSize" Value="13"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="Bd"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4">
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="Bd" Property="Background" Value="{StaticResource BtnHover}"/>
                    </Trigger>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="Bd" Property="Background" Value="{StaticResource BtnPressed}"/>
                    </Trigger>
                    <Trigger Property="IsEnabled" Value="False">
                        <Setter TargetName="Bd" Property="Opacity" Value="0.5"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

- [ ] **Step 4: 更新GridViewColumnHeader样式**

```xml
<!-- GridView 表头 -->
<Style TargetType="GridViewColumnHeader">
    <Setter Property="Background" Value="{StaticResource HeaderBg}"/>
    <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource HeaderBorder}"/>
    <Setter Property="BorderThickness" Value="0,0,1,1"/>
    <Setter Property="Padding" Value="8,6"/>
    <Setter Property="FontSize" Value="13"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
</Style>
```

- [ ] **Step 5: 更新ListViewItem样式**

```xml
<!-- ListViewItem -->
<Style TargetType="ListViewItem">
    <Setter Property="Foreground" Value="{StaticResource TextBrush}"/>
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="Padding" Value="4,2"/>
    <Setter Property="Height" Value="36"/>
    <Setter Property="FontSize" Value="13"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ListViewItem">
                <Border x:Name="Bd"
                        Background="{TemplateBinding Background}">
                    <GridViewRowPresenter
                        Columns="{Binding Path=View.Columns,
                                  RelativeSource={RelativeSource AncestorType=ListView}}"
                        Content="{TemplateBinding Content}"
                        Margin="4,0"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="Bd" Property="Background" Value="{StaticResource ListItemHover}"/>
                    </Trigger>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter TargetName="Bd" Property="Background" Value="{StaticResource ListItemSelected}"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

- [ ] **Step 6: 提交代码**

```bash
git add MainWindow.xaml
git commit -m "feat: update global styles for VS Code style"
```

---

### Task 3: 更新MainWindow布局

**Covers:** [S4]

**Files:**
- Modify: `MainWindow.xaml:134-374`

**Interfaces:**
- Consumes: 全局样式
- Produces: 更新后的布局

- [ ] **Step 1: 更新主窗口属性**

```xml
<Window x:Class="yt_dlp_gui.MainWindow"
        ...
        Title="yt-dlp-gui"
        Width="1000"
        Height="600"
        Background="#1E1E1E"
        Foreground="{StaticResource TextBrush}">
```

- [ ] **Step 2: 更新主布局Grid**

```xml
<Grid Margin="16">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
```

- [ ] **Step 3: 更新URL区域**

```xml
<!-- URL -->
<StackPanel Grid.Row="0" Margin="0,0,0,12">
    <TextBlock Text="视频链接 (URL)" FontWeight="Bold" Margin="0,0,0,6"/>
    <TextBox x:Name="UrlTextBox"/>
</StackPanel>
```

- [ ] **Step 4: 更新保存路径区域**

```xml
<!-- 保存路径 -->
<Grid Grid.Row="1" Margin="0,0,0,12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>

    <TextBlock Text="保存路径" VerticalAlignment="Center" FontWeight="Bold" Margin="0,0,8,0"/>
    <TextBox Grid.Column="1"
             x:Name="PathTextBox"
             IsReadOnly="True"/>
    <Button Grid.Column="2"
            Content="选择文件夹"
            Margin="8,0,0,0"
            Click="SelectPath_Click"/>
</Grid>
```

- [ ] **Step 5: 更新格式和Cookies区域**

```xml
<!-- 格式 + Cookies -->
<Grid Grid.Row="2" Margin="0,0,0,12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <local:FormatSelectorControl x:Name="FormatSelectorControl"
                                 Height="80"/>

    <StackPanel Grid.Column="1" Orientation="Horizontal" Margin="12,0,0,0">
        <TextBlock Text="Cookies" VerticalAlignment="Center" FontWeight="Bold" Margin="0,0,8,0"/>
        <TextBox x:Name="CookiesPathTextBox"
                 Width="200"
                 IsReadOnly="True"/>
        <Button Content="选择文件"
                Margin="8,0,0,0"
                Click="SelectCookies_Click"/>
    </StackPanel>
</Grid>
```

- [ ] **Step 6: 更新操作按钮区域**

```xml
<!-- 操作按钮 -->
<Grid Grid.Row="3" Margin="0,0,0,12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>

    <Button Content="历史记录"
            Margin="0,0,8,0"
            Click="History_Click">
        <Button.ContextMenu>
            <ContextMenu x:Name="HistoryContextMenu"/>
        </Button.ContextMenu>
    </Button>

    <Button Grid.Column="1"
            Content="查看可用格式 (-F)"
            Margin="0,0,8,0"
            Click="CheckFormats_Click"/>

    <Button Grid.Column="2"
            Content="高级选项"
            Margin="0,0,8,0"
            Click="ToggleAdvancedOptions_Click"/>

    <Button Grid.Column="3"
            Content="开始下载"
            Background="{StaticResource BtnBg}"
            Foreground="White"
            Click="DownloadVideo_Click"/>
</Grid>
```

- [ ] **Step 7: 更新状态栏**

```xml
<!-- 更新状态栏 -->
<Grid Grid.Row="6" Margin="0,8,0,0">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>

    <TextBlock x:Name="UpdateStatusText"
               Text="当前版本: 0.0.0"
               VerticalAlignment="Center"
               Foreground="{StaticResource SubTextBrush}"/>

    <StackPanel Grid.Column="1" Orientation="Horizontal">
        <Button x:Name="CheckUpdateButton"
                Content="检查更新"
                Padding="12,0"
                Click="CheckUpdate_Click"/>
        <ProgressBar x:Name="UpdateProgressBar"
                     Width="120"
                     Height="8"
                     Margin="8,0,0,0"
                     VerticalAlignment="Center"
                     Minimum="0"
                     Maximum="100"
                     Visibility="Collapsed"/>
    </StackPanel>
</Grid>
```

- [ ] **Step 8: 提交代码**

```bash
git add MainWindow.xaml
git commit -m "feat: update MainWindow layout for VS Code style"
```

---

### Task 4: 更新FormatSelectorControl

**Covers:** [S5]

**Files:**
- Modify: `Controls/FormatSelectorControl.xaml`
- Modify: `Controls/FormatSelectorControl.xaml.cs`

**Interfaces:**
- Consumes: 全局样式
- Produces: 更新后的FormatSelectorControl

- [ ] **Step 1: 更新XAML**

```xml
<UserControl x:Class="yt_dlp_gui.Controls.FormatSelectorControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d"
             d:DesignHeight="100" d:DesignWidth="400"
             Background="Transparent">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- 格式ID输入框 -->
        <Grid Grid.Row="0" Margin="0,0,0,6">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <TextBox x:Name="FormatIdTextBox"
                     Text="bv*+ba/b"/>

            <Button x:Name="DropdownButton"
                    Grid.Column="1"
                    Width="32"
                    Margin="4,0,0,0"
                    Content="▼"
                    Click="DropdownButton_Click"/>
        </Grid>

        <!-- 快速选择按钮 -->
        <WrapPanel Grid.Row="1" Margin="0,0,0,6">
            <Button Content="最佳画质"
                    Margin="0,0,4,0"
                    Height="28"
                    Padding="8,0"
                    Click="QuickSelect_Click"
                    Tag="best"/>
            <Button Content="1080p"
                    Margin="0,0,4,0"
                    Height="28"
                    Padding="8,0"
                    Click="QuickSelect_Click"
                    Tag="1080p"/>
            <Button Content="720p"
                    Margin="0,0,4,0"
                    Height="28"
                    Padding="8,0"
                    Click="QuickSelect_Click"
                    Tag="720p"/>
            <Button Content="仅音频"
                    Margin="0,0,4,0"
                    Height="28"
                    Padding="8,0"
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
               Width="450">

            <Border Background="#252526"
                    BorderBrush="#3C3C3C"
                    BorderThickness="1"
                    CornerRadius="4">

                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>

                    <!-- 视频格式选择 -->
                    <StackPanel Grid.Row="0" Margin="8,8,8,4">
                        <TextBlock Text="视频格式" FontWeight="Bold" Margin="0,0,0,4"/>
                        <ListBox x:Name="VideoFormatListBox"
                                 MaxHeight="120"
                                 Background="Transparent"
                                 BorderThickness="0"
                                 SelectionChanged="VideoFormatListBox_SelectionChanged">
                            <ListBox.ItemTemplate>
                                <DataTemplate>
                                    <Grid Margin="4">
                                        <Grid.ColumnDefinitions>
                                            <ColumnDefinition Width="*"/>
                                            <ColumnDefinition Width="Auto"/>
                                        </Grid.ColumnDefinitions>
                                        <StackPanel Grid.Column="0">
                                            <TextBlock Text="{Binding Resolution}"/>
                                            <TextBlock Text="{Binding DisplayText}" FontSize="11" Foreground="{StaticResource SubTextBrush}"/>
                                        </StackPanel>
                                        <TextBlock Grid.Column="1" Text="{Binding FormatId}" VerticalAlignment="Center"/>
                                    </Grid>
                                </DataTemplate>
                            </ListBox.ItemTemplate>
                        </ListBox>
                    </StackPanel>

                    <!-- 音频格式选择 -->
                    <StackPanel Grid.Row="1" Margin="8,4,8,4">
                        <TextBlock Text="音频格式" FontWeight="Bold" Margin="0,0,0,4"/>
                        <ListBox x:Name="AudioFormatListBox"
                                 MaxHeight="80"
                                 Background="Transparent"
                                 BorderThickness="0"
                                 SelectionChanged="AudioFormatListBox_SelectionChanged">
                            <ListBox.ItemTemplate>
                                <DataTemplate>
                                    <Grid Margin="4">
                                        <Grid.ColumnDefinitions>
                                            <ColumnDefinition Width="*"/>
                                            <ColumnDefinition Width="Auto"/>
                                        </Grid.ColumnDefinitions>
                                        <StackPanel Grid.Column="0">
                                            <TextBlock Text="{Binding DisplayText}"/>
                                        </StackPanel>
                                        <TextBlock Grid.Column="1" Text="{Binding FormatId}" VerticalAlignment="Center"/>
                                    </Grid>
                                </DataTemplate>
                            </ListBox.ItemTemplate>
                        </ListBox>
                    </StackPanel>

                    <!-- 组合预览 -->
                    <StackPanel Grid.Row="2" Margin="8,4,8,8" Orientation="Horizontal">
                        <TextBlock Text="组合: " Foreground="{StaticResource SubTextBrush}"/>
                        <TextBlock x:Name="CombinedFormatText"
                                   Text="选择视频和音频格式"
                                   Foreground="{StaticResource LinkBrush}"
                                   FontWeight="Bold"/>
                    </StackPanel>
                </Grid>
            </Border>
        </Popup>
    </Grid>
</UserControl>
```

- [ ] **Step 2: 提交代码**

```bash
git add Controls/FormatSelectorControl.xaml
git commit -m "feat: update FormatSelectorControl for VS Code style"
```

---

### Task 5: 更新AdvancedOptionsPanel

**Covers:** [S5]

**Files:**
- Modify: `Controls/AdvancedOptionsPanel.xaml`

**Interfaces:**
- Consumes: 全局样式
- Produces: 更新后的AdvancedOptionsPanel

- [ ] **Step 1: 更新AdvancedOptionsPanel**

（由于AdvancedOptionsPanel内容较多，这里只列出关键更新）

```xml
<!-- 更新所有TextBox高度为32px -->
<TextBox ... Height="32"/>

<!-- 更新所有按钮高度为32px -->
<Button ... Height="32"/>

<!-- 更新所有标签字体大小为13px -->
<TextBlock ... FontSize="13"/>

<!-- 更新选项卡样式 -->
<TabControl ...>
    <TabItem Header="下载" Background="#252526" Foreground="{StaticResource TextBrush}">
        ...
    </TabItem>
    ...
</TabControl>
```

- [ ] **Step 2: 提交代码**

```bash
git add Controls/AdvancedOptionsPanel.xaml
git commit -m "feat: update AdvancedOptionsPanel for VS Code style"
```

---

### Task 6: 测试和调整

**Covers:** [S6]

**Files:**
- Test: 运行应用测试

**Interfaces:**
- Consumes: 所有更新后的组件
- Produces: 最终测试结果

- [ ] **Step 1: 运行应用测试**

```bash
dotnet run
```

- [ ] **Step 2: 检查以下项目**
- 输入框高度是否统一为32px
- 按钮高度是否统一为32px
- 字体大小是否统一为13px
- 颜色是否统一
- 间距是否统一

- [ ] **Step 3: 提交最终代码**

```bash
git add -A
git commit -m "feat: complete UI redesign for VS Code style"
```
