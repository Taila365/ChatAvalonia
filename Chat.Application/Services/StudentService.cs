using Chat.Core.Models;
using SqlSugar;

namespace Chat.Application.Services;

public class StudentService : IStudentService
{
    private readonly ISqlSugarClient _db;

    public StudentService(ISqlSugarClient db)
    {
        _db = db;

       // _db.CodeFirst.InitTables<Student>();   //触发建表
    }

    public async Task<List<Student>> GetStudentsAsync()
    {
        return await _db.Queryable<Student>().ToListAsync();
    }

    public async Task<bool> TestOrmAsync()
    {
        

        var data = await _db.Queryable<Student>()
            .Where(w => DateTime.Now.Year - w.Birthday.Year > 21)
            .ToListAsync();


        var date1 = new DateTime(2025, 1, 1);
        var date2 = new DateTime(2026, 1, 1);
        var d = date2 - date1;
        var x = d.TotalDays;



        return true;
    }




}


