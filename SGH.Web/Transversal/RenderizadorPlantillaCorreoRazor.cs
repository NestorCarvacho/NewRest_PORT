using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using SGH.Application.Transversal;

namespace SGH.Web.Transversal;

public sealed class RenderizadorPlantillaCorreoRazor(
    IRazorViewEngine motorVistas,
    ITempDataProvider proveedorTempData,
    IServiceProvider proveedorServicios,
    IHttpContextAccessor contextoHttp) : IRenderizadorPlantillaCorreo
{
    public async Task<string> RenderizarHtmlAsync(string rutaPlantilla, object modelo, CancellationToken cancellationToken)
    {
        var contexto = contextoHttp.HttpContext ?? new DefaultHttpContext { RequestServices = proveedorServicios };
        var actionContext = new ActionContext(contexto, new RouteData(), new ActionDescriptor());
        var viewResult = motorVistas.GetView(executingFilePath: null, viewPath: rutaPlantilla, isMainPage: true);
        if (!viewResult.Success)
            throw new InvalidOperationException($"No se encontró la plantilla de correo '{rutaPlantilla}'.");

        await using var escritor = new StringWriter();
        var vista = viewResult.View;
        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = modelo
        };
        var vistaContexto = new ViewContext(
            actionContext,
            vista,
            viewDictionary,
            new TempDataDictionary(contexto, proveedorTempData),
            escritor,
            new HtmlHelperOptions());

        await vista.RenderAsync(vistaContexto);
        return escritor.ToString();
    }
}