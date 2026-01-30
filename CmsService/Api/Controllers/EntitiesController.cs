using CmsService.Api.Contracts;
using CmsService.Api.Security;
using CmsService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CmsService.Api.Controllers;

[Authorize(AuthenticationSchemes = "UserBasic")]
[ApiController]
[Route("api/entities")]
public class EntitiesController : ControllerBase
{
    private readonly WriteDbContext _writeDb;
    private readonly ReadDbContext _readDb;
    private readonly IUserContextAccessor _userContext;

    public EntitiesController(
        WriteDbContext writeDb,
        ReadDbContext readDb,
        IUserContextAccessor userContext)
    {
        _writeDb = writeDb;
        _readDb = readDb;
        _userContext = userContext;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var isAdmin = _userContext.Current.IsAdmin;

        var entity = await _readDb.Entities
            .Where(e =>
                e.Id == id &&
                (
                    isAdmin ||
                    (
                        !e.IsDeleted &&
                        e.LatestPublishedVersion != null &&
                        !e.IsDisabledByAdmin
                    )
                )
            )
            .Select(e => new EntityResponse(
                e.Id,
                e.LatestPublishedVersion,
                e.LatestPublishedVersion == null
                    ? null
                    : e.Versions
                        .Where(v => v.Version == e.LatestPublishedVersion)
                        .Select(v => v.Payload)
                        .FirstOrDefault()
            ))
            .FirstOrDefaultAsync();

        if (entity is null)
            return NotFound();

        return Ok(entity);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var isAdmin = _userContext.Current.IsAdmin;

        var entities = await _readDb.Entities
            .Where(e =>
                isAdmin ||
                (
                    !e.IsDeleted &&
                    !e.IsDisabledByAdmin &&
                    e.LatestPublishedVersion != null
                )
            )
            .Select(e => new EntityResponse(
                e.Id,
                e.LatestPublishedVersion,
                e.LatestPublishedVersion == null
                    ? null
                    : e.Versions
                        .Where(v => v.Version == e.LatestPublishedVersion)
                        .Select(v => v.Payload)
                        .FirstOrDefault()
            ))
            .ToListAsync();

        return Ok(entities);
    }
    
    [HttpPost("{id}/disable")]
    public async Task<IActionResult> Disable(string id)
    {
        if (!_userContext.Current.IsAdmin)
            return Forbid("UserBasic");

        var entity = await _writeDb.Entities.FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null)
            return NotFound();

        entity.DisableByAdmin();
        await _writeDb.SaveChangesAsync();

        return NoContent();
    }
}