using ELibrary.Database;
using ELibrary.Database.Repositories;
using ELibrary.Shared.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Database configuration - SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=ELibrary.db";

builder.Services.AddDbContext<ELibraryDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ELibrary.Database")));

// Repository
builder.Services.AddScoped<IBookRepository, BookRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Apply migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ELibraryDbContext>();
        context.Database.Migrate();

        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(exception, "Unhandled exception");

        var result = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.InternalServerError,        
            Content = new StringContent(new
            {
                error = exception?.Message,
                stackTrace = exception?.StackTrace
            }.ToString() ?? string.Empty),        
        };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            // Add more exception types as needed
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(result);
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Logger.LogInformation("E-Library API is starting...");
app.Logger.LogInformation("Swagger UI available at: https://localhost:7001/swagger"); //TODO: Setup swagger

app.Run();
