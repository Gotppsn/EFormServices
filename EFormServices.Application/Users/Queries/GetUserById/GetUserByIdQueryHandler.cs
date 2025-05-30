// EFormServices.Application/Users/Queries/GetUserById/GetUserByIdQueryHandler.cs
// Got code 30/05/2025
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result<UserDto>.Failure("User not authenticated");

        var canViewAllUsers = _currentUser.HasPermission("view_users");
        var canViewOwnProfile = request.Id == _currentUser.UserId;

        if (!canViewAllUsers && !canViewOwnProfile)
            return Result<UserDto>.Failure("Insufficient permissions to view this user");

        var user = await _context.Users
            .Where(u => u.Id == request.Id && u.OrganizationId == _currentUser.OrganizationId)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure("User not found");

        return Result<UserDto>.Success(user);
    }
}