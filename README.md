# Asesoría Legal Global — Sistema de Registro y Gestión de Documentos Legales

Proyecto académico desarrollado para el **Parcial 2 (TP4)** de la asignatura **Laboratorio de Programación 3**.

- **Institución:** Instituto Superior Santo Domingo (ISSD)
- **Carrera:** Tecnicatura Superior en Desarrollo de Software
- **Materia:** Laboratorio de Programación 3
- **Profesor:** Christian Mansilla
- **Alumna:** Ana Valentina Vargas

---

## Descripción del Proyecto

Sitio web corporativo desarrollado para la firma ficticia **Asesoría Legal Global**, orientado a la captación y registración de clientes y a la administración segura de documentos jurídicos en espacios personales por usuario.

El proyecto fue construido en **ASP.NET Core Razor Pages** aplicando los contenidos prácticos de la Unidad III de la materia:
- Controles de validación con `DataAnnotations` y validación en cliente/servidor.
- Subida y administración de archivos en el servidor con `IFormFile`, `IWebHostEnvironment` y `System.IO.File`.
- Gestión de estado mediante Cookies (`CookieOptions`), Variables de Sesión (`HttpContext.Session`) y Variables de Aplicación (`Singleton` en memoria).
- Estructura base reutilizable mediante *Master Page* (`_Layout.cshtml` y `_ViewStart.cshtml`).

---

## Requisitos del Parcial

### 1. Estructura de la Página Maestra 
- **Master Page (`Pages/Shared/_Layout.cshtml` y `Pages/_ViewStart.cshtml`):** Actúa como plantilla base común para todas las páginas del sistema (`Index`, `Registro`, `Login` y `Documentos`), utilizando `@RenderBody()` para el contenido principal y `@RenderSectionAsync()` para scripts y cabeceras opcionales.
- **Navbar dinámico:** Barra de navegación superior con el logo institucional y enlaces mediante Tag Helpers (`asp-page`). Adapta sus opciones según el estado de la sesión del cliente (mostrando `Inicio`, `Registro`, `Iniciar Sesión` y `Gestión Documental` cuando no hay sesión iniciada, o `Inicio`, `Mis Documentos`, el usuario activo y `Cerrar Sesión` cuando el cliente ya ingresó).
- **Footer persistente:** Pie de página institucional presente en todas las vistas del sitio con cálculo dinámico del año actual (`@DateTime.Now.Year`).

### 2. Formulario de Registro de Usuarios
- **Datos solicitados (`Pages/Registro.cshtml` y `Pages/Registro.cshtml.cs`):**
  - Nombre y Apellido.
  - Nombre de usuario (restringido a letras, números y guion bajo de 3 a 20 caracteres).
  - Edad (valor numérico obligatorio, aceptando únicamente mayores de 15 años mediante `[Range(16, 120)]`).
  - Correo electrónico.
  - Contraseña y Confirmación de contraseña.
- **Validaciones:**
  - Todos los campos son obligatorios (`[Required]`).
  - Validación de formato de correo electrónico (`[EmailAddress]` y `[RegularExpression]`).
  - Validación de coincidencia entre contraseña y confirmación (`[Compare(nameof(Password))]`).
  - Validación numérica configurada en español en `Program.cs` y validación en tiempo real en el cliente con `jQuery Unobtrusive Validation` (`_ValidationScriptsPartial.cshtml`).
- **Almacenamiento Temporal:**
  - Al presionar **"Registrar"**, el sistema almacena el **nombre de usuario en una variable de Sesión** (`HttpContext.Session.SetString`) y la **contraseña en una Cookie** (`Response.Cookies.Append` con `HttpOnly = true` e `IsEssential = true`), además de registrar al usuario en el servicio en memoria y redirigir al inicio con un mensaje de confirmación para que inicie sesión.

### 3. Sistema de Gestión de Documentos Legales 
- **Inicio de Sesión y Redirección (`Pages/Login.cshtml` y `Pages/Documentos.cshtml`):**
  - Al iniciar sesión con credenciales válidas, el cliente es dirigido automáticamente a su propio espacio de gestión documental (`/Documentos`).
  - Si un visitante intenta acceder a `/Documentos` sin haber iniciado sesión, el sistema impide el acceso y lo redirige a `/Login` informando que debe autenticarse.
