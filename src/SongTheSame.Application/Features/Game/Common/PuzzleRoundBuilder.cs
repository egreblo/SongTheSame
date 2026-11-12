using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Similarity;
using SongTheSame.Audio;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Game.Common;

/// <summary>Turns a chosen song into a puzzle round: picks the phase 2 segment and finds its best twins.</summary>
public sealed class PuzzleRoundBuilder(ISongFeatureStore features, ISimilarityService similarity)
{
    private const int BestMatchesToStore = 3;
    private const int ExtraMatchesForDuplicateFiltering = 6;

    public async Task<PuzzleRound> BuildAsync(
        int index, Song song, IReadOnlyDictionary<Guid, Song> songsById, CancellationToken cancellationToken)
    {
        var chroma = await features.GetAsync(song.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Song {song.Id} has no features.");
        var start = SegmentPicker.PickStart(chroma, GameRules.SegmentFrames);

        var candidates = await similarity.FindBestMatchesAsync(
            song.Id, start, BestMatchesToStore + ExtraMatchesForDuplicateFiltering, cancellationToken);
        var matches = candidates
            .Where(match => !SongIdentity.IsSameWork(song, songsById[match.SongId]))
            .Take(BestMatchesToStore)
            .Select(match => new RoundMatch(match.SongId, match.Score));

        return PuzzleRound.Create(
            index, song.Id, start, start / chroma.FramesPerSecond, GameRules.SegmentFrames / chroma.FramesPerSecond, matches);
    }
}
