using Configuration.API.DTOs;
using Configuration.API.Entities;
using Configuration.API.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Configuration.API.Controllers;

[ApiController]
[Route("api/v1/config")]
public class ConfigurationController : ControllerBase
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(ConfigurationDbContext context, ILogger<ConfigurationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{organizationId}")]
    public async Task<ActionResult<ConfigurationResponse>> GetConfiguration(Guid organizationId)
    {
        var settings = await _context.OrganizationSettings
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync();

        var settingsDict = settings.ToDictionary(x => x.Key, x => x.Value);
        var lastUpdated = settings.Any() ? settings.Max(x => x.UpdatedAt ?? x.CreatedAt) : (DateTime?)null;

        return Ok(new ConfigurationResponse(organizationId, settingsDict, lastUpdated));
    }

    [HttpPut("{organizationId}")]
    public async Task<ActionResult<ConfigurationResponse>> UpdateConfiguration(
        Guid organizationId,
        [FromBody] UpdateConfigurationRequest request)
    {
        if (request.Settings == null || !request.Settings.Any())
        {
            throw new ArgumentException("Settings dictionary cannot be empty.");
        }

        var existingSettings = await _context.OrganizationSettings
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var kvp in request.Settings)
        {
            var existing = existingSettings.FirstOrDefault(x => x.Key == kvp.Key);
            if (existing != null)
            {
                _logger.LogInformation(
                    "Updating setting {Key} for organization {OrganizationId}. Old value: {OldValue}, New value: {NewValue}",
                    kvp.Key, organizationId, existing.Value, kvp.Value);

                existing.Value = kvp.Value;
                existing.UpdatedAt = now;
            }
            else
            {
                _logger.LogInformation(
                    "Creating setting {Key} for organization {OrganizationId} with value: {Value}",
                    kvp.Key, organizationId, kvp.Value);

                var newSetting = new OrganizationSetting
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Key = kvp.Key,
                    Value = kvp.Value,
                    CreatedAt = now
                };
                _context.OrganizationSettings.Add(newSetting);
            }
        }

        await _context.SaveChangesAsync();

        var updatedSettings = await _context.OrganizationSettings
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync();

        var settingsDict = updatedSettings.ToDictionary(x => x.Key, x => x.Value);
        var lastUpdated = updatedSettings.Max(x => x.UpdatedAt ?? x.CreatedAt);

        return Ok(new ConfigurationResponse(organizationId, settingsDict, lastUpdated));
    }

    [HttpGet("{organizationId}/features")]
    public async Task<ActionResult<List<FeatureFlagResponse>>> GetFeatures(Guid organizationId)
    {
        var features = await _context.FeatureFlags
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Key)
            .Select(x => new FeatureFlagResponse(x.Key, x.Description, x.IsEnabled, x.UpdatedAt))
            .ToListAsync();

        return Ok(features);
    }

    [HttpPut("{organizationId}/features/{featureKey}")]
    public async Task<ActionResult<FeatureFlagResponse>> ToggleFeature(
        Guid organizationId,
        string featureKey,
        [FromBody] ToggleFeatureRequest request)
    {
        var now = DateTime.UtcNow;
        var feature = await _context.FeatureFlags
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Key == featureKey);

        if (feature != null)
        {
            _logger.LogInformation(
                "Updating feature flag {FeatureKey} for organization {OrganizationId}. Old enabled: {OldEnabled}, New enabled: {NewEnabled}",
                featureKey, organizationId, feature.IsEnabled, request.IsEnabled);

            feature.IsEnabled = request.IsEnabled;
            feature.UpdatedAt = now;

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                feature.Description = request.Description;
            }
        }
        else
        {
            _logger.LogInformation(
                "Creating feature flag {FeatureKey} for organization {OrganizationId} with enabled: {Enabled}",
                featureKey, organizationId, request.IsEnabled);

            feature = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Key = featureKey,
                Description = request.Description ?? featureKey,
                IsEnabled = request.IsEnabled,
                CreatedAt = now
            };
            _context.FeatureFlags.Add(feature);
        }

        await _context.SaveChangesAsync();

        return Ok(new FeatureFlagResponse(feature.Key, feature.Description, feature.IsEnabled, feature.UpdatedAt));
    }

    [HttpGet("health")]
    public ActionResult<HealthResponse> GetHealth()
    {
        return Ok(new HealthResponse("Configuration", "Healthy", DateTime.UtcNow));
    }
}
