using System.Text;
using System.Windows;
using System.Windows.Controls;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Controls
{
    public partial class AdvancedOptionsPanel : UserControl
    {
        public AdvancedOptionsPanel()
        {
            InitializeComponent();
        }

        // 下载选项
        public string OutputTemplate => OutputTemplateTextBox.Text;
        public string SpeedLimit => UnlimitedSpeedCheckBox.IsChecked == true ? "" : SpeedLimitTextBox.Text;
        public string Retries => RetriesTextBox.Text;
        public string FragmentThreads => FragmentThreadsTextBox.Text;
        public bool ForceReDownload => ForceReDownloadCheckBox.IsChecked == true;
        public bool IgnoreErrors => IgnoreErrorsCheckBox.IsChecked == true;

        // 网络选项
        public string Proxy => ProxyTextBox.Text;
        public string Timeout => TimeoutTextBox.Text;
        public string ConnectionLimit => ConnectionLimitTextBox.Text;
        public bool IPv6Preference => IPv6PreferenceCheckBox.IsChecked == true;

        // 字幕选项
        public string SubtitleLanguage => SubtitleLanguageTextBox.Text;
        public string SubtitleFormat => (SubtitleFormatComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "srt";
        public bool AutoTranslate => AutoTranslateCheckBox.IsChecked == true;
        public bool EmbedSubtitles => EmbedSubtitlesCheckBox.IsChecked == true;

        // 播放列表选项
        public bool DownloadPlaylist => DownloadPlaylistCheckBox.IsChecked == true;
        public string PlaylistSort
        {
            get
            {
                var selectedItem = PlaylistSortComboBox.SelectedItem as ComboBoxItem;
                return selectedItem?.Content.ToString() switch
                {
                    "升序" => "asc",
                    "降序" => "desc",
                    "随机" => "random",
                    _ => ""
                };
            }
        }
        public string PlaylistLimit => PlaylistLimitTextBox.Text;
        public string PlaylistItemLimit => PlaylistItemLimitTextBox.Text;

        // 后处理选项
        public bool ConvertFile => ConvertFileCheckBox.IsChecked == true;
        public bool DownloadThumbnail => DownloadThumbnailCheckBox.IsChecked == true;
        public bool WriteMetadata => WriteMetadataCheckBox.IsChecked == true;
        public string FilenameTemplate => FilenameTemplateTextBox.Text;

        // 认证选项
        public string Username => UsernameTextBox.Text;
        public string Password => PasswordTextBox.Password;
        public string OAuthToken => OAuthTokenTextBox.Text;
        public string TwoFactorAuth => TwoFactorAuthTextBox.Text;
        public string NetrcFilePath => NetrcFilePathTextBox.Text;

        // 自定义参数
        public string CustomArguments => CustomArgumentsTextBox.Text;

        /// <summary>
        /// 生成yt-dlp命令行参数
        /// </summary>
        public string GenerateArguments()
        {
            var args = new StringBuilder();

            // 下载选项
            if (!string.IsNullOrWhiteSpace(OutputTemplate) && OutputTemplate != "%(title)s.%(ext)s")
                args.Append($" --output \"{OutputTemplate}\"");

            if (!string.IsNullOrWhiteSpace(SpeedLimit))
                args.Append($" --limit-rate {SpeedLimit}k");

            if (!string.IsNullOrWhiteSpace(Retries) && Retries != "3")
                args.Append($" --retries {Retries}");

            if (!string.IsNullOrWhiteSpace(FragmentThreads) && FragmentThreads != "1")
                args.Append($" --concurrent-fragments {FragmentThreads}");

            if (ForceReDownload)
                args.Append(" --force-overwrites");

            if (IgnoreErrors)
                args.Append(" --ignore-errors");

            // 网络选项
            if (!string.IsNullOrWhiteSpace(Proxy))
                args.Append($" --proxy \"{Proxy}\"");

            if (!string.IsNullOrWhiteSpace(Timeout))
                args.Append($" --socket-timeout {Timeout}");

            if (!string.IsNullOrWhiteSpace(ConnectionLimit))
                args.Append($" --max-concurrent-downloads {ConnectionLimit}");

            if (IPv6Preference)
                args.Append(" -6");

            // 字幕选项
            if (!string.IsNullOrWhiteSpace(SubtitleLanguage))
                args.Append($" --sub-langs \"{SubtitleLanguage}\"");

            if (SubtitleFormat != "srt")
                args.Append($" --sub-format {SubtitleFormat}");

            if (AutoTranslate)
                args.Append(" --convert-subs srt");

            if (EmbedSubtitles)
                args.Append(" --embed-subs");

            // 播放列表选项
            if (DownloadPlaylist)
                args.Append(" --yes-playlist");

            if (!string.IsNullOrWhiteSpace(PlaylistSort))
                args.Append($" --playlist-sort {PlaylistSort}");

            if (!string.IsNullOrWhiteSpace(PlaylistLimit))
                args.Append($" --playlist-items 1:{PlaylistLimit}");

            if (!string.IsNullOrWhiteSpace(PlaylistItemLimit))
                args.Append($" --max-downloads {PlaylistItemLimit}");

            // 后处理选项
            if (ConvertFile)
                args.Append(" --merge-output-format mp4");

            if (DownloadThumbnail)
                args.Append(" --write-thumbnail --convert-thumbnails jpg");

            if (WriteMetadata)
                args.Append(" --add-metadata");

            // 文件名模板在输出模板中已经处理

            // 认证选项
            if (!string.IsNullOrWhiteSpace(Username))
                args.Append($" --username \"{Username}\"");

            if (!string.IsNullOrWhiteSpace(Password))
                args.Append($" --password \"{Password}\"");

            if (!string.IsNullOrWhiteSpace(OAuthToken))
                args.Append($" --ap-mso OAuth --ap-password \"{OAuthToken}\"");

            if (!string.IsNullOrWhiteSpace(TwoFactorAuth))
                args.Append($" --ap-password \"{TwoFactorAuth}\"");

            if (!string.IsNullOrWhiteSpace(NetrcFilePath))
                args.Append($" --netrc \"{NetrcFilePath}\"");

            // 自定义参数
            if (!string.IsNullOrWhiteSpace(CustomArguments))
            {
                string customArgs = CustomArguments.Trim();
                args.Append($" {customArgs}");
            }

            return args.ToString();
        }

        private void SelectNetrcFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "netrc files (*.netrc)|*.netrc|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                NetrcFilePathTextBox.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// 验证自定义参数语法
        /// </summary>
        public (bool IsValid, string Message) ValidateSyntax()
        {
            string input = CustomArgumentsTextBox.Text?.Trim() ?? string.Empty;
            return ArgumentsValidator.ValidateSyntax(input);
        }

        /// <summary>
        /// 清空自定义参数
        /// </summary>
        public void ClearArguments()
        {
            CustomArgumentsTextBox.Text = string.Empty;
            ValidationResultTextBlock.Text = string.Empty;
            ValidationResultTextBlock.Foreground = System.Windows.Media.Brushes.White;
        }

        private void ValidateSyntax_Click(object sender, RoutedEventArgs e)
        {
            var (isValid, message) = ValidateSyntax();
            ValidationResultTextBlock.Text = message;
            ValidationResultTextBlock.Foreground = isValid
                ? System.Windows.Media.Brushes.LightGreen
                : System.Windows.Media.Brushes.OrangeRed;
        }

        private void ClearArguments_Click(object sender, RoutedEventArgs e)
        {
            ClearArguments();
        }
    }
}