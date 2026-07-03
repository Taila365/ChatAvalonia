using SqlSugar;

namespace Chat.Core.Models;

[SugarTable("user")]
public class User
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    // 手动绑定数据库字段 user_name
    [SugarColumn(ColumnName = "user_name")]
    public string UserName { get; set; }

    [SugarColumn(ColumnName = "password")]
    public string Password { get; set; }
}