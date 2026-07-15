using System.Collections.Generic;
using System.Linq;
using yt_dlp_gui.Models;
using yt_dlp_gui.Services;

namespace yt_dlp_gui.Controls
{
    public class FormatSelector
    {
        private readonly FormatParser _parser = new();

        public string FormatId { get; set; } = "bv*+ba/b";

        public List<FormatInfo> Formats { get; private set; } = new();

        public void UpdateFormats(string output)
        {
            try
            {
                Formats = _parser.Parse(output);
            }
            catch
            {
                Formats = new List<FormatInfo>();
            }
        }

        public void SelectBestQuality()
        {
            FormatId = "bv*+ba/b";
        }

        public void Select1080p()
        {
            FormatId = "bv[height<=1080]+ba/b[height<=1080]";
        }

        public void Select720p()
        {
            FormatId = "bv[height<=720]+ba/b[height<=720]";
        }

        public void SelectAudioOnly()
        {
            FormatId = "ba";
        }

        internal string BuildCustomFormat(string videoFormatId, string audioFormatId)
        {
            if (!string.IsNullOrEmpty(videoFormatId) && !string.IsNullOrEmpty(audioFormatId))
            {
                return $"{videoFormatId}+{audioFormatId}";
            }
            if (!string.IsNullOrEmpty(videoFormatId))
            {
                return videoFormatId;
            }
            if (!string.IsNullOrEmpty(audioFormatId))
            {
                return audioFormatId;
            }
            return FormatId;
        }

        public List<FormatInfo> GetVideoFormats()
        {
            return Formats.Where(f => f.IsVideo).ToList();
        }

        public List<FormatInfo> GetAudioFormats()
        {
            return Formats.Where(f => f.IsAudio).ToList();
        }
    }
}
