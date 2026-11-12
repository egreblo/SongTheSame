using SongTheSame.Audio;

namespace SongTheSame.Audio.Tests;

public class PcmResamplerTests
{
    [Fact]
    public void Stereo_is_averaged_to_mono()
    {
        var mono = PcmResampler.DownmixToMono([1f, 3f, 2f, 4f], channels: 2);

        mono.ShouldBe([2f, 3f]);
    }

    [Fact]
    public void Halving_the_rate_halves_the_length_and_keeps_a_constant_signal()
    {
        var input = Enumerable.Repeat(0.5f, 44100).ToArray();

        var output = PcmResampler.Resample(input, 44100, 22050);

        output.Length.ShouldBe(22050);
        output.ShouldAllBe(sample => Math.Abs(sample - 0.5f) < 1e-5f);
    }

    [Fact]
    public void Same_rate_returns_a_copy()
    {
        float[] input = [0.1f, 0.2f, 0.3f];

        PcmResampler.Resample(input, 22050, 22050).ShouldBe(input);
    }
}
