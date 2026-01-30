using CmsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CmsService.Infrastructure.Persistence.Configurations;

public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Entities");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.IsDeleted)
            .IsRequired();

        builder.Property(e => e.IsDisabledByAdmin)
            .IsRequired();

        builder.Property(e => e.LatestPublishedVersion);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasMany(e => e.Versions)
            .WithOne()
            .HasForeignKey(v => v.EntityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.LatestPublishedVersion);
    }
}

// Why (only important points):
//
// Cascade delete simplifies delete events
//
//     Index improves read performance
//
//     Explicit table naming avoids surprises