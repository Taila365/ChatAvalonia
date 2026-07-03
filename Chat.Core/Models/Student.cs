using SqlSugar;

namespace Chat.Core.Models;

[SugarTable("students", TableDescription = "学生信息表")]
public class Student
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)] // 主键+自增
    public int Id { get; set; }

    [SugarColumn(ColumnName = "no", Length = 10, IsNullable = false, ColumnDescription = "学号")]
    public string No { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "name", Length = 30, IsNullable = false, ColumnDescription = "姓名")]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "id_number", Length = 18, IsNullable = false, ColumnDescription = "身份证")]
    public string IdNumber { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "gender", IsNullable = false, ColumnDescription = "性别")]
    public EnumGender Gender { get; set; }

    [SugarColumn(ColumnName = "birthday", IsNullable = false, ColumnDescription = "出生日期")]
    public DateTime Birthday { get; set; }

    [SugarColumn(ColumnName = "weight", ColumnDescription = "体重(公斤)")]
    public int Weight { get; set; }

    [SugarColumn(ColumnName = "height", DecimalDigits = 2, Length = 3, ColumnDescription = "身高(米)")]
    public decimal Height { get; set; }

    [SugarColumn(ColumnName = "created_time", IsOnlyIgnoreUpdate = true)]
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;//时间戳
}

public enum EnumGender
{
    男,
    女
}

