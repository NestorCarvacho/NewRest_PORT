using SGH.Infrastructure.Data;
using SGH.Infrastructure.Solicitud.Commands;

namespace SGH.Tests.Solicitud;

public class ErroresGeneracionTests
{
    [Fact]
    public void PasajerosSinHabitacionSeMuestraComoErrorLegible()
    {
        var resultado = new LIB_ControlResultado { Codigo = "51051" };
        resultado.SetBadResult("Error técnico", "(51051) Asigne una habitaciÃ³n a todos los pasajeros.");

        var mensaje = ComandoSolicitudAlojamiento.MensajeError("dbo.sp_Solicitud_GenerarReservas", resultado);

        Assert.Equal("Asigne una habitación a todos los pasajeros antes de generar reservas.", mensaje);
    }

    [Theory]
    [InlineData("dbo.sp_Solicitud_GenerarReservas", "51056")]
    [InlineData("dbo.sp_Solicitud_AsignarHabitacion", "51051")]
    public void OtrosErroresConservanDetalleOriginal(string procedimiento, string codigo)
    {
        var resultado = new LIB_ControlResultado { Codigo = codigo };
        resultado.SetBadResult("Error técnico", "Detalle original");

        Assert.Equal("Error técnico. Detalle original", ComandoSolicitudAlojamiento.MensajeError(procedimiento, resultado));
    }
}
