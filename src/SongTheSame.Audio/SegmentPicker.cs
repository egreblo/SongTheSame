namespace SongTheSame.Audio;

/// <summary>Chooses the part of a preview that players are asked to match in phase 2.</summary>
public static class SegmentPicker
{
    private const int Margin = 10;
    private const int Step = 5;
    private const float MinimumVoicedShare = 0.95f;

    /// <summary>
    /// Returns the start frame of the clearest tonal window: no (or almost no) silence and chroma
    /// frames with distinct peaks, preferring the middle of the preview on ties.
    /// </summary>
    public static int PickStart(ChromaSequence chroma, int segmentFrames)
    {
        var lastStart = chroma.FrameCount - segmentFrames - Margin;
        if (lastStart < Margin) return Math.Max(0, (chroma.FrameCount - segmentFrames) / 2);

        var middle = (chroma.FrameCount - segmentFrames) / 2.0;
        var bestStart = Margin;
        var bestScore = double.NegativeInfinity;

        for (var start = Margin; start <= lastStart; start += Step)
        {
            var score = Clarity(chroma, start, segmentFrames) - Math.Abs(start - middle) * 1e-4;
            if (score <= bestScore) continue;
            bestScore = score;
            bestStart = start;
        }

        return bestStart;
    }

    private static double Clarity(ChromaSequence chroma, int start, int length)
    {
        var voiced = 0;
        var peakSum = 0.0;
        for (var frame = start; frame < start + length; frame++)
        {
            var peak = 0f;
            foreach (var value in chroma.Frame(frame)) peak = Math.Max(peak, value);
            if (peak <= 0f) continue;
            voiced++;
            peakSum += peak;
        }

        if ((float)voiced / length < MinimumVoicedShare) return double.NegativeInfinity;
        return peakSum / length;
    }
}
