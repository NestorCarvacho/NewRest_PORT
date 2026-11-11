using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGH.Application.Mid;

namespace SGH.Web.Mid;

public static class RespuestaDeMid
{
    public static IActionResult De(EjecucionMid ejecucion)
    {
        if (ejecucion.EsArchivo)
            return new FileContentResult(ejecucion.Archivo!, ejecucion.TipoArchivo!)
            {
                FileDownloadName = ejecucion.NombreArchivo
            };

        if (ejecucion.Status != EjecucionMid.OkStatus)
            return new JsonResult(new { registros = new { resultado = false, mensaje = ejecucion.Mensaje } });

        return new JsonResult(new { registros = ejecucion.Registros });
    }

    public static IActionResult DeExcepcion(Exception excepcion, bool desarrollo)
    {
        if (excepcion is ArgumentException or InvalidOperationException or InvalidDataException)
            return new JsonResult(new { registros = new { resultado = false, mensaje = excepcion.Message } });

        if (excepcion is SqlException sql && sql.Number >= 50000)
            return new JsonResult(new { registros = new { resultado = false, mensaje = sql.Message } });

        var mensaje = desarrollo ? excepcion.Message : "No se pudo completar la operación. Intente nuevamente.";
        return new JsonResult(new { registros = new { resultado = false, mensaje } });
    }
}
