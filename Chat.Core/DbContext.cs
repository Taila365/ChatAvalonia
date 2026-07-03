using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Npgsql;
using SqlSugar;

namespace Chat.Core;

public static class DbContext
{
   
    public static void AddMySqlSetup(this IServiceCollection services)
    {
        var connStr = new MySqlConnectionStringBuilder()
        {
            Server = "127.0.0.1",
            UserID = "root",
            Password = "123456",
            Database = "chat_db",
            Port = 3306,
            SslMode = MySqlSslMode.None,
            Pooling = true,
            CharacterSet = "utf8"
        }.ToString();

        var configConnection = new ConnectionConfig()
        {
            DbType = DbType.MySql,
            //ConnectionString = "Data Source=localhost;Database=Furion;User ID=root;Password=000000;pooling=true;port=3306;sslmode=none;CharSet=utf8;",
            ConnectionString = connStr,
            IsAutoCloseConnection = true
        };

        SqlSugarScope sqlSugar = new SqlSugarScope(configConnection,
        db =>
        {
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                 Console.WriteLine(sql);//输出sql
            };
        });

        services.AddSingleton<ISqlSugarClient>(sqlSugar);
    }
}
