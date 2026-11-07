using SGH.Domain.Reserva;

namespace SGH.Tests.Reserva;

public class ReglasReservaTests
{
    [Fact]
    public void AltaExigeAreaTurnoYSalidaPosterior()
    {
        var llegada = new DateTime(2026, 11, 7);

        Assert.Equal("Seleccione Área", ReglasReserva.ValidarAlta(0, 1, llegada, null));
        Assert.Equal("Seleccione Turno", ReglasReserva.ValidarAlta(3, 0, llegada, null));
        Assert.Equal(
            "Fecha Salida debe ser mayor o igual a la fecha de Llegada",
            ReglasReserva.ValidarAlta(3, 1, llegada, llegada.AddDays(-1)));
        Assert.Null(ReglasReserva.ValidarAlta(3, 1, llegada, llegada));
    }

    [Fact]
    public void MovimientoSoloAceptaCheckInYO()
    {
        var fecha = new DateTime(2026, 11, 7, 15, 0, 0);

        Assert.Null(ReglasReserva.ValidarMovimiento(8, fecha, ReglasReserva.CheckIn));
        Assert.Null(ReglasReserva.ValidarMovimiento(8, fecha, ReglasReserva.CheckOut));
        Assert.NotNull(ReglasReserva.ValidarMovimiento(0, fecha, ReglasReserva.CheckIn));
        Assert.NotNull(ReglasReserva.ValidarMovimiento(8, null, ReglasReserva.CheckOut));
        Assert.NotNull(ReglasReserva.ValidarMovimiento(8, fecha, "X"));
    }
}
