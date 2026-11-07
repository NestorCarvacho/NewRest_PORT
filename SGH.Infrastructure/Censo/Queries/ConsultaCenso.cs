using System.Data;
using Dapper;
using SGH.Domain.Censo;
using SGH.Domain.Censo.Entities;
using SGH.Domain.Censo.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Censo.Queries;

public sealed class ConsultaCenso : IConsultaCenso
{
    private readonly IFabricaConexionSql _fabrica;

    public ConsultaCenso(IFabricaConexionSql fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<IReadOnlyList<CensoListado>> BuscarAsync(FiltroCenso filtro, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var filas = await conexion.QueryAsync<CensoFila>(
            new CommandDefinition(
                "sp_Censo_Buscar",
                new
                {
                    filtro.EmpresaMandanteId,
                    filtro.CampamentoId,
                    filtro.ModuloId,
                    filtro.PisoId,
                    filtro.HabitacionId,
                    FechaCenso = filtro.Fecha,
                    filtro.Turno
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120,
                cancellationToken: cancellationToken));

        return filas.Select(fila => new CensoListado(
            fila.Id ?? string.Empty,
            fila.FechaCenso ?? string.Empty,
            fila.PabellonNombre ?? string.Empty,
            fila.ModuloNombre ?? string.Empty,
            fila.HabitacionNombre ?? string.Empty,
            fila.HabitacionId ?? string.Empty,
            fila.EstadoDesc ?? string.Empty,
            fila.EstadoHabitacion ?? string.Empty,
            fila.NumeroCamas ?? string.Empty,
            fila.CamasOcupadas ?? string.Empty,
            fila.CamasCheckIn ?? string.Empty,
            string.IsNullOrWhiteSpace(fila.Turno) ? filtro.Turno : fila.Turno)).ToList();
    }

    public async Task<CatalogoCenso> ObtenerCatalogoAsync(int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        using var lectura = await conexion.QueryMultipleAsync(
            new CommandDefinition(
                "sp_Censo_ListarCatalogo",
                new { CampamentoId = campamentoId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var modulos = (await lectura.ReadAsync<OpcionFila>()).ToList();
        var pisos = (await lectura.ReadAsync<OpcionFila>()).ToList();
        var habitaciones = (await lectura.ReadAsync<OpcionFila>()).ToList();

        return new CatalogoCenso(ALista(modulos), ALista(pisos), ALista(habitaciones));
    }

    public async Task<bool> FiltroEstaEnCampamentoAsync(int campamentoId, int moduloId, int pisoId, int habitacionId, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var marca = await conexion.QuerySingleAsync<MarcaPertenece>(
            new CommandDefinition(
                "sp_Censo_FiltroPertenece",
                new
                {
                    CampamentoId = campamentoId,
                    ModuloId = moduloId,
                    PisoId = pisoId,
                    HabitacionId = habitacionId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return marca.Pertenece;
    }

    private static List<OpcionCenso> ALista(IEnumerable<OpcionFila> filas) =>
        filas.Select(fila => new OpcionCenso(fila.Id, fila.Etiqueta ?? string.Empty, fila.PadreId)).ToList();

    private sealed class CensoFila
    {
        public string? Id { get; set; }
        public string? FechaCenso { get; set; }
        public string? PabellonNombre { get; set; }
        public string? ModuloNombre { get; set; }
        public string? HabitacionNombre { get; set; }
        public string? HabitacionId { get; set; }
        public string? EstadoDesc { get; set; }
        public string? EstadoHabitacion { get; set; }
        public string? NumeroCamas { get; set; }
        public string? CamasOcupadas { get; set; }
        public string? CamasCheckIn { get; set; }
        public string? Turno { get; set; }
    }

    private sealed class MarcaPertenece
    {
        public bool Pertenece { get; set; }
    }

    private sealed class OpcionFila
    {
        public int Id { get; set; }
        public string? Etiqueta { get; set; }
        public int PadreId { get; set; }
    }
}
