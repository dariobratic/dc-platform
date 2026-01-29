using Gateway.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Configure CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Simple health endpoint with detailed response
app.MapGet("/api/health", () =>
{
    return Results.Ok(new HealthResponse
    {
        ServiceName = "Gateway",
        Status = "Healthy",
        Timestamp = DateTime.UtcNow
    });
});

app.Run();
