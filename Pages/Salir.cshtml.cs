using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaAsesoria2.Pages;

public class SalirModel : PageModel
{
    public IActionResult OnGet()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("PasswordCookie");

        return RedirectToPage("/Index");
    }
}
