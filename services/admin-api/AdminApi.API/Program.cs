using AdminApi.API.Middleware;
using AdminApi.API.Models;
using AdminApi.API.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Typed HttpClients for downstream services
builder.Services.AddHttpClient<IDirectoryServiceClient, DirectoryServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Directory"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IAuditServiceClient, AuditServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Audit"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Service health checker uses IHttpClientFactory directly
builder.Services.AddHttpClient();
builder.Services.AddScoped<IServiceHealthChecker, ServiceHealthChecker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? string.Empty);

        var userId = httpContext.User?.FindFirst("sub")?.Value;
        if (userId != null)
            diagnosticContext.Set("UserId", userId);

        var orgId = httpContext.Request.Headers["X-Organization-Id"].FirstOrDefault();
        if (orgId != null)
            diagnosticContext.Set("OrganizationId", orgId);
    };
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");
app.MapGet("/api/health", () =>
{
    return Results.Ok(new HealthResponse(
        ServiceName: "AdminApi",
        Status: "Healthy",
        Timestamp: DateTime.UtcNow));
});

app.Run();
