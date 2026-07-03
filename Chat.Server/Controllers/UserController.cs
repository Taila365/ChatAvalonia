using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using Chat.Application.Services;
using System.Threading.Tasks;

namespace Chat.Server.Controllers;

[NonUnify]
public class UserController : IDynamicApiController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 用户注册接口
    /// 请求地址：POST http://127.0.0.1:8081/api/user/register
    /// </summary>
    /// <param name="data">表单提交账号密码</param>
    /// <returns>true=注册成功，false=用户名已存在/参数非法</returns>
    [HttpPost("register")]
    public async Task<bool> Register([FromForm] RegisterRequest data)
    {
        // 基础参数校验
        if (string.IsNullOrWhiteSpace(data.UserName) || string.IsNullOrWhiteSpace(data.Password))
            return false;

        // 调用服务层执行注册
        bool createResult = await _userService.CreateUserAsync(data.UserName, data.Password);
        return createResult;
    }
}

// 注册接收表单DTO，放在当前文件底部或单独Dto文件
public class RegisterRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}