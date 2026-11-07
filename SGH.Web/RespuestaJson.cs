using Microsoft.AspNetCore.Mvc;

namespace SGH.Web;

public static class RespuestaJson
{
    public static JsonResult Mensaje(string mensaje, int estado = StatusCodes.Status400BadRequest) =>
        new(new { mensaje }) { StatusCode = estado };

    public static JsonResult DeFallo(Exception excepcion, bool desarrollo) =>
        Mensaje(
            desarrollo ? excepcion.Message : "No se pudo completar la operación. Intente nuevamente.",
            StatusCodes.Status500InternalServerError);
}
