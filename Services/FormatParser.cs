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
            // Full format: ID EXT RESOLUTION FPS HDR CH | FILESIZE TBR PROTO | VCODEC VBR ACODEC ABR ASR MORE INFO
            // Simplified:  ID EXT RESOLUTION FPS | FILESIZE PROTO VCODEC ACODEC
            var pipeIndex = line.IndexOf('|');
            if (pipeIndex < 0) return null;

            var leftPart = line[..pipeIndex].Trim();
            var rightPart = line[(pipeIndex + 1)..].Trim();

            // Left side: ID EXT RESOLUTION FPS
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

            // Right side: split by 2+ spaces
            var rightTokens = Regex.Split(rightPart, @"\s{2,}");

            // Detect format: 6+ tokens = full format, 4 tokens = simplified
            bool isFullFormat = rightTokens.Length >= 6;

            var isAudioOnly = resolutionRaw.Contains("audio only", StringComparison.OrdinalIgnoreCase)
                           || resolutionRaw == "audio";

            if (isAudioOnly)
            {
                format.IsAudio = true;
                format.Resolution = "audio only";

                // Full format columns after pipe: FILESIZE[0] TBR[1] PROTO[2] VCODEC[3] ACODEC[4] ABR[5] ASR[6]
                // Simplified: FILESIZE[0] PROTO[1] VCODEC[2] ACODEC[3]
                if (isFullFormat && rightTokens.Length >= 5)
                {
                    format.AudioCodec = rightTokens[4]; // ACODEC
                    if (rightTokens.Length >= 6)
                        format.AudioBitrate = rightTokens[5]; // ABR
                    if (rightTokens.Length >= 7)
                        format.AudioSampleRate = rightTokens[6]; // ASR
                }
                else if (rightTokens.Length >= 4)
                {
                    format.AudioCodec = rightTokens[3]; // Simplified ACODEC
                }

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

                // fpsRaw already contains the FPS value from the left side
                // Just ensure it has "fps" suffix
                format.Fps = fpsRaw.EndsWith("fps") ? fpsRaw : fpsRaw + "fps";

                // VCODEC: Full format index 3, Simplified index 2
                // Full format columns: FILESIZE[0] TBR[1] PROTO[2] VCODEC[3]
                // Simplified: FILESIZE[0] PROTO[1] VCODEC[2]
                if (isFullFormat && rightTokens.Length >= 4)
                    format.Codec = rightTokens[3]; // Full format VCODEC
                else if (rightTokens.Length >= 3)
                    format.Codec = rightTokens[2]; // Simplified VCODEC

                // TBR (total bitrate): Full format index 1
                if (isFullFormat && rightTokens.Length >= 2)
                    format.Bitrate = rightTokens[1];
            }

            return format;
        }
    }
}
