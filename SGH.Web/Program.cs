using System.Globalization;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.RateLimiting;
using SGH.Infrastructure.DependencyInjection;
using SGH.Web.Cuenta;
using SGH.Web.Autorizacion;
using SGH.Application.Navegacion;
using SGH.Web.Infraestructura;
using SGH.Web.Mid;
using SGH.Web.Configuracion;
using SGH.Web.Transversal;

var builder = WebApplication.CreateBuilder(args);
builder.AgregarAlmacenSecretosPorAmbiente();

builder.Services.AddInfraestructura(builder.Configuration);
builder.Services.AddServiciosTransversalesWeb();
builder.Services.AddLocalization();
var culturasSoportadas = new[]
{
    new CultureInfo("es-CL"),
    new CultureInfo("en-US"),
    new CultureInfo("pt-BR"),
    new CultureInfo("fr-FR")
};
builder.Services.Configure<RequestLocalizationOptions>(opciones =>
{
    opciones.DefaultRequestCulture = new RequestCulture(culturasSoportadas[0]);
    opciones.SupportedCultures = culturasSoportadas;
    opciones.SupportedUICultures = culturasSoportadas;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EjecutorPeticionMid>();
builder.Services.AddScoped<SGH.Web.Pages.Modulos.IEjecutorPostModulo, SGH.Web.Pages.Modulos.EjecutorPostModulo>();
builder.Services.AddMemoryCache();
builder.Services.AddAntiforgery(opciones =>
{
    opciones.HeaderName = "RequestVerificationToken";
});
builder.Services.ConfigureHttpJsonOptions(opciones =>
{
    opciones.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opciones.SerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddSingleton<EmisorCookieSesion>();
builder.Services.AddSingleton<ClavePendienteAlmacen>();
builder.Services.AddSingleton<RegistroVentanaIngreso>();
builder.Services.AddScoped<FiltroPilaAvisos>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Login";
        opciones.AccessDeniedPath = "/AccesoDenegado";
        opciones.SlidingExpiration = true;
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        opciones.Events.OnRedirectToLogin = contexto => ResponderSesion(contexto, "Debe iniciar sesión.");
        opciones.Events.OnRedirectToAccessDenied = contexto => ResponderSesion(contexto, "No tiene permiso para esta operación.");
    });
builder.Services.AddRateLimiter(opciones =>
{
    opciones.OnRejected = (contexto, _) =>
    {
        var http = contexto.HttpContext;
        var espera = RegistroVentanaIngreso.Duracion;
        if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan reintento))
            espera = reintento;
        LimiteIngreso.MarcarEspera(http.Response, espera);

        var avisos = http.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(http);
        avisos[LimiteIngreso.ClaveAviso] = LimiteIngreso.Mensaje;
        avisos.Save();

        var destino = http.Request.Path.StartsWithSegments("/Cuenta/CambiarClave")
            ? "/Cuenta/CambiarClave"
            : "/Cuenta/Login";
        http.Response.Redirect(destino);
        return ValueTask.CompletedTask;
    };
    opciones.AddPolicy("ingreso", contexto =>
    {
        if (!PoliticaLimiteIngreso.CuentaHaciaLimitePorIp(contexto))
            return RateLimitPartition.GetNoLimiter("lectura");

        var direccion = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
        return RateLimitPartition.GetFixedWindowLimiter(direccion, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = RegistroVentanaIngreso.Tope,
            Window = RegistroVentanaIngreso.Duracion,
            QueueLimit = 0
        });
    });
});
builder.Services.AddSingleton<IAuthorizationHandler, PerfilAutorizacionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MenuAccesoAuthorizationHandler>();
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy(PoliticasAplicacion.AdministrarUsuarios, politica =>
        politica.RequireAuthenticatedUser()
            .AddRequirements(new PerfilAutorizacionRequirement(false), new MenuAccesoRequirement(MenuClaves.Usuarios)));
    opciones.AddPolicy(PoliticasAplicacion.GestionarReservas, politica =>
        politica.RequireAuthenticatedUser()
            .AddRequirements(
                new PerfilAutorizacionRequirement(true),
                new MenuAccesoRequirement(MenuClaves.Solicitud, MenuClaves.Reserva)));
    opciones.AddPolicy(PoliticasAplicacion.EjecutarCheckInOut, politica =>
        politica.RequireAuthenticatedUser()
            .AddRequirements(
                new PerfilAutorizacionRequirement(false),
                new MenuAccesoRequirement(MenuClaves.Reserva)));
    opciones.AddPolicy(PoliticasAplicacion.GestionarCensos, politica =>
        politica.RequireAuthenticatedUser()
            .AddRequirements(
                new PerfilAutorizacionRequirement(false),
                new MenuAccesoRequirement(MenuClaves.Censo, MenuClaves.CensoQr)));
    opciones.AddPolicy(PoliticasAplicacion.GestionarHabitaciones, politica =>
        politica.RequireAuthenticatedUser()
            .AddRequirements(
                new PerfilAutorizacionRequirement(false),
                new MenuAccesoRequirement(MenuClaves.Habitaciones, MenuClaves.Banos, MenuClaves.Censo)));
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(opciones =>
{
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddControllers()
    .AddJsonOptions(opciones =>
    {
        opciones.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opciones.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddRazorPages(opciones =>
{
    opciones.Conventions.AuthorizeFolder("/");
    opciones.Conventions.AuthorizePage("/Cuenta/Usuarios", PoliticasAplicacion.AdministrarUsuarios);
    opciones.Conventions.AuthorizePage("/Habitacion/Datos", PoliticasAplicacion.GestionarHabitaciones);
    opciones.Conventions.AuthorizePage("/Habitacion/Banos", PoliticasAplicacion.GestionarHabitaciones);
    opciones.Conventions.AllowAnonymousToPage("/Cuenta/Login");
    opciones.Conventions.AllowAnonymousToPage("/Cuenta/CambiarClave");
    opciones.Conventions.AllowAnonymousToPage("/Cuenta/Salir");
    opciones.Conventions.AllowAnonymousToPage("/Error");
    opciones.Conventions.AllowAnonymousToPage("/NoEncontrado");
    opciones.Conventions.AllowAnonymousToPage("/AccesoDenegado");
    opciones.Conventions.AllowAnonymousToPage("/Privacy");
    opciones.Conventions.AllowAnonymousToPage("/Cookies");
}).AddMvcOptions(opciones => opciones.Filters.AddService<FiltroPilaAvisos>());

var app = builder.Build();
app.ValidarSecretosObligatorios();

app.UseMiddleware<CorrelacionSolicitudMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/NoEncontrado");
app.UseRouting();
app.UseRateLimiter();
app.Use(async (contexto, siguiente) =>
{
    var ruta = contexto.Request.Path;
    if (PoliticaLimiteIngreso.CuentaHaciaLimitePorIp(contexto))
    {
        var direccion = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
        var registro = contexto.RequestServices.GetRequiredService<RegistroVentanaIngreso>();
        contexto.Items[RegistroVentanaIngreso.ClaveRestantes] = registro.Registrar(direccion);
    }

    await siguiente();
});
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static Task ResponderSesion(Microsoft.AspNetCore.Authentication.RedirectContext<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions> contexto, string mensaje)
{
    if (contexto.Request.Path.Value?.Contains("/Mid/", StringComparison.OrdinalIgnoreCase) == true)
    {
        contexto.Response.StatusCode = StatusCodes.Status200OK;
        contexto.Response.ContentType = "application/json";
        return contexto.Response.WriteAsync($"{{\"registros\":{{\"resultado\":false,\"mensaje\":\"{mensaje}\"}}}}");
    }

    contexto.Response.Redirect(contexto.RedirectUri);
    return Task.CompletedTask;
}

public partial class Program;
