using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.RateLimiting;
using SGH.Infrastructure.DependencyInjection;
using SGH.Web.Cuenta;
using SGH.Web.Autorizacion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfraestructura(builder.Configuration);
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
builder.Services.AddMemoryCache();
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
        if (!HttpMethods.IsPost(contexto.Request.Method))
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
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy(PoliticasAplicacion.AdministrarUsuarios, politica =>
        politica.RequireAuthenticatedUser().AddRequirements(new PerfilAutorizacionRequirement(false)));
    opciones.AddPolicy(PoliticasAplicacion.GestionarReservas, politica =>
        politica.RequireAuthenticatedUser().AddRequirements(new PerfilAutorizacionRequirement(true)));
    opciones.AddPolicy(PoliticasAplicacion.EjecutarCheckInOut, politica =>
        politica.RequireAuthenticatedUser().AddRequirements(new PerfilAutorizacionRequirement(false)));
    opciones.AddPolicy(PoliticasAplicacion.GestionarCensos, politica =>
        politica.RequireAuthenticatedUser().AddRequirements(new PerfilAutorizacionRequirement(false)));
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(opciones =>
{
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddRazorPages(opciones =>
{
    opciones.Conventions.AuthorizeFolder("/");
    opciones.Conventions.AuthorizePage("/Reserva/Datos", PoliticasAplicacion.GestionarReservas);
    opciones.Conventions.AuthorizePage("/Solicitud/Datos", PoliticasAplicacion.AdministrarUsuarios);
    opciones.Conventions.AuthorizePage("/Censo/Datos", PoliticasAplicacion.GestionarCensos);
    opciones.Conventions.AuthorizePage("/Censo/EstadoPago", PoliticasAplicacion.GestionarCensos);
    opciones.Conventions.AuthorizePage("/Cuenta/Usuarios", PoliticasAplicacion.AdministrarUsuarios);
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
    var ingreso = ruta.StartsWithSegments("/Cuenta/Login") || ruta.StartsWithSegments("/Cuenta/CambiarClave");
    if (ingreso && HttpMethods.IsPost(contexto.Request.Method))
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
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
