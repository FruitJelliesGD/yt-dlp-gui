using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Tests.Models
{
    [TestClass]
    public class FormatInfoTests
    {
        [TestMethod]
        public void DisplayText_VideoFormat_ReturnsFormattedString()
        {
            var format = new FormatInfo
            {
                IsVideo = true,
                Resolution = "1920x1080",
                Codec = "VP9",
                Bitrate = "2.5Mbps",
                Fps = "60fps"
            };
            
            Assert.AreEqual("1920x1080 (VP9, 2.5Mbps, 60fps)", format.DisplayText);
        }
        
        [TestMethod]
        public void DisplayText_AudioFormat_ReturnsFormattedString()
        {
            var format = new FormatInfo
            {
                IsAudio = true,
                AudioCodec = "Opus",
                AudioQuality = "192kbps",
                AudioBitrate = "128Kbps",
                AudioSampleRate = "48kHz"
            };
            
            Assert.AreEqual("Opus (192kbps, 128Kbps, 48kHz)", format.DisplayText);
        }
        
        [TestMethod]
        public void AudioQuality_PropertyExists()
        {
            var format = new FormatInfo
            {
                AudioQuality = "320kbps"
            };
            
            Assert.AreEqual("320kbps", format.AudioQuality);
        }
    }
}
