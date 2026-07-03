using Chat.Core.Models;
using Chat.Server.Plugins;
using Furion.DatabaseAccessor;
using SqlSugar;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;

namespace Chat.Server;

public class Worker : BackgroundService
{
    private readonly IHttpService _httpService;
    private readonly WebSocketOnlinePlugin _onlinePlugin;
    private readonly WebSocketChatPlugin _chatPlugin;
    private readonly ISqlSugarClient _db;

    // ✅ 全部 DI 注入
    public Worker(
        IHttpService httpService,
        WebSocketOnlinePlugin onlinePlugin,
        WebSocketChatPlugin chatPlugin,
        ISqlSugarClient db) // ✅ 加上
    {
        _httpService = httpService;
        _onlinePlugin = onlinePlugin;
        _chatPlugin = chatPlugin;
        _db = db;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _db.CodeFirst.InitTables<User>();
        _db.CodeFirst.InitTables<ChatMessage>();
        Console.WriteLine("✅ 数据库表初始化完成");
        // ✅ 配置并启动 WebSocket 服务
        await _httpService.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(8082)
            .ConfigureContainer(a => a.AddConsoleLogger())
            .ConfigurePlugins(a =>
            {
                a.UseWebSocket("/ws");
                a.Add(_onlinePlugin);
                a.Add(_chatPlugin);
            }));

        await _httpService.StartAsync();
        Console.WriteLine("✅ WebSocket聊天服务已启动：ws://0.0.0.0:8082/ws");

        // 等待程序停止
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        // 优雅停止
        await _httpService.StopAsync();
        Console.WriteLine("✅ WebSocket服务已停止");
    }
}