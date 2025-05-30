// Got code 27/05/2025
namespace EFormServices.Domain.Entities;

public class FileAttachment : BaseEntity
{
    public int FormSubmissionId { get; private set; }
    public int FormFieldId { get; private set; }
    public string FileName { get; private set; }
    public long FileSize { get; private set; }
    public string ContentType { get; private set; }
    public string StoragePath { get; private set; }
    public string FileHash { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public FormSubmission FormSubmission { get; private set; } = null!;
    public FormField FormField { get; private set; } = null!;

    private FileAttachment() { }

    public FileAttachment(int formSubmissionId, int formFieldId, string fileName, long fileSize, 
                         string contentType, string storagePath, string fileHash)
    {
        FormSubmissionId = formSubmissionId;
        FormFieldId = formFieldId;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        FileSize = fileSize;
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        FileHash = fileHash ?? throw new ArgumentNullException(nameof(fileHash));
        UploadedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void UpdateStoragePath(string storagePath)
    {
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        UpdateTimestamp();
    }

    public string GetFileExtension()
    {
        return Path.GetExtension(FileName).ToLowerInvariant();
    }

    public bool IsImage()
    {
        var imageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp" };
        return imageTypes.Contains(ContentType.ToLowerInvariant());
    }

    public bool IsPdf()
    {
        return ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
    }

    public string GetFileSizeFormatted()
    {
        if (FileSize < 1024)
            return $"{FileSize} B";
        if (FileSize < 1024 * 1024)
            return $"{FileSize / 1024:F1} KB";
        if (FileSize < 1024 * 1024 * 1024)
            return $"{FileSize / (1024 * 1024):F1} MB";
        return $"{FileSize / (1024 * 1024 * 1024):F1} GB";
    }
}