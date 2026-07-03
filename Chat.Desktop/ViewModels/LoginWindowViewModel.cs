using Chat.Application.Dtos;
using Chat.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Furion.HttpRemote;
using Irihi.Avalonia.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ursa.Controls;

namespace Chat.Desktop.ViewModels;

public partial class LoginWindowViewModel : ViewModelBase, IDialogContext
{
    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _rememberPassword = true;

    public bool IsNotBusy => !IsBusy;

    private readonly IHttpRemoteService _httpRemoteService;
    private readonly string _configPath;

    public LoginWindowViewModel(IHttpRemoteService httpRemoteService)
    {
        _httpRemoteService = httpRemoteService;

        // 配置文件保存到系统 AppData 目录
        _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ChatApp",
            "loginConfig.json"
        );

        // 启动时自动读取保存的账号密码
        LoadSavedAccount();
    }

    /// <summary>
    /// 读取本地保存的账号密码
    /// </summary>
    private void LoadSavedAccount()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                string json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<LoginConfig>(json);
                if (config != null)
                {
                    UserName = config.UserName;
                    Password = config.Password;
                    RememberPassword = true;
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// 保存账号密码到本地
    /// </summary>
    private void SaveAccount()
    {
        try
        {
            string dir = Path.GetDirectoryName(_configPath)!;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!RememberPassword)
            {
                if (File.Exists(_configPath))
                {
                    File.Delete(_configPath);
                }
                return;
            }

            var config = new LoginConfig
            {
                UserName = UserName,
                Password = Password
            };
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch { }
    }

    [RelayCommand]
    private async Task OkClick()
    {
        var inputUserName = UserName.Trim();
        var inputPwd = Password.Trim();
        if (string.IsNullOrWhiteSpace(inputUserName) || string.IsNullOrWhiteSpace(inputPwd))
        {
            StatusMessage = "用户名或密码不能为空";
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            var formData = new Dictionary<string, string>
            {
                ["UserName"] = inputUserName,
                ["Password"] = inputPwd
            };

            var ret = await _httpRemoteService.PostAsAsync<LoginResponse>(
                AppConfig.LoginUrl,
                builder => builder.SetFormUrlEncodedContent(formData, Encoding.UTF8)
            );

            if (ret is { Success: true })
            {
                // 登录成功，保存账号密码
                SaveAccount();
                RequestClose?.Invoke(this, true);
            }
            else
            {
                StatusMessage = ret?.Message ?? "登录失败";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"网络异常：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelClick()
    {
        RequestClose?.Invoke(this, false);
    }

    [RelayCommand]
    private async Task RegisterClick()
    {
        var vm = new RegisterViewModel(_httpRemoteService);
        var ret = await Dialog.ShowCustomAsync<RegisterView, RegisterViewModel, bool>(vm);
        if (ret)
        {
            UserName = vm.UserName;
        }
    }

    public event EventHandler<object?>? RequestClose;
    public void Close()
    {
        RequestClose?.Invoke(this, false);
    }
}

/// <summary>
/// 登录配置保存类
/// </summary>
public class LoginConfig
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}