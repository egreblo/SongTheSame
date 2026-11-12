using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;

namespace SongTheSame.Application.Features.Game.SkipClip;

public sealed record SkipClipCommand(Guid SessionId, string PlayerKey, int RoundIndex);

/// <summary>Phase 1: the player gives up on the current clip length and unlocks the next, longer one.</summary>
public sealed class SkipClipHandler(
    SessionLoader loader,
    ISessionRepository sessions,
    SessionViewBuilder views) : IHandler<SkipClipCommand, SessionView>
{
    public async Task<SessionView> HandleAsync(SkipClipCommand request, CancellationToken cancellationToken)
    {
        var (session, puzzle) = await loader.LoadAsync(request.SessionId, request.PlayerKey, cancellationToken);

        session.RegisterPhase1Attempt(request.RoundIndex, Guid.Empty, correct: false);
        await sessions.SaveAsync(cancellationToken);
        return await views.BuildAsync(session, puzzle, cancellationToken);
    }
}
