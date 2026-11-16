using System.Diagnostics;

namespace SGH.Web.Infraestructura;

public sealed class CorrelacionSolicitudMiddleware(RequestDelegate siguiente)
{
    public const string ClaveItem = "SGH.CorrelacionId";
    public const string Cabecera = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext contexto)
    {
        var id = Activity.Current?.Id ?? contexto.TraceIdentifier;
        contexto.Items[ClaveItem] = id;
        contexto.Response.OnStarting(() =>
        {
            contexto.Response.Headers[Cabecera] = id;
            return Task.CompletedTask;
        });

        await siguiente(contexto);
    }
}

public static class CorrelacionSolicitudExtensiones
{
    public static string? CorrelacionId(this HttpContext contexto) =>
        contexto.Items[CorrelacionSolicitudMiddleware.ClaveItem] as string;
}