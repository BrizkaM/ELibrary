using ELibrary.Api.Configuration;

namespace ELibrary.Api.Extensions;

/// <summary>
/// Extension methods for configuration setup.
/// Handles environment variable substitution and strongly-typed binding.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds and configures all application settings with environment variable support.
    /// </summary>
    public static IServiceCollection AddApplicationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind and register Keycloak settings
        var keycloakSettings = new KeycloakSettings();
        configuration.GetSection(KeycloakSettings.SectionName).Bind(keycloakSettings);
        
        // Environment variable override for Authority
        var authorityEnv = Environment.GetEnvironmentVariable("KEYCLOAK_AUTHORITY");
        if (!string.IsNullOrEmpty(authorityEnv))
        {
            keycloakSettings.Authority = authorityEnv;
        }
        
        // Environment variable override for Audience
        var audienceEnv = Environment.GetEnvironmentVariable("KEYCLOAK_AUDIENCE");
        if (!string.IsNullOrEmpty(audienceEnv))
        {
            keycloakSettings.Audience = audienceEnv;
        }
        
        services.AddSingleton(keycloakSettings);
        
        // Bind and register CORS settings
        var corsSettings = new CorsSettings();
        configuration.GetSection(CorsSettings.SectionName).Bind(corsSettings);
        
        // Environment variable override for CORS origins (comma-separated)
        var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS_ORIGINS");
        if (!string.IsNullOrEmpty(corsOriginsEnv))
        {
            corsSettings.AllowedOrigins = corsOriginsEnv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        
        services.AddSingleton(corsSettings);
        
        // Bind and register Health Check settings
        var healthCheckSettings = new HealthCheckSettings();
        configuration.GetSection(HealthCheckSettings.SectionName).Bind(healthCheckSettings);
        services.AddSingleton(healthCheckSettings);
        
        // Bind and register Rate Limiting settings
        var rateLimitingSettings = new RateLimitingSettings();
        configuration.GetSection(RateLimitingSettings.SectionName).Bind(rateLimitingSettings);
        services.AddSingleton(rateLimitingSettings);
        
        return services;
    }
    
    /// <summary>
    /// Gets connection string with environment variable fallback.
    /// Supports ${VARIABLE_NAME} syntax in appsettings.
    /// </summary>
    public static string GetConnectionStringWithFallback(
        this IConfiguration configuration,
        string name = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(name);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            return string.Empty;
        }
        
        // Check for environment variable placeholder
        if (connectionString.StartsWith("${") && connectionString.EndsWith("}"))
        {
            var envVarName = connectionString.Substring(2, connectionString.Length - 3);
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            
            if (string.IsNullOrEmpty(envValue))
            {
                throw new InvalidOperationException(
                    $"Environment variable '{envVarName}' is not set. " +
                    $"Please set it or update the connection string in appsettings.");
            }
            
            return envValue;
        }
        
        return connectionString;
    }
    
    /// <summary>
    /// Validates that all required configuration is present.
    /// Call this during startup to fail fast if configuration is missing.
    /// </summary> 
    public static void ValidateConfiguration(this IServiceProvider services, IWebHostEnvironment environment)
    {
        var keycloakSettings = services.GetRequiredService<KeycloakSettings>();
        var corsSettings = services.GetRequiredService<CorsSettings>();
        
        var errors = new List<string>();
        
        // Validate Keycloak settings (required for non-Development)
        if (!environment.IsDevelopment())
        {
            if (string.IsNullOrEmpty(keycloakSettings.Authority))
            {
                errors.Add("Keycloak Authority is not configured. Set KEYCLOAK_AUTHORITY environment variable.");
            }
        }
        
        // Validate CORS settings
        if (corsSettings.AllowedOrigins.Length == 0)
        {
            errors.Add("No CORS origins configured. Set CORS_ORIGINS environment variable.");
        }
        
        if (errors.Any())
        {
            throw new InvalidOperationException(
                "Configuration validation failed:\n" + string.Join("\n", errors.Select(e => $"  - {e}")));
        }
    }
}

