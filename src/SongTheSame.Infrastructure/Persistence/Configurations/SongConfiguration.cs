using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Infrastructure.Persistence.Configurations;

internal sealed class SongConfiguration : IEntityTypeConfiguration<Song>
{
    public void Configure(EntityTypeBuilder<Song> builder)
    {
        builder.ToTable("songs");
        builder.HasKey(song => song.Id);

        builder.Property(song => song.Title).HasMaxLength(300).IsRequired();
        builder.Property(song => song.Artist).HasMaxLength(300).IsRequired();
        builder.Property(song => song.AlbumTitle).HasMaxLength(300);
        builder.Property(song => song.CoverUrl).HasMaxLength(500);
        builder.Property(song => song.SearchText).HasMaxLength(700).IsRequired();
        builder.Property(song => song.Origin).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(song => song.DeezerId).IsUnique();

        // Trigram index makes ILIKE '%token%' searches fast.
        builder.HasIndex(song => song.SearchText).HasMethod("gin").HasOperators("gin_trgm_ops");
    }
}
