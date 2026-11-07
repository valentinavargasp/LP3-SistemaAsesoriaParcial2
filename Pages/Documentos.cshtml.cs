using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DocumentosModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public DocumentosModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public string? UsuarioActual { get; set; }
    public string? ClaveRecuperadaCookie { get; set; }
    public string? MensajeExito { get; set; }
    public string? MensajeError { get; set; }

    [BindProperty]
    public IFormFile? DocumentoSubido { get; set; } 

    public List<string> ArchivosGuardados { get; set; } = new();

    public IActionResult OnGet()
    {
        // 1. Validar que exista la sesión activa del usuario
        UsuarioActual = HttpContext.Session.GetString("UsuarioLogueado");
        if (string.IsNullOrEmpty(UsuarioActual))
        {
            return RedirectToPage("/Registro");
        }

        // 2. Leer la contraseña desde la Cookie
        ClaveRecuperadaCookie = Request.Cookies["PasswordCookie"];

        CargarArchivos();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        UsuarioActual = HttpContext.Session.GetString("UsuarioLogueado");
        if (string.IsNullOrEmpty(UsuarioActual))
        {
            return RedirectToPage("/Registro");
        }
        ClaveRecuperadaCookie = Request.Cookies["PasswordCookie"];

        if (DocumentoSubido is null || DocumentoSubido.Length <= 0)
        {
            MensajeError = "Debe seleccionar un documento legal para cargar.";
            CargarArchivos();
            return Page();
        }

        // Validar extensiones de documentos legales
        string extension = Path.GetExtension(DocumentoSubido.FileName).ToLower();
        string[] extensionesPermitidas = { ".pdf", ".docx", ".doc", ".txt" };
        if (!extensionesPermitidas.Contains(extension))
        {
            MensajeError = "Formato no permitido. Solo se aceptan documentos PDF, Word o TXT.";
            CargarArchivos();
            return Page();
        }

        // Ruta física de destino dentro de wwwroot
        string carpetaDestino = Path.Combine(_env.WebRootPath, "documentos_legales");
        if (!Directory.Exists(carpetaDestino))
        {
            Directory.CreateDirectory(carpetaDestino);
        }

        string nombreLimpio = Path.GetFileName(DocumentoSubido.FileName);
        string rutaCompleta = Path.Combine(carpetaDestino, nombreLimpio);

        // Control de duplicados 
        if (System.IO.File.Exists(rutaCompleta))
        {
            MensajeError = $"El archivo '{nombreLimpio}' ya existe en el servidor.";
            CargarArchivos();
            return Page();
        }

        // Guardar físicamente el archivo en el servidor
        using (FileStream stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await DocumentoSubido.CopyToAsync(stream);
        }

        MensajeExito = $"Documento '{nombreLimpio}' subido con éxito al repositorio legal.";
        CargarArchivos();
        return Page();
    }

    public IActionResult OnPostCerrarSesion()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("PasswordCookie");
        return RedirectToPage("/Index");
    }

    private void CargarArchivos()
    {
        ArchivosGuardados.Clear();
        string carpetaDestino = Path.Combine(_env.WebRootPath, "documentos_legales");
        if (Directory.Exists(carpetaDestino))
        {
            string[] rutas = Directory.GetFiles(carpetaDestino);
            foreach (var r in rutas)
            {
                ArchivosGuardados.Add(Path.GetFileName(r));
            }
        }
    }
}