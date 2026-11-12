using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Game.GuessSong;

public sealed record GuessSongCommand(Guid SessionId, string PlayerKey, int RoundIndex, Guid SongId);

/// <summary>Phase 1: the player names the song. A different edition of the same work counts as correct.</summary>
public sealed class GuessSongHandler(
    SessionLoader loader,
    ISessionRepository sessions,
    ISongCatalog catalog,
    SessionViewBuilder views) : IHandler<GuessSongCommand, SessionView>
{
    public async Task<SessionView> HandleAsync(GuessSongCommand request, CancellationToken cancellationToken)
    {
        var (session, puzzle) = await loader.LoadAsync(request.SessionId, request.PlayerKey, cancellationToken);
        var round = puzzle.Rounds.FirstOrDefault(r => r.Index == request.RoundIndex)
            ?? throw new KeyNotFoundException("Round not found.");

        var songs = await catalog.GetByIdsAsync([request.SongId, round.SongId], cancellationToken);
        var guess = songs.FirstOrDefault(song => song.Id == request.SongId)
            ?? throw new ArgumentException("Unknown song.", nameof(request));
        var target = songs.First(song => song.Id == round.SongId);

        session.RegisterPhase1Attempt(request.RoundIndex, guess.Id, SongIdentity.IsSameWork(guess, target));
        await sessions.SaveAsync(cancellationToken);
        return await views.BuildAsync(session, puzzle, cancellationToken);
    }
}
