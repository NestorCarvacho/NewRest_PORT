using Microsoft.AspNetCore.Mvc;
using SGH.Web.Infraestructura;

namespace SGH.Web.Mid;

public sealed class EjecutorPeticionMid(
    IWebHostEnvironment ambiente,
    IHttpContextAccessor contextoHttp,
    ILogger<EjecutorPeticionMid> logger)
{
    public Task<IActionResult> EjecutarAsync(
        Func<Task<IActionResult>> accion,
        string modulo,
        string? opcion = null,
        CancellationToken cancellationToken = default) =>
        EjecutarAsync(accion, modulo, opcion, null, cancellationToken);

    public async Task<IActionResult> EjecutarAsync(
        Func<Task<IActionResult>> accion,
        string modulo,
        string? opcion,
        int? usuarioId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await accion();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var correlacion = contextoHttp.HttpContext?.CorrelacionId();
            logger.LogError(
                ex,
                "Fallo Mid. Modulo {Modulo}, Opcion {Opcion}, UsuarioId {UsuarioId}, Correlacion {CorrelacionId}",
                modulo,
                opcion ?? "(sin opción)",
                usuarioId,
                correlacion);

            return RespuestaDeMid.DeExcepcion(ex, ambiente.IsDevelopment());
        }
    }
}