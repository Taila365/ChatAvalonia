using Chat.Application.Dtos;
using Chat.Application.Services;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Chat.Server.Controllers;

[NonUnify]
[Route("api")]
[Produces("application/json")]  // ✅ 强制返回 JSON
public class LoginController : IDynamicApiController
{
    private readonly IUserService _userService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(IUserService userService, ILogger<LoginController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<LoginResponse> Login([FromForm] LoginRequest data)
    {
        _logger.LogInformation($"收到登录表单：UserName={data.UserName}, Pwd={data.Password}");

        if (string.IsNullOrWhiteSpace(data.UserName) || string.IsNullOrWhiteSpace(data.Password))
        {
            _logger.LogWarning("表单参数为空");
            return new LoginResponse { Success = false, Message = "用户名或密码不能为空" };
        }

        var ok = await _userService.ValidateCredentialsAsync(data.UserName, data.Password);
        if (ok)
        {
            _logger.LogInformation($"账号{data.UserName}验证成功");
            return new LoginResponse { Success = true, Message = "验证通过" };
        }

        _logger.LogWarning($"账号{data.UserName}密码校验失败");
        return new LoginResponse { Success = false, Message = "账号或密码错误" };
    }

    [HttpPost("register")]
    public async Task<LoginResponse> Register([FromForm] LoginRequest data)
    {
        _logger.LogInformation($"收到注册表单：UserName={data.UserName}, Pwd={data.Password}");

        if (string.IsNullOrWhiteSpace(data.UserName) || string.IsNullOrWhiteSpace(data.Password))
        {
            return new LoginResponse { Success = false, Message = "用户名或密码不能为空" };
        }

        var success = await _userService.CreateUserAsync(data.UserName, data.Password);
        if (!success)
        {
            return new LoginResponse { Success = false, Message = "用户名已存在" };
        }

        return new LoginResponse { Success = true, Message = "注册成功" };
    }
}