using Microsoft.Extensions.Logging;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.IngestTrack;

namespace SongTheSame.Application.Features.Catalog.IngestPlaylist;

/// <summary>Ingests every track of a Deezer playlist. A failing track is logged and skipped, never fatal.</summary>
public sealed partial class IngestPlaylistHandler(
    IMusicSource source,
    IHandler<IngestTrackCommand, IngestOutcome> ingestTrack,
    ILogger<IngestPlaylistHandler> logger) : IHandler<IngestPlaylistCommand, IngestPlaylistResult>
{
    public async Task<IngestPlaylistResult> HandleAsync(IngestPlaylistCommand request, CancellationToken cancellationToken)
    {
        var tracks = await source.GetPlaylistTracksAsync(request.PlaylistId, request.Limit, cancellationToken);
        int added = 0, skipped = 0, failed = 0;

        foreach (var track in tracks)
        {
            try
            {
                var outcome = await ingestTrack.HandleAsync(new IngestTrackCommand(track, request.Origin), cancellationToken);
                if (outcome == IngestOutcome.Added) added++;
                else skipped++;
                LogOutcome(track.Artist, track.Title, outcome);
            }
            catch (Exception exception) when (exception is HttpRequestException or InvalidDataException or InvalidOperationException)
            {
                failed++;
                LogFailure(exception, track.Artist, track.Title);
            }
        }

        return new IngestPlaylistResult(added, skipped, failed);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Artist} - {Title}: {Outcome}")]
    private partial void LogOutcome(string artist, string title, IngestOutcome outcome);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Artist} - {Title}: failed")]
    private partial void LogFailure(Exception exception, string artist, string title);
}
