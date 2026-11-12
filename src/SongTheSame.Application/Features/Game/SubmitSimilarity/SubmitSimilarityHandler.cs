using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Application.Features.Similarity;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Game.SubmitSimilarity;

public sealed record SubmitSimilarityCommand(Guid SessionId, string PlayerKey, int RoundIndex, Guid SongId);

/// <summary>Phase 2: the player names the song that contains the most similar part to the revealed segment.</summary>
public sealed class SubmitSimilarityHandler(
    SessionLoader loader,
    ISessionRepository sessions,
    ISongCatalog catalog,
    ISimilarityService similarity,
    SessionViewBuilder views,
    TimeProvider clock) : IHandler<SubmitSimilarityCommand, SessionView>
{
    public async Task<SessionView> HandleAsync(SubmitSimilarityCommand request, CancellationToken cancellationToken)
    {
        var (session, puzzle) = await loader.LoadAsync(request.SessionId, request.PlayerKey, cancellationToken);
        var progress = session.PlayableRound(request.RoundIndex);
        if (!progress.Phase1Done) throw new GameRuleException("Finish phase 1 before phase 2.");
        if (progress.Phase2Done) throw new GameRuleException("Phase 2 of this round is already answered.");

        var round = puzzle.Rounds.Single(r => r.Index == request.RoundIndex);
        var songs = await catalog.GetByIdsAsync([request.SongId, round.SongId], cancellationToken);
        var guess = songs.FirstOrDefault(song => song.Id == request.SongId)
            ?? throw new ArgumentException("Unknown song.", nameof(request));
        var target = songs.First(song => song.Id == round.SongId);
        if (SongIdentity.IsSameWork(guess, target))
            throw new GameRuleException("Pick a different song: the answer must be another song, not the one from phase 1.");

        var result = await similarity.ScoreAsync(round.SongId, round.SegmentStartFrame, guess.Id, cancellationToken);
        var bestRaw = round.BestMatches.Count > 0 ? round.BestMatches.Max(match => match.Score) : 0;
        var score = RoundScoring.RelativeToBest(result.Score, bestRaw);
        session.RegisterPhase2(request.RoundIndex, guess.Id, score, clock.GetUtcNow(), result.MatchStartSeconds);
        await sessions.SaveAsync(cancellationToken);
        return await views.BuildAsync(session, puzzle, cancellationToken);
    }
}
