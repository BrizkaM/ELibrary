using ELibrary.Domain.Interfaces;
using ELibrary.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ELibrary.Infrastructure
{
    /// <summary>
    /// Dependency injection configuration for Infrastructure layer.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers Infrastructure layer services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? "Data Source=ELibrary.db";

            services.AddDbContext<ELibraryDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly("ELibrary.Infrastructure")));

            // Repositories
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBorrowBookRecordRepository, BorrowBookRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}