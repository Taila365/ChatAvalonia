using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Chat.Desktop.ViewModels;
using Chat.Desktop.Views;

namespace Chat.Desktop;

public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is ChatViewModel vm)
        {
            if (!vm.IsConnected)
            {
                await vm.ReConnectCommand.ExecuteAsync(null);
            }
        }
    }

    // ✅ 双击在线用户发起私聊
    private void OnlineUser_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is string userName)
        {
            if (DataContext is ChatViewModel vm)
            {
                // 不能和自己私聊
                if (userName == vm.CurrentUserName) return;

                // 打开私聊窗口
                var privateVm = new PrivateChatViewModel(vm.Client, vm.CurrentUserName, userName);
                var window = new PrivateChatWindow
                {
                    DataContext = privateVm
                };
                window.Show();
            }
        }
    }
}
