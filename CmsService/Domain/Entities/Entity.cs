namespace CmsService.Domain.Entities;

public class Entity
{
    public string Id { get; private set; } = null!;
    public bool IsDeleted { get; private set; }
    public bool IsDisabledByAdmin { get; private set; }
    public int? LatestPublishedVersion { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<EntityVersion> _versions = new();
    public IReadOnlyCollection<EntityVersion> Versions => _versions.AsReadOnly();

    protected Entity() { }

    public Entity(string id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddVersion(EntityVersion version)
    {
        _versions.Add(version);

        if (version.IsPublished)
        {
            LatestPublishedVersion = version.Version;
        }
    }

    public void RecomputeLatestPublishedVersion()
    {
        LatestPublishedVersion = _versions
            .Where(v => v.IsPublished)
            .Select(v => (int?)v.Version)
            .Max();
    }

    public void DisableByAdmin()
    {
        IsDisabledByAdmin = true;
    }
}