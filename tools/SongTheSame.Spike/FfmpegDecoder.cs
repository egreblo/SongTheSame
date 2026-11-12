using System.Diagnostics;

namespace SongTheSame.Spike;

/// <summary>Spike-only decoder: shells out to ffmpeg and returns mono float PCM.</summary>
public static class FfmpegDecoder
{
    public static async Task<float[]> DecodeAsync(string path, int sampleRate, string? audioFilter = null)
    {
        var filter = audioFilter is null ? "" : $"-af \"{audioFilter}\" ";
        var args = $"-v error -i \"{path}\" {filter}-f f32le -ac 1 -ar {sampleRate} -";

        using var process = Process.Start(new ProcessStartInfo("ffmpeg", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        }) ?? throw new InvalidOperationException("Could not start ffmpeg.");

        using var buffer = new MemoryStream();
        var errors = process.StandardError.ReadToEndAsync();
        await process.StandardOutput.BaseStream.CopyToAsync(buffer);
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"ffmpeg failed: {await errors}");

        var bytes = buffer.ToArray();
        var samples = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, samples, 0, samples.Length * sizeof(float));
        return samples;
    }
}
