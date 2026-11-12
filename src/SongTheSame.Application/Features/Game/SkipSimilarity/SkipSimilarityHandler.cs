using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;

namespace SongTheSame.Application.Features.Game.SkipSimilarity;

public sealed record SkipSimilarityCommand(Guid SessionId, string PlayerKey, int RoundIndex);

/// <summary>Phase 2: the player gives up on finding a twin. The round ends with 0 points for phase 2.</summary>
public sealed class SkipSimilarityHandler(
    SessionLoader loader,
    ISessionRepository sessions,
    SessionViewBuilder views,
    TimeProvider clock) : IHandler<SkipSimilarityCommand, SessionView>
{
    public async Task<SessionView> HandleAsync(SkipSimilarityCommand request, CancellationToken cancellationToken)
    {
        var (session, puzzle) = await loader.LoadAsync(request.SessionId, request.PlayerKey, cancellationToken);

        session.SkipPhase2(request.RoundIndex, clock.GetUtcNow());
        await sessions.SaveAsync(cancellationToken);
        return await views.BuildAsync(session, puzzle, cancellationToken);
    }
}
