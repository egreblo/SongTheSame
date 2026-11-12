using Microsoft.Extensions.DependencyInjection;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Application.Features.Similarity;

namespace SongTheSame.Application;

public static class DependencyInjection
{
    private static readonly Type HandlerInterface = typeof(IHandler<,>);

    /// <summary>Registers every <see cref="IHandler{TRequest,TResponse}"/> found in this assembly.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var handlers = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == HandlerInterface)
                .Select(i => (Service: i, Implementation: type)));

        foreach (var (service, implementation) in handlers)
            services.AddScoped(service, implementation);

        services.AddScoped<ISimilarityService, SimilarityService>();
        services.AddScoped<SessionLoader>();
        services.AddScoped<SessionViewBuilder>();
        services.AddScoped<PuzzleRoundBuilder>();
        services.AddScoped<SessionStarter>();
        services.AddScoped<DailyPuzzleProvider>();
        return services;
    }
}
