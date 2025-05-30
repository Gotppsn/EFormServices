// EFormServices.Application/FormFields/Commands/AddFormField/AddFormFieldCommand.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Models;
using EFormServices.Domain.Enums;
using MediatR;

namespace EFormServices.Application.FormFields.Commands.AddFormField;

public record AddFormFieldCommand : IRequest<Result<int>>
{
    public int FormId { get; init; }
    public FieldType FieldType { get; init; }
    public string Label { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsRequired { get; init; }
    public int SortOrder { get; init; }
    public Dictionary<string, object>? ValidationRules { get; init; }
    public Dictionary<string, object>? Settings { get; init; }
    public List<FormFieldOptionRequest>? Options { get; init; }
}

public record FormFieldOptionRequest
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}