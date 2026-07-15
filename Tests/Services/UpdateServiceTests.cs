using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Services;

namespace yt_dlp_gui.Tests.Services
{
    [TestClass]
    public class UpdateServiceTests
    {
        [TestMethod]
        public void CurrentVersion_ReturnsNonNull()
        {
            var service = new UpdateService();
            var version = service.CurrentVersion;
            Assert.IsFalse(string.IsNullOrEmpty(version));
            Assert.IsTrue(Version.TryParse(version, out _));
        }

        [TestMethod]
        public void CompareVersions_Greater_Returns1()
        {
            Assert.AreEqual(1, UpdateService.CompareVersions("1.0.1", "1.0.0"));
            Assert.AreEqual(1, UpdateService.CompareVersions("2.0.0", "1.9.9"));
            Assert.AreEqual(1, UpdateService.CompareVersions("1.0.0", "0.9.9"));
        }

        [TestMethod]
        public void CompareVersions_Less_ReturnsMinus1()
        {
            Assert.AreEqual(-1, UpdateService.CompareVersions("1.0.0", "1.0.1"));
            Assert.AreEqual(-1, UpdateService.CompareVersions("1.9.9", "2.0.0"));
            Assert.AreEqual(-1, UpdateService.CompareVersions("0.9.9", "1.0.0"));
        }

        [TestMethod]
        public void CompareVersions_Equal_Returns0()
        {
            Assert.AreEqual(0, UpdateService.CompareVersions("1.0.0", "1.0.0"));
            Assert.AreEqual(0, UpdateService.CompareVersions("2.3.4", "2.3.4"));
        }

        [TestMethod]
        public void CompareVersions_InvalidVersion_Returns0()
        {
            Assert.AreEqual(0, UpdateService.CompareVersions("abc", "1.0.0"));
            Assert.AreEqual(0, UpdateService.CompareVersions("1.0.0", "xyz"));
            Assert.AreEqual(0, UpdateService.CompareVersions("abc", "xyz"));
        }

        [TestMethod]
        public void CreateUpdateScript_CreatesBatchFile()
        {
            var service = new UpdateService();
            var newExe = Path.Combine(Path.GetTempPath(), "test-new.exe");
            File.WriteAllText(newExe, "test");

            try
            {
                var scriptPath = service.CreateUpdateScript(newExe);
                Assert.IsTrue(File.Exists(scriptPath), "Batch script should be created");
                Assert.IsTrue(scriptPath.EndsWith(".bat"), "Script should be .bat file");

                var content = File.ReadAllText(scriptPath);
                Assert.IsTrue(content.Contains("timeout"), "Script should wait for process exit");
                Assert.IsTrue(content.Contains("move /y"), "Script should move files");
                Assert.IsTrue(content.Contains("start"), "Script should restart app");
            }
            finally
            {
                File.Delete(newExe);
                if (File.Exists(Path.Combine(Path.GetTempPath(), Path.GetFileName(newExe))))
                    File.Delete(Path.Combine(Path.GetTempPath(), Path.GetFileName(newExe)));
            }
        }
    }
}
