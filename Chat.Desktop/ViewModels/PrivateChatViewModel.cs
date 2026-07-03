using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Chat.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Furion.HttpRemote;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;
using WebSocketClient = TouchSocket.Http.WebSockets.WebSocketClient;

namespace Chat.Desktop.ViewModels;

public partial class PrivateChatViewModel : ViewModelBase, IDisposable
{
    #region 字段
    private readonly WebSocketClient _webSocketClient;
    private readonly string _currentUserName;
    private readonly string _targetUserName;
    private bool _disposed;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    #endregion

    #region 属性
    public WebSocketClient Client => _webSocketClient;
    public string TargetUserName => _targetUserName;
    public string CurrentUserName => _currentUserName;
    public AvaloniaList<ChatModel> ChatLists { get; } = new();

    [ObservableProperty] private string _sendContent = string.Empty;
    #endregion

    public PrivateChatViewModel(WebSocketClient webSocketClient, string currentUserName, string targetUserName)
    {
        _webSocketClient = webSocketClient ?? throw new ArgumentNullException(nameof(webSocketClient));
        _currentUserName = currentUserName?.Trim() ?? throw new ArgumentNullException(nameof(currentUserName));
        _targetUserName = targetUserName?.Trim() ?? throw new ArgumentNullException(nameof(targetUserName));

        _webSocketClient.Received += OnClientReceived;

        // ✅ 打开窗口就加载历史消息
        _ = LoadPrivateHistory();
    }

    #region WebSocket 接收消息
    private async Task OnClientReceived(object? sender, WSDataFrameEventArgs e)
    {
        try
        {
            var df = e.DataFrame;
            if (df.Opcode != WSDataType.Text) return;

            var json = df.ToText();
            using var doc = JsonDocument.Parse(json);
            int msgType = doc.RootElement.GetProperty("MsgType").GetInt32();

            // ✅ MsgType=5：私聊消息
            if (msgType == 5)
            {
                var msg = JsonSerializer.Deserialize<ChatMessageDto>(json);
                if (msg == null) return;

                // 只处理发给我的私聊，并且是当前聊天对象发的
                if (msg.ToUserName == _currentUserName && msg.UserName == _targetUserName)
                {
                    AddRemoteChatMsg(msg);
                }
            }
        }
        catch { }
        await e.InvokeNext();
    }
    #endregion

    #region 历史消息
    public class FurionResult<T>
    {
        public int Code { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }

    private async Task LoadPrivateHistory()
    {
        try
        {
            var http = Furion.App.GetRequiredService<IHttpRemoteService>();
            var result = await http.GetAsAsync<FurionResult<List<ChatMessageDto>>>(
                $"{AppConfig.PrivateHistoryUrl}?userName={_currentUserName}&targetUserName={_targetUserName}"
            );

            var history = result.Data;
            if (history != null)
            {
                foreach (var msg in history)
                {
                    if (msg.UserName == _currentUserName)
                    {
                        AddLocalChatMsg(msg);
                    }
                    else
                    {
                        AddRemoteChatMsg(msg);
                    }
                }
            }

            AddSystemTip("📜 已加载历史消息");
        }
        catch (Exception ex)
        {
            AddSystemTip($"加载历史消息失败：{ex.Message}");
        }
    }
    #endregion

    #region 发送私聊消息
    [RelayCommand]
    private async Task SendClick()
    {
        string input = SendContent.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            AddSystemTip("消息内容不能为空");
            return;
        }

        try
        {
            var msgDto = new ChatMessageDto
            {
                UserName = _currentUserName,
                ToUserName = _targetUserName,
                Content = input,
                SendTime = DateTime.Now,
                MsgType = 5  // 私聊消息
            };
            string json = JsonSerializer.Serialize(msgDto, _jsonOptions);

            AddLocalChatMsg(msgDto);
            await _webSocketClient.SendAsync(json);
            SendContent = string.Empty;
        }
        catch (Exception ex)
        {
            AddSystemTip($"发送失败：{ex.Message}");
        }
    }
    #endregion

    #region UI 渲染
    public void AddLocalChatMsg(ChatMessageDto dto)
    {
        if (dto == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            ChatLists.Add(new ChatModel
            {
                NickName = dto.UserName,
                Content = dto.Content,
                SendTime = dto.SendTime,
                TextAlignment = HorizontalAlignment.Right,
                TextDock = Dock.Right
            });
        });
    }

    public void AddRemoteChatMsg(ChatMessageDto dto)
    {
        if (dto == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            ChatLists.Add(new ChatModel
            {
                NickName = dto.UserName,
                Content = dto.Content,
                SendTime = dto.SendTime,
                TextAlignment = HorizontalAlignment.Left,
                TextDock = Dock.Left
            });
        });
    }

    public void AddSystemTip(string tip)
    {
        if (string.IsNullOrWhiteSpace(tip)) return;
        Dispatcher.UIThread.Post(() =>
        {
            ChatLists.Add(new ChatModel
            {
                NickName = "系统",
                Content = tip,
                SendTime = DateTime.Now,
                TextAlignment = HorizontalAlignment.Center,
                TextDock = Dock.Bottom
            });
        });
    }
    #endregion

    #region 资源释放
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _webSocketClient.Received -= OnClientReceived;
            ChatLists.Clear();
        }
        _disposed = true;
    }
    #endregion
}
