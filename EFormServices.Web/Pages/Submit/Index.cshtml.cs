// EFormServices.Web/Pages/Submit/Index.cshtml.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFormServices.Web.Pages.Submit;

public class IndexModel : PageModel
{
    public string FormKey { get; set; } = string.Empty;

    public IActionResult OnGet(string formKey)
    {
        if (string.IsNullOrEmpty(formKey))
            return NotFound();

        FormKey = formKey;
        return Page();
    }
}