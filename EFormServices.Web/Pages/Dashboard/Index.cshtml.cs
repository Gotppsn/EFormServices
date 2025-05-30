// EFormServices.Web/Pages/Dashboard/Index.cshtml.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFormServices.Web.Pages.Dashboard;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}