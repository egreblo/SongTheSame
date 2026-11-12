using Microsoft.EntityFrameworkCore;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Leagues;
using SongTheSame.Domain.Players;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Reports;
using SongTheSame.Domain.Similarity;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Song> Songs => Set<Song>();

    public DbSet<SongFeatures> SongFeatures => Set<SongFeatures>();

    public DbSet<SimilarityCalibration> SimilarityCalibrations => Set<SimilarityCalibration>();

    public DbSet<Puzzle> Puzzles => Set<Puzzle>();

    public DbSet<GameSession> GameSessions => Set<GameSession>();

    public DbSet<RoundReport> RoundReports => Set<RoundReport>();

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<League> Leagues => Set<League>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
