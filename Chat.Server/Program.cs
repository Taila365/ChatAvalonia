using Chat.Application.Services;
using Chat.Core;
using Chat.Core.Models;
using Chat.Server;
using Chat.Server.Plugins;
using SqlSugar;
using TouchSocket.Http;

// ✅ 启动 Furion Web API（端口 8081）+ Worker（端口 8082）
var host = await Serve.RunAsync(
    services =>
    {
        // ✅ 数据库
        services.AddMySqlSetup();

        // ✅ 业务服务
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IStudentService, StudentService>();

        // ✅ WebSocket
        services.AddSingleton<IHttpService, HttpService>();
        services.AddSingleton<WebSocketOnlinePlugin>();
        services.AddSingleton<WebSocketChatPlugin>();

        // ✅ Worker 后台启动 WebSocket（端口 8082）
        services.AddHostedService<Worker>();

        // ✅ 跨域
        services.AddCorsAccessor(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    },
    "http://0.0.0.0:8081"  // ✅ Web API 用 8080
);


await host.WaitForShutdownAsync();