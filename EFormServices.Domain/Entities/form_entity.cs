// EFormServices.Domain/Entities/form_entity.cs
// Got code 30/05/2025
using EFormServices.Domain.ValueObjects;
using EFormServices.Domain.Enums;

namespace EFormServices.Domain.Entities;

public class Form : BaseEntity
{
    public int OrganizationId { get; private set; }
    public int? DepartmentId { get; private set; }
    public int CreatedByUserId { get; private set; }
    public int? ApprovalWorkflowId { get; private set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; private set; }
    public FormType FormType { get; private set; }
    public bool IsTemplate { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPublic { get; private set; }
    public FormSettings Settings { get; private set; }
    public FormMetadata Metadata { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string FormKey { get; set; } = string.Empty;

    public Organization Organization { get; private set; } = null!;
    public Department? Department { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public ApprovalWorkflow? ApprovalWorkflow { get; private set; }

    private readonly List<FormField> _formFields = new();
    private readonly List<FormSubmission> _formSubmissions = new();

    public IReadOnlyCollection<FormField> FormFields => _formFields.AsReadOnly();
    public IReadOnlyCollection<FormSubmission> FormSubmissions => _formSubmissions.AsReadOnly();

    public Form() 
    { 
        Settings = FormSettings.Default();
        Metadata = FormMetadata.Default();
        FormKey = GenerateFormKey();
    }

    public Form(int organizationId, int createdByUserId, string title, FormType formType,
                string? description = null, int? departmentId = null, int? approvalWorkflowId = null)
    {
        OrganizationId = organizationId;
        DepartmentId = departmentId;
        CreatedByUserId = createdByUserId;
        ApprovalWorkflowId = approvalWorkflowId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        FormType = formType;
        IsTemplate = false;
        IsActive = true;
        IsPublic = false;
        Settings = FormSettings.Default();
        Metadata = FormMetadata.Default();
        FormKey = GenerateFormKey();
        UpdateTimestamp();
    }

    public void UpdateDetails(string title, string? description = null)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        UpdateTimestamp();
    }

    public void UpdateSettings(FormSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdateTimestamp();
    }

    public void SetApprovalWorkflow(int? approvalWorkflowId)
    {
        ApprovalWorkflowId = approvalWorkflowId;
        UpdateTimestamp();
    }

    public void ConvertToTemplate()
    {
        if (!IsTemplate)
        {
            IsTemplate = true;
            UpdateTimestamp();
        }
    }

    public void Publish()
    {
        if (PublishedAt == null)
        {
            PublishedAt = DateTime.UtcNow;
            IsActive = true;
            UpdateTimestamp();
        }
    }

    public void MakePublic()
    {
        if (!IsPublic)
        {
            IsPublic = true;
            UpdateTimestamp();
        }
    }

    public void MakePrivate()
    {
        if (IsPublic)
        {
            IsPublic = false;
            UpdateTimestamp();
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdateTimestamp();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdateTimestamp();
        }
    }

    public void AddField(FormField field)
    {
        _formFields.Add(field);
        UpdateTimestamp();
    }

    public void RemoveField(int fieldId)
    {
        var field = _formFields.FirstOrDefault(f => f.Id == fieldId);
        if (field != null)
        {
            _formFields.Remove(field);
            UpdateTimestamp();
        }
    }

    public bool IsPublished => PublishedAt.HasValue;
    public int SubmissionCount => _formSubmissions.Count;

    private static string GenerateFormKey()
    {
        return Guid.NewGuid().ToString("N")[..12].ToLowerInvariant();
    }
}