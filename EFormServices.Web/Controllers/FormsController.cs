// EFormServices.Web/Controllers/FormsController.cs
// Got code 30/05/2025
using EFormServices.Application.Forms.Commands.CreateForm;
using EFormServices.Application.Forms.Commands.PublishForm;
using EFormServices.Application.Forms.Commands.UpdateForm;
using EFormServices.Application.Forms.Queries.GetFormById;
using EFormServices.Application.Forms.Queries.GetForms;
using EFormServices.Application.FormFields.Commands.AddFormField;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FormsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetForms([FromQuery] GetFormsQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForm(int id)
    {
        var result = await _mediator.Send(new GetFormByIdQuery(id));
        
        if (!result.IsSuccess)
            return NotFound(result.Errors);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateForm([FromBody] CreateFormCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetForm), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateForm(int id, [FromBody] UpdateFormCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(result.Data);
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishForm(int id)
    {
        var result = await _mediator.Send(new PublishFormCommand(id));
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(new { message = "Form published successfully" });
    }

    [HttpPost("{id}/fields")]
    public async Task<IActionResult> AddFormField(int id, [FromBody] AddFormFieldCommand command)
    {
        if (id != command.FormId)
            return BadRequest("Form ID mismatch");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(new { fieldId = result.Data });
    }
}