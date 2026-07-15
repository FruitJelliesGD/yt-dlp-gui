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
        private bool _isVideo;
        public bool IsVideo
        {
            get => _isVideo;
            set
            {
                _isVideo = value;
                if (value) _isAudio = false;
            }
        }

        private bool _isAudio;
        public bool IsAudio
        {
            get => _isAudio;
            set
            {
                _isAudio = value;
                if (value) _isVideo = false;
            }
        }

        public string DisplayText => IsVideo
            ? $"{Resolution} ({Codec}, {Bitrate}, {Fps})"
            : $"{AudioCodec} ({AudioBitrate}, {AudioSampleRate})";
    }
}
