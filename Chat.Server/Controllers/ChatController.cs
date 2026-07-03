using Chat.Core.Models;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using Furion;  // ✅ 加上这个

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ISqlSugarClient _db;

    public ChatController(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <summary>
    /// 获取最近100条历史消息
    /// </summary>
    [HttpGet("history")]
    
    public async Task<IActionResult> GetHistory(int roomId = 1)
    {
        var list = await _db.Queryable<ChatMessage>()
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.SendTime)
            .Take(100)
            .OrderBy(m => m.SendTime)
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>
    /// 获取私聊历史消息
    /// </summary>
    [HttpGet("private/history")]
    public async Task<IActionResult> GetPrivateHistory(string userName, string targetUserName)
    {
        var list = await _db.Queryable<ChatMessage>()
            .Where(m => 
                (m.UserName == userName && m.ToUserName == targetUserName) ||
                (m.UserName == targetUserName && m.ToUserName == userName)
            )
            .OrderByDescending(m => m.SendTime)
            .Take(100)
            .OrderBy(m => m.SendTime)
            .ToListAsync();

        return Ok(list);
    }
}
