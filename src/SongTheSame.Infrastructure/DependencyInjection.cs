using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SongTheSame.Application.Abstractions;
using SongTheSame.Infrastructure.Audio;
using SongTheSame.Infrastructure.Deezer;
using SongTheSame.Infrastructure.Persistence;

namespace SongTheSame.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Database";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddMemoryCache();
        services.AddScoped<ISongCatalog, SongCatalog>();
        services.AddScoped<ISongFeatureStore, SongFeatureStore>();
        services.AddScoped<ICalibrationStore, CalibrationStore>();
        services.AddScoped<IPuzzleRepository, PuzzleRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<ILeagueRepository, LeagueRepository>();

        services.AddHttpClient<IMusicSource, DeezerMusicSource>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SongTheSame/0.1");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddSingleton<IChromaExtractor, Mp3ChromaExtractor>();

        return services;
    }
}
