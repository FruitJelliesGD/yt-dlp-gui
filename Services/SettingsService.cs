using System;
using System.IO;
using System.Text.Json;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Services
{
    public class SettingsService
    {
        private static readonly string AppDataPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        "yt-dlp-gui");
        private static readonly string SettingsFile = 
            Path.Combine(AppDataPath, "settings.json");

        public UserSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
                }
            }
            catch (Exception)
            {
                // If loading fails, return default settings
            }
            return new UserSettings();
        }

        public void SaveSettings(UserSettings settings)
        {
            try
            {
                Directory.CreateDirectory(AppDataPath);
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception)
            {
                // Silently fail - settings are not critical
            }
        }
    }
}