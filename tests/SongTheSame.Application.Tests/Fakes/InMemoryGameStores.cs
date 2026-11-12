using SongTheSame.Application.Abstractions;
using SongTheSame.Audio;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Players;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Similarity;

namespace SongTheSame.Application.Tests.Fakes;

internal sealed class InMemoryPuzzleRepository : IPuzzleRepository
{
    public List<Puzzle> Puzzles { get; } = [];

    public Task<Puzzle?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Puzzles.FirstOrDefault(puzzle => puzzle.Id == id));

    public Task<IReadOnlyList<Puzzle>> GetManyAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Puzzle>>(Puzzles.Where(puzzle => ids.Contains(puzzle.Id)).ToList());

    public Task<IReadOnlyList<Puzzle>> ListTrackedFromDayAsync(DateOnly day, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Puzzle>>(Puzzles.Where(p => p.Kind == PuzzleKind.Daily && p.Day >= day).OrderBy(p => p.Day).ToList());

    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<Puzzle?> GetByDayAsync(DateOnly day, CancellationToken cancellationToken) =>
        Task.FromResult(Puzzles.FirstOrDefault(puzzle => puzzle.Kind == PuzzleKind.Daily && puzzle.Day == day));

    public Task<Puzzle?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(Puzzles.FirstOrDefault(puzzle => puzzle.Code == code));

    public Task<IReadOnlyList<Puzzle>> ListCustomByOwnerAsync(Guid ownerId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Puzzle>>(Puzzles.Where(puzzle => puzzle.OwnerId == ownerId).ToList());

    public Task<HashSet<Guid>> GetRecentSongIdsAsync(DateOnly day, int days, CancellationToken cancellationToken) =>
        Task.FromResult(Puzzles
            .Where(puzzle => puzzle.Day < day && puzzle.Day >= day.AddDays(-days))
            .SelectMany(puzzle => puzzle.Rounds.Select(round => round.SongId))
            .ToHashSet());

    public Task<bool> TryAddAsync(Puzzle puzzle, CancellationToken cancellationToken)
    {
        if (puzzle.Kind == PuzzleKind.Daily && Puzzles.Any(existing => existing.Kind == PuzzleKind.Daily && existing.Day == puzzle.Day))
            return Task.FromResult(false);
        Puzzles.Add(puzzle);
        return Task.FromResult(true);
    }
}

internal sealed class InMemorySessionRepository : ISessionRepository
{
    public List<GameSession> Sessions { get; } = [];

    public Task<GameSession?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Sessions.FirstOrDefault(session => session.Id == id));

    public Task<IReadOnlyList<GameSession>> GetManyAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<GameSession>>(Sessions.Where(session => ids.Contains(session.Id)).ToList());

    public Task<GameSession?> FindAsync(string playerKey, Guid puzzleId, CancellationToken cancellationToken) =>
        Task.FromResult(Sessions.FirstOrDefault(session => session.PlayerKey == playerKey && session.PuzzleId == puzzleId));

    public Task<IReadOnlyList<GameSession>> ListByPlayerAsync(string playerKey, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<GameSession>>(Sessions.Where(session => session.PlayerKey == playerKey).ToList());

    public Task<bool> TryAddAsync(GameSession session, CancellationToken cancellationToken)
    {
        Sessions.Add(session);
        return Task.FromResult(true);
    }

    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<IReadOnlyList<DailyResult>> GetDailyResultsAsync(
        IReadOnlyCollection<string> playerKeys, DateOnly firstDay, DateOnly lastDay, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DailyResult>>(Sessions
            .Where(s => playerKeys.Contains(s.PlayerKey) && s.Day >= firstDay && s.Day <= lastDay && s.IsCompleted)
            .Select(s => new DailyResult(s.PlayerKey, s.Day, s.TotalPoints)).ToList());

    public Task<IReadOnlyList<int>> GetCompletedTotalsAsync(Guid puzzleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<int>>(Sessions.Where(s => s.PuzzleId == puzzleId && s.IsCompleted).Select(s => s.TotalPoints).ToList());
}

internal sealed class InMemoryFeatureStore : ISongFeatureStore
{
    private readonly Dictionary<Guid, ChromaSequence> _features = [];

    public void Add(Guid songId, int seed)
    {
        var random = new Random(seed);
        var data = new float[300 * ChromaSequence.PitchClasses];
        for (var frame = 0; frame < 300; frame++)
        {
            var peak = random.Next(12);
            for (var pc = 0; pc < 12; pc++) data[frame * 12 + pc] = pc == peak ? 1f : -1f / 11f;
        }

        _features[songId] = new ChromaSequence(data, 10);
    }

    public Task<ChromaSequence?> GetAsync(Guid songId, CancellationToken cancellationToken) =>
        Task.FromResult(_features.GetValueOrDefault(songId));

    public Task<IReadOnlyList<SongChroma>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<SongChroma>>(_features.Select(pair => new SongChroma(pair.Key, pair.Value)).ToList());
}

internal sealed class InMemoryCalibrationStore : ICalibrationStore
{
    private SimilarityCalibration? _latest;

    public Task<SimilarityCalibration?> GetLatestAsync(CancellationToken cancellationToken) => Task.FromResult(_latest);

    public Task SaveAsync(SimilarityCalibration calibration, CancellationToken cancellationToken)
    {
        _latest = calibration;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryProfileRepository : IProfileRepository
{
    private readonly Dictionary<Guid, Profile> _profiles = [];

    public Task<Profile?> GetAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(_profiles.GetValueOrDefault(userId));

    public Task<IReadOnlyList<Profile>> GetManyAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Profile>>(_profiles.Values.Where(profile => userIds.Contains(profile.UserId)).ToList());

    public Task<bool> TryAddAsync(Profile profile, CancellationToken cancellationToken) =>
        Task.FromResult(_profiles.TryAdd(profile.UserId, profile));

    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
