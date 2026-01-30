using CmsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CmsService.Infrastructure.Persistence;

public class WriteDbContext : DbContext
{
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<EntityVersion> EntityVersions => Set<EntityVersion>();

    public WriteDbContext(DbContextOptions<WriteDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
    }
}