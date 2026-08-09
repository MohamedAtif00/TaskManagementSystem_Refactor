using Microsoft.AspNetCore.StaticFiles;

namespace AutomatedTaskSystem.Helper
{
    public class RollbackAttachmentHelper
    {
        public const string StorageFolder = "RollbackAttachments";
        public const long MaxFileSize = 10 * 1024 * 1024;
        public const int MaxFileCount = 5;

        private static readonly string[] AllowedExtensions = new[]
        {
            ".pdf",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx",
            ".ppt",
            ".pptx"
        };

        private static readonly string[] AllowedContentTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation"
        };

        /// <summary>
        /// Validates every file up front so a rejected attachment never leaves a partially
        /// applied rollback behind. Returns null when all files are acceptable.
        /// </summary>
        public string? ValidateAttachments(List<IFormFile>? files)
        {
            if (files is null || files.Count == 0)
                return null;

            if (files.Count > MaxFileCount)
                return $"You can attach up to {MaxFileCount} files.";

            foreach (var file in files)
            {
                if (file is null || file.Length == 0)
                    return "One of the attached files is empty.";

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                    return $"\"{file.FileName}\" is not allowed. Only Word, PDF, Excel and PowerPoint files are accepted.";

                if (!AllowedContentTypes.Contains(file.ContentType?.ToLowerInvariant()))
                    return $"\"{file.FileName}\" is not allowed. Only Word, PDF, Excel and PowerPoint files are accepted.";

                if (file.Length > MaxFileSize)
                    return $"\"{file.FileName}\" exceeds the {MaxFileSize / (1024 * 1024)} MB limit.";
            }

            return null;
        }

        public async Task<string> SaveAttachment(
            IFormFile file,
            IWebHostEnvironment webHostEnvironment
        )
        {
            var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, StorageFolder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{SanitizeFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(fileStream);

            return Path.Combine(StorageFolder, uniqueFileName).Replace("\\", "/");
        }

        public string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
                contentType = "application/octet-stream";
            return contentType;
        }

        /// <summary>
        /// Strips any directory component supplied by the client so the upload cannot
        /// escape the storage folder.
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            foreach (var invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');
            return string.IsNullOrWhiteSpace(name) ? "attachment" : name;
        }
    }
}
