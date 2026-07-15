using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Controls;

namespace yt_dlp_gui.Tests.Controls
{
    [TestClass]
    public class FormatSelectorTests
    {
        [TestMethod]
        public void UpdateFormats_ValidOutput_UpdatesFormatList()
        {
            var selector = new FormatSelector();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
248  webm  1920x1080   60  |  100.50MiB  https  vp9             audio only
251  webm  audio only  0   |   10.23MiB  https  audio only      opus";

            selector.UpdateFormats(output);

            Assert.IsNotNull(selector.Formats);
            Assert.AreEqual(2, selector.Formats.Count);
        }

        [TestMethod]
        public void FormatId_DefaultValue_ReturnsBvBa()
        {
            var selector = new FormatSelector();

            Assert.AreEqual("bv*+ba/b", selector.FormatId);
        }

        [TestMethod]
        public void SelectBestQuality_SetsCorrectFormatId()
        {
            var selector = new FormatSelector();
            selector.FormatId = "custom";

            selector.SelectBestQuality();

            Assert.AreEqual("bv*+ba/b", selector.FormatId);
        }

        [TestMethod]
        public void Select1080p_SetsCorrectFormatId()
        {
            var selector = new FormatSelector();

            selector.Select1080p();

            Assert.AreEqual("bv[height<=1080]+ba/b[height<=1080]", selector.FormatId);
        }

        [TestMethod]
        public void Select720p_SetsCorrectFormatId()
        {
            var selector = new FormatSelector();

            selector.Select720p();

            Assert.AreEqual("bv[height<=720]+ba/b[height<=720]", selector.FormatId);
        }

        [TestMethod]
        public void SelectAudioOnly_SetsCorrectFormatId()
        {
            var selector = new FormatSelector();

            selector.SelectAudioOnly();

            Assert.AreEqual("ba", selector.FormatId);
        }

        [TestMethod]
        public void BuildCustomFormat_BothFormats_CombinesWithPlus()
        {
            var selector = new FormatSelector();

            var result = selector.BuildCustomFormat("248", "251");

            Assert.AreEqual("248+251", result);
        }

        [TestMethod]
        public void BuildCustomFormat_VideoOnly_ReturnsVideoFormat()
        {
            var selector = new FormatSelector();

            var result = selector.BuildCustomFormat("248", "");

            Assert.AreEqual("248", result);
        }

        [TestMethod]
        public void BuildCustomFormat_AudioOnly_ReturnsAudioFormat()
        {
            var selector = new FormatSelector();

            var result = selector.BuildCustomFormat("", "251");

            Assert.AreEqual("251", result);
        }

        [TestMethod]
        public void BuildCustomFormat_NeitherFormat_ReturnsDefaultFormatId()
        {
            var selector = new FormatSelector();

            var result = selector.BuildCustomFormat("", "");

            Assert.AreEqual("bv*+ba/b", result);
        }

        [TestMethod]
        public void GetVideoFormats_WithMixedFormats_ReturnsOnlyVideo()
        {
            var selector = new FormatSelector();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
248  webm  1920x1080   60  |  100.50MiB  https  vp9             audio only
251  webm  audio only  0   |   10.23MiB  https  audio only      opus";
            selector.UpdateFormats(output);

            var videoFormats = selector.GetVideoFormats();

            Assert.AreEqual(1, videoFormats.Count);
            Assert.AreEqual("248", videoFormats[0].FormatId);
        }

        [TestMethod]
        public void GetAudioFormats_WithMixedFormats_ReturnsOnlyAudio()
        {
            var selector = new FormatSelector();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
248  webm  1920x1080   60  |  100.50MiB  https  vp9             audio only
251  webm  audio only  0   |   10.23MiB  https  audio only      opus";
            selector.UpdateFormats(output);

            var audioFormats = selector.GetAudioFormats();

            Assert.AreEqual(1, audioFormats.Count);
            Assert.AreEqual("251", audioFormats[0].FormatId);
        }

        [TestMethod]
        public void UpdateFormats_EmptyOutput_EmptiesFormatList()
        {
            var selector = new FormatSelector();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
248  webm  1920x1080   60  |  100.50MiB  https  vp9             audio only";
            selector.UpdateFormats(output);
            Assert.AreEqual(1, selector.Formats.Count);

            selector.UpdateFormats("");

            Assert.AreEqual(0, selector.Formats.Count);
        }

        [TestMethod]
        public void UpdateFormats_FullFormatWithTwoPipes_UpdatesFormatList()
        {
            var selector = new FormatSelector();
            var output = @"ID  EXT   RESOLUTION  FPS  HDR  CH | FILESIZE   TBR    PROTO | VCODEC          VBR    ACODEC  ABR    ASR   MORE INFO
248  webm  1920x1080   60       | 100.50MiB  2500k  https  vp9             2000k  audio only  500k   48k   
251  webm  audio only  0        |  10.23MiB  128k   https  audio only           opus         128k   48k   ";

            selector.UpdateFormats(output);

            Assert.AreEqual(2, selector.Formats.Count);
            Assert.AreEqual("248", selector.Formats[0].FormatId);
            Assert.AreEqual("251", selector.Formats[1].FormatId);
        }

        [TestMethod]
        public void GetVideoFormats_WithFullFormat_ReturnsOnlyVideo()
        {
            var selector = new FormatSelector();
            var output = @"ID  EXT   RESOLUTION  FPS  HDR  CH | FILESIZE   TBR    PROTO | VCODEC          VBR    ACODEC  ABR    ASR   MORE INFO
248  webm  1920x1080   60       | 100.50MiB  2500k  https  vp9             2000k  audio only  500k   48k   
251  webm  audio only  0        |  10.23MiB  128k   https  audio only           opus         128k   48k   ";
            selector.UpdateFormats(output);

            var videoFormats = selector.GetVideoFormats();

            Assert.AreEqual(1, videoFormats.Count);
            Assert.AreEqual("248", videoFormats[0].FormatId);
        }

        [TestMethod]
        public void GetAudioFormats_WithFullFormat_ReturnsOnlyAudio()
        {
            var selector = new FormatSelector();
            var output = @"ID  EXT   RESOLUTION  FPS  HDR  CH | FILESIZE   TBR    PROTO | VCODEC          VBR    ACODEC  ABR    ASR   MORE INFO
248  webm  1920x1080   60       | 100.50MiB  2500k  https  vp9             2000k  audio only  500k   48k   
251  webm  audio only  0        |  10.23MiB  128k   https  audio only           opus         128k   48k   ";
            selector.UpdateFormats(output);

            var audioFormats = selector.GetAudioFormats();

            Assert.AreEqual(1, audioFormats.Count);
            Assert.AreEqual("251", audioFormats[0].FormatId);
        }

        [TestMethod]
        public void UpdateFormats_MalformedInput_EmptiesFormatList()
        {
            var selector = new FormatSelector();

            selector.UpdateFormats("not a valid format output at all");

            Assert.AreEqual(0, selector.Formats.Count);
        }
    }
}
