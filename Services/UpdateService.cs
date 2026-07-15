using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Services
{
    public class UpdateService
    {
        private const string GitHubOwner = "FruitJelliesGD";
        private const string GitHubRepo = "yt-dlp-gui";

        private static readonly HttpClient _http = new()
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        public string CurrentVersion =>
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";

        /// <summary>
        /// Compare two semantic version strings. Returns:
        ///   1 if a > b, -1 if a < b, 0 if equal.
        /// </summary>
        public static int CompareVersions(string a, string b)
        {
            if (!Version.TryParse(a, out var va)) return 0;
            if (!Version.TryParse(b, out var vb)) return 0;
            return va.CompareTo(vb);
        }

        /// <summary>
        /// Check GitHub releases for a newer version.
        /// Returns null if already up-to-date or on error.
        /// </summary>
        public async Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken ct = default)
        {
            var url = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "yt-dlp-gui-updater");

            using var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.TryGetProperty("tag_name", out var tag) ? tag.GetString() ?? "" : "";
            var body = root.TryGetProperty("body", out var bodyEl) ? bodyEl.GetString() ?? "" : "";
            var publishedAt = root.TryGetProperty("published_at", out var pub) ? pub.GetString() ?? "" : "";
            var assets = root.TryGetProperty("assets", out var assetsEl) ? assetsEl : default;

            var latestVersion = tagName.TrimStart('v', 'V');
            if (!Version.TryParse(latestVersion, out var latest))
                return null;
            if (!Version.TryParse(CurrentVersion, out var current))
                return null;

            if (latest <= current)
                return null;

            // Find first .exe asset
            string downloadUrl = "";
            if (assets.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.TryGetProperty("browser_download_url", out var dl)
                            ? dl.GetString() ?? "" : "";
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(downloadUrl))
                return null;

            DateTime.TryParse(publishedAt, out var published);

            return new UpdateInfo
            {
                Version = latestVersion,
                ReleaseNotes = body,
                DownloadUrl = downloadUrl,
                PublishedAt = published,
            };
        }

        /// <summary>
        /// Download the update exe to a temp file. Returns the temp file path.
        /// </summary>
        public async Task<string> DownloadUpdateAsync(
            string downloadUrl,
            IProgress<double>? progress = null,
            CancellationToken ct = default)
        {
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                $"yt-dlp-gui-update-{Guid.NewGuid():N}.exe");

            using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            request.Headers.Add("User-Agent", "yt-dlp-gui-updater");

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
            await using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[81920];
            long downloaded = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                downloaded += bytesRead;
                if (totalBytes > 0)
                    progress?.Report((double)downloaded / totalBytes * 100);
            }

            progress?.Report(100);
            return tempPath;
        }

        /// <summary>
        /// Backup current exe, replace with new exe, and restart the app.
        /// Call this AFTER the app has been closed — or schedule it.
        /// Returns the batch script path that performs the swap + restart.
        /// </summary>
        public string CreateUpdateScript(string newExePath)
        {
            var currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            var backupPath = currentExe + ".bak";

            // Write a batch script that:
            // 1. Waits for the current process to exit
            // 2. Backs up the current exe
            // 3. Replaces with the new exe
            // 4. Starts the app again
            // 5. Cleans up
            var scriptPath = Path.Combine(Path.GetTempPath(), $"yt-dlp-gui-update-{Guid.NewGuid():N}.bat");

            var script = $@"@echo off
timeout /t 2 /nobreak >nul
move /y ""{currentExe}"" ""{backupPath}"" >nul 2>&1
move /y ""{newExePath}"" ""{currentExe}"" >nul 2>&1
start """" ""{currentExe}""
del ""{scriptPath}""";
            File.WriteAllText(scriptPath, script);
            return scriptPath;
        }

        /// <summary>
        /// Launch the update script and exit the current process.
        /// </summary>
        public void ApplyUpdate(string scriptPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = scriptPath,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            // Give the script a moment to start, then exit
            Environment.Exit(0);
        }

        /// <summary>
        /// Rollback: restore the backup exe if one exists.
        /// </summary>
        public static void Rollback()
        {
            var currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            var backupPath = currentExe + ".bak";
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, currentExe, overwrite: true);
                File.Delete(backupPath);
            }
        }
    }
}
