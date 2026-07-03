using System;

namespace Chat.Desktop.Models;

/// <summary>
/// WebSocket聊天消息传输实体
/// </summary>
public class ChatMessageDto
{
    /// <summary>
    /// 发送人用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 消息文本内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; set; }

    /// <summary>
    /// 消息类型 0=普通聊天 1=系统通知
    /// </summary>
    public int MsgType { get; set; }
    public int RoomId { get; set; } = 1;  // ✅ 加这个，默认聊天室1
    /// <summary>
    /// 私聊接收者用户名（MsgType=5 私聊时使用）
    /// </summary>
    public string ToUserName { get; set; } = string.Empty;

}