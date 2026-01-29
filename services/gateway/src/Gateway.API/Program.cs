using Gateway.API.Middleware;
using Gateway.API.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value);
    };
});
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");
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
