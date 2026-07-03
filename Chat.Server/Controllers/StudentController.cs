using Chat.Application.Dtos;
using Chat.Application.Services;
using Furion.DynamicApiController;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Server.Controllers;

[NonUnify]
public class StudentController : IDynamicApiController
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public async Task<string> Get()
    {
        var ret = await _studentService.TestOrmAsync();

        return $"service调用完毕：{ret}";
    }

    [HttpPost]
    public async Task<List<StudentDto>> Students()
    {
        var students = await _studentService.GetStudentsAsync();
        return students.Adapt<List<StudentDto>>();
    }
}
