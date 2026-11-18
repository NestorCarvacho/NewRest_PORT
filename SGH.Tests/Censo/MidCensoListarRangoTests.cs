using SGH.Application.Censo.DTOs;
using SGH.Application.Censo.Interfaces;
using SGH.Application.Mid;
using SGH.Application.Sesion;
using SGH.Domain.Censo.Entities;

namespace SGH.Tests.Censo;

public class MidCensoListarRangoTests
{
    [Fact]
    public async Task ListarRangoAgrupaDiasEnUnaRespuesta()
    {
        var buscar = new BuscarCensoContador();
        var mid = new MidCenso(buscar, new BuscarMiosCensoNulo(), new CatalogoCensoNulo(), new GuardarCensoNulo());
        var filtro = new FiltroPeticion
        {
            Opcion = "listarrango",
            Resto = new Dictionary<string, System.Text.Json.JsonElement>
            {
                ["desde"] = System.Text.Json.JsonSerializer.SerializeToElement("2026-01-01"),
                ["hasta"] = System.Text.Json.JsonSerializer.SerializeToElement("2026-01-03"),
                ["turno"] = System.Text.Json.JsonSerializer.SerializeToElement("D")
            }
        };

        var ejecucion = await mid.EjecutarAsync(filtro, new DatosSesion(1, 10, 20), CancellationToken.None);

        Assert.Equal(200, ejecucion.Status);
        Assert.Equal(3, buscar.Llamadas);
    }

    private sealed class BuscarCensoContador : IBuscarCensos
    {
        public int Llamadas { get; private set; }

        public Task<ResultadoBusquedaCenso> EjecutarAsync(SolicitudBuscarCensos solicitud, CancellationToken cancellationToken)
        {
            Llamadas++;
            var fila = new CensoFilaDto("1", solicitud.Fecha!.Value.ToString("dd/MM/yyyy"), "M", "1", "H1", "9", "ok", "ok", "2", "1", "0", solicitud.Turno!);
            return Task.FromResult<ResultadoBusquedaCenso>(new ResultadoBusquedaCenso.Lista([fila]));
        }
    }

    private sealed class BuscarMiosCensoNulo : IBuscarMisCensos
    {
        public Task<ResultadoBusquedaMisCensos> EjecutarAsync(SolicitudBuscarMisCensos solicitud, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }

    private sealed class CatalogoCensoNulo : IObtenerCatalogoCenso
    {
        public Task<ResultadoCatalogoCenso> EjecutarAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }

    private sealed class GuardarCensoNulo : IGuardarCensos
    {
        public Task<ResultadoGuardadoCenso> EjecutarAsync(SolicitudGuardarCensos solicitud, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }
}