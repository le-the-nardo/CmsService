using CmsService.Api.Contracts;
using CmsService.Domain.Entities;
using CmsService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CmsService.Domain.Services;

public sealed class EntityDomainService
{
    private readonly WriteDbContext _writeDb;

    public EntityDomainService(WriteDbContext writeDb)
    {
        _writeDb = writeDb;
    }

    public async Task ProcessEventAsync(CmsEventDto evt)
    {
        var type = evt.Type.ToLowerInvariant();

        switch (type)
        {
            case "publish":
                await PublishAsync(evt);
                break;

            case "unpublish":
                await UnpublishAsync(evt);
                break;

            case "delete":
                await DeleteAsync(evt);
                break;

            default:
                throw new InvalidOperationException($"Unknown event type: {evt.Type}");
        }
        
        await _writeDb.SaveChangesAsync();
    }

    private async Task PublishAsync(CmsEventDto evt)
    {
        if (evt.Version is null || evt.Payload is null)
            throw new InvalidOperationException("Publish requires version and payload");

        var entity = _writeDb.Entities.Local
            .FirstOrDefault(e => e.Id == evt.Id);

        if (entity is null)
        {
            entity = await _writeDb.Entities
                .Include(e => e.Versions)
                .FirstOrDefaultAsync(e => e.Id == evt.Id);

            if (entity is null)
            {
                entity = new Entity(evt.Id);
                _writeDb.Entities.Add(entity);
            }
        }

        // Idempotency
        if (entity.Versions.Any(v => v.Version == evt.Version))
            return;

        var version = new EntityVersion(
            evt.Id,
            evt.Version.Value,
            evt.Payload?.GetRawText(),
            true,
            evt.Timestamp
        );

        Publish(entity, version);

        _writeDb.EntityVersions.Add(version);
    }

    private async Task UnpublishAsync(CmsEventDto evt)
    {
        if (evt.Version is null)
            throw new InvalidOperationException("Unpublish requires version.");

        var entity = await _writeDb.Entities
            .Include(e => e.Versions)
            .FirstOrDefaultAsync(e => e.Id == evt.Id);

        if (entity is null)
            return;

        Unpublish(entity, evt.Version.Value);
    }

    private async Task DeleteAsync(CmsEventDto evt)
    {
        var entity = await _writeDb.Entities
            .FirstOrDefaultAsync(e => e.Id == evt.Id);

        if (entity is null)
            return;

        //Delete(entity);
        _writeDb.Entities.Remove(entity);
    }

    /* ========= Domain Rules ========= */

    private static void Publish(Entity entity, EntityVersion version)
    {
        if (entity.LatestPublishedVersion.HasValue &&
            version.Version <= entity.LatestPublishedVersion.Value)
        {
            throw new InvalidOperationException(
                "Published version must be greater than the last published version."
            );
        }

        entity.AddVersion(version);
    }

    private static void Unpublish(Entity entity, int version)
    {
        var target = entity.Versions.FirstOrDefault(v => v.Version == version);

        if (target is null)
            throw new InvalidOperationException("Version not found for unpublish.");

        target.Unpublish();
        entity.RecomputeLatestPublishedVersion();
    }

}
