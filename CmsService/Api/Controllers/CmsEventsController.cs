using CmsService.Api.Contracts;
using CmsService.Domain.Services;
using CmsService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsService.Api.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("cms/events")]
[Authorize(AuthenticationSchemes = "CmsBasic")]
public class CmsEventsController : ControllerBase
{
    private readonly EntityDomainService _domain;
    private readonly ILogger<CmsEventsController> _logger;

    public CmsEventsController(
        EntityDomainService domain,
        ILogger<CmsEventsController> logger)
    {
        _domain = domain;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveEvents([FromBody] List<CmsEventDto> events)
    {
        foreach (var evt in events)
        {
            try
            {
                await _domain.ProcessEventAsync(evt);
                _logger.LogInformation(
                    "Processed event {Type} for entity {EntityId}",
                    evt.Type,
                    evt.Id
                );
                
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed processing event {Type} for entity {EntityId}",
                    evt.Type,
                    evt.Id
                );
            }
        }
    
       
        return Ok();
    }
}