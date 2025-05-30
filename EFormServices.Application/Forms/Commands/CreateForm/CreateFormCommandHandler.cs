// EFormServices.Application/Forms/Commands/CreateForm/CreateFormCommandHandler.cs
// Got code 30/05/2025
using AutoMapper;
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Forms.Commands.CreateForm;

public class CreateFormCommandHandler : IRequestHandler<CreateFormCommand, Result<FormDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public CreateFormCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<Result<FormDto>> Handle(CreateFormCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result<FormDto>.Failure("User not authenticated");

        if (!_currentUser.HasPermission("create_forms"))
            return Result<FormDto>.Failure("Insufficient permissions to create forms");

        if (request.DepartmentId.HasValue)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == request.DepartmentId && d.OrganizationId == _currentUser.OrganizationId, cancellationToken);
            
            if (department == null)
                return Result<FormDto>.Failure("Department not found");
        }

        if (request.ApprovalWorkflowId.HasValue)
        {
            var workflow = await _context.ApprovalWorkflows
                .FirstOrDefaultAsync(w => w.Id == request.ApprovalWorkflowId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);
            
            if (workflow == null)
                return Result<FormDto>.Failure("Approval workflow not found");
        }

        var settings = request.Settings != null 
            ? new FormSettings(
                request.Settings.AllowMultipleSubmissions,
                request.Settings.RequireAuthentication,
                request.Settings.ShowProgressBar,
                request.Settings.AllowSaveAndContinue,
                request.Settings.ShowSubmissionNumber,
                request.Settings.MaxSubmissions,
                request.Settings.SubmissionStartDate,
                request.Settings.SubmissionEndDate,
                request.Settings.RedirectUrl,
                request.Settings.SuccessMessage)
            : FormSettings.Default();

        var metadata = request.Metadata != null
            ? new FormMetadata(
                request.Metadata.Version,
                request.Metadata.Category,
                request.Metadata.Tags.ToList(),
                request.Metadata.Language,
                request.Metadata.EstimatedCompletionMinutes)
            : FormMetadata.Default();

        var form = new Form(
            _currentUser.OrganizationId.Value,
            _currentUser.UserId!.Value,
            request.Title,
            request.FormType,
            request.Description,
            request.DepartmentId,
            request.ApprovalWorkflowId
        );

        form.UpdateSettings(settings);

        _context.Forms.Add(form);
        await _context.SaveChangesAsync(cancellationToken);

        var formDto = _mapper.Map<FormDto>(form);
        return Result<FormDto>.Success(formDto);
    }
}