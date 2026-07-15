using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using yt_dlp_gui.Models;
using yt_dlp_gui.Services;

namespace yt_dlp_gui.Controls
{
    public partial class FormatSelectorControl : UserControl
    {
        private readonly FormatParser _parser = new();
        private List<FormatInfo> _formats = new();
        private FormatInfo? _selectedVideoFormat;
        private FormatInfo? _selectedAudioFormat;

        public string FormatId
        {
            get => FormatIdTextBox.Text;
            set => FormatIdTextBox.Text = value;
        }

        public FormatSelectorControl()
        {
            InitializeComponent();
        }

        public void UpdateFormats(string ytDlpOutput)
        {
            _formats = _parser.Parse(ytDlpOutput);

            // 分离视频和音频格式
            var videoFormats = _formats.Where(f => f.IsVideo).ToList();
            var audioFormats = _formats.Where(f => f.IsAudio).ToList();

            VideoFormatListBox.ItemsSource = videoFormats;
            AudioFormatListBox.ItemsSource = audioFormats;

            // 重置选择
            _selectedVideoFormat = null;
            _selectedAudioFormat = null;
            UpdateCombinedFormat();
        }

        private void DropdownButton_Click(object sender, RoutedEventArgs e)
        {
            FormatPopup.IsOpen = !FormatPopup.IsOpen;
        }

        private void VideoFormatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VideoFormatListBox.SelectedItem is FormatInfo selected)
            {
                _selectedVideoFormat = selected;
                UpdateCombinedFormat();
            }
        }

        private void AudioFormatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AudioFormatListBox.SelectedItem is FormatInfo selected)
            {
                _selectedAudioFormat = selected;
                UpdateCombinedFormat();
            }
        }

        private void UpdateCombinedFormat()
        {
            if (_selectedVideoFormat != null && _selectedAudioFormat != null)
            {
                // 组合视频+音频格式
                FormatIdTextBox.Text = $"{_selectedVideoFormat.FormatId}+{_selectedAudioFormat.FormatId}";
                CombinedFormatText.Text = $"{_selectedVideoFormat.FormatId}+{_selectedAudioFormat.FormatId}";
            }
            else if (_selectedVideoFormat != null)
            {
                // 只选择视频格式
                FormatIdTextBox.Text = _selectedVideoFormat.FormatId;
                CombinedFormatText.Text = _selectedVideoFormat.FormatId;
            }
            else if (_selectedAudioFormat != null)
            {
                // 只选择音频格式
                FormatIdTextBox.Text = _selectedAudioFormat.FormatId;
                CombinedFormatText.Text = _selectedAudioFormat.FormatId;
            }
            else
            {
                // 没有选择
                FormatIdTextBox.Text = "bv*+ba/b";
                CombinedFormatText.Text = "选择视频和音频格式";
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
