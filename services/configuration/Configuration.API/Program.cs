using Configuration.API.Middleware;
using Configuration.API.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Register DbContext
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ConfigurationDb")
        ?? throw new InvalidOperationException("Connection string 'ConfigurationDb' not found.");
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    await db.Database.MigrateAsync();
}

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

        var wsId = httpContext.Request.Headers["X-Workspace-Id"].FirstOrDefault();
        if (wsId != null)
            diagnosticContext.Set("WorkspaceId", wsId);
    };
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
