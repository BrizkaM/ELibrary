using System.Reflection;
using ELibrary.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ELibrary.Application
{
    /// <summary>
    /// Dependency injection configuration for Application layer with MediatR.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers Application layer services including MediatR, AutoMapper, and FluentValidation.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // MediatR - Register all handlers and behaviors from assembly
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);

                // Register Pipeline Behaviors (order matters!)
                // 1. Logging (first - log everything)
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));

                // 2. Validation (validate before processing)
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

                // 3. Performance (monitor performance)
                cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));

                // 4. Transaction (wrap Commands in transactions)
                cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            });

            // AutoMapper - scans assembly for mapping profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(assembly);
            });

            // FluentValidation - Automatic validation for Commands and Queries
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
