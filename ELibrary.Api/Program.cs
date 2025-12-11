using ELibrary.Application;
using ELibrary.Infrastructure;
using ELibrary.Api;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// ============================================================================
// SERILOG EARLY INITIALIZATION
// ============================================================================
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
	.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
	.MinimumLevel.Override("System", LogEventLevel.Warning)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.CreateBootstrapLogger();

Log.Information("Starting ELibrary API application");

try
{
	var builder = WebApplication.CreateBuilder(args);

	// ============================================================================
	// SERILOG FULL CONFIGURATION
	// ============================================================================
	builder.Host.UseSerilog((context, services, configuration) => configuration
		.ReadFrom.Configuration(context.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext()
		.Enrich.WithMachineName()
		.Enrich.WithEnvironmentName()
		// ODSTRANÌNO: .Enrich.WithThreadId() - zpùsobuje chybu
		.Enrich.WithProperty("Application", "ELibrary")
		.Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)

		// Console sink
		.WriteTo.Console(
			outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")

		// File sink
		.WriteTo.File(
			path: "logs/elibrary-.log",
			rollingInterval: RollingInterval.Day,
			retainedFileCountLimit: 30,
			outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")

		// Seq sink (optional)
		.WriteTo.Seq(
			serverUrl: context.Configuration["Serilog:SeqServerUrl"] ?? "http://localhost:5341",
			apiKey: context.Configuration["Serilog:SeqApiKey"])
	);

    // ============================================================================
    // API VERSIONING
    // ============================================================================
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });


    // ============================================================================
    // SERVICES CONFIGURATION
    // ============================================================================

    // Infrastructure Layer (Database, Repositories, Unit of Work)
    builder.Services.AddInfrastructure(builder.Configuration);

	// Application Layer (Services, AutoMapper, FluentValidation)
	builder.Services.AddApplication();

    // HEALTH CHECKS
    builder.Services.AddHealthChecks()
        .AddCheck("self", () =>
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"))
        .AddDbContextCheck<ELibraryDbContext>(
            name: "database",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
            tags: new[] { "db", "sql", "sqlite" });

    // Presentation Layer (Controllers, Swagger, CORS)
    builder.Services.AddPresentation(builder.Configuration, builder.Environment);

	var app = builder.Build();

	// ============================================================================
	// MIDDLEWARE PIPELINE
	// ============================================================================

	app.UseCors("AllowBlazorClient");

	// Serilog Request Logging
	app.UseSerilogRequestLogging(options =>
	{
		options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

		options.GetLevel = (httpContext, elapsed, ex) => ex != null
			? LogEventLevel.Error
			: httpContext.Response.StatusCode > 499
				? LogEventLevel.Error
				: LogEventLevel.Information;

		options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
		{
			diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
			diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
			diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
			diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
		};
	});

	// Database migrations
	using (var scope = app.Services.CreateScope())
	{
		var services = scope.ServiceProvider;
		try
		{
			var context = services.GetRequiredService<ELibraryDbContext>();

			Log.Information("Applying database migrations");
			context.Database.Migrate();

			Log.Information("Database initialized successfully");
		}
		catch (Exception ex)
		{
			Log.Error(ex, "An error occurred while migrating the database");
			throw;
		}
	}

    // HEALTH CHECKS ENDPOINTS
    app.MapHealthChecks("/health");

    // Detailed health check with JSON response
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    });

    // Liveness check (lightweight)
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("self") || check.Name == "self"
    });

    Log.Information("Health check endpoints configured: /health, /health/ready, /health/live");


    // Swagger UI
    if (app.Environment.IsDevelopment())
	{
		Log.Information("Starting in Development mode - Swagger UI enabled");

		app.UseSwagger();
		app.UseSwaggerUI(c =>
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Library API v1");
			c.RoutePrefix = string.Empty;
		});
	}
	else
	{
		Log.Information("Starting in {Environment} mode", app.Environment.EnvironmentName);
	}

	// Global exception handling
	app.UseExceptionHandler(errorApp =>
	{
		errorApp.Run(async context =>
		{
			var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
			var exception = exceptionHandlerFeature?.Error;

			Log.Error(exception,
				"Unhandled exception. Path: {Path}, Method: {Method}",
				context.Request.Path,
				context.Request.Method);

			if (exception is FluentValidation.ValidationException fluentValidationException)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;

				var errors = fluentValidationException.Errors
					.GroupBy(e => e.PropertyName)
					.ToDictionary(
						g => g.Key,
						g => g.Select(e => e.ErrorMessage).ToArray()
					);

				await context.Response.WriteAsJsonAsync(new
				{
					type = "ValidationError",
					title = "One or more validation errors occurred",
					status = 400,
					errors = errors
				});
				return;
			}

			context.Response.StatusCode = exception switch
			{
				ArgumentException => StatusCodes.Status400BadRequest,
				KeyNotFoundException => StatusCodes.Status404NotFound,
				DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
				_ => StatusCodes.Status500InternalServerError
			};

			await context.Response.WriteAsJsonAsync(new
			{
				type = "Error",
				title = "An error occurred",
				status = context.Response.StatusCode,
				detail = app.Environment.IsDevelopment() ? exception?.Message : "An error occurred processing your request",
				instance = context.Request.Path.ToString()
			});
		});
	});

	app.UseHttpsRedirection();
	app.UseAuthorization();
	app.MapControllers();

	Log.Information("E-Library API started successfully");
	Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.Information("Application shutting down");
	Log.CloseAndFlush();
}