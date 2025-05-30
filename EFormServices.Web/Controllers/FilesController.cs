// EFormServices.Web/Controllers/FilesController.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Infrastructure.Data;
using EFormServices.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public FilesController(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    [HttpGet("{fileId}")]
    public async Task<IActionResult> DownloadFile(int fileId)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Unauthorized();

        var file = await _context.FileAttachments
            .Include(fa => fa.FormSubmission)
                .ThenInclude(fs => fs.Form)
            .FirstOrDefaultAsync(fa => fa.Id == fileId);

        if (file == null)
            return NotFound("File not found");

        if (file.FormSubmission.Form.OrganizationId != _currentUser.OrganizationId)
            return Forbid();

        var canAccess = _currentUser.HasPermission("view_forms") ||
                       file.FormSubmission.Form.CreatedByUserId == _currentUser.UserId ||
                       file.FormSubmission.SubmittedByUserId == _currentUser.UserId;

        if (!canAccess)
            return Forbid("Insufficient permissions");

        try
        {
            var fileStream = await _fileStorage.GetFileAsync(file.StoragePath);
            return File(fileStream, file.ContentType, file.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("Physical file not found");
        }
    }
}