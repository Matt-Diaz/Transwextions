using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Transwextions.Data.Extensions;

public static class DatabaseSetupExtensions
{
    /// <summary>
    /// Registers SQLite and ensures data directory exists
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="fileName">Directory for persistent SQL Lite data.</param>
    /// <returns></returns>
    public static IServiceCollection AddAppDatabase(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        string fileName = "app.db")
    {
        var dataDirectory = Path.Combine(hostEnvironment.ContentRootPath, "data");
        Directory.CreateDirectory(dataDirectory);
        var connectionString = $"Data Source={Path.Combine(dataDirectory, fileName)}";

        services.AddDbContextFactory<TranswextionsContext>(opt => opt.UseSqlite(connectionString));

        return services;
    }

    /// <summary>
    /// Applies migrations
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static async Task MigrateDatabase(
        this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TranswextionsContext>>();
        await using var context = await factory.CreateDbContextAsync();

        await context.Database.MigrateAsync();
    }
}