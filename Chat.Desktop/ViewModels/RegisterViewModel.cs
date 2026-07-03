using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Furion.HttpRemote;
using Irihi.Avalonia.Shared.Contracts;
using Chat.Application.Dtos;

namespace Chat.Desktop.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly IHttpRemoteService _httpRemoteService;

    public RegisterViewModel(IHttpRemoteService httpRemoteService)
    {
        _httpRemoteService = httpRemoteService;
    }

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public event EventHandler<object?>? RequestClose;
    public void Close()
    {
        RequestClose?.Invoke(this, false);
    }

    [RelayCommand]
    private void CancelClick()
    {
        RequestClose?.Invoke(this, false);
    }

    [RelayCommand]
    private async Task RegisterClick()
    {
        var name = UserName.Trim();
        var pwd = Password.Trim();
        var confirmPwd = ConfirmPassword.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(pwd))
        {
            StatusMessage = "用户名或密码不能为空";
            return;
        }

        if (pwd != confirmPwd)
        {
            StatusMessage = "两次输入的密码不一致";
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;

            // ✅ 改成 LoginResponse，不是 bool！
            var ret = await _httpRemoteService.PostAsAsync<LoginResponse>(
                AppConfig.RegisterUrl,
                builder => builder.SetFormUrlEncodedContent(
                    new
                    {
                        UserName = name,
                        Password = pwd
                    }, contentEncoding: Encoding.UTF8)
            );

            // ✅ 判断 Success 字段
            if (ret.Success)
            {
                StatusMessage = "注册成功！";
                await Task.Delay(800);
                RequestClose?.Invoke(this, true);
            }
            else
            {
                StatusMessage = ret.Message;
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
}