using Microsoft.OpenApi.Models;

namespace ELibrary.Api
{
    /// <summary>
    /// Dependency injection configuration for Presentation (API) layer.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers Presentation layer services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="environment">The hosting environment.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddPresentation(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            // Controllers
            services.AddControllers();

            // Swagger/OpenAPI - only in Development
            if (environment.IsDevelopment())
            {
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "E-Library API",
                        Version = "v1",
                        Description = "E-Library Management System API with Clean Architecture, CQRS, FluentValidation and Serilog",
                        Contact = new OpenApiContact
                        {
                            Name = "E-Library Team",
                            Email = "dev@elibrary.com"
                        }
                    });

                    // Optional: Include XML comments
                    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    // c.IncludeXmlComments(xmlPath);
                });
            }

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorClient",
                    policy =>
                    {
                        policy.WithOrigins(
                                "https://localhost:7002",
                                "http://localhost:5002")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            return services;
        }
    }
}