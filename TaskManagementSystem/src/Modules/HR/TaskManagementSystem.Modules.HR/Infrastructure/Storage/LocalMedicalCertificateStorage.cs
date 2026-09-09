using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Storage;

internal sealed class LocalMedicalCertificateStorage(
    IHostEnvironment environment,
    IOptions<LeaveSettingsOptions> leaveSettings) : IMedicalCertificateStorage
{
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    public bool IsValid(string fileName, long fileSizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileSizeBytes <= 0 || fileSizeBytes > MaxFileSize)
        {
            return false;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        int leaveRequestId,
        CancellationToken cancellationToken = default)
    {
        var uploadsFolder = GetUploadsFolder();
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{leaveRequestId}_{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        return Path.Combine(leaveSettings.Value.MedicalCertificateRelativePath, uniqueFileName)
            .Replace('\\', '/');
    }

    public Task<(Stream Content, string FileName, string ContentType)?> OpenAsync(
        string relativePath,
        string? fileName,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(environment.ContentRootPath, "wwwroot", relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<(Stream, string, string)?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var resolvedName = fileName ?? Path.GetFileName(fullPath);
        var contentType = Path.GetExtension(resolvedName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        return Task.FromResult<(Stream, string, string)?>((stream, resolvedName, contentType));
    }

    public Task DeleteAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var fullPath = Path.Combine(environment.ContentRootPath, "wwwroot", relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetUploadsFolder() =>
        Path.Combine(
            environment.ContentRootPath,
            "wwwroot",
            leaveSettings.Value.MedicalCertificateRelativePath);
}
