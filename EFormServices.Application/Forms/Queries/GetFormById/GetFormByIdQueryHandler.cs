// EFormServices.Application/Forms/Queries/GetFormById/GetFormByIdQueryHandler.cs
// Got code 30/05/2025
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Forms.Queries.GetFormById;

public class GetFormByIdQueryHandler : IRequestHandler<GetFormByIdQuery, Result<FormDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetFormByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<Result<FormDto>> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Forms.AsQueryable();

        if (_currentUser.IsAuthenticated && _currentUser.OrganizationId.HasValue)
        {
            query = query.Where(f => f.OrganizationId == _currentUser.OrganizationId);

            if (!_currentUser.HasPermission("view_all_forms"))
            {
                query = query.Where(f => f.CreatedByUserId == _currentUser.UserId || 
                                       f.IsPublic ||
                                       (f.DepartmentId == null) ||
                                       (f.Department!.Users.Any(u => u.Id == _currentUser.UserId)));
            }
        }
        else
        {
            query = query.Where(f => f.IsPublic);
        }

        var form = await query
            .Where(f => f.Id == request.Id)
            .ProjectTo<FormDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (form == null)
            return Result<FormDto>.Failure("Form not found");

        return Result<FormDto>.Success(form);
    }
}