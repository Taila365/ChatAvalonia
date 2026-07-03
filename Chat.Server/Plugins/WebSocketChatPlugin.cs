using Chat.Application.Dtos;
using Chat.Core.Models;
using Chat.Desktop.Models;
using SqlSugar;
using System.Collections.Concurrent;
using System.Text.Json;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace Chat.Server.Plugins;

public class WebSocketChatPlugin : PluginBase, IWebSocketReceivedPlugin
{
    private readonly IHttpService _httpService;
    private readonly ISqlSugarClient _db;

    // ✅ 在线用户字典：客户端ID → (用户名, 房间ID)
    public static readonly ConcurrentDictionary<string, (string UserName, int RoomId)> OnlineUsers = new();

    public WebSocketChatPlugin(IHttpService httpService, ISqlSugarClient db)
    {
        _httpService = httpService;
        _db = db;
    }

    public async Task OnWebSocketReceived(IWebSocket webSocket, WSDataFrameEventArgs e)
    {
        if (e.DataFrame.Opcode != WSDataType.Text)
        {
            await e.InvokeNext();
            return;
        }

        string json = e.DataFrame.ToText();
        var msg = JsonSerializer.Deserialize<ChatMessageDto>(json);
        if (msg == null)
        {
            await e.InvokeNext();
            return;
        }

        string senderId = ((TcpSessionClientBase)webSocket.Client).Id;




        if (msg.MsgType == 5)
        {
            // ✅ 存私聊消息到数据库
            await _db.Insertable(new ChatMessage
            {
                UserName = msg.UserName,
                ToUserName = msg.ToUserName,
                Content = msg.Content,
                SendTime = msg.SendTime,
                MsgType = msg.MsgType,
                RoomId = 0
            }).ExecuteCommandAsync();

            // 找到接收者的连接
            foreach (var tcpClient in _httpService.Clients)
            {
                string clientId = ((TcpSessionClientBase)tcpClient).Id;
                if (OnlineUsers.TryGetValue(clientId, out var targetUserInfo)
                    && targetUserInfo.UserName == msg.ToUserName)
                {
                    try
                    {
                        if (tcpClient.WebSocket != null)
                        {
                            await tcpClient.WebSocket.SendAsync(json);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"私聊发送失败：{ex.Message}");
                    }
                    break;
                }
            }
            await e.InvokeNext();
            return;
        }
        // ✅ MsgType=4：切换房间
        if (msg.MsgType == 4)
        {
            OnlineUsers.AddOrUpdate(
                senderId,
                (msg.UserName, msg.RoomId),
                (k, v) => (v.UserName, msg.RoomId));
            await BroadcastOnlineList(msg.RoomId);
            await e.InvokeNext();
            return;
        }

        // ✅ MsgType=3：上线通知
        if (msg.MsgType == 3)
        {
            if (!OnlineUsers.ContainsKey(senderId))
            {
                OnlineUsers.TryAdd(senderId, (msg.UserName, msg.RoomId == 0 ? 1 : msg.RoomId));
                await BroadcastOnlineList(1);
            }
            await e.InvokeNext();
            return;
        }

        // ✅ 存消息到数据库，带房间ID
        if (msg.MsgType == 0)
        {
            await _db.Insertable(new ChatMessage
            {
                UserName = msg.UserName,
                Content = msg.Content,
                SendTime = msg.SendTime,
                MsgType = msg.MsgType,
                RoomId = msg.RoomId
            }).ExecuteCommandAsync();
        }

        // ✅ 确保用户在列表里
        if (!OnlineUsers.ContainsKey(senderId))
        {
            OnlineUsers.TryAdd(senderId, (msg.UserName, msg.RoomId == 0 ? 1 : msg.RoomId));
            await BroadcastOnlineList(msg.RoomId == 0 ? 1 : msg.RoomId);
        }

        // ✅ 获取当前用户所在房间
        int currentRoomId = OnlineUsers.TryGetValue(senderId, out var userInfo) ? userInfo.RoomId : 1;

        // ✅ 只广播给同一个房间的人
        foreach (var tcpClient in _httpService.Clients)
        {
            string clientId = ((TcpSessionClientBase)tcpClient).Id;
            if (clientId == senderId) continue;

            // 只发给同一个房间的用户
            if (OnlineUsers.TryGetValue(clientId, out var targetUser)
                && targetUser.RoomId == currentRoomId)
            {
                try
                {
                    if (tcpClient.WebSocket != null)
                    {
                        await tcpClient.WebSocket.SendAsync(json);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"广播消息失败：{ex.Message}");
                }
            }
        }

        await e.InvokeNext();
    }

    // ✅ 只广播指定房间的在线列表
    private async Task BroadcastOnlineList(int roomId)
    {
        var usersInRoom = OnlineUsers.Values
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

        foreach (var tcpClient in _httpService.Clients)
        {
            string clientId = ((TcpSessionClientBase)tcpClient).Id;
            if (OnlineUsers.TryGetValue(clientId, out var user)
                && user.RoomId == roomId)
            {
                try
                {
                    if (tcpClient.WebSocket != null)
                    {
                        await tcpClient.WebSocket.SendAsync(json);
                    }
                }
                catch { }
            }
        }
    }
}