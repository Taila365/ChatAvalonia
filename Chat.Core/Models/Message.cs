using SqlSugar;

namespace Chat.Core.Models;

/// <summary>
/// 聊天消息表
/// </summary>
[SugarTable("chat_message")]
public class ChatMessage
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 发送人
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; set; }

    /// <summary>
    /// 消息类型：0=普通消息，1=系统消息
    /// </summary>
    public int MsgType { get; set; }

    public int RoomId { get; set; } = 1;  // ✅ 加这个

    /// <summary>
    /// 私聊接收者（MsgType=5 私聊时使用）
    /// </summary>
    public string ToUserName { get; set; } = string.Empty;
}