// EFormServices.Web/Controllers/OrganizationsController.cs
// Got code 30/05/2025
using EFormServices.Application.Organizations.Commands.CreateOrganization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganizationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return CreatedAtAction("GetOrganization", new { id = result.Data!.Id }, result.Data);
    }
}