using ELibrary.Application.Interfaces;
using ELibrary.Application.Services;
using ELibrary.Infrastructure;
using ELibrary.Infrastructure.Repositories;
using ELibrary.Domain.Interfaces;
using ELibrary.Domain.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
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
    // SERVICES CONFIGURATION
    // ============================================================================

    builder.Services.AddControllers();

    // Database configuration
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? "Data Source=ELibrary.db";

    builder.Services.AddDbContext<ELibraryDbContext>(options =>
        options.UseSqlite(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("ELibrary.Infrastructure")));

    // Repository
    builder.Services.AddScoped<IBookRepository, BookRepository>();
    builder.Services.AddScoped<IBorrowBookRecordRepository, BorrowBookRepository>();

    // Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Application layer
    builder.Services.AddScoped<IBookService, BookService>();
    builder.Services.AddScoped<IBorrowBookRecordService, BorrowBookRecordService>();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<BookDtoValidator>();

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "E-Library API",
            Version = "v1",
            Description = "E-Library Management System API with FluentValidation and Serilog"
        });
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazorClient",
            policy =>
            {
                policy.WithOrigins("https://localhost:7002", "http://localhost:5002")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
    });

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