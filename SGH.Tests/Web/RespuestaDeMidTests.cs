using Microsoft.AspNetCore.Mvc;
using SGH.Application.Mid;
using SGH.Web.Mid;

namespace SGH.Tests.Web;

public sealed class RespuestaDeMidTests
{
    [Fact]
    public void DeExcepcion_reglaDominio_propagaMensaje()
    {
        var resultado = RespuestaDeMid.DeExcepcion(new InvalidOperationException("Seleccione campamento."), desarrollo: false);
        var json = Assert.IsType<JsonResult>(resultado);
        var mensaje = LeerMensaje(json);
        Assert.Equal("Seleccione campamento.", mensaje);
    }

    [Fact]
    public void DeExcepcion_excepcionGenerica_enProduccion_ocultaDetalle()
    {
        var resultado = RespuestaDeMid.DeExcepcion(new Exception("secreto"), desarrollo: false);
        var mensaje = LeerMensaje(Assert.IsType<JsonResult>(resultado));
        Assert.Equal("No se pudo completar la operación. Intente nuevamente.", mensaje);
    }

    [Fact]
    public void De_fallo_devuelveRegistrosConResultadoFalse()
    {
        var resultado = RespuestaDeMid.De(EjecucionMid.Fallo(400, "Falta opción"));
        var json = Assert.IsType<JsonResult>(resultado);
        var mensaje = LeerMensaje(json);
        Assert.Equal("Falta opción", mensaje);
    }

    private static string LeerMensaje(JsonResult json)
    {
        var registros = json.Value!.GetType().GetProperty("registros")!.GetValue(json.Value);
        return (registros!.GetType().GetProperty("mensaje")!.GetValue(registros) as string)!;
    }
}