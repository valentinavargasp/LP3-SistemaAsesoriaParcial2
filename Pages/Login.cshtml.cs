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

        var cliente = _gestorClientes.ValidarCredenciales(Usuario.Trim(), Password);
        if (cliente is null)
        {
            ModelState.AddModelError(string.Empty, "El usuario o la contraseña ingresados son incorrectos.");
            return Page();
        }

        // Guardar el usuario en la variable de sesión y dirigir a su espacio de documentos
        HttpContext.Session.SetString("UsuarioLogueado", cliente.Usuario);
        HttpContext.Session.SetString("NombreCompleto", cliente.NombreApellido);

        return RedirectToPage("/Documentos");
    }
}
