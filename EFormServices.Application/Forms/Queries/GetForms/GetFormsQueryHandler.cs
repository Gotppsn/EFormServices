// EFormServices.Application/Forms/Queries/GetForms/GetFormsQueryHandler.cs
// Got code 30/05/2025
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Forms.Queries.GetForms;

public class GetFormsQueryHandler : IRequestHandler<GetFormsQuery, Result<PagedResult<FormDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetFormsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<FormDto>>> Handle(GetFormsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result<PagedResult<FormDto>>.Failure("User not authenticated");

        var query = _context.Forms
            .Where(f => f.OrganizationId == _currentUser.OrganizationId);

        if (!_currentUser.HasPermission("view_all_forms"))
        {
            query = query.Where(f => f.CreatedByUserId == _currentUser.UserId || 
                                   f.IsPublic ||
                                   (f.DepartmentId == null) ||
                                   (f.Department!.Users.Any(u => u.Id == _currentUser.UserId)));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(f => f.Title.Contains(request.SearchTerm) ||
                                   (f.Description != null && f.Description.Contains(request.SearchTerm)));
        }

        if (request.FormType.HasValue)
            query = query.Where(f => f.FormType == request.FormType);

        if (request.DepartmentId.HasValue)
            query = query.Where(f => f.DepartmentId == request.DepartmentId);

        if (request.IsTemplate.HasValue)
            query = query.Where(f => f.IsTemplate == request.IsTemplate);

        if (request.IsPublished.HasValue)
            query = query.Where(f => f.IsPublished == request.IsPublished);

        if (request.IsActive.HasValue)
            query = query.Where(f => f.IsActive == request.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy.ToLowerInvariant() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(f => f.Title) : query.OrderBy(f => f.Title),
            "formtype" => request.SortDescending ? query.OrderByDescending(f => f.FormType) : query.OrderBy(f => f.FormType),
            "publishedat" => request.SortDescending ? query.OrderByDescending(f => f.PublishedAt) : query.OrderBy(f => f.PublishedAt),
            "updatedat" => request.SortDescending ? query.OrderByDescending(f => f.UpdatedAt) : query.OrderBy(f => f.UpdatedAt),
            _ => request.SortDescending ? query.OrderByDescending(f => f.CreatedAt) : query.OrderBy(f => f.CreatedAt)
        };

        var forms = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<FormDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<FormDto>(forms, totalCount, request.Page, request.PageSize);
        return Result<PagedResult<FormDto>>.Success(pagedResult);
    }
}