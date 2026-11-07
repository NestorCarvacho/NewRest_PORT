using Microsoft.AspNetCore.Authentication.Cookies;
using SGH.Infrastructure.DependencyInjection;
using SGH.Web.Cuenta;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfraestructura(builder.Configuration);
builder.Services.AddSingleton<EmisorCookieSesion>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Login";
        opciones.AccessDeniedPath = "/AccesoDenegado";
        opciones.SlidingExpiration = true;
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });
builder.Services.AddAuthorization();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(opciones =>
{
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddRazorPages(opciones =>
{
    opciones.Conventions.AuthorizeFolder("/");
    opciones.Conventions.AllowAnonymousToPage("/Cuenta/Login");
    opciones.Conventions.AllowAnonymousToPage("/Cuenta/CambiarClave");
    opciones.Conventions.AllowAnonymousToPage("/Cuenta/Salir");
    opciones.Conventions.AllowAnonymousToPage("/Error");
    opciones.Conventions.AllowAnonymousToPage("/NoEncontrado");
    opciones.Conventions.AllowAnonymousToPage("/AccesoDenegado");
    opciones.Conventions.AllowAnonymousToPage("/Privacy");
    opciones.Conventions.AllowAnonymousToPage("/Cookies");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/NoEncontrado");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
