using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;

namespace SongTheSame.Application.Features.Game.GetSession;

public sealed record GetSessionQuery(Guid SessionId, string PlayerKey);

public sealed class GetSessionHandler(SessionLoader loader, SessionViewBuilder views) : IHandler<GetSessionQuery, SessionView>
{
    public async Task<SessionView> HandleAsync(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var (session, puzzle) = await loader.LoadAsync(request.SessionId, request.PlayerKey, cancellationToken);
        return await views.BuildAsync(session, puzzle, cancellationToken);
    }
}
