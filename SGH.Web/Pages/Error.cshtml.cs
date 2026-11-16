using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Web.Infraestructura;

namespace SGH.Web.Pages;

[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel(IWebHostEnvironment ambiente) : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public bool EsDesarrollo => ambiente.IsDevelopment();

    public void OnGet()
    {
        RequestId = HttpContext.CorrelacionId()
            ?? Activity.Current?.Id
            ?? HttpContext.TraceIdentifier;
    }
}

