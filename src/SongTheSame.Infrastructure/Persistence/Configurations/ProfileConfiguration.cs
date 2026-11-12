using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SongTheSame.Domain.Players;

namespace SongTheSame.Infrastructure.Persistence.Configurations;

internal sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");
        builder.HasKey(profile => profile.UserId);
        builder.Property(profile => profile.UserId).ValueGeneratedNever();
        builder.Property(profile => profile.DisplayName).HasMaxLength(Profile.MaxDisplayNameLength).IsRequired();
    }
}
