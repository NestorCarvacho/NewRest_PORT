using SGH.Web.Ui;

namespace SGH.Tests.Ui;

public sealed class BibliotecaUiModelsTests
{
    [Fact]
    public void TablaListado_vm_tiene_paginacion_cliente_por_defecto()
    {
        var vm = new TablaListadoVm { Columnas = ["Id", "Nombre"] };
        Assert.Equal(2, vm.Columnas.Count);
        Assert.True(vm.IncluirPaginacionCliente);
        Assert.Equal(BibliotecaUiClases.Tabla, vm.ClaseContenedor);
    }

    [Fact]
    public void ModuloEscritorio_usa_id_proporcionado()
    {
        var vm = new ModuloEscritorioVm("reserva", "Reservas");
        Assert.Equal("reserva", vm.ModuloId);
        Assert.Equal("Reservas", vm.TituloBienvenida);
    }
}