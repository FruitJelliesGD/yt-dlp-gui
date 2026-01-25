using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace yt_dlp_gui
{
    /// <summary>
    /// Formats.xaml 的交互逻辑
    /// </summary>
    public partial class Formats : Window
    {
        public Formats(string text)
        {
            InitializeComponent();
            OutputTextBox.Text = text;
            Loaded += Formats_Loaded;
        }

        private void Formats_Loaded(object sender, RoutedEventArgs e)
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
    }
}
