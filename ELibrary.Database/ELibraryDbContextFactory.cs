
using ELibrary.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Factory class for creating ELibraryDbContext instances at design time.
/// Used by Entity Framework Core tools for migrations and database updates.
/// </summary>
public class ELibraryDbContextFactory : IDesignTimeDbContextFactory<ELibraryDbContext>
{
    /// <summary>
    /// Creates a new instance of ELibraryDbContext.
    /// Reads connection string from appsettings.json in the WebApi project.
    /// </summary>
    /// <param name="args">Command line arguments (not used)</param>
    /// <returns>A new instance of ELibraryDbContext configured with the application's connection string</returns>
    public ELibraryDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "../ELibrary.Api"
        );

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var builder = new DbContextOptionsBuilder<ELibraryDbContext>();
        builder.UseSqlite(connectionString);

        return new ELibraryDbContext(builder.Options);
    }
}
