using SongTheSame.Audio;
using SongTheSame.Infrastructure.Audio;

namespace SongTheSame.Infrastructure.Tests.Audio;

public class Mp3ChromaExtractorTests
{
    private const int PitchClassA = 9;

    [Fact]
    public void A_pure_440_Hz_tone_produces_chroma_dominated_by_A()
    {
        var mp3 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tone_a440.mp3"));

        var chroma = new Mp3ChromaExtractor().Extract(mp3);

        chroma.DurationSeconds.ShouldBeInRange(11.0, 12.5);
        var middle = chroma.Frame(chroma.FrameCount / 2);
        var strongest = 0;
        for (var pc = 1; pc < ChromaSequence.PitchClasses; pc++)
            if (middle[pc] > middle[strongest]) strongest = pc;
        strongest.ShouldBe(PitchClassA);
    }
}
