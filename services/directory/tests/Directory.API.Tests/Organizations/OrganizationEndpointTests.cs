using System.Net;
using System.Net.Http.Json;
using Directory.API.Tests.Extensions;
using Directory.API.Tests.Fixtures;
using FluentAssertions;

namespace Directory.API.Tests.Organizations;

[Collection("Integration")]
[Trait("Category", "Integration")]
public class OrganizationEndpointTests
{
    private readonly HttpClient _client;

    public OrganizationEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static string UniqueSlug() => $"test-org-{Guid.NewGuid():N}"[..30];

    [Fact]
    public async Task CreateOrganization_WithValidData_ReturnsCreatedWithOrganization()
    {
        // Arrange
        var slug = UniqueSlug();
        var request = new { Name = "Test Organization", Slug = slug };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var org = await response.ReadAsAsync<OrganizationDto>();
        org.Id.Should().NotBeEmpty();
        org.Name.Should().Be("Test Organization");
        org.Slug.Should().Be(slug);
        org.Status.Should().Be("Active");
        org.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task CreateOrganization_WithSettings_ReturnsOrganizationWithSettings()
    {
        // Arrange
        var slug = UniqueSlug();
        var request = new
        {
            Name = "Org With Settings",
            Slug = slug,
            Settings = new Dictionary<string, string>
            {
                ["theme"] = "dark",
                ["language"] = "en"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var org = await response.ReadAsAsync<OrganizationDto>();
        org.Settings.Should().ContainKey("theme").WhoseValue.Should().Be("dark");
        org.Settings.Should().ContainKey("language").WhoseValue.Should().Be("en");
    }

    [Fact]
    public async Task CreateOrganization_WithDuplicateSlug_ReturnsConflict()
    {
        // Arrange
        var slug = UniqueSlug();
        await _client.PostAsJsonAsync("/api/v1/organizations", new { Name = "First Org", Slug = slug });

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/organizations", new { Name = "Second Org", Slug = slug });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateOrganization_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new { Name = "", Slug = UniqueSlug() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrganization_WithInvalidSlug_ReturnsBadRequest()
    {
        // Arrange — slug must be lowercase alphanumeric with hyphens
        var request = new { Name = "Test Org", Slug = "INVALID SLUG!!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/organizations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrganization_WithExistingId_ReturnsOrganization()
    {
        // Arrange
        var slug = UniqueSlug();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Get Test Org", Slug = slug });
        var created = await createResponse.ReadAsAsync<OrganizationDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/organizations/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var org = await response.ReadAsAsync<OrganizationDto>();
        org.Id.Should().Be(created.Id);
        org.Name.Should().Be("Get Test Org");
        org.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task GetOrganization_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/organizations/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOrganization_WithValidData_ReturnsUpdatedOrganization()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Original Name", Slug = UniqueSlug() });
        var created = await createResponse.ReadAsAsync<OrganizationDto>();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/organizations/{created.Id}",
            new { Name = "Updated Name" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.ReadAsAsync<OrganizationDto>();
        updated.Name.Should().Be("Updated Name");
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOrganization_WithSettings_ReturnsUpdatedSettings()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Settings Org", Slug = UniqueSlug(), Settings = new Dictionary<string, string> { ["key1"] = "val1" } });
        var created = await createResponse.ReadAsAsync<OrganizationDto>();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/organizations/{created.Id}",
            new { Name = "Settings Org", Settings = new Dictionary<string, string> { ["key1"] = "updated", ["key2"] = "new" } });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.ReadAsAsync<OrganizationDto>();
        updated.Settings.Should().ContainKey("key1").WhoseValue.Should().Be("updated");
        updated.Settings.Should().ContainKey("key2").WhoseValue.Should().Be("new");
    }

    [Fact]
    public async Task UpdateOrganization_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/organizations/{Guid.NewGuid()}",
            new { Name = "Updated" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOrganization_ThatExists_ReturnsNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "To Delete", Slug = UniqueSlug() });
        var created = await createResponse.ReadAsAsync<OrganizationDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/organizations/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteOrganization_ThenGet_ReturnsNotFound()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Delete Then Get", Slug = UniqueSlug() });
        var created = await createResponse.ReadAsAsync<OrganizationDto>();

        await _client.DeleteAsync($"/api/v1/organizations/{created.Id}");

        // Act
        var response = await _client.GetAsync($"/api/v1/organizations/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOrganization_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/v1/organizations/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

// Internal DTO for deserializing responses (avoids coupling to actual response types)
file record OrganizationDto(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    Dictionary<string, string> Settings,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
