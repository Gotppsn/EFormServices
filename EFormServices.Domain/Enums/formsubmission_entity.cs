// Got code 27/05/2025
using EFormServices.Domain.Enums;

namespace EFormServices.Domain.Entities;

public class FormSubmission : BaseEntity
{
    public int FormId { get; private set; }
    public int SubmittedByUserId { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public string TrackingNumber { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public Form Form { get; private set; } = null!;
    public User SubmittedByUser { get; private set; } = null!;

    private readonly List<SubmissionValue> _submissionValues = new();
    private readonly List<FileAttachment> _fileAttachments = new();
    private readonly List<ApprovalProcess> _approvalProcesses = new();

    public IReadOnlyCollection<SubmissionValue> SubmissionValues => _submissionValues.AsReadOnly();
    public IReadOnlyCollection<FileAttachment> FileAttachments => _fileAttachments.AsReadOnly();
    public IReadOnlyCollection<ApprovalProcess> ApprovalProcesses => _approvalProcesses.AsReadOnly();

    private FormSubmission() { }

    public FormSubmission(int formId, int submittedByUserId, string? ipAddress = null, string? userAgent = null)
    {
        FormId = formId;
        SubmittedByUserId = submittedByUserId;
        Status = SubmissionStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        TrackingNumber = GenerateTrackingNumber();
        IpAddress = ipAddress;
        UserAgent = userAgent;
        UpdateTimestamp();
    }

    public void UpdateStatus(SubmissionStatus status)
    {
        Status = status;
        UpdateTimestamp();
    }

    public void AddValue(int formFieldId, string fieldName, string value, string valueType)
    {
        var existingValue = _submissionValues.FirstOrDefault(sv => sv.FormFieldId == formFieldId);
        if (existingValue != null)
        {
            existingValue.UpdateValue(value, valueType);
        }
        else
        {
            _submissionValues.Add(new SubmissionValue(Id, formFieldId, fieldName, value, valueType));
        }
        UpdateTimestamp();
    }

    public void RemoveValue(int formFieldId)
    {
        var value = _submissionValues.FirstOrDefault(sv => sv.FormFieldId == formFieldId);
        if (value != null)
        {
            _submissionValues.Remove(value);
            UpdateTimestamp();
        }
    }

    public void AddFileAttachment(int formFieldId, string fileName, long fileSize, string contentType, string storagePath, string fileHash)
    {
        _fileAttachments.Add(new FileAttachment(Id, formFieldId, fileName, fileSize, contentType, storagePath, fileHash));
        UpdateTimestamp();
    }

    public void Approve()
    {
        if (Status == SubmissionStatus.PendingApproval)
        {
            Status = SubmissionStatus.Approved;
            UpdateTimestamp();
        }
    }

    public void Reject()
    {
        if (Status == SubmissionStatus.PendingApproval)
        {
            Status = SubmissionStatus.Rejected;
            UpdateTimestamp();
        }
    }

    public void SubmitForApproval()
    {
        if (Status == SubmissionStatus.Submitted)
        {
            Status = SubmissionStatus.PendingApproval;
            UpdateTimestamp();
        }
    }

    public string? GetFieldValue(string fieldName)
    {
        return _submissionValues.FirstOrDefault(sv => sv.FieldName == fieldName)?.Value;
    }

    public bool HasField(string fieldName)
    {
        return _submissionValues.Any(sv => sv.FieldName == fieldName);
    }

    public bool IsComplete => Status != SubmissionStatus.Draft;
    public bool RequiresApproval => Status == SubmissionStatus.PendingApproval;

    private static string GenerateTrackingNumber()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var random = Random.Shared.Next(1000, 9999).ToString();
        return $"F{timestamp}{random}";
    }
}