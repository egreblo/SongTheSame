using SongTheSame.Application.Abstractions;
using SongTheSame.Audio;

namespace SongTheSame.Application.Tests.Fakes;

internal sealed class FakeChromaExtractor(int frames = 295) : IChromaExtractor
{
    public ChromaSequence Extract(byte[] encodedAudio) =>
        new(new float[frames * ChromaSequence.PitchClasses], framesPerSecond: 10);
}
