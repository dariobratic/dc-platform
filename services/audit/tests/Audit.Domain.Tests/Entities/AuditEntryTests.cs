using Audit.Domain.Entities;
using Audit.Domain.Events;
using Audit.Domain.Exceptions;
using Xunit;

namespace Audit.Domain.Tests.Entities;

public class AuditEntryTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        const string action = "organization.created";
        const string entityType = "Organization";
        const string serviceName = "Directory";
        const string userEmail = "user@example.com";
        const string details = "{\"before\":null,\"after\":{\"name\":\"Acme\"}}";
        const string ipAddress = "192.168.1.1";
        const string userAgent = "Mozilla/5.0";
        const string correlationId = "abc-123";

        // Act
        var entry = AuditEntry.Create(
            userId,
            action,
            entityType,
            entityId,
            serviceName,
            userEmail,
            organizationId,
            workspaceId,
            details,
            ipAddress,
            userAgent,
            correlationId);

        // Assert
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(userId, entry.UserId);
        Assert.Equal(userEmail, entry.UserEmail);
        Assert.Equal(action, entry.Action);
        Assert.Equal(entityType, entry.EntityType);
        Assert.Equal(entityId, entry.EntityId);
        Assert.Equal(organizationId, entry.OrganizationId);
        Assert.Equal(workspaceId, entry.WorkspaceId);
        Assert.Equal(details, entry.Details);
        Assert.Equal(ipAddress, entry.IpAddress);
        Assert.Equal(userAgent, entry.UserAgent);
        Assert.Equal(serviceName, entry.ServiceName);
        Assert.Equal(correlationId, entry.CorrelationId);
    }

    [Fact]
    public void Create_ShouldSetTimestampToUtcNow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var entry = AuditEntry.Create(
            userId,
            "test.action",
            "TestEntity",
            entityId,
            "TestService");
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(entry.Timestamp >= before && entry.Timestamp <= after);
    }

    [Fact]
    public void Create_ShouldGenerateNewId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        var entry = AuditEntry.Create(
            userId,
            "test.action",
            "TestEntity",
            entityId,
            "TestService");

        // Assert
        Assert.NotEqual(Guid.Empty, entry.Id);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        const string action = "test.action";
        const string entityType = "TestEntity";
        const string serviceName = "TestService";

        // Act
        var entry = AuditEntry.Create(
            userId,
            action,
            entityType,
            entityId,
            serviceName);

        // Assert
        Assert.Single(entry.DomainEvents);
        var domainEvent = entry.DomainEvents.First() as AuditEntryCreated;
        Assert.NotNull(domainEvent);
        Assert.Equal(entry.Id, domainEvent.AuditEntryId);
        Assert.Equal(action, domainEvent.Action);
        Assert.Equal(entityType, domainEvent.EntityType);
        Assert.Equal(entityId, domainEvent.EntityId);
        Assert.Equal(serviceName, domainEvent.ServiceName);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            AuditEntry.Create(
                Guid.Empty,
                "test.action",
                "TestEntity",
                entityId,
                "TestService"));

        Assert.Contains("UserId", exception.Message);
    }

    [Fact]
    public void Create_WithNullAction_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            AuditEntry.Create(
                userId,
                null!,
                "TestEntity",
                entityId,
                "TestService"));

        Assert.Contains("Action", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyAction_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            AuditEntry.Create(
                userId,
                "",
                "TestEntity",
                entityId,
                "TestService"));

        Assert.Contains("Action", exception.Message);
    }

    [Fact]
    public void Create_WithNullEntityType_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            AuditEntry.Create(
                userId,
                "test.action",
                null!,
                entityId,
                "TestService"));

        Assert.Contains("EntityType", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyEntityType_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            AuditEntry.Create(
                userId,
                "test.action",
                "",
                entityId,
                "TestService"));

        Assert.Contains("EntityType", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyEntityId_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            AuditEntry.Create(
                userId,
                "test.action",
                "TestEntity",
                Guid.Empty,
                "TestService"));

        Assert.Contains("EntityId", exception.Message);
    }

    [Fact]
    public void Create_WithNullServiceName_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            AuditEntry.Create(
                userId,
                "test.action",
                "TestEntity",
                entityId,
                null!));

        Assert.Contains("ServiceName", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyServiceName_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            AuditEntry.Create(
                userId,
                "test.action",
                "TestEntity",
                entityId,
                ""));

        Assert.Contains("ServiceName", exception.Message);
    }

    [Fact]
    public void Create_WithOptionalFields_ShouldSetThem()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        const string userEmail = "user@example.com";
        const string details = "{\"test\":\"data\"}";
        const string ipAddress = "10.0.0.1";
        const string userAgent = "TestAgent/1.0";
        const string correlationId = "corr-123";

        // Act
        var entry = AuditEntry.Create(
            userId,
            "test.action",
            "TestEntity",
            entityId,
            "TestService",
            userEmail,
            organizationId,
            workspaceId,
            details,
            ipAddress,
            userAgent,
            correlationId);

        // Assert
        Assert.Equal(userEmail, entry.UserEmail);
        Assert.Equal(organizationId, entry.OrganizationId);
        Assert.Equal(workspaceId, entry.WorkspaceId);
        Assert.Equal(details, entry.Details);
        Assert.Equal(ipAddress, entry.IpAddress);
        Assert.Equal(userAgent, entry.UserAgent);
        Assert.Equal(correlationId, entry.CorrelationId);
    }

    [Fact]
    public void Create_WithNullOptionalFields_ShouldLeaveNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        var entry = AuditEntry.Create(
            userId,
            "test.action",
            "TestEntity",
            entityId,
            "TestService");

        // Assert
        Assert.Null(entry.UserEmail);
        Assert.Null(entry.OrganizationId);
        Assert.Null(entry.WorkspaceId);
        Assert.Null(entry.Details);
        Assert.Null(entry.IpAddress);
        Assert.Null(entry.UserAgent);
        Assert.Null(entry.CorrelationId);
    }

    [Fact]
    public void Entity_ShouldNotHaveUpdateMethods()
    {
        // Arrange
        var type = typeof(AuditEntry);

        // Act
        var methods = type.GetMethods()
            .Where(m => m.DeclaringType == type && m.IsPublic)
            .Select(m => m.Name.ToLower())
            .ToList();

        // Assert - verify no Update, Delete, or Set methods exist
        Assert.DoesNotContain(methods, m => m.Contains("update"));
        Assert.DoesNotContain(methods, m => m.Contains("delete"));
        Assert.DoesNotContain(methods, m => m.StartsWith("set"));
    }
}
