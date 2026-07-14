using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Services;

namespace yt_dlp_gui.Tests.Services
{
    [TestClass]
    public class FormatParserTests
    {
        [TestMethod]
        public void Parse_ValidOutput_ReturnsFormatList()
        {
            var parser = new FormatParser();
            var output = @"ID  EXT   RESOLUTION  FPS  |  FILESIZE   PROTO  VCODEC          ACODEC
248  webm  1920x1080   60  |  100.50MiB  https  vp9             audio only
251  webm  audio only  0   |   10.23MiB  https  audio only      opus";
            
            var formats = parser.Parse(output);
            
            Assert.AreEqual(2, formats.Count);
            Assert.IsTrue(formats[0].IsVideo);
            Assert.IsTrue(formats[1].IsAudio);
        }
    }
}
