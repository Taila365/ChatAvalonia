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

public partial class ChatViewModel : ViewModelBase, IDisposable
{
    #region 字段
    private readonly WebSocketClient _webSocketClient;
    private readonly string _currentLoginUserName;
    private readonly int _roomId;  // ✅ 房间ID
    private bool _disposed;
    private bool _isReconnecting;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    #endregion

    #region 属性
    public WebSocketClient Client => _webSocketClient;
    public string CurrentUserName => _currentLoginUserName;
    public AvaloniaList<ChatModel> ChatLists { get; } = new();
    public AvaloniaList<string> OnlineUsers { get; } = new();

    [ObservableProperty] private string _sendContent = string.Empty;
    [ObservableProperty] private string _connectStatus = "未连接";
    [ObservableProperty] private bool _isConnected;
    #endregion

    // ✅ 构造函数加 roomId 参数
    public ChatViewModel(WebSocketClient webSocketClient, string loginUserName, int roomId)
    {
        _webSocketClient = webSocketClient ?? throw new ArgumentNullException(nameof(webSocketClient));
        _currentLoginUserName = loginUserName?.Trim() ?? throw new ArgumentNullException(nameof(loginUserName));
        _roomId = roomId;

        _webSocketClient.Connected += OnClientConnected;
        _webSocketClient.Closed += OnClientClosed;
        _webSocketClient.Received += OnClientReceived;
    }

    #region WebSocket 事件
    private Task OnClientConnected(object? sender, TouchSocketEventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            IsConnected = true;
            ConnectStatus = "已连接";
            AddSystemTip($"✅ 聊天室{_roomId}连接成功");
            await LoadHistoryMessages();

            // ✅ 上线报到带 roomId
            var onlineMsg = new ChatMessageDto
            {
                UserName = _currentLoginUserName,
                Content = string.Empty,
                SendTime = DateTime.Now,
                MsgType = 3,
                RoomId = _roomId
            };
            string json = JsonSerializer.Serialize(onlineMsg, _jsonOptions);
            await Client.SendAsync(json);

            // ✅ 切换到当前房间
            var switchRoomMsg = new ChatMessageDto
            {
                UserName = _currentLoginUserName,
                RoomId = _roomId,
                MsgType = 4
            };
            await Client.SendAsync(JsonSerializer.Serialize(switchRoomMsg, _jsonOptions));
        });
        return Task.CompletedTask;
    }

    private Task OnClientClosed(object? sender, ClosedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsConnected = false;
            ConnectStatus = "连接断开";
            AddSystemTip($"❌ 服务器断开：{e.Message}");
        });
        return Task.CompletedTask;
    }

    private async Task OnClientReceived(object? sender, WSDataFrameEventArgs e)
    {
        try
        {
            var df = e.DataFrame;
            if (df.Opcode != WSDataType.Text) return;

            var json = df.ToText();
            using var doc = JsonDocument.Parse(json);
            int msgType = doc.RootElement.GetProperty("MsgType").GetInt32();

            // ✅ 在线用户列表：只处理当前房间的
            if (msgType == 2)
            {
                int listRoomId = doc.RootElement.TryGetProperty("RoomId", out var roomIdProp)
                    ? roomIdProp.GetInt32()
                    : 1;

                // 不是当前房间的列表，忽略
                if (listRoomId != _roomId)
                {
                    await e.InvokeNext();
                    return;
                }

                var userList = new List<string>();
                var users = doc.RootElement.GetProperty("Users").EnumerateArray();
                foreach (var user in users)
                {
                    userList.Add(user.GetString()!);
                }

                Dispatcher.UIThread.Post(() =>
                {
                    OnlineUsers.Clear();
                    foreach (var user in userList)
                    {
                        OnlineUsers.Add(user);
                    }
                });
            }
            // ✅ 普通聊天消息：只显示当前房间的
            else if (msgType == 0)
            {
                var msg = JsonSerializer.Deserialize<ChatMessageDto>(json);
                // 不是当前房间的消息，忽略
                if (msg!.RoomId != _roomId)
                {
                    await e.InvokeNext();
                    return;
                }
                AddRemoteChatMsg(msg);
            }
            // ✅ 系统消息
            else if (msgType == 1)
            {
                var msg = JsonSerializer.Deserialize<ChatMessageDto>(json);
                AddSystemTip(msg!.Content);
            }
        }
        catch (Exception ex)
        {
            AddSystemTip($"接收消息异常：{ex.Message}");
        }
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

    private async Task LoadHistoryMessages()
    {
        try
        {
            var http = Furion.App.GetRequiredService<IHttpRemoteService>();
            var result = await http.GetAsAsync<FurionResult<List<ChatMessageDto>>>(
               $"{AppConfig.HistoryUrl}?roomId={_roomId}"
            );

            var history = result.Data;
            foreach (var msg in history)
            {
                if (msg.UserName == _currentLoginUserName)
                {
                    AddLocalChatMsg(msg);
                }
                else
                {
                    AddRemoteChatMsg(msg);
                }
            }

            AddSystemTip($"📜 已加载聊天室{_roomId}历史消息");
        }
        catch (Exception ex)
        {
            AddSystemTip($"加载历史消息失败：{ex.Message}");
        }
    }
    #endregion

    #region 发送消息
    [RelayCommand]
    private async Task SendClick()
    {
        string input = SendContent.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            AddSystemTip("消息内容不能为空");
            return;
        }

        if (!IsConnected)
        {
            AddSystemTip("未连接聊天服务器，尝试重连...");
            await ReConnect();
            if (!IsConnected)
            {
                AddSystemTip("重连失败：无法发送消息");
                return;
            }
        }

        try
        {
            var msgDto = new ChatMessageDto
            {
                UserName = _currentLoginUserName,
                Content = input,
                SendTime = DateTime.Now,
                MsgType = 0,
                RoomId = _roomId  // ✅ 带房间ID
            };
            string json = JsonSerializer.Serialize(msgDto, _jsonOptions);

            AddLocalChatMsg(msgDto);
            await Client.SendAsync(json);
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

    #region 重连逻辑
    [RelayCommand]
    private async Task ReConnect()
    {
        if (IsConnected)
        {
            AddSystemTip("当前已连接，无需重连");
            return;
        }
        if (_isReconnecting)
        {
            AddSystemTip("正在重连中，请稍后...");
            return;
        }

        try
        {
            _isReconnecting = true;
            AddSystemTip($"🔄 开始连接聊天室{_roomId}：{AppConfig.WebSocketUrl}");
            await Client.ConnectAsync(AppConfig.WebSocketUrl);
        }
        catch (WebSocketConnectException wsEx)
        {
            ConnectStatus = "连接失败";
            AddSystemTip($"❌ WebSocket握手失败：{wsEx.Message}");
        }
        catch (Exception ex)
        {
            ConnectStatus = "连接失败";
            AddSystemTip($"❌ 重连异常：{ex.Message}");
        }
        finally
        {
            _isReconnecting = false;
        }
    }

    [RelayCommand]
    private async Task CloseConnect()
    {
        if (!IsConnected) return;
        try
        {
            await Client.CloseAsync("主动断开连接");
        }
        catch (Exception ex)
        {
            AddSystemTip($"断开连接异常：{ex.Message}");
        }
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
            _webSocketClient.Connected -= OnClientConnected;
            _webSocketClient.Closed -= OnClientClosed;
            _webSocketClient.Received -= OnClientReceived;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _webSocketClient.CloseAsync("窗口销毁");
                }
                catch { }
            });
            ChatLists.Clear();
        }
        _disposed = true;
    }
    #endregion
}