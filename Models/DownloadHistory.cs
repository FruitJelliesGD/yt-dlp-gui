using System;

namespace yt_dlp_gui.Models
{
    public class DownloadHistory
    {
        public string Url { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string FileName { get; set; } = "";
        public string FileSize { get; set; } = "";
    }
}