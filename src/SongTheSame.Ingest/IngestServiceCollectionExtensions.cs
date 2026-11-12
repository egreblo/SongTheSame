using Microsoft.Extensions.DependencyInjection;
using SongTheSame.Ingest.Commands;

namespace SongTheSame.Ingest;

internal static class IngestServiceCollectionExtensions
{
    public static IServiceCollection AddIngestCommands(this IServiceCollection services) =>
        services
            .AddScoped<CommandRouter>()
            .AddScoped<MigrateCommand>()
            .AddScoped<PlaylistCommand>()
            .AddScoped<SeedCommand>()
            .AddScoped<CalibrateCommand>()
            .AddScoped<PuzzlesCommand>()
            .AddScoped<RefreshMatchesCommand>();
}
