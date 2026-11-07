using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegistroModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "El nombre y apellido son obligatorios.")]
    public string NombreApellido { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    public string Usuario { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "La edad es obligatoria.")]
    [Range(16, 120, ErrorMessage = "Debe ser mayor de 15 años.")] 
    public int? Edad { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Debe confirmar la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")] 
    public string ConfirmPassword { get; set; } = string.Empty;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // 1. Guardar el nombre de usuario en la Sesión (servidor)
        HttpContext.Session.SetString("UsuarioLogueado", Usuario);
        HttpContext.Session.SetString("NombreCompleto", NombreApellido);

        // 2. Guardar la contraseña en una Cookie (cliente)
        CookieOptions opciones = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddDays(7),
            HttpOnly = true,
            IsEssential = true
        };
        Response.Cookies.Append("PasswordCookie", Password, opciones);

        // Redirigir directamente al espacio documental
        return RedirectToPage("/Documentos");
    }
}