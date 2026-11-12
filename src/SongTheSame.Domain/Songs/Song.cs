namespace SongTheSame.Domain.Songs;

/// <summary>A catalog song. Audio is never stored; only metadata and (separately) chroma features.</summary>
public sealed class Song
{
    private Song()
    {
    }

    public Guid Id { get; private set; }

    public long DeezerId { get; private set; }

    public string Title { get; private set; } = "";

    public string Artist { get; private set; } = "";

    public string? AlbumTitle { get; private set; }

    public string? CoverUrl { get; private set; }

    public int DurationSeconds { get; private set; }

    /// <summary>Deezer popularity rank; higher means more popular.</summary>
    public int Popularity { get; private set; }

    public SongOrigin Origin { get; private set; }

    /// <summary>Normalised "title artist" text that search runs against.</summary>
    public string SearchText { get; private set; } = "";

    public DateTimeOffset AddedAt { get; private set; }

    public static Song Create(
        long deezerId,
        string title,
        string artist,
        string? albumTitle,
        string? coverUrl,
        int durationSeconds,
        int popularity,
        SongOrigin origin,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(artist)) throw new ArgumentException("Artist is required.", nameof(artist));

        return new Song
        {
            Id = Guid.NewGuid(),
            DeezerId = deezerId,
            Title = title.Trim(),
            Artist = artist.Trim(),
            AlbumTitle = albumTitle,
            CoverUrl = coverUrl,
            DurationSeconds = durationSeconds,
            Popularity = popularity,
            Origin = origin,
            SearchText = Songs.SearchText.Normalize($"{title} {artist}"),
            AddedAt = now,
        };
    }
}
