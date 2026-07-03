using Chat.Application.Dtos;
using Chat.Desktop.Models;
using System.Text.Json;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace Chat.Server.Plugins;

public class WebSocketOnlinePlugin : PluginBase, IWebSocketConnectedPlugin, IWebSocketClosedPlugin
{
    private readonly IHttpService _httpService;

    public WebSocketOnlinePlugin(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task OnWebSocketConnected(IWebSocket webSocket, HttpContextEventArgs e)
    {
        var sysMsg = new ChatMessageDto
        {
            UserName = "系统",
            Content = "新用户加入聊天室",
            SendTime = DateTime.Now,
            MsgType = 1
        };
        await BroadcastAll(JsonSerializer.Serialize(sysMsg));
        await e.InvokeNext();
    }

    public async Task OnWebSocketClosed(IWebSocket webSocket, ClosedEventArgs e)
    {
        string clientId = ((TcpSessionClientBase)webSocket.Client).Id;

        // ✅ 下线时从字典移除
        if (WebSocketChatPlugin.OnlineUsers.TryRemove(clientId, out var userInfo))
        {
            // 下线通知
            var sysMsg = new ChatMessageDto
            {
                UserName = "系统",
                Content = $"{userInfo.UserName} 离开聊天室",
                SendTime = DateTime.Now,
                MsgType = 1
            };
            await BroadcastAll(JsonSerializer.Serialize(sysMsg));

            // 广播该房间最新在线列表
            await BroadcastOnlineList(userInfo.RoomId);
        }
        await e.InvokeNext();
    }

    private async Task BroadcastOnlineList(int roomId)
    {
        var usersInRoom = WebSocketChatPlugin.OnlineUsers.Values
            .Where(u => u.RoomId == roomId)
            .Select(u => u.UserName)
            .ToList();

        var listMsg = new
        {
            MsgType = 2,
            Users = usersInRoom,
            RoomId = roomId
        };
        string json = JsonSerializer.Serialize(listMsg);
        await BroadcastAll(json);
    }

    private async Task BroadcastAll(string jsonMsg)
    {
        foreach (var tcpClient in _httpService.Clients)
        {
            try
            {
                if (tcpClient.WebSocket != null)
                {
                    await tcpClient.WebSocket.SendAsync(jsonMsg);
                }
            }
            catch { }
        }
    }
}