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
        public string AudioQuality { get; set; } = string.Empty;
        public bool IsVideo { get; set; }
        public bool IsAudio { get; set; }
        
        public string DisplayText => IsVideo 
            ? $"{Resolution} ({Codec}, {Bitrate}, {Fps})"
            : $"{AudioCodec} ({AudioQuality}, {AudioBitrate}, {AudioSampleRate})";
    }
}
