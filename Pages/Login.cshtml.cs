using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaAsesoria2.Servicios;

namespace SistemaAsesoria2.Pages;

public class LoginModel : PageModel
{
    private readonly GestorClientes _gestorClientes;

    public LoginModel(GestorClientes gestorClientes)
    {
        _gestorClientes = gestorClientes;
    }

    [BindProperty]
    [Required(ErrorMessage = "Debe ingresar su nombre de usuario.")]
    public string Usuario { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Debe ingresar su contraseña.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [TempData]
    public string? Aviso { get; set; }

    public IActionResult OnGet()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioLogueado")))
        {
            return RedirectToPage("/Documentos");
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string usuarioIngresado = Usuario.Trim();

        // 1. Verificar contra la Sesión y la Cookie creadas en el Registro
        string? usuarioEnSesion = HttpContext.Session.GetString("UsuarioRegistrado");
        string? claveEnCookie = Request.Cookies["PasswordCookie"];

        bool coincideSesionYCookie =
            !string.IsNullOrEmpty(usuarioEnSesion) &&
            !string.IsNullOrEmpty(claveEnCookie) &&
            string.Equals(usuarioEnSesion, usuarioIngresado, StringComparison.OrdinalIgnoreCase) &&
            claveEnCookie == Password;

        // 2. O verificar contra el servicio de clientes registrados en memoria
        var cliente = _gestorClientes.ValidarCredenciales(usuarioIngresado, Password);

        if (!coincideSesionYCookie && cliente is null)
        {
            ModelState.AddModelError(string.Empty, "El usuario o la contraseña ingresados son incorrectos.");
            return Page();
        }

        string nombreUsuarioFinal = cliente?.Usuario ?? usuarioEnSesion!;
        string nombreCompletoFinal = cliente?.NombreApellido ?? HttpContext.Session.GetString("NombreCompleto") ?? nombreUsuarioFinal;

        // Activar la sesión del usuario logueado y redirigir a su espacio de gestión de archivos
        HttpContext.Session.SetString("UsuarioLogueado", nombreUsuarioFinal);
        HttpContext.Session.SetString("NombreCompleto", nombreCompletoFinal);

        return RedirectToPage("/Documentos");
    }
}
