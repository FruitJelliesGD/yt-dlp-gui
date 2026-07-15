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
