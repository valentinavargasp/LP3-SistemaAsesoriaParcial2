using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using SistemaAsesoria2.Servicios;

namespace SistemaAsesoria2.Pages;

[RequestSizeLimit(100 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
public class DocumentosModel : PageModel
{
    private readonly IWebHostEnvironment _env;
    private readonly GestorClientes _gestorClientes;

    // Límite de 5 MB para la subida de documentos
    private const long LimiteBytes = 5 * 1024 * 1024;

    // Solo documentos legales (se rechazan .zip, imágenes .png/.jpg, ejecutables, etc.)
    private static readonly string[] ExtensionesPermitidas =
        { ".pdf", ".docx", ".doc", ".txt" };

    public DocumentosModel(IWebHostEnvironment env, GestorClientes gestorClientes)
    {
        _env = env;
        _gestorClientes = gestorClientes;
    }

    public string? UsuarioActual { get; set; }
    public string? NombreCompletoActual { get; set; }
    public string? EmailActual { get; set; }
    public string CarpetaUsuario { get; set; } = string.Empty;

    [TempData]
    public string? MensajeExito { get; set; }

    [TempData]
    public string? MensajeError { get; set; }

    [BindProperty]
    public IFormFile? DocumentoSubido { get; set; }

    public List<string> ArchivosGuardados { get; set; } = new();

    public IActionResult OnGet()
    {
        // Validar que exista la sesión activa del usuario
        UsuarioActual = HttpContext.Session.GetString("UsuarioLogueado");
        if (string.IsNullOrEmpty(UsuarioActual))
        {
            TempData["Aviso"] = "Debe iniciar sesión para acceder a su espacio de gestión documental.";
            return RedirectToPage("/Login");
        }

        CargarDatosPerfilYArchivos();
        return Page();
    }

    public IActionResult OnGetVer(string nombreArchivo)
    {
        UsuarioActual = HttpContext.Session.GetString("UsuarioLogueado");
        if (string.IsNullOrEmpty(UsuarioActual))
        {
            return RedirectToPage("/Login");
        }

        if (string.IsNullOrEmpty(nombreArchivo))
        {
            return RedirectToPage();
        }

        string carpetaDestino = ObtenerRutaEspacioUsuario();
        string nombreLimpio = Path.GetFileName(nombreArchivo);
        string rutaArchivo = Path.Combine(carpetaDestino, nombreLimpio);

        if (!System.IO.File.Exists(rutaArchivo))
        {
            MensajeError = $"No se encontró el documento '{nombreLimpio}'.";
            return RedirectToPage();
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(nombreLimpio, out string? contentType))
        {
            contentType = "application/octet-stream";
        }

        string extension = Path.GetExtension(nombreLimpio).ToLowerInvariant();
        if (extension == ".pdf" || extension == ".txt")
        {
            // Se abre directamente en la pestaña del navegador para visualizarlo
            return PhysicalFile(rutaArchivo, contentType);
        }

        // Para documentos Word (.doc / .docx), se descarga con su nombre original
        return PhysicalFile(rutaArchivo, contentType, nombreLimpio);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        UsuarioActual = HttpContext.Session.GetString("UsuarioLogueado");
        if (string.IsNullOrEmpty(UsuarioActual))
        {
            TempData["Aviso"] = "Su sesión ha expirado. Por favor, inicie sesión nuevamente.";
            return RedirectToPage("/Login");
        }

        if (DocumentoSubido is null || DocumentoSubido.Length <= 0)
        {
            ModelState.AddModelError(nameof(DocumentoSubido), "Debe seleccionar un documento legal para subir.");
        }
        else
        {
            string extension = Path.GetExtension(DocumentoSubido.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(DocumentoSubido),
                    $"El formato '{extension}' no está permitido. Solo se aceptan documentos PDF (.pdf), Word (.doc, .docx) o texto (.txt).");
            }

            if (DocumentoSubido.Length > LimiteBytes)
            {
                ModelState.AddModelError(
                    nameof(DocumentoSubido),
                    "El archivo supera el tamaño máximo permitido de 5 MB.");
            }
        }

        if (!ModelState.IsValid)
        {
            CargarDatosPerfilYArchivos();
            return Page();
        }

        // Ruta física del espacio personal del usuario dentro de wwwroot/documentos_legales
        string carpetaDestino = ObtenerRutaEspacioUsuario();
        Directory.CreateDirectory(carpetaDestino);

        string nombreLimpio = Path.GetFileName(DocumentoSubido!.FileName);
        string rutaCompleta = Path.Combine(carpetaDestino, nombreLimpio);

        // Control de duplicados con System.IO.File.Exists
        if (System.IO.File.Exists(rutaCompleta))
        {
            ModelState.AddModelError(
                nameof(DocumentoSubido),
                $"Ya existe un documento llamado '{nombreLimpio}' en su espacio personal.");
            CargarDatosPerfilYArchivos();
            return Page();
        }

        // Guardar físicamente el archivo en el servidor
        using (FileStream stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await DocumentoSubido.CopyToAsync(stream);
        }

        MensajeExito = $"El documento '{nombreLimpio}' fue cargado correctamente en su espacio personal.";
        return RedirectToPage();
    }

    public IActionResult OnPostEliminar(string nombreArchivo)
    {
        UsuarioActual = HttpContext.Session.GetString("UsuarioLogueado");
        if (string.IsNullOrEmpty(UsuarioActual))
        {
            return RedirectToPage("/Login");
        }

        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            MensajeError = "No se especificó el documento a eliminar.";
            return RedirectToPage();
        }

        string carpetaDestino = ObtenerRutaEspacioUsuario();
        string nombreLimpio = Path.GetFileName(nombreArchivo);
        string rutaArchivo = Path.Combine(carpetaDestino, nombreLimpio);

        if (System.IO.File.Exists(rutaArchivo))
        {
            System.IO.File.Delete(rutaArchivo);
            MensajeExito = $"El documento '{nombreLimpio}' fue eliminado correctamente.";
        }
        else
        {
            MensajeError = $"No se encontró el archivo '{nombreLimpio}' para eliminar.";
        }

        return RedirectToPage();
    }

    private void CargarDatosPerfilYArchivos()
    {
        NombreCompletoActual = HttpContext.Session.GetString("NombreCompleto");
        var cliente = _gestorClientes.ObtenerPorUsuario(UsuarioActual!);
        if (cliente is not null)
        {
            NombreCompletoActual ??= cliente.NombreApellido;
            EmailActual = cliente.Email;
        }

        ArchivosGuardados.Clear();
        string carpetaDestino = ObtenerRutaEspacioUsuario();
        if (Directory.Exists(carpetaDestino))
        {
            ArchivosGuardados = Directory.GetFiles(carpetaDestino)
                .Select(Path.GetFileName)
                .Where(nombre => !string.IsNullOrEmpty(nombre) && !nombre.StartsWith("."))
                .OrderBy(nombre => nombre)
                .ToList()!;
        }
    }

    private string ObtenerRutaEspacioUsuario()
    {
        char[] invalidos = Path.GetInvalidFileNameChars();
        string usuarioSanitizado = new string((UsuarioActual ?? "general")
            .Where(c => !invalidos.Contains(c))
            .ToArray())
            .Trim();

        if (string.IsNullOrEmpty(usuarioSanitizado))
        {
            usuarioSanitizado = "usuario";
        }

        CarpetaUsuario = usuarioSanitizado;
        return Path.Combine(_env.WebRootPath, "documentos_legales", CarpetaUsuario);
    }
}
