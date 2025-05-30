// EFormServices.Application/Common/Mappings/MappingProfile.cs
// Got code 30/05/2025
using AutoMapper;
using EFormServices.Application.Common.DTOs;
using EFormServices.Domain.Entities;

namespace EFormServices.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Organization, OrganizationDto>()
            .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => new OrganizationSettingsDto
            {
                TimeZone = src.Settings.TimeZone,
                DateFormat = src.Settings.DateFormat,
                Currency = src.Settings.Currency,
                AllowPublicForms = src.Settings.AllowPublicForms,
                MaxFileUploadSizeMB = src.Settings.MaxFileUploadSizeMB,
                FormRetentionDays = src.Settings.FormRetentionDays,
                RequireApprovalForPublish = src.Settings.RequireApprovalForPublish
            }));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles
                .Where(ur => ur.IsActive && (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
                .Select(ur => ur.Role.Name)
                .ToList()));

        CreateMap<Form, FormDto>()
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            .ForMember(dest => dest.SubmissionCount, opt => opt.MapFrom(src => src.SubmissionCount))
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.FullName))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => new FormSettingsDto
            {
                AllowMultipleSubmissions = src.Settings.AllowMultipleSubmissions,
                RequireAuthentication = src.Settings.RequireAuthentication,
                ShowProgressBar = src.Settings.ShowProgressBar,
                AllowSaveAndContinue = src.Settings.AllowSaveAndContinue,
                ShowSubmissionNumber = src.Settings.ShowSubmissionNumber,
                MaxSubmissions = src.Settings.MaxSubmissions,
                SubmissionStartDate = src.Settings.SubmissionStartDate,
                SubmissionEndDate = src.Settings.SubmissionEndDate,
                RedirectUrl = src.Settings.RedirectUrl,
                SuccessMessage = src.Settings.SuccessMessage
            }))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new FormMetadataDto
            {
                Version = src.Metadata.Version,
                Category = src.Metadata.Category,
                Tags = src.Metadata.Tags.ToList(),
                Language = src.Metadata.Language,
                EstimatedCompletionMinutes = src.Metadata.EstimatedCompletionMinutes
            }));
    }
}