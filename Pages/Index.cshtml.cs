using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaAsesoria2.Pages;

public class IndexModel : PageModel
{
    [TempData]
    public string? MensajeRegistro { get; set; }

    public void OnGet()
    {
    }
}
