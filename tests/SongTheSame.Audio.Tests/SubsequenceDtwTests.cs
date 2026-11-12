using SongTheSame.Audio;

namespace SongTheSame.Audio.Tests;

public class SubsequenceDtwTests
{
    [Fact]
    public void Identical_query_inside_reference_has_zero_cost()
    {
        var reference = BuildSequence(pitchClassPerFrame: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
        var query = reference.Slice(3, 4);

        var result = ChromaSimilarityScorer.Compare(query, reference);

        result.Cost.ShouldBe(0f, 1e-4f);
        result.Transposition.ShouldBe(0);
        result.EndFrame.ShouldBe(6);
    }

    [Fact]
    public void Transposed_query_is_matched_with_the_right_shift()
    {
        var reference = BuildSequence([0, 2, 4, 5, 7, 9, 11, 0]);
        var transposed = BuildSequence([3, 5, 7, 8]); // reference frames 0..3 shifted up by 3 semitones

        var result = ChromaSimilarityScorer.Compare(transposed, reference);

        result.Cost.ShouldBe(0f, 1e-4f);
        result.Transposition.ShouldBe(3);
    }

    [Fact]
    public void Slower_query_is_still_a_near_perfect_match()
    {
        var reference = BuildSequence([0, 2, 4, 5, 7, 9, 11, 1]);
        var stretched = BuildSequence([2, 2, 4, 4, 5, 5, 7, 7]); // frames 1..4 played at half speed

        var result = ChromaSimilarityScorer.Compare(stretched, reference);

        result.Cost.ShouldBe(0f, 1e-4f);
    }

    [Fact]
    public void Unrelated_query_costs_more_than_a_related_one()
    {
        var reference = BuildSequence([0, 2, 4, 5, 7, 9, 11, 0]);
        var related = reference.Slice(1, 4);
        var unrelated = BuildSequence([0, 0, 0, 0]);

        ChromaSimilarityScorer.Compare(unrelated, reference).Cost
            .ShouldBeGreaterThan(ChromaSimilarityScorer.Compare(related, reference).Cost);
    }

    /// <summary>One-hot chroma frames, mean-centred and normalised like the real extractor does.</summary>
    private static ChromaSequence BuildSequence(int[] pitchClassPerFrame)
    {
        var data = new float[pitchClassPerFrame.Length * ChromaSequence.PitchClasses];
        for (var frame = 0; frame < pitchClassPerFrame.Length; frame++)
        {
            for (var pc = 0; pc < ChromaSequence.PitchClasses; pc++)
                data[frame * 12 + pc] = pc == pitchClassPerFrame[frame] ? 1f : 0f;
        }

        return new ChromaSequence(data, framesPerSecond: 10);
    }
}
