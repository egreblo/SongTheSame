using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Infrastructure.Persistence.Configurations;

internal sealed class SongFeaturesConfiguration : IEntityTypeConfiguration<SongFeatures>
{
    public void Configure(EntityTypeBuilder<SongFeatures> builder)
    {
        builder.ToTable("song_features");
        builder.HasKey(features => features.SongId);

        builder.HasOne<Song>().WithOne().HasForeignKey<SongFeatures>(features => features.SongId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(features => features.Data).IsRequired();
    }
}
