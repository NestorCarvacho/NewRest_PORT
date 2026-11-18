using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SGH.Application.Mid;
using SGH.Application.Sesion;

namespace SGH.Web.Pages.Modulos;

/// <summary>
/// Base opcional para páginas de módulo: POST con formulario → mismo Mid que el escritorio (JSON).
/// </summary>
[ValidateAntiForgeryToken]
public abstract class ModuloDatosPageModel(IEjecutorPostModulo ejecutorPost) : PageModel
{
    protected IEjecutorPostModulo EjecutorPost => ejecutorPost;

    [BindProperty(Name = "opcion")]
    public string? OpcionFormulario { get; set; }

    public string? AvisoExito => TempData[EjecutorPostModulo.ClaveAvisoExito] as string;
    public string? AvisoError => TempData[EjecutorPostModulo.ClaveAvisoError] as string;

    public virtual Task<IActionResult> OnPostAsync(CancellationToken cancellationToken) =>
        EjecutorPost.EjecutarAsync(
            this,
            ConversorFormularioMid.DesdeFormulario(Request.Form),
            OpcionesMid(),
            EjecutarMidAsync,
            NombreModulo(),
            cancellationToken);

    protected abstract IReadOnlyList<AccionApi> OpcionesMid();
    protected abstract string NombreModulo();
    protected abstract Task<EjecucionMid> EjecutarMidAsync(
        FiltroPeticion filtro,
        DatosSesion sesion,
        CancellationToken cancellationToken);
}