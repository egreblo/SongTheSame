using SongTheSame.Audio;

namespace SongTheSame.Audio.Tests;

public class ChromaCodecTests
{
    [Fact]
    public void Round_trip_keeps_values_within_quantisation_error()
    {
        var values = new float[24];
        for (var i = 0; i < values.Length; i++) values[i] = MathF.Sin(i) * 0.9f;
        var original = new ChromaSequence(values, 10);

        var decoded = ChromaCodec.Decode(ChromaCodec.Encode(original), 10);

        decoded.FrameCount.ShouldBe(2);
        for (var i = 0; i < values.Length; i++)
            decoded.Frame(i / 12)[i % 12].ShouldBe(values[i], 1f / 127f);
    }

    [Fact]
    public void Encoded_size_is_one_byte_per_value()
    {
        var sequence = new ChromaSequence(new float[12 * 300], 10);

        ChromaCodec.Encode(sequence).Length.ShouldBe(3600);
    }
}
