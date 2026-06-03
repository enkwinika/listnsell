using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NLog;
using rexell.Models;

namespace rexell.Services
{
    public class FileUploadService : IFileUploadService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _uploadBasePath;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const int DefaultMaxFiles = 5;
        private const int DefaultMaxSizeInMb = 10;

        public FileUploadService(string uploadBasePath)
        {
            _uploadBasePath = uploadBasePath;
        }

        public async Task<AjaxResults> UploadImagesAsync(HttpFileCollectionBase files, int maxFiles = DefaultMaxFiles, int maxSizeInMb = DefaultMaxSizeInMb)
        {
            try
            {
                Logger.Info($"Starting file upload. Files count: {files?.Count ?? 0}");

                var uploadedFiles = new List<string>();

                if (files == null || files.Count == 0)
                {
                    Logger.Warn("No files provided for upload");
                    return new AjaxResults
                    {
                        code = "0",
                        title = "Error",
                        message = "No files provided"
                    };
                }

                var uploadPath = Path.Combine(_uploadBasePath, "Content", "uploads", "listings");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                    Logger.Info($"Created upload directory: {uploadPath}");
                }

                int maxSizeInBytes = maxSizeInMb * 1024 * 1024;

                for (int i = 0; i < files.Count && i < maxFiles; i++)
                {
                    var file = files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        // Validate file size
                        if (file.ContentLength > maxSizeInBytes)
                        {
                            Logger.Warn($"File size exceeds limit. Size: {file.ContentLength}, Limit: {maxSizeInBytes}");
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = $"File size must be less than {maxSizeInMb}MB"
                            };
                        }

                        // Validate file type
                        var extension = Path.GetExtension(file.FileName)?.ToLower();

                        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                        {
                            Logger.Warn($"Invalid file type: {extension}");
                            return new AjaxResults
                            {
                                code = "0",
                                title = "Error",
                                message = "Only image files are allowed (jpg, jpeg, png, gif)"
                            };
                        }

                        // Generate unique filename
                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        // Save file asynchronously
                        await Task.Run(() => file.SaveAs(filePath));

                        // Store relative path
                        var relativePath = $"/Content/uploads/listings/{fileName}";
                        uploadedFiles.Add(relativePath);

                        Logger.Info($"File uploaded successfully: {fileName}");
                    }
                }

                Logger.Info($"Upload completed. Total files uploaded: {uploadedFiles.Count}");

                return new AjaxResults
                {
                    code = "1",
                    title = "Success",
                    message = "Images uploaded successfully",
                    data = uploadedFiles
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error uploading files");
                return new AjaxResults
                {
                    code = "0",
                    title = "Error",
                    message = "Failed to upload images"
                };
            }
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return false;

                var fullPath = Path.Combine(_uploadBasePath, filePath.TrimStart('/').Replace("/", "\\"));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Logger.Info($"File deleted: {filePath}");
                    return true;
                }

                Logger.Warn($"File not found for deletion: {filePath}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error deleting file: {filePath}");
                return false;
            }
        }
    }
}
