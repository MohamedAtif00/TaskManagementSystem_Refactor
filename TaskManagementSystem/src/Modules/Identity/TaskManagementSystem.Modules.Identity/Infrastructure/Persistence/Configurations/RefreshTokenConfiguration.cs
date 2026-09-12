using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.ToTable("RefreshTokens", "identity");
        entity.HasKey(x => x.Token);
        entity.Property(x => x.Token).HasColumnName("Token").HasMaxLength(450);
        entity.Property(x => x.Created).HasColumnName("Created");
        entity.Property(x => x.Expires).HasColumnName("Expires");
        entity.Property(x => x.Used).HasColumnName("Used");
        entity.Property(x => x.UserId).HasColumnName("UserId");
        entity.HasIndex(x => x.UserId);
    }
}
