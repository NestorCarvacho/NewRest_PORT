using SGH.Application.Habitacion;
using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Tests.Habitacion;

public sealed class HabitacionLegadoDefaultsTests
{
    [Fact]
    public void EdicionConservaCapacidadYCompletaPersonas()
    {
        var actual = new HabitacionDetalle(
            31643, "Cabinas P-10", 3407, "M", "R", 10, true, true,
            5, 7, null, null, 1,
            2, 10, 0, 0, 0, false, false, false);

        var desdeUi = new DatosGuardarHabitacion(
            31643, 3407, "Cabinas P-10", true, 10, "M", 0, 0, "R", null, null, true,
            0, 0, 0, 0, false, false, false, 2, 0, 99);

        var fusionado = HabitacionLegadoDefaults.FusionarEdicion(desdeUi, actual);

        Assert.Equal(5, fusionado.TipoInstalacionId);
        Assert.Equal(7, fusionado.StandardHabitacionId);
        Assert.Equal(10, fusionado.PersonasCantidadMax);
        Assert.Equal(2, fusionado.BanoId);
    }
}