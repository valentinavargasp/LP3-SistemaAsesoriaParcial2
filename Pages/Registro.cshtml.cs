using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaAsesoria2.Servicios;

namespace SistemaAsesoria2.Pages;

public class RegistroModel : PageModel
{
    private readonly GestorClientes _gestorClientes;

    public RegistroModel(GestorClientes gestorClientes)
    {
        _gestorClientes = gestorClientes;
    }

    [BindProperty]
    [Required(ErrorMessage = "El nombre y apellido son obligatorios.")]
    public string NombreApellido { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [RegularExpression(@"^[A-Za-z0-9_.-]{3,25}$",
        ErrorMessage = "El usuario debe tener entre 3 y 25 caracteres (letras, números, punto o guion).")]
    public string Usuario { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "La edad es obligatoria.")]
    [Range(16, 120, ErrorMessage = "Solo se aceptan usuarios mayores de 15 años.")]
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

        var nuevoCliente = new ClienteRegistrado(
            Usuario.Trim(),
            NombreApellido.Trim(),
            Email.Trim(),
            Edad!.Value,
            Password
        );

        bool registrado = _gestorClientes.Registrar(nuevoCliente);
        if (!registrado)
        {
            ModelState.AddModelError(nameof(Usuario), "Ese nombre de usuario ya se encuentra registrado.");
            return Page();
        }

        // 1. Al hacer clic en "Registrar", almacenar el nombre de usuario en una Sesión (servidor)
        HttpContext.Session.SetString("UsuarioRegistrado", nuevoCliente.Usuario);
        HttpContext.Session.SetString("NombreCompleto", nuevoCliente.NombreApellido);

        // 2. Al hacer clic en "Registrar", almacenar la contraseña en una Cookie (cliente)
        CookieOptions opciones = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddDays(7),
            HttpOnly = true,
            IsEssential = true
        };
        Response.Cookies.Append("PasswordCookie", Password, opciones);

        // Si había una sesión iniciada previa de otro usuario, cerrarla para que inicie sesión el nuevo
        HttpContext.Session.Remove("UsuarioLogueado");

        // 3. Mostrar confirmación en el Inicio para que el usuario inicie sesión
        TempData["MensajeRegistro"] = $"¡Usuario '{nuevoCliente.Usuario}' registrado correctamente! Ahora puede iniciar sesión para acceder a su espacio documental.";
        return RedirectToPage("/Index");
    }
}
