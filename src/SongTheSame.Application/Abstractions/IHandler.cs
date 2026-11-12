namespace SongTheSame.Application.Abstractions;

/// <summary>A use case: takes a request object, returns a response. One handler per file.</summary>
public interface IHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
