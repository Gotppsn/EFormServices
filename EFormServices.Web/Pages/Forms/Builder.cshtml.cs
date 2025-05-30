// EFormServices.Web/Pages/Forms/Builder.cshtml.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFormServices.Web.Pages.Forms;

[Authorize]
public class BuilderModel : PageModel
{
    public int? FormId { get; set; }

    public IActionResult OnGet(int? id)
    {
        FormId = id;
        return Page();
    }
}