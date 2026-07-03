using Avalonia.Controls;
using Avalonia.Layout;
using System;

namespace Chat.Desktop.Models;

public class ChatModel
{
    /// <summary>发送人昵称</summary>
    public string NickName { get; set; } = string.Empty;
    /// <summary>消息内容</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>发送时间</summary>
    public DateTime SendTime { get; set; }
    /// <summary>文字对齐</summary>
    public HorizontalAlignment TextAlignment { get; set; }
    /// <summary>气泡停靠方向</summary>
    public Dock TextDock { get; set; }
    public bool IsSystemMessage => NickName == "系统";
    public bool IsNotSystemMessage => NickName != "系统";
}