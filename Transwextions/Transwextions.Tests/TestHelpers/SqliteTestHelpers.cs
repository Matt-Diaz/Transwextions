using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Transwextions.Data;

namespace Transwextions.Tests.TestHelpers;

public static class SqliteTestHelpers
{
    public static (SqliteConnection Conn, TranswextionsContext Db) CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:;Mode=Memory;Cache=Shared");
        connection.Open();

        var options = new DbContextOptionsBuilder<TranswextionsContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new TranswextionsContext(options);
        db.Database.Migrate();
        return (connection, db);
    }
}