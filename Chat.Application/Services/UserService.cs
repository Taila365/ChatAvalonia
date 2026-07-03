using Chat.Core.Models;
using SqlSugar;

namespace Chat.Application.Services;

public class UserService : IUserService
{
    private readonly ISqlSugarClient _db;

    public UserService(ISqlSugarClient db)
    {
        _db = db;
        // 移除此处CodeFirst建表，放到Program程序启动时一次性执行
        // _db.CodeFirst.InitTables<User>();
    }

    /// <summary>
    /// 同步创建用户
    /// </summary>
    public bool CreateUser(string userName, string password)
    {
        string name = userName.Trim();
        string pwd = password.Trim();

        var existUser = _db.Queryable<User>()
            .First(w => w.UserName == name);
        if (existUser != null)
            return false;

        var user = new User
        {
            UserName = name,
            Password = pwd
        };

        int affectRows = _db.Insertable(user).ExecuteCommand();
        return affectRows == 1;
    }

    /// <summary>
    /// 异步创建用户
    /// </summary>
    public async Task<bool> CreateUserAsync(string userName, string password)
    {
        string name = userName.Trim();
        string pwd = password.Trim();

        var existUser = await _db.Queryable<User>()
            .Where(u => SqlFunc.Trim(u.UserName) == name)
            .FirstAsync();

        if (existUser != null)
            return false;

        var user = new User
        {
            UserName = name,
            Password = pwd
        };

        int rows = await _db.Insertable(user).ExecuteCommandAsync();
        return rows == 1;
    }

    /// <summary>
    /// 根据用户名修改密码
    /// </summary>
    public async Task<bool> EditUserAsync(string userName, string password)
    {
        string name = userName.Trim();
        string newPwd = password.Trim();

        int affectRows = await _db.Updateable<User>()
            .SetColumns(it => new User() { Password = newPwd })
            .Where(it => it.UserName == name)
            .ExecuteCommandAsync();

        return affectRows == 1;
    }

    /// <summary>
    /// 账号密码校验登录
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string userName, string password)
    {
        string name = userName.Trim();
        string pwd = password.Trim();

        // SqlFunc.Trim 数据库层去除字段前后空格，解决肉眼一致但匹配失败问题
        var user = await _db.Queryable<User>()
            .FirstAsync(w => SqlFunc.Trim(w.UserName) == name && SqlFunc.Trim(w.Password) == pwd);

        return user != null;
    }
}