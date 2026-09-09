namespace TaskManagementSystem.Modules.HR.Application;

public interface IMedicalCertificateStorage
{
    bool IsValid(string fileName, long fileSizeBytes);

    Task<string> SaveAsync(Stream content, string fileName, int leaveRequestId, CancellationToken cancellationToken = default);

    Task<(Stream Content, string FileName, string ContentType)?> OpenAsync(
        string relativePath,
        string? fileName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string? relativePath, CancellationToken cancellationToken = default);
}
