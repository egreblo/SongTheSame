using Microsoft.Extensions.Time.Testing;
using SongTheSame.Application.Features.CustomGames.CreateCustomGame;
using SongTheSame.Application.Features.CustomGames.GetCustomGame;
using SongTheSame.Application.Features.CustomGames.ListMyCustomGames;
using SongTheSame.Application.Features.CustomGames.StartCustomSession;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Application.Features.Similarity;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Players;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Similarity;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Tests.Features.CustomGames;

public class CustomGameFlowTests
{
    private static readonly Guid Owner = Guid.Parse("00000000-0000-4000-8000-00000000000a");

    private readonly InMemorySongCatalog _catalog = new();
    private readonly InMemoryPuzzleRepository _puzzles = new();
    private readonly InMemorySessionRepository _sessions = new();
    private readonly InMemoryProfileRepository _profiles = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero));
    private readonly CreateCustomGameHandler _create;
    private readonly StartCustomSessionHandler _start;

    public CustomGameFlowTests()
    {
        var features = new InMemoryFeatureStore();
        for (var i = 1; i <= 20; i++)
        {
            var song = Song.Create(i, $"Song {i}", $"Artist {i}", null, null, 200, 100 - i, SongOrigin.International, _clock.GetUtcNow());
            _catalog.Songs.Add(song);
            features.Add(song.Id, seed: i);
        }

        var calibrations = new InMemoryCalibrationStore();
        calibrations.SaveAsync(SimilarityCalibration.Create(1, Enumerable.Range(0, 500).Select(i => 0.1f + i / 1000f), _clock.GetUtcNow()), CancellationToken.None);

        var builder = new PuzzleRoundBuilder(features, new SimilarityService(features, calibrations));
        _create = new CreateCustomGameHandler(_puzzles, _catalog, builder, _clock);
        var views = new SessionViewBuilder(_catalog, new FakeMusicSource());
        _start = new StartCustomSessionHandler(_puzzles, new SessionStarter(_sessions, views, _clock));
    }

    private static CancellationToken Ct => CancellationToken.None;

    private List<Guid> SongIds(params int[] numbers) => numbers.Select(n => _catalog.Songs[n].Id).ToList();

    [Fact]
    public async Task A_game_is_created_from_five_songs_in_the_chosen_order()
    {
        var created = await _create.HandleAsync(new CreateCustomGameCommand(Owner, "  Eighties night  ", SongIds(4, 3, 2, 1, 0)), Ct);

        created.Title.ShouldBe("Eighties night");
        var puzzle = _puzzles.Puzzles.Single();
        puzzle.Kind.ShouldBe(PuzzleKind.Custom);
        puzzle.Rounds.Select(round => round.SongId).ShouldBe(SongIds(4, 3, 2, 1, 0));
        puzzle.Rounds.ShouldAllBe(round => round.BestMatches.Count == 3);
    }

    [Fact]
    public async Task Exactly_five_distinct_songs_are_required()
    {
        await Should.ThrowAsync<ArgumentException>(() => _create.HandleAsync(new CreateCustomGameCommand(Owner, "Short", SongIds(0, 1, 2)), Ct));
        await Should.ThrowAsync<GameRuleException>(() => _create.HandleAsync(new CreateCustomGameCommand(Owner, "Dup", SongIds(0, 1, 2, 3, 3)), Ct));
        await Should.ThrowAsync<ArgumentException>(
            () => _create.HandleAsync(new CreateCustomGameCommand(Owner, "Unknown", [.. SongIds(0, 1, 2, 3), Guid.NewGuid()]), Ct));
    }

    [Fact]
    public async Task Players_start_the_game_by_code_and_get_their_own_progress()
    {
        var created = await _create.HandleAsync(new CreateCustomGameCommand(Owner, "Party", SongIds(0, 1, 2, 3, 4)), Ct);

        var first = await _start.HandleAsync(new StartCustomSessionCommand(PlayerKey.Anonymous(Guid.NewGuid()), created.Code.ToLowerInvariant()), Ct);
        var other = await _start.HandleAsync(new StartCustomSessionCommand(PlayerKey.Anonymous(Guid.NewGuid()), created.Code), Ct);

        first.SessionId.ShouldNotBe(other.SessionId);
        first.Rounds.Count.ShouldBe(5);
        _sessions.Sessions.Count.ShouldBe(2);
    }

    [Fact]
    public async Task An_unknown_code_is_not_found_and_info_shows_the_author()
    {
        await Should.ThrowAsync<KeyNotFoundException>(() => _start.HandleAsync(new StartCustomSessionCommand("anon:x", "NOPE2222"), Ct));

        await _profiles.TryAddAsync(Profile.Create(Owner, "Ana", _clock.GetUtcNow()), Ct);
        var created = await _create.HandleAsync(new CreateCustomGameCommand(Owner, "Party", SongIds(0, 1, 2, 3, 4)), Ct);
        var info = await new GetCustomGameHandler(_puzzles, _profiles).HandleAsync(new GetCustomGameQuery(created.Code), Ct);

        info.AuthorName.ShouldBe("Ana");
        info.Title.ShouldBe("Party");
    }

    [Fact]
    public async Task Owners_list_only_their_own_games()
    {
        await _create.HandleAsync(new CreateCustomGameCommand(Owner, "Mine", SongIds(0, 1, 2, 3, 4)), Ct);
        var list = new ListMyCustomGamesHandler(_puzzles);

        (await list.HandleAsync(new ListMyCustomGamesQuery(Owner), Ct)).ShouldHaveSingleItem().Title.ShouldBe("Mine");
        (await list.HandleAsync(new ListMyCustomGamesQuery(Guid.NewGuid()), Ct)).ShouldBeEmpty();
    }
}
