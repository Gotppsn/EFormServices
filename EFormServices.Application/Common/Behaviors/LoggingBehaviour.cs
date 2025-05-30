// EFormServices.Application/Common/Behaviors/LoggingBehaviour.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace EFormServices.Application.Common.Behaviors;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUser.UserId?.ToString() ?? "Anonymous";
        var organizationId = _currentUser.OrganizationId?.ToString() ?? "N/A";

        _logger.LogInformation("Request: {Name} by User {UserId} from Organization {OrganizationId} {@Request}",
            requestName, userId, organizationId, request);

        return Task.CompletedTask;
    }
}