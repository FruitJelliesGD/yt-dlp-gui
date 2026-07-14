using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Services
{
    public class FormatParser
    {
        private static readonly Regex ResolutionRegex = new(
            @"(\d{3,4}x\d{3,4})",
            RegexOptions.Compiled);

        private static readonly Regex FpsRegex = new(
            @"(\d+)\s*fps",
            RegexOptions.Compiled);

        public List<FormatInfo> Parse(string output)
        {
            var formats = new List<FormatInfo>();
            if (string.IsNullOrWhiteSpace(output))
                return formats;

            var lines = output.Split('\n');

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("ID")) continue;
                if (line.TrimStart().StartsWith("-")) continue;

                var format = ParseLine(line);
                if (format != null)
                    formats.Add(format);
            }

            return formats;
        }

        private FormatInfo? ParseLine(string line)
        {
            // yt-dlp -F output uses pipe separator:
            // ID  EXT  RESOLUTION  FPS | FILESIZE  PROTO  VCODEC  ACODEC
            var pipeIndex = line.IndexOf('|');
            if (pipeIndex < 0) return null;

            var leftPart = line[..pipeIndex].Trim();
            var rightPart = line[(pipeIndex + 1)..].Trim();

            // Left side: ID EXT RESOLUTION FPS
            // Use regex to extract the tokens
            var leftMatch = Regex.Match(leftPart, @"^(\d+)\s+(\S+)\s+(.+?)\s+(\S+)\s*$");
            if (!leftMatch.Success)
                return null;

            var formatId = leftMatch.Groups[1].Value;
            var extension = leftMatch.Groups[2].Value;
            var resolutionRaw = leftMatch.Groups[3].Value.Trim();
            var fpsRaw = leftMatch.Groups[4].Value.Trim();

            var format = new FormatInfo
            {
                FormatId = formatId,
                Extension = extension
            };

            // Right side: FILESIZE PROTO VCODEC ACODEC
            var rightTokens = Regex.Split(rightPart, @"\s{2,}");
            var filesize = rightTokens.Length > 0 ? rightTokens[0] : string.Empty;

            var isAudioOnly = resolutionRaw.Contains("audio only", StringComparison.OrdinalIgnoreCase)
                           || resolutionRaw == "audio";

            if (isAudioOnly)
            {
                format.IsAudio = true;
                format.Resolution = "audio only";
                // VCODEC column is "audio only", ACODEC is the actual codec
                if (rightTokens.Length >= 3)
                    format.AudioCodec = rightTokens[2];
                else if (rightTokens.Length >= 2)
                    format.AudioCodec = rightTokens[1];
                format.AudioBitrate = filesize;
                format.Fps = "0";
            }
            else
            {
                format.IsVideo = true;
                format.Resolution = resolutionRaw;
                format.Fps = fpsRaw;

                var resMatch = ResolutionRegex.Match(resolutionRaw);
                if (resMatch.Success)
                    format.Resolution = resMatch.Value;

                var fpsMatch = FpsRegex.Match(line);
                if (fpsMatch.Success)
                    format.Fps = fpsMatch.Groups[1].Value + "fps";

                if (rightTokens.Length >= 3)
                    format.Codec = rightTokens[2];
                format.Bitrate = filesize;
            }

            return format;
        }
    }
}
