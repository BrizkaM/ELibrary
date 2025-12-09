using System.Reflection;
using ELibrary.Application.Interfaces;
using ELibrary.Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace ELibrary.Application
{
    /// <summary>
    /// Dependency injection configuration for Application layer.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers Application layer services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Application Services
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBorrowBookRecordService, BorrowBookRecordService>();

            // AutoMapper - scans assembly containing BookService type
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(assembly);
            });

            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}