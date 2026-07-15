using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Services;

namespace yt_dlp_gui.Tests.Services
{
    [TestClass]
    public class FormatParserTests
    {
        [TestMethod]
        public void Parse_FullFormat_ReturnsCorrectVideoFields()
        {
            var parser = new FormatParser();
            var output = @"ID  EXT   RESOLUTION  FPS  HDR  CH | FILESIZE   TBR    PROTO | VCODEC          VBR    ACODEC  ABR    ASR   MORE INFO
248  webm  1920x1080   60       | 100.50MiB  2500k  https | vp9             2000k  audio only  500k   48k   
251  webm  audio only  0        |  10.23MiB  128k   https | audio only      opus         128k   48k   ";

            var formats = parser.Parse(output);

            Assert.AreEqual(2, formats.Count);

            // Video format
            var video = formats[0];
            Assert.IsTrue(video.IsVideo);
            Assert.IsFalse(video.IsAudio);
            Assert.AreEqual("248", video.FormatId);
            Assert.AreEqual("webm", video.Extension);
            Assert.AreEqual("1920x1080", video.Resolution);
            Assert.AreEqual("60fps", video.Fps);
            Assert.AreEqual("vp9", video.Codec);
            Assert.AreEqual("2500k", video.Bitrate);

            // Audio format
            var audio = formats[1];
            Assert.IsTrue(audio.IsAudio);
            Assert.IsFalse(audio.IsVideo);
            Assert.AreEqual("251", audio.FormatId);
            Assert.AreEqual("webm", audio.Extension);
            Assert.AreEqual("audio only", audio.Resolution);
            Assert.AreEqual("0", audio.Fps);
            Assert.AreEqual("opus", audio.AudioCodec);
            Assert.AreEqual("128k", audio.AudioBitrate);
            Assert.AreEqual("48k", audio.AudioSampleRate);
        }

        [TestMethod]
        public void Parse_TwoPipeDataLines_ParsesCorrectly()
        {
            var parser = new FormatParser();
            // Real yt-dlp -F output with HDR/CH columns produces two pipes per data line
            var output = @"ID  EXT   RESOLUTION  FPS  HDR  CH | FILESIZE   TBR    PROTO | VCODEC          VBR    ACODEC  ABR    ASR   MORE INFO
248  webm  1920x1080   60       | 100.50MiB  2500k  https | vp9             2000k  audio only  500k   48k   
271  webm  2560x1440   30       |  80.25MiB  1800k  https | vp9             1500k  audio only  300k   48k   
399  mp4   3840x2160   60  HDR  | 500.00MiB  8000k  https | av01.0.12H.08  6000k  audio only  2000k  48k   ";

            var formats = parser.Parse(output);

            Assert.AreEqual(3, formats.Count);

            // 1080p60 vp9 - codec at correct index, not shifted
            var v1080 = formats[0];
            Assert.IsTrue(v1080.IsVideo);
            Assert.AreEqual("248", v1080.FormatId);
            Assert.AreEqual("1920x1080", v1080.Resolution);
            Assert.AreEqual("vp9", v1080.Codec);
            Assert.AreEqual("2500k", v1080.Bitrate);

            // 1440p30 vp9
            var v1440 = formats[1];
            Assert.IsTrue(v1440.IsVideo);
            Assert.AreEqual("271", v1440.FormatId);
            Assert.AreEqual("2560x1440", v1440.Resolution);
            Assert.AreEqual("vp9", v1440.Codec);
            Assert.AreEqual("1800k", v1440.Bitrate);

            // 4K60 HDR AV1
            var v4k = formats[2];
            Assert.IsTrue(v4k.IsVideo);
            Assert.AreEqual("399", v4k.FormatId);
            Assert.AreEqual("3840x2160", v4k.Resolution);
            Assert.AreEqual("av01.0.12H.08", v4k.Codec);
            Assert.AreEqual("8000k", v4k.Bitrate);
        }

        [TestMethod]
        public void Parse_SimplifiedFormat_ReturnsCorrectFields()
        {
            var parser = new FormatParser();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
248  webm  1920x1080   60  |  100.50MiB  https  vp9             audio only
251  webm  audio only  0   |   10.23MiB  https  audio only      opus";

            var formats = parser.Parse(output);

            Assert.AreEqual(2, formats.Count);

            // Video format
            var video = formats[0];
            Assert.IsTrue(video.IsVideo);
            Assert.AreEqual("248", video.FormatId);
            Assert.AreEqual("1920x1080", video.Resolution);
            Assert.AreEqual("vp9", video.Codec);
            // Simplified format has no TBR, so bitrate is empty
            Assert.AreEqual(string.Empty, video.Bitrate);

            // Audio format
            var audio = formats[1];
            Assert.IsTrue(audio.IsAudio);
            Assert.AreEqual("251", audio.FormatId);
            Assert.AreEqual("opus", audio.AudioCodec);
            // Simplified format has no ABR/ASR
            Assert.AreEqual(string.Empty, audio.AudioBitrate);
            Assert.AreEqual(string.Empty, audio.AudioSampleRate);
        }

        [TestMethod]
        public void Parse_EmptyOutput_ReturnsEmptyList()
        {
            var parser = new FormatParser();
            var formats = parser.Parse("");
            Assert.AreEqual(0, formats.Count);
        }

        [TestMethod]
        public void Parse_NullOutput_ReturnsEmptyList()
        {
            var parser = new FormatParser();
            var formats = parser.Parse(null!);
            Assert.AreEqual(0, formats.Count);
        }

        [TestMethod]
        public void Parse_HeaderOnly_ReturnsEmptyList()
        {
            var parser = new FormatParser();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
-----------------------------------------------------------------------";
            var formats = parser.Parse(output);
            Assert.AreEqual(0, formats.Count);
        }

        [TestMethod]
        public void Parse_VideoWithResolutionExtraction_ReturnsCorrectResolution()
        {
            var parser = new FormatParser();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
137  mp4   1920x1080   30  |  200.00MiB  https  avc1.640028    audio only";

            var formats = parser.Parse(output);

            Assert.AreEqual(1, formats.Count);
            Assert.AreEqual("1920x1080", formats[0].Resolution);
        }

        [TestMethod]
        public void Parse_AudioFormat_SetsCorrectFields()
        {
            var parser = new FormatParser();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
251  opus  audio only  0   |   5.23MiB   https  audio only      opus";

            var formats = parser.Parse(output);

            Assert.AreEqual(1, formats.Count);
            var audio = formats[0];
            Assert.IsTrue(audio.IsAudio);
            Assert.AreEqual("opus", audio.AudioCodec);
            Assert.AreEqual("audio only", audio.Resolution);
            Assert.AreEqual("0", audio.Fps);
        }
    }
}
