namespace CmsService.Domain.Entities;

public class EntityVersion
{
    public Guid Id { get; private set; }
    public string EntityId { get; private set; } = null!;
    public int Version { get; private set; }
    public string? Payload { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime Timestamp { get; private set; }
    
    protected EntityVersion() { }

    public EntityVersion(
        string entityId,
        int version,
        string? payload,
        bool isPublished,
        DateTime timestamp)
    {
        Id = Guid.NewGuid();
        EntityId = entityId;
        Version = version;
        Payload = payload;
        IsPublished = isPublished;
        Timestamp = timestamp;
    }

    public void Unpublish()
    {
        IsPublished = false;
    }
}