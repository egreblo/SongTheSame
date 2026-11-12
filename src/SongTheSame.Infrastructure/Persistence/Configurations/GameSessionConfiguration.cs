using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SongTheSame.Domain.Game;

namespace SongTheSame.Infrastructure.Persistence.Configurations;

internal sealed class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("game_sessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.PlayerKey).HasMaxLength(80).IsRequired();
        builder.HasIndex(session => new { session.PlayerKey, session.PuzzleId }).IsUnique();
        builder.HasIndex(session => session.Day);

        builder.OwnsMany(session => session.Rounds, rounds =>
        {
            rounds.ToTable("round_progress");
            rounds.WithOwner().HasForeignKey("GameSessionId");
            rounds.HasKey("GameSessionId", nameof(RoundProgress.Index));
            rounds.Property(round => round.Index).ValueGeneratedNever();
        });
        builder.Navigation(session => session.Rounds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
