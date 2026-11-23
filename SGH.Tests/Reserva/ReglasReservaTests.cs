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

    [Fact]
    public void AnularRechazaReservasCerradas()
    {
        Assert.Null(ReglasReserva.ValidarAnular(5, ReglasReserva.EstadoAsignada));
        Assert.NotNull(ReglasReserva.ValidarAnular(0, "A"));
        Assert.NotNull(ReglasReserva.ValidarAnular(5, ReglasReserva.EstadoAnulada));
        Assert.NotNull(ReglasReserva.ValidarAnular(5, ReglasReserva.EstadoFinalizada));
    }

    [Fact]
    public void FinalizarExigeAsignadaConCheckOut()
    {
        Assert.Null(ReglasReserva.ValidarFinalizar(9, ReglasReserva.EstadoAsignada, "11/11/2026 13:00"));
        Assert.NotNull(ReglasReserva.ValidarFinalizar(9, "P", "11/11/2026 13:00"));
        Assert.NotNull(ReglasReserva.ValidarFinalizar(9, ReglasReserva.EstadoAsignada, null));
    }
}
