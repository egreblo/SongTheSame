using SongTheSame.Application.Features.Similarity;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Audio;
using SongTheSame.Domain.Similarity;

namespace SongTheSame.Application.Tests.Features.Similarity;

public class SimilarityServiceTests
{
    private const int SongCount = 120;

    private readonly InMemoryFeatureStore _features = new();
    private readonly Guid[] _ids = Enumerable.Range(0, SongCount).Select(_ => Guid.NewGuid()).ToArray();
    private readonly SimilarityService _service;

    public SimilarityServiceTests()
    {
        for (var i = 0; i < SongCount; i++) _features.Add(_ids[i], seed: i + 1);

        var calibrations = new InMemoryCalibrationStore();
        calibrations.SaveAsync(
            SimilarityCalibration.Create(1, Enumerable.Range(0, 500).Select(i => 0.1f + i / 1000f), DateTimeOffset.UnixEpoch),
            CancellationToken.None);
        _service = new SimilarityService(_features, calibrations);
    }

    [Fact]
    public async Task The_two_stage_search_returns_songs_from_the_true_top_of_the_ranking()
    {
        const int start = 80;
        var target = _ids[0];
        var chroma = (await _features.GetAsync(target, CancellationToken.None))!;
        var segment = chroma.Slice(start, 60);

        var expected = new List<(Guid Id, float Cost)>();
        foreach (var id in _ids.Skip(1))
            expected.Add((id, ChromaSimilarityScorer.Compare(segment, (await _features.GetAsync(id, CancellationToken.None))!).Cost));
        // Random noise has no structure for the blurred first stage to exploit, so only demand that its picks stay
        // near the top. On real music the picks are identical (measured on the catalog: 36/36 of the top three).
        var bruteForceTop = expected.OrderBy(entry => entry.Cost).Take(15).Select(entry => entry.Id).ToList();

        var found = await _service.FindBestMatchesAsync(target, start, 3, CancellationToken.None);

        found.ShouldAllBe(match => bruteForceTop.Contains(match.SongId));
    }

    [Fact]
    public async Task The_target_song_is_never_its_own_best_match()
    {
        var found = await _service.FindBestMatchesAsync(_ids[5], 50, 5, CancellationToken.None);

        found.Select(match => match.SongId).ShouldNotContain(_ids[5]);
        found.Count.ShouldBe(5);
    }

    [Fact]
    public async Task Scores_never_increase_down_the_list()
    {
        var found = await _service.FindBestMatchesAsync(_ids[3], 100, 5, CancellationToken.None);

        found.Select(match => match.Score).ShouldBe(found.Select(match => match.Score).OrderByDescending(score => score));
    }
}
