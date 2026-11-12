using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Infrastructure.Persistence.Configurations;

internal sealed class PuzzleConfiguration : IEntityTypeConfiguration<Puzzle>
{
    public void Configure(EntityTypeBuilder<Puzzle> builder)
    {
        builder.ToTable("puzzles");
        builder.HasKey(puzzle => puzzle.Id);
        builder.Property(puzzle => puzzle.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(puzzle => puzzle.Code).HasMaxLength(16);
        builder.Property(puzzle => puzzle.Title).HasMaxLength(Puzzle.MaxTitleLength);

        // One daily puzzle per day, but any number of custom games created on the same day.
        builder.HasIndex(puzzle => puzzle.Day).IsUnique().HasFilter("\"Kind\" = 'Daily'");
        builder.HasIndex(puzzle => puzzle.Code).IsUnique();
        builder.HasIndex(puzzle => puzzle.OwnerId);

        builder.HasMany(puzzle => puzzle.Rounds).WithOne().HasForeignKey("PuzzleId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(puzzle => puzzle.Rounds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class PuzzleRoundConfiguration : IEntityTypeConfiguration<PuzzleRound>
{
    public void Configure(EntityTypeBuilder<PuzzleRound> builder)
    {
        builder.ToTable("puzzle_rounds");
        builder.HasKey(round => round.Id);
        builder.HasIndex("PuzzleId", nameof(PuzzleRound.Index)).IsUnique();
        builder.HasIndex(round => round.SongId);

        builder.OwnsMany(round => round.BestMatches, matches => matches.ToJson());
    }
}
