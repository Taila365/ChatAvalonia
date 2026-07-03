using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Chat.Desktop.ViewModels;
using Chat.Desktop.Views;
using System.Threading.Tasks;
using Ursa.Controls;

namespace Chat.Desktop;

public partial class LoginWindow : SplashWindow
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    protected override Task<Window?> CreateNextWindow()
    {
        if (DialogResult is false)
        {
            return Task.FromResult<Window?>(null);
        }

        // ✅ 拿到登录的用户名
        var loginVm = (LoginWindowViewModel)DataContext;
        string userName = loginVm.UserName;

        // ✅ 创建 MainWindow，传用户名
        var mainVm = new MainWindowViewModel(userName);
        return Task.FromResult<Window?>(new MainWindow()
        {
            DataContext = mainVm
        });
    }
}