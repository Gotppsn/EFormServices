// EFormServices.Web/Pages/Approvals/Pending.cshtml.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFormServices.Web.Pages.Approvals;

[Authorize(Policy = "ApprovalManagement")]
public class PendingModel : PageModel
{
    public void OnGet()
    {
    }
}