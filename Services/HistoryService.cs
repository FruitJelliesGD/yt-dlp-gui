using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using yt_dlp_gui.Models;

namespace yt_dlp_gui.Services
{
    public class HistoryService
    {
        private static readonly string AppDataPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        "yt-dlp-gui");
        private static readonly string HistoryFile = 
            Path.Combine(AppDataPath, "history.json");
        
        private const int MaxHistoryItems = 100;

        public List<DownloadHistory> LoadHistory()
        {
            try
            {
                if (File.Exists(HistoryFile))
                {
                    string json = File.ReadAllText(HistoryFile);
                    return JsonSerializer.Deserialize<List<DownloadHistory>>(json) ?? new List<DownloadHistory>();
                }
            }
            catch (Exception)
            {
                // If loading fails, return empty list
            }
            return new List<DownloadHistory>();
        }

        public void SaveHistory(List<DownloadHistory> history)
        {
            try
            {
                Directory.CreateDirectory(AppDataPath);
                
                // Keep only the most recent items
                if (history.Count > MaxHistoryItems)
                {
                    history = history.GetRange(0, MaxHistoryItems);
                }
                
                string json = JsonSerializer.Serialize(history, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(HistoryFile, json);
            }
            catch (Exception)
            {
                // Silently fail - history is not critical
            }
        }

        public void AddEntry(string url, string status, string fileName = "", string fileSize = "")
        {
            var history = LoadHistory();
            
            history.Insert(0, new DownloadHistory
            {
                Url = url,
                Status = status,
                Timestamp = DateTime.Now,
                FileName = fileName,
                FileSize = fileSize
            });
            
            SaveHistory(history);
        }

        public void ClearHistory()
        {
            try
            {
                if (File.Exists(HistoryFile))
                {
                    File.Delete(HistoryFile);
                }
            }
            catch (Exception)
            {
                // Silently fail
            }
        }
    }
}