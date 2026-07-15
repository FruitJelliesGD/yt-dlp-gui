using Microsoft.VisualStudio.TestTools.UnitTesting;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Tests.Models
{
    [TestClass]
    public class ArgumentsValidatorTests
    {
        [TestMethod]
        public void ValidateSyntax_EmptyInput_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("");

            Assert.IsTrue(isValid);
            Assert.Contains("输入为空", message);
        }

        [TestMethod]
        public void ValidateSyntax_NullInput_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax(null);

            Assert.IsTrue(isValid);
            Assert.Contains("输入为空", message);
        }

        [TestMethod]
        public void ValidateSyntax_SingleArg_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--no-check-certificates");

            Assert.IsTrue(isValid);
            Assert.Contains("语法验证通过", message);
        }

        [TestMethod]
        public void ValidateSyntax_MultipleArgs_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--no-check-certificates --geo-bypass");

            Assert.IsTrue(isValid);
            Assert.Contains("语法验证通过", message);
        }

        [TestMethod]
        public void ValidateSyntax_UnclosedSingleQuote_ReturnsInvalid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--output 'test");

            Assert.IsFalse(isValid);
            Assert.Contains("单引号未配对", message);
        }

        [TestMethod]
        public void ValidateSyntax_UnclosedDoubleQuote_ReturnsInvalid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--output \"test");

            Assert.IsFalse(isValid);
            Assert.Contains("双引号未配对", message);
        }

        [TestMethod]
        public void ValidateSyntax_MatchedQuotes_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--output \"test.mp4\"");

            Assert.IsTrue(isValid);
            Assert.Contains("语法验证通过", message);
        }

        [TestMethod]
        public void ValidateSyntax_SingleAndDoubleQuotes_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--output 'test.mp4' --name \"video\"");

            Assert.IsTrue(isValid);
            Assert.Contains("语法验证通过", message);
        }

        [TestMethod]
        public void ValidateSyntax_WhitespaceOnly_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("   ");

            Assert.IsTrue(isValid);
            Assert.Contains("输入为空", message);
        }

        [TestMethod]
        public void ValidateSyntax_ArgWithSpacesInQuotes_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--output \"my video.mp4\"");

            Assert.IsTrue(isValid);
            Assert.Contains("语法验证通过", message);
        }

        [TestMethod]
        public void ValidateSyntax_MultipleArgsWithQuotes_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--output \"test.mp4\" --username 'user' --no-check-certificates");

            Assert.IsTrue(isValid);
            Assert.Contains("语法验证通过", message);
        }

        [TestMethod]
        public void ValidateSyntax_OnlySingleQuote_ReturnsInvalid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("'");

            Assert.IsFalse(isValid);
            Assert.Contains("单引号未配对", message);
        }

        [TestMethod]
        public void ValidateSyntax_OnlyDoubleQuote_ReturnsInvalid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("\"");

            Assert.IsFalse(isValid);
            Assert.Contains("双引号未配对", message);
        }

        [TestMethod]
        public void ValidateSyntax_NestedQuotes_ReturnsValid()
        {
            var (isValid, message) = ArgumentsValidator.ValidateSyntax("--output \"test 'with' quotes.mp4\"");

            Assert.IsTrue(isValid);
            Assert.Contains("语法验证通过", message);
        }
    }
}
