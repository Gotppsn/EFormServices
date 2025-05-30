// EFormServices.Web/Pages/Forms/Index.cshtml.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFormServices.Web.Pages.Forms;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}