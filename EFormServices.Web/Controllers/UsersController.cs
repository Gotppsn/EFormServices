// EFormServices.Web/Controllers/UsersController.cs
// Got code 30/05/2025
using EFormServices.Application.Users.Commands.CreateUser;
using EFormServices.Application.Users.Queries.GetUserById;
using EFormServices.Application.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));
        
        if (!result.IsSuccess)
            return NotFound(result.Errors);

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result.Data);
    }
}