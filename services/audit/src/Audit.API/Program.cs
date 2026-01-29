using Audit.API.Middleware;
using Audit.Application;
using Audit.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value);

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

app.Run();
