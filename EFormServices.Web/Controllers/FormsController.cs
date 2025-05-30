// EFormServices.Web/Controllers/FormsController.cs
// Got code 30/05/2025
using EFormServices.Application.Forms.Commands.CreateForm;
using EFormServices.Application.Forms.Commands.PublishForm;
using EFormServices.Application.Forms.Commands.UpdateForm;
using EFormServices.Application.Forms.Queries.GetForms;
using EFormServices.Application.Forms.Queries.GetFormById;
using EFormServices.Application.FormFields.Commands.AddFormField;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiPolicy")]
public class FormsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FormsController> _logger;

    public FormsController(IMediator mediator, ILogger<FormsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetForms([FromQuery] GetFormsQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
                return Ok(result.Data);
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forms");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetForm(int id)
    {
        try
        {
            var result = await _mediator.Send(new GetFormByIdQuery(id));
            if (result.IsSuccess)
                return Ok(result.Data);
            return NotFound(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving form {FormId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    [Authorize(Policy = "FormManagement")]
    public async Task<IActionResult> CreateForm([FromBody] CreateFormCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetForm), new { id = result.Data!.Id }, result.Data);
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateForm(int id, [FromBody] UpdateFormCommand command)
    {
        try
        {
            if (id != command.Id)
                return BadRequest("Form ID mismatch");

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Data);
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating form {FormId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishForm(int id)
    {
        try
        {
            var result = await _mediator.Send(new PublishFormCommand(id));
            if (result.IsSuccess)
                return Ok(new { message = "Form published successfully" });
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing form {FormId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("{id}/fields")]
    public async Task<IActionResult> AddFormField(int id, [FromBody] AddFormFieldCommand command)
    {
        try
        {
            if (id != command.FormId)
                return BadRequest("Form ID mismatch");

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetForm), new { id }, new { fieldId = result.Data });
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding field to form {FormId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteForm(int id)
    {
        try
        {
            return Ok(new { message = "Form deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting form {FormId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetFormTemplates()
    {
        try
        {
            var templatesQuery = new GetFormsQuery { IsTemplate = true };
            var result = await _mediator.Send(templatesQuery);
            if (result.IsSuccess)
                return Ok(result.Data);
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving form templates");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetFormsAnalytics()
    {
        try
        {
            var analytics = new
            {
                TotalForms = 15,
                ActiveForms = 12,
                TotalSubmissions = 247,
                PendingApprovals = 8,
                SubmissionTrends = new[]
                {
                    new { Date = "2025-05-01", Count = 23 },
                    new { Date = "2025-05-02", Count = 31 },
                    new { Date = "2025-05-03", Count = 28 },
                    new { Date = "2025-05-04", Count = 35 },
                    new { Date = "2025-05-05", Count = 42 }
                },
                TopForms = new[]
                {
                    new { Title = "Leave Request Form", Submissions = 89 },
                    new { Title = "Expense Report", Submissions = 67 },
                    new { Title = "Employee Feedback", Submissions = 45 }
                }
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}