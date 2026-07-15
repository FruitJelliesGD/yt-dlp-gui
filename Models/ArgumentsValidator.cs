using System;

namespace yt_dlp_gui.Models
{
    public static class ArgumentsValidator
    {
        /// <summary>
        /// 验证自定义参数语法
        /// </summary>
        public static (bool IsValid, string Message) ValidateSyntax(string input)
        {
            input = input?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(input))
            {
                return (true, "输入为空，将不添加任何自定义参数");
            }

            // 检查引号是否配对
            bool inSingleQuote = false;
            bool inDoubleQuote = false;

            foreach (char c in input)
            {
                if (c == '\0')
                {
                    return (false, "语法错误：输入包含非法字符");
                }

                if (c == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                }
                else if (c == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                }
            }

            if (inSingleQuote)
            {
                return (false, "语法错误：单引号未配对");
            }

            if (inDoubleQuote)
            {
                return (false, "语法错误：双引号未配对");
            }

            return (true, "语法验证通过");
        }
    }
}
