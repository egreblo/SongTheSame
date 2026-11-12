using SongTheSame.Audio;

namespace SongTheSame.Application.Abstractions;

/// <summary>Turns an encoded audio file (mp3) into a chroma sequence.</summary>
public interface IChromaExtractor
{
    ChromaSequence Extract(byte[] encodedAudio);
}
