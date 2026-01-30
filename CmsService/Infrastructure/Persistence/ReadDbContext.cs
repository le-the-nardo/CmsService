using CmsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CmsService.Infrastructure.Persistence;

public class ReadDbContext : DbContext
{
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<EntityVersion> EntityVersions => Set<EntityVersion>();

    public ReadDbContext(DbContextOptions<ReadDbContext> options)
        : base(options)
    {
        // Reduce memory and CPU usage
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly);
    }
}