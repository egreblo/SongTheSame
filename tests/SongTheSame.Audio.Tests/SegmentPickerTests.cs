using SongTheSame.Audio;

namespace SongTheSame.Audio.Tests;

public class SegmentPickerTests
{
    private static ChromaSequence Build(int frames, Func<int, float> peakAt)
    {
        var data = new float[frames * ChromaSequence.PitchClasses];
        for (var frame = 0; frame < frames; frame++)
        {
            var peak = peakAt(frame);
            if (peak <= 0f) continue;
            data[frame * 12] = peak;
            for (var pc = 1; pc < 12; pc++) data[frame * 12 + pc] = (1f - peak) / 11f;
        }

        return new ChromaSequence(data, 10);
    }

    [Fact]
    public void Avoids_silent_regions()
    {
        // Frames 0-149 are silent, 150-299 are tonal.
        var chroma = Build(300, frame => frame < 150 ? 0f : 0.8f);

        var start = SegmentPicker.PickStart(chroma, 60);

        start.ShouldBeGreaterThanOrEqualTo(150);
    }

    [Fact]
    public void Prefers_the_clearest_window()
    {
        // Mild peaks everywhere, but a very clear stretch at frames 200-260.
        var chroma = Build(300, frame => frame is >= 200 and < 260 ? 0.95f : 0.5f);

        var start = SegmentPicker.PickStart(chroma, 60);

        start.ShouldBe(200);
    }

    [Fact]
    public void Falls_back_to_the_middle_for_very_short_input()
    {
        var chroma = Build(70, _ => 0.5f);

        SegmentPicker.PickStart(chroma, 60).ShouldBe(5);
    }
}
