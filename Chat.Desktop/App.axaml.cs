using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Chat.Application.Services;
using Chat.Desktop.ViewModels;
using Chat.Desktop.Views;
using Furion;
using Microsoft.Extensions.DependencyInjection;
using System;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;
using TouchSocket.Http;
using Chat.Core;
using SqlSugar;


namespace Chat.Desktop
{
    public partial class App : Avalonia.Application
    {
        public App()
        {
            // ✅ 唯一的一次 Serve.RunNative，所有服务在这里注册
            Serve.RunNative(services =>
            {
                // ✅ 必须加这个！不要注释！
                services.AddHttpRemote();
                services.AddMySqlSetup();

                // 1. ViewModel 注册
                services.AddSingleton<DataListViewModel>();
                services.AddSingleton<LoginWindowViewModel>();
                services.AddSingleton<IStudentService, StudentService>();
                services.AddSingleton<IUserService, UserService>();
                // 2. WebSocketClient 全局单例
                services.AddSingleton<TouchSocket.Http.WebSockets.WebSocketClient>(sp =>
     new TouchSocket.Http.WebSockets.WebSocketClient());
                // 3. ChatViewModel


            }, includeWeb: false);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // ✅ 这里只解析，不要再写 Serve.RunNative！！！
                desktop.MainWindow = new LoginWindow
                {
                    DataContext = Furion.App.GetRequiredService<LoginWindowViewModel>()
                };
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}