// EFormServices.Infrastructure/Services/FileStorageService.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace EFormServices.Infrastructure.Services;

public interface IFileStorageService
{
    Task<(string filePath, string fileHash, long fileSize)> SaveFileAsync(IFormFile file, string containerName);
    Task<Stream> GetFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _basePath = _configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<(string filePath, string fileHash, long fileSize)> SaveFileAsync(IFormFile file, string containerName)
    {
        var containerPath = Path.Combine(_basePath, containerName);
        if (!Directory.Exists(containerPath))
        {
            Directory.CreateDirectory(containerPath);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(containerPath, fileName);
        var relativePath = Path.Combine(containerName, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        var fileHash = await CalculateFileHashAsync(fullPath);
        var fileSize = new FileInfo(fullPath).Length;

        _logger.LogInformation("File saved: {FilePath}, Size: {FileSize} bytes", relativePath, fileSize);

        return (relativePath, fileHash, fileSize);
    }

    public async Task<Stream> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return await Task.FromResult(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    private static async Task<string> CalculateFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToBase64String(hash);
    }
}