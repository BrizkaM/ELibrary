namespace ELibrary.Api.Configuration;

/// <summary>
/// Keycloak authentication configuration settings.
/// Loaded from appsettings.json Authentication:Keycloak section.
/// </summary>
public class KeycloakSettings
{
    public const string SectionName = "Authentication:Keycloak";
    
    /// <summary>
    /// Keycloak realm URL (e.g., http://localhost:8080/realms/elibrary)
    /// </summary>
    public string Authority { get; set; } = string.Empty;
    
    /// <summary>
    /// Expected audience claim in the token
    /// </summary>
    public string Audience { get; set; } = "elibrary-api";
    
    /// <summary>
    /// Client ID for this API
    /// </summary>
    public string ClientId { get; set; } = "elibrary-api";
    
    /// <summary>
    /// Whether to require HTTPS for metadata endpoint
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;
    
    /// <summary>
    /// Whether to validate the issuer claim
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;
    
    /// <summary>
    /// Whether to validate the audience claim
    /// </summary>
    public bool ValidateAudience { get; set; } = true;
    
    /// <summary>
    /// Whether to validate token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;
    
    /// <summary>
    /// Clock skew tolerance in seconds
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 0;
}

/// <summary>
/// CORS configuration settings.
/// Loaded from appsettings.json Cors section.
/// </summary>
public class CorsSettings
{
    public const string SectionName = "Cors";
    
    /// <summary>
    /// List of allowed origins for CORS.
    /// Can be a string array in JSON or comma-separated string from environment variable.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Health checks configuration settings.
/// </summary>
public class HealthCheckSettings
{
    public const string SectionName = "HealthChecks";
    
    /// <summary>
    /// Whether health checks are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Whether to include detailed error information
    /// </summary>
    public bool DetailedErrors { get; set; } = false;
}

/// <summary>
/// Rate limiting configuration settings.
/// </summary>
public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";
    
    /// <summary>
    /// Whether rate limiting is enabled
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;
    
    /// <summary>
    /// Number of requests permitted per window
    /// </summary>
    public int PermitLimit { get; set; } = 100;
    
    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int WindowSeconds { get; set; } = 60;
}
