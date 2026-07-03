using CommunityToolkit.Mvvm.ComponentModel;
using Furion;
using TouchSocket.Http.WebSockets;

namespace Chat.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ChatViewModel ChatRoom1 { get; }
    public ChatViewModel ChatRoom2 { get; }
    // ✅ 构造函数接收用户名
    public MainWindowViewModel(string loginUserName)
    {
        // ✅ 每个房间 new 一个，不要从 DI 拿单例！
        ChatRoom1 = new ChatViewModel(new WebSocketClient(), loginUserName, 1);
        ChatRoom2 = new ChatViewModel(new WebSocketClient(), loginUserName, 2);
    }
}