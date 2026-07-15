using System.Collections.Generic;

namespace yt_dlp_gui.Models
{
    public class UserSettings
    {
        public string SavePath { get; set; } = "";
        public string FormatPreset { get; set; } = "bv*+ba/b";
        public string CookiesPath { get; set; } = "";
        public string CustomArgs { get; set; } = "";
        
        // Advanced options
        public AdvancedSettings Advanced { get; set; } = new();
    }

    public class AdvancedSettings
    {
        public string OutputTemplate { get; set; } = "%(title)s.%(ext)s";
        public string SpeedLimit { get; set; } = "";
        public bool UnlimitedSpeed { get; set; } = true;
        public string Retries { get; set; } = "3";
        public string FragmentThreads { get; set; } = "1";
        public bool ForceReDownload { get; set; } = false;
        public bool IgnoreErrors { get; set; } = false;
        
        // Network
        public string Proxy { get; set; } = "";
        public string Timeout { get; set; } = "";
        public string ConnectionLimit { get; set; } = "";
        public bool IPv6Preference { get; set; } = false;
        
        // Subtitles
        public string SubtitleLanguage { get; set; } = "";
        public string SubtitleFormat { get; set; } = "srt";
        public bool AutoTranslate { get; set; } = false;
        public bool EmbedSubtitles { get; set; } = false;
        
        // Playlist
        public bool DownloadPlaylist { get; set; } = false;
        public string PlaylistSort { get; set; } = "";
        public string PlaylistLimit { get; set; } = "";
        public string PlaylistItemLimit { get; set; } = "";
        
        // Post-processing
        public bool ConvertFile { get; set; } = false;
        public bool DownloadThumbnail { get; set; } = false;
        public bool WriteMetadata { get; set; } = false;
        public string FilenameTemplate { get; set; } = "%(title)s.%(ext)s";
        
        // Authentication
        public string Username { get; set; } = "";
        public string NetrcFilePath { get; set; } = "";
    }
}