using Microsoft.Extensions.Caching.Memory;
using SGH.Application.Navegacion;
using SGH.Domain.Cuenta;
using SGH.Domain.Navegacion;
using SGH.Domain.Navegacion.Interfaces;

namespace SGH.Tests.Navegacion;

public sealed class ServicioMenuPerfilTests
{
    [Fact]
    public async Task Sin_filas_en_bd_usa_respaldo_para_operador()
    {
        var servicio = CrearServicio([]);
        var menu = await servicio.ObtenerMenuAsync(ReglasCuenta.PerfilOperacionesA, CancellationToken.None);

        Assert.False(menu.DesdeBaseDeDatos);
        Assert.Contains(menu.Grupos.SelectMany(g => g.Columnas).SelectMany(c => c.Items), i => i.ClaveModulo == MenuClaves.Censo);
    }

    [Fact]
    public async Task Filas_con_link_de_censo_habilitan_modulo_censo()
    {
        var filas = new[]
        {
            new MenuPerfilItem(8, "Censo", "/app/gestion/censo", 2, "side", 3, null),
            new MenuPerfilItem(2, "Gestión", "/app/gestion", null, "nav", 2, null)
        };
        var servicio = CrearServicio(filas);

        var tiene = await servicio.TieneAccesoModuloAsync(68, MenuClaves.Censo, CancellationToken.None);
        Assert.True(tiene);
    }

    private static ServicioMenuPerfil CrearServicio(IReadOnlyList<MenuPerfilItem> filas) =>
        new(new ConsultaFalsa(filas), new MemoryCache(new MemoryCacheOptions()));

    private sealed class ConsultaFalsa(IReadOnlyList<MenuPerfilItem> filas) : IConsultaMenuPerfil
    {
        public Task<IReadOnlyList<MenuPerfilItem>> ObtenerPorPerfilAsync(int perfilId, CancellationToken cancellationToken) =>
            Task.FromResult(filas);
    }
}