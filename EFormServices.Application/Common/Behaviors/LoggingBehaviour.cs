// EFormServices.Application/Common/Behaviors/LoggingBehaviour.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EFormServices.Application.Common.Behaviors;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUser.UserId?.ToString() ?? "Anonymous";
        var organizationId = _currentUser.OrganizationId?.ToString() ?? "N/A";

        _logger.LogInformation("Request: {Name} by User {UserId} from Organization {OrganizationId} {@Request}",
            requestName, userId, organizationId, request);

        return await next();
    }
}