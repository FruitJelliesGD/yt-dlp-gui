using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace yt_dlp_gui
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public class DownloadTask : INotifyPropertyChanged
        {
            public string Url { get; set; }
            public string Format { get; set; }
            public string SavePath { get; set; }
            public string CookiesPath { get; set; }

            private string _status = "等待中";
            public string Status
            {
                get => _status;
                set { _status = value; OnPropertyChanged(nameof(Status)); }
            }

            private double _progress;
            public double Progress
            {
                get => _progress;
                set { _progress = value; OnPropertyChanged(nameof(Progress)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            private string _speed = "--";
            public string Speed
            {
                get => _speed;
                set { _speed = value; OnPropertyChanged(nameof(Speed)); }
            }

            private string _eta = "--";
            public string ETA
            {
                get => _eta;
                set { _eta = value; OnPropertyChanged(nameof(ETA)); }
            }

            private bool _isPaused;
            public bool IsPaused
            {
                get => _isPaused;
                set { _isPaused = value; OnPropertyChanged(nameof(IsPaused)); }
            }

            private bool _isDownloading;
            public bool IsDownloading
            {
                get => _isDownloading;
                set { _isDownloading = value; OnPropertyChanged(nameof(IsDownloading)); }
            }

            private Process _process;
            public Process Process
            {
                get => _process;
                set { _process = value; }
            }

            public void Cancel()
            {
                if (_process != null && !_process.HasExited)
                {
                    try
                    {
                        _process.Kill();
                    }
                    catch { }
                }
                Status = "已取消";
                IsDownloading = false;
                IsPaused = false;
            }

            public void Pause()
            {
                if (_process != null && !_process.HasExited)
                {
                    try
                    {
                        _process.Kill();
                    }
                    catch { }
                }
                IsPaused = true;
                IsDownloading = false;
                Status = "已暂停";
            }

            public override string ToString()
            {
                return Url;
            }

        }

        public ObservableCollection<DownloadTask> TaskList { get; } = new();

        private static readonly Regex ProgressRegex =
            new(@"\[download\]\s+(\d+(?:\.\d+)?)%");

        private static readonly Regex SpeedRegex =
            new(@"at\s+([\d\.]+\s*(?:KiB|MiB|GiB)/s)");


        private readonly ConcurrentQueue<DownloadTask> _taskQueue = new();
        private readonly SemaphoreSlim _semaphore = new(2); // 同时下载 2 个
        private bool _isProcessing = false;
        public MainWindow()
        {
            InitializeComponent();
            TaskListView.ItemsSource = TaskList;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnableDarkTitleBar(this);
        }

        // ================= 深色标题栏 =================

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);

        private static void EnableDarkTitleBar(Window window)
        {
            if (Environment.OSVersion.Version.Major < 10)
                return;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                return;

            int useDarkMode = 1;
            DwmSetWindowAttribute(
                hwnd,
                DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref useDarkMode,
                sizeof(int));
        }
    
        // --- 所有方法都必须写在 MainWindow 这个大括号内部 ---

        private void SelectPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                PathTextBox.Text = dialog.FolderName;
            }
        }

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

        private void DownloadVideo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UrlTextBox.Text))
            {
                MessageBox.Show("请输入 URL");
                return;
            }

            var task = new DownloadTask
            {
                Url = UrlTextBox.Text.Trim(),
                Format = string.IsNullOrWhiteSpace(FormatSelectorControl.FormatId)
        ? "bv*+ba/b"
        : FormatSelectorControl.FormatId.Trim(),
                SavePath = PathTextBox.Text.Trim(),
                CookiesPath = CookiesPathTextBox.Text.Trim(),
            };

            TaskList.Add(task);
            _taskQueue.Enqueue(task);

            StartQueueProcessor();

        }

        private void StartQueueProcessor()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            Task.Run(async () =>
            {
                while (_taskQueue.TryDequeue(out var task))
                {
                    await _semaphore.WaitAsync();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await RunDownloadTask(task);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    });
                }

                _isProcessing = false;
            });
        }

        private async Task RunDownloadTask(DownloadTask task)
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    task.Status = "下载中";
                    task.Progress = 0;
                    task.IsDownloading = true;
                    task.IsPaused = false;
                });

                string args = $"-f \"{task.Format}\" \"{task.Url}\" -P \"{task.SavePath}\" --continue";

                if (!string.IsNullOrWhiteSpace(task.CookiesPath))
                    args += $" --cookies \"{task.CookiesPath}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                var process = Process.Start(psi);
                task.Process = process;

                var startTime = DateTime.Now;

                while (!process.StandardOutput.EndOfStream)
                {
                    if (task.IsPaused)
                    {
                        break;
                    }

                    var line = process.StandardOutput.ReadLine();

                    var progressMatch = ProgressRegex.Match(line);
                    var speedMatch = SpeedRegex.Match(line);

                    Dispatcher.Invoke(() =>
                    {
                        if (progressMatch.Success)
                        {
                            task.Progress = double.Parse(progressMatch.Groups[1].Value);

                            // Calculate ETA
                            if (task.Progress > 0 && task.Progress < 100)
                            {
                                var elapsed = DateTime.Now - startTime;
                                var totalSeconds = elapsed.TotalSeconds * 100 / task.Progress;
                                var remaining = TimeSpan.FromSeconds(totalSeconds - elapsed.TotalSeconds);
                                task.ETA = remaining.TotalHours >= 1
                                    ? $"{(int)remaining.TotalHours}h {remaining.Minutes}m {remaining.Seconds}s"
                                    : remaining.TotalMinutes >= 1
                                        ? $"{(int)remaining.TotalMinutes}m {remaining.Seconds}s"
                                        : $"{remaining.Seconds}s";
                            }
                            else if (task.Progress >= 100)
                            {
                                task.ETA = "0s";
                            }
                        }

                        if (speedMatch.Success)
                        {
                            task.Speed = speedMatch.Groups[1].Value;
                        }
                    });
                }

                if (!task.IsPaused)
                {
                    process.WaitForExit();

                    Dispatcher.Invoke(() =>
                    {
                        task.Progress = process.ExitCode == 0 ? 100 : task.Progress;
                        task.Status = process.ExitCode == 0 ? "完成" : "失败";
                        task.Speed = "--";
                        task.ETA = "--";
                        task.IsDownloading = false;
                    });
                }
                else
                {
                    process.Dispose();
                }
            });
        }

        private void SelectCookies_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"; // 过滤只看txt
            if (dialog.ShowDialog() == true)
            {
                CookiesPathTextBox.Text = dialog.FileName;
            }
        }

        // 在 CheckFormats_Click 和 DownloadVideo_Click 中都要用到这个逻辑
        private string GetCommonArgs()
        {
            string args = "";

            if (!string.IsNullOrWhiteSpace(CookiesPathTextBox.Text))
            {
                args += $" --cookies \"{CookiesPathTextBox.Text}\"";
            }

            return args;
        }

        private void ResumeDownload(DownloadTask task)
        {
            if (task.IsPaused)
            {
                _taskQueue.Enqueue(task);
                StartQueueProcessor();
            }
        }

        private void Url_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DownloadTask task)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = task.Url,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DownloadTask task)
            {
                if (task.IsPaused)
                {
                    ResumeDownload(task);
                }
                else if (task.IsDownloading)
                {
                    task.Pause();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DownloadTask task)
            {
                task.Cancel();
            }
        }

    } // 这是 MainWindow 类的结尾
} // 这是命名空间的结尾