- **Carga y Administración Segura de Documentos (`Pages/Documentos.cshtml.cs`):**
  - Cada cliente cuenta con su propia subcarpeta aislada dentro del servidor: `wwwroot/documentos_legales/<usuario>/`.
  - Validación de archivo seleccionado no vacío (`DocumentoSubido.Length > 0`).
  - Lista blanca de extensiones permitidas (`.pdf`, `.doc`, `.docx`, `.txt`).
  - Límite de tamaño máximo por archivo de **5 MB**.
  - Limpieza del nombre del archivo mediante `Path.GetFileName()` para evitar ataques de rutas relativas (*Path Traversal*).
  - Control de archivos duplicados con `System.IO.File.Exists()` para evitar sobrescribir documentos existentes.
  - Listado de los documentos propios del cliente con opción de **Ver / Descargar** y **Eliminar** (`System.IO.File.Delete()`).

### 4. Bonos Extra 
- **Diseño Web Responsivo y CSS Puro (`wwwroot/css/site.css`):** Hoja de estilos propia desarrollada en CSS3 puro (sin frameworks externos), utilizando variables CSS, *Flexbox*, *CSS Grid* y *Media Queries* (`@media`) para adaptarse a pantallas de escritorio, tablets y teléfonos móviles.
- **Uso de las Imágenes de la Empresa (`wwwroot/imagenes/`):** Integración de las tres imágenes institucionales suministradas en el parcial (`logo 1.png`, `logo 2.png` y `logo 3.png`) distribuidas en la barra de navegación, la portada principal, los formularios de acceso y el panel documental.

---

## Decisiones Técnicas (Según las Guías de la Materia)

1. **Diferencia entre Cookie y Variable de Sesión :**
   - La **Cookie** viaja y se almacena en el navegador del cliente (`Response.Cookies.Append` / `Request.Cookies`), configurada como `HttpOnly` para resguardar la contraseña frente a accesos desde JavaScript.
   - La **Variable de Sesión** (`HttpContext.Session`) almacena los datos del usuario en la memoria del servidor web, identificando al navegador mediante la cookie de sesión de ASP.NET Core (`.AspNetCore.Session`) con un tiempo de inactividad (`IdleTimeout`) de 20 minutos.
2. **Variables de Aplicación en Memoria vs. Archivos en Disco:**
   - Dado que la consigna solicita **almacenamiento temporal** (sin base de datos), el padrón de clientes registrados se mantiene en memoria RAM mediante el servicio singleton `GestorClientes` (`builder.Services.AddSingleton<GestorClientes>()`). Al detener y reiniciar la aplicación web, los datos en memoria RAM se reinician.
   - Por el contrario, los documentos subidos mediante `IFormFile` y `FileStream` se graban físicamente en el disco dentro de `wwwroot/documentos_legales/<usuario>/`, por lo que persisten en el sistema de archivos local.

---

## Estructura del Proyecto

```text
SistemaAsesoria2/
├── Program.cs                        Configuración de servicios, sesión, singleton y pipeline HTTP
├── SistemaAsesoria2.csproj           Archivo de proyecto .NET
├── Servicios/
│   └── GestorClientes.cs             Servicio Singleton en memoria para clientes registrados
├── Pages/
│   ├── Shared/
│   │   ├── _Layout.cshtml            Master Page del sitio (Navbar + RenderBody + Footer)
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewStart.cshtml             Asigna _Layout como plantilla global
│   ├── _ViewImports.cshtml           Tag Helpers de Razor Pages
│   ├── Index.cshtml(.cs)             Portada institucional y mensajes de confirmación
│   ├── Registro.cshtml(.cs)          Formulario de registro, validaciones, Sesión y Cookie
│   ├── Login.cshtml(.cs)             Inicio de sesión de clientes
│   ├── Documentos.cshtml(.cs)        Espacio personal de carga, consulta y baja de archivos
│   ├── Salir.cshtml(.cs)             Cierre de sesión y limpieza de cookie
│   └── Error.cshtml(.cs)             Página de manejo de errores
└── wwwroot/
    ├── css/
    │   └── site.css                  Estilos corporativos en CSS puro y diseño responsivo
    ├── imagenes/                     Logos y banners institucionales suministrados
    └── documentos_legales/           Repositorio físico de documentos por usuario
```

---

## Ejecución del Proyecto

1. Clonar el repositorio y ubicarse en la carpeta del proyecto:
   ```bash
   git clone <url-del-repositorio>
   cd SistemaAsesoria2
   ```
2. Ejecutar la aplicación:
   ```bash
   dotnet run
   ```
3. Abrir en el navegador la URL indicada en consola (por defecto `http://localhost:5226`).
