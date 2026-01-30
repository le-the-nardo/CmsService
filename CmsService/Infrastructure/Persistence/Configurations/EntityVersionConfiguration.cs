using CmsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CmsService.Infrastructure.Persistence.Configurations;

public class EntityVersionConfiguration : IEntityTypeConfiguration<EntityVersion>
{
    public void Configure(EntityTypeBuilder<EntityVersion> builder)
    {
        builder.ToTable("EntityVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.EntityId)
            .IsRequired();

        builder.Property(v => v.Version)
            .IsRequired();

        builder.Property(v => v.IsPublished)
            .IsRequired();

        builder.Property(v => v.Timestamp)
            .IsRequired();

        builder.Property(v => v.Payload)
            .HasColumnType("TEXT");

        builder.HasIndex(v => new { v.EntityId, v.Version })
            .IsUnique();
    }
}

// Why:
//
// (EntityId, Version) unique enforces domain rule
//
//     TEXT avoids SQLite JSON limitations