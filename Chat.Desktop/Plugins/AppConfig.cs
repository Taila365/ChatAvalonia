using System;
using System.IO;
using System.Text.Json;

namespace Chat.Desktop;

public static class AppConfig
{
    private static ServerSettings? _settings;
    private static readonly string _configPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    public static ServerSettings Settings
    {
        get
        {
            if (_settings == null)
            {
                string json = File.ReadAllText(_configPath);
                _settings = JsonSerializer.Deserialize<ConfigRoot>(json)?.ServerSettings ?? new ServerSettings();
            }
            return _settings;
        }
    }

    // ✅ 便捷属性
    public static string LoginUrl => $"http://{Settings.ServerIp}:{Settings.HttpPort}/api/login";
    public static string RegisterUrl => $"http://{Settings.ServerIp}:{Settings.HttpPort}/api/register";
    public static string HistoryUrl => $"http://{Settings.ServerIp}:{Settings.HttpPort}/api/chat/history";
    public static string PrivateHistoryUrl => $"http://{Settings.ServerIp}:{Settings.HttpPort}/api/chat/private/history";
    public static string WebSocketUrl => $"ws://{Settings.ServerIp}:{Settings.WebSocketPort}/ws";
}

public class ConfigRoot
{
    public ServerSettings ServerSettings { get; set; } = new();
}

public class ServerSettings
{
    public string ServerIp { get; set; } = "127.0.0.1";
    public int HttpPort { get; set; } = 8081;
    public int WebSocketPort { get; set; } = 8082;
}