// EFormServices.Web/Pages/Admin/Users.cshtml.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFormServices.Web.Pages.Admin;

[Authorize(Policy = "ManageUsers")]
public class UsersModel : PageModel
{
    public void OnGet()
    {
    }
}