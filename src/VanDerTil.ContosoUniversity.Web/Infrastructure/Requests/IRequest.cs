using Microsoft.Extensions.DependencyInjection;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure.Requests;

public interface IRequest<TResponse>
{
}

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}


public interface IRequestDispatcher
{
    Task<TResponse> Handle<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}

public sealed class ReflectionRequestDispatcher : IRequestDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    public ReflectionRequestDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public async Task<TResponse> Handle<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = (IRequestHandler<IRequest<TResponse>, TResponse>)_serviceProvider.GetRequiredService(handlerType);

        return await handler.Handle(request, cancellationToken);
    }
}
