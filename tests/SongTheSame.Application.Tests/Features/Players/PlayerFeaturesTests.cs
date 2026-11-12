using Microsoft.Extensions.Time.Testing;
using SongTheSame.Application.Features.Players.ClaimSessions;
using SongTheSame.Application.Features.Players.GetMe;
using SongTheSame.Application.Features.Players.UpdateProfile;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Players;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Application.Tests.Features.Players;

public class PlayerFeaturesTests
{
    private static readonly Guid UserId = Guid.Parse("6f1e2d3c-0000-4000-8000-000000000001");
    private const string AnonKey = "anon:11111111-1111-1111-1111-111111111111";

    private static Puzzle NewPuzzle(DateOnly day) =>
        Puzzle.CreateDaily(day, Enumerable.Range(0, 5).Select(i => PuzzleRound.Create(i, Guid.NewGuid(), 50, 5, 6, [])), DateTimeOffset.UnixEpoch);

    [Fact]
    public async Task First_sign_in_creates_a_profile_from_the_suggested_name_and_later_calls_keep_it()
    {
        var profiles = new InMemoryProfileRepository();
        var handler = new GetMeHandler(profiles, new FakeTimeProvider());

        var first = await handler.HandleAsync(new GetMeQuery(UserId, "Ana Horvat"), CancellationToken.None);
        var again = await handler.HandleAsync(new GetMeQuery(UserId, "Different"), CancellationToken.None);

        first.DisplayName.ShouldBe("Ana Horvat");
        again.DisplayName.ShouldBe("Ana Horvat");
    }

    [Fact]
    public async Task A_missing_suggested_name_falls_back_to_player()
    {
        var handler = new GetMeHandler(new InMemoryProfileRepository(), new FakeTimeProvider());

        (await handler.HandleAsync(new GetMeQuery(UserId, " "), CancellationToken.None)).DisplayName.ShouldBe("Player");
    }

    [Fact]
    public async Task Renaming_updates_the_stored_profile_and_trims_long_names()
    {
        var profiles = new InMemoryProfileRepository();
        await new GetMeHandler(profiles, new FakeTimeProvider()).HandleAsync(new GetMeQuery(UserId, "Ana"), CancellationToken.None);

        var renamed = await new UpdateProfileHandler(profiles)
            .HandleAsync(new UpdateProfileCommand(UserId, new string('x', 100)), CancellationToken.None);

        renamed.DisplayName.Length.ShouldBe(Profile.MaxDisplayNameLength);
    }

    [Fact]
    public async Task Claiming_moves_anonymous_games_to_the_account()
    {
        var sessions = new InMemorySessionRepository();
        var puzzle = NewPuzzle(new DateOnly(2026, 11, 13));
        sessions.Sessions.Add(GameSession.Start(AnonKey, puzzle, DateTimeOffset.UnixEpoch));

        var moved = await new ClaimSessionsHandler(sessions).HandleAsync(new ClaimSessionsCommand(UserId, AnonKey), CancellationToken.None);

        moved.ShouldBe(1);
        sessions.Sessions.Single().PlayerKey.ShouldBe(PlayerKey.ForUser(UserId));
    }

    [Fact]
    public async Task Claiming_never_overwrites_a_game_the_account_already_has_for_that_puzzle()
    {
        var sessions = new InMemorySessionRepository();
        var puzzle = NewPuzzle(new DateOnly(2026, 11, 13));
        sessions.Sessions.Add(GameSession.Start(PlayerKey.ForUser(UserId), puzzle, DateTimeOffset.UnixEpoch));
        sessions.Sessions.Add(GameSession.Start(AnonKey, puzzle, DateTimeOffset.UnixEpoch));

        var moved = await new ClaimSessionsHandler(sessions).HandleAsync(new ClaimSessionsCommand(UserId, AnonKey), CancellationToken.None);

        moved.ShouldBe(0);
        sessions.Sessions.Count(session => session.PlayerKey == AnonKey).ShouldBe(1);
    }
}
