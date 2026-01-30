using System;
using System.Text.Json;
using System.Threading.Tasks;
using CmsService.Api.Contracts;
using CmsService.Domain.Entities;
using CmsService.Domain.Services;
using CmsService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CmsService.Tests.Domain.Services;

public class EntityDomainServiceTests
{
    private static WriteDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WriteDbContext(options);
    }

    private static CmsEventDto CreatePublishEvent(
        string entityId,
        int version,
        string json = """{ "title": "content" }""")
    {
        return new CmsEventDto(
            Id: entityId,
            Type: "publish",
            Version: version,
            Payload: JsonDocument.Parse(json).RootElement,
            Timestamp: DateTime.UtcNow);
    }

    [Fact]
    public async Task ProcessEventAsync_Should_Publish_New_Entity()
    {
        // Arrange
        var db = CreateDbContext();
        var service = new EntityDomainService(db);

        var entityId = Guid.NewGuid().ToString();
        var evt = CreatePublishEvent(entityId, 1);

        // Act
        await service.ProcessEventAsync(evt);

        // Assert
        var entity = await db.Entities
            .Include(e => e.Versions)
            .FirstOrDefaultAsync(e => e.Id == entityId);

        entity.Should().NotBeNull();
        entity!.Versions.Should().HaveCount(1);
        entity.LatestPublishedVersion.Should().Be(1);
    }

    [Fact]
    public async Task ProcessEventAsync_Should_Be_Idempotent_For_Same_Version()
    {
        // Arrange
        var db = CreateDbContext();
        var service = new EntityDomainService(db);

        var entityId = Guid.NewGuid().ToString();
        var evt = CreatePublishEvent(entityId, 1);

        // Act
        await service.ProcessEventAsync(evt);
        await service.ProcessEventAsync(evt);

        // Assert
        var entity = await db.Entities
            .Include(e => e.Versions)
            .FirstAsync();

        entity.Versions.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProcessEventAsync_Should_Throw_When_Publishing_Older_Version()
    {
        // Arrange
        var db = CreateDbContext();
        var service = new EntityDomainService(db);

        var entityId = Guid.NewGuid().ToString();

        await service.ProcessEventAsync(CreatePublishEvent(entityId, 2));

        var olderEvent = CreatePublishEvent(entityId, 1);

        // Act
        var act = async () => await service.ProcessEventAsync(olderEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Published version must be greater than the last published version.");
    }

    [Fact]
    public async Task ProcessEventAsync_Should_Unpublish_Version()
    {
        // Arrange
        var db = CreateDbContext();
        var service = new EntityDomainService(db);

        var entityId = Guid.NewGuid().ToString();

        await service.ProcessEventAsync(CreatePublishEvent(entityId, 1));
        await service.ProcessEventAsync(CreatePublishEvent(entityId, 2));

        var unpublishEvent = new CmsEventDto
        (
            Id: entityId,
            Type: "unpublish",
            Version: 2,
            Payload: JsonDocument.Parse("{\"title\":\"test\"}").RootElement,
            Timestamp: DateTime.UtcNow
        );

        // Act
        await service.ProcessEventAsync(unpublishEvent);

        // Assert
        var entity = await db.Entities
            .Include(e => e.Versions)
            .FirstAsync();

        entity.LatestPublishedVersion.Should().Be(1);
        entity.Versions.First(v => v.Version == 2).IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessEventAsync_Should_Throw_When_Unpublishing_Nonexistent_Version()
    {
        // Arrange
        var db = CreateDbContext();
        var service = new EntityDomainService(db);

        var entityId = Guid.NewGuid().ToString();

        await service.ProcessEventAsync(CreatePublishEvent(entityId, 1));
        
        var evt = new CmsEventDto
        (
            Id: entityId,
            Type: "unpublish",
            Version: 99,
            Payload: JsonDocument.Parse("{\"title\":\"test\"}").RootElement,
            Timestamp: DateTime.UtcNow
        );

        // Act
        var act = async () => await service.ProcessEventAsync(evt);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Version not found for unpublish.");
    }

    [Fact]
    public async Task ProcessEventAsync_Should_Delete_Entity()
    {
        // Arrange
        var db = CreateDbContext();
        var service = new EntityDomainService(db);

        var entityId = Guid.NewGuid().ToString();

        await service.ProcessEventAsync(CreatePublishEvent(entityId, 1));

        var deleteEvent = new CmsEventDto(
            Id: entityId,
            Type: "delete",
            Version: 1,
            Payload: JsonDocument.Parse("{}").RootElement,
            Timestamp: DateTime.UtcNow);

        // Act
        await service.ProcessEventAsync(deleteEvent);

        // Assert
        var entity = await db.Entities.FirstOrDefaultAsync();
        entity.Should().BeNull();
    }

    [Fact]
    public async Task ProcessEventAsync_Should_Throw_For_Unknown_Event_Type()
    {
        // Arrange
        var db = CreateDbContext();
        var service = new EntityDomainService(db);

        var evt = new CmsEventDto(
            Id: Guid.NewGuid().ToString(),
            Type: "invalid-event",
            Version: 0,
            Payload: JsonDocument.Parse("{}").RootElement,
            Timestamp: DateTime.UtcNow);

        // Act
        var act = async () => await service.ProcessEventAsync(evt);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Unknown event type: invalid-event");
    }
}
