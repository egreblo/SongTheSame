namespace SongTheSame.Audio;

/// <summary>Compact storage format for chroma sequences: one signed byte per value (about 3.6 KB per 30 s preview).</summary>
public static class ChromaCodec
{
    /// <summary>Increase when extraction or encoding changes in a way that invalidates stored features.</summary>
    public const int CurrentVersion = 1;

    private const float Scale = 127f;

    public static byte[] Encode(ChromaSequence sequence)
    {
        var values = sequence.ToArray();
        var bytes = new byte[values.Length];
        for (var i = 0; i < values.Length; i++)
            bytes[i] = (byte)(sbyte)Math.Clamp((int)MathF.Round(values[i] * Scale), -127, 127);
        return bytes;
    }

    public static ChromaSequence Decode(byte[] data, double framesPerSecond)
    {
        var values = new float[data.Length];
        for (var i = 0; i < data.Length; i++)
            values[i] = (sbyte)data[i] / Scale;
        return new ChromaSequence(values, framesPerSecond);
    }
}
