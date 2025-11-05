using Microsoft.EntityFrameworkCore;
using HealthChecks.UI.Client;
using OrderManagement.Api.Middleware;
using OrderManagement.Application;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Data;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Services.AddSerilog(builder.Configuration);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Order Management API", 
        Version = "v1",
        Description = "API for managing orders and products"
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger for development and local testing
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
        c.RoutePrefix = "swagger";
    });
}

// Add middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

app.UseAuthorization();

// Add health checks endpoint
app.MapHealthChecks("/health", new()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();

// Run database migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        Log.Information("Starting database migration...");
        await context.Database.MigrateAsync();
        Log.Information("Database migration completed");
        
        Log.Information("Starting database seeding...");
        await DbSeeder.SeedAsync(context);
        Log.Information("Database seeding completed");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

Log.Information("Starting Order Management API...");
app.Run();