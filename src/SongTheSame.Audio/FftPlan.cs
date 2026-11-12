namespace SongTheSame.Audio;

/// <summary>Radix-2 in-place complex FFT with precomputed twiddle factors. Not thread-safe per instance.</summary>
public sealed class FftPlan
{
    private readonly int _size;
    private readonly float[] _cos;
    private readonly float[] _sin;
    private readonly int[] _bitReversed;

    public FftPlan(int size)
    {
        if (size < 2 || (size & (size - 1)) != 0)
            throw new ArgumentException("FFT size must be a power of two.", nameof(size));

        _size = size;
        _cos = new float[size / 2];
        _sin = new float[size / 2];
        for (var i = 0; i < size / 2; i++)
        {
            var angle = -2.0 * Math.PI * i / size;
            _cos[i] = (float)Math.Cos(angle);
            _sin[i] = (float)Math.Sin(angle);
        }

        _bitReversed = BuildBitReversalTable(size);
    }

    public int Size => _size;

    public void Forward(float[] real, float[] imaginary)
    {
        for (var i = 0; i < _size; i++)
        {
            var j = _bitReversed[i];
            if (j <= i) continue;
            (real[i], real[j]) = (real[j], real[i]);
            (imaginary[i], imaginary[j]) = (imaginary[j], imaginary[i]);
        }

        for (var length = 2; length <= _size; length <<= 1)
        {
            var half = length / 2;
            var step = _size / length;
            for (var start = 0; start < _size; start += length)
            {
                for (var k = 0; k < half; k++)
                {
                    var wr = _cos[k * step];
                    var wi = _sin[k * step];
                    var a = start + k;
                    var b = a + half;
                    var tr = real[b] * wr - imaginary[b] * wi;
                    var ti = real[b] * wi + imaginary[b] * wr;
                    real[b] = real[a] - tr;
                    imaginary[b] = imaginary[a] - ti;
                    real[a] += tr;
                    imaginary[a] += ti;
                }
            }
        }
    }

    private static int[] BuildBitReversalTable(int size)
    {
        var bits = (int)Math.Log2(size);
        var table = new int[size];
        for (var i = 0; i < size; i++)
        {
            var reversed = 0;
            for (var b = 0; b < bits; b++)
                if ((i & (1 << b)) != 0) reversed |= 1 << (bits - 1 - b);
            table[i] = reversed;
        }

        return table;
    }
}
