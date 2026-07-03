using Chat.Core.Models;

namespace Chat.Application.Services;

public interface IStudentService
{
    Task<bool> TestOrmAsync();

    Task<List<Student>> GetStudentsAsync();
}
