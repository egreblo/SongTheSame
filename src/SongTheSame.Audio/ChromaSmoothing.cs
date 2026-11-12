namespace SongTheSame.Audio;

internal static class ChromaSmoothing
{
    private const int Dim = ChromaSequence.PitchClasses;

    /// <summary>Centered moving average over time, per pitch class.</summary>
    public static float[] MovingAverage(float[] data, int windowFrames)
    {
        var frames = data.Length / Dim;
        if (windowFrames <= 1 || frames == 0) return (float[])data.Clone();

        var result = new float[data.Length];
        var radius = windowFrames / 2;
        for (var frame = 0; frame < frames; frame++)
        {
            var from = Math.Max(0, frame - radius);
            var to = Math.Min(frames - 1, frame + radius);
            for (var pc = 0; pc < Dim; pc++)
            {
                var sum = 0f;
                for (var f = from; f <= to; f++) sum += data[f * Dim + pc];
                result[frame * Dim + pc] = sum / (to - from + 1);
            }
        }

        return result;
    }

    /// <summary>Subtracts each frame's mean so only the pitch-class contrast remains.</summary>
    public static void CenterFrames(float[] data)
    {
        for (var offset = 0; offset < data.Length; offset += Dim)
        {
            var mean = 0f;
            for (var pc = 0; pc < Dim; pc++) mean += data[offset + pc];
            mean /= Dim;
            for (var pc = 0; pc < Dim; pc++) data[offset + pc] -= mean;
        }
    }

    /// <summary>Scales every frame to unit length. Silent frames stay all-zero.</summary>
    public static void NormalizeFrames(float[] data)
    {
        for (var offset = 0; offset < data.Length; offset += Dim)
        {
            var sumSquares = 0f;
            for (var pc = 0; pc < Dim; pc++) sumSquares += data[offset + pc] * data[offset + pc];
            if (sumSquares < 1e-9f) continue;
            var inverseNorm = 1f / MathF.Sqrt(sumSquares);
            for (var pc = 0; pc < Dim; pc++) data[offset + pc] *= inverseNorm;
        }
    }
}
