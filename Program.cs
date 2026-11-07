using SistemaAsesoria2.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Servicios de Razor Pages y mensajes de validación numérica en español
builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        campo => $"El campo {campo} debe ser un valor numérico.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
});

// Servicio singleton en memoria para almacenar los clientes registrados
builder.Services.AddSingleton<GestorClientes>();

// Configuración de Sesión en memoria
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de sesión (después de UseRouting y antes de MapRazorPages)
app.UseSession();

app.UseAuthorization();
app.MapRazorPages();

app.Run();
