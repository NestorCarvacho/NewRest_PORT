using System.Data;
using Dapper;
using SGH.Domain.Reserva;
using SGH.Domain.Reserva.Entities;
using SGH.Domain.Reserva.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Reserva.Queries;

public sealed class ConsultaReserva : IConsultaReserva
{
    private readonly IFabricaConexionSql _fabrica;

    public ConsultaReserva(IFabricaConexionSql fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<IReadOnlyList<ReservaListada>> BuscarAsync(FiltroReserva filtro, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var filas = await conexion.QueryAsync<ReservaFila>(
            new CommandDefinition(
                "sp_Reserva_Buscar",
                new
                {
                    filtro.CampamentoId,
                    filtro.Id,
                    filtro.AreaId,
                    filtro.Origen,
                    filtro.SolicitudId,
                    filtro.Estado,
                    filtro.FechaReserva,
                    filtro.FechaLlegada,
                    filtro.FechaSalida,
                    filtro.CheckIn,
                    filtro.CheckOut,
                    filtro.PasajeroId,
                    filtro.HabitacionId,
                    filtro.TieneAgua
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120,
                cancellationToken: cancellationToken));

        return filas.Select(fila => new ReservaListada(
            fila.Id,
            fila.GrupoEmpresa ?? string.Empty,
            fila.Pasajero ?? string.Empty,
            fila.Habitacion ?? string.Empty,
            fila.Estado ?? string.Empty,
            fila.EstadoNumero ?? string.Empty,
            fila.Llegada ?? string.Empty,
            fila.Salida ?? string.Empty,
            fila.CheckIn,
            fila.CheckOut)).ToList();
    }

    public async Task<CatalogoReserva> ObtenerCatalogoAsync(int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        using var lectura = await conexion.QueryMultipleAsync(
            new CommandDefinition(
                "sp_Reserva_ListarCatalogo",
                new { EmpresaMandanteId = empresaMandanteId, CampamentoId = campamentoId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var grupos = (await lectura.ReadAsync<OpcionNumeroFila>()).ToList();
        var origenes = (await lectura.ReadAsync<OpcionTextoFila>()).ToList();
        var pasajeros = (await lectura.ReadAsync<OpcionNumeroFila>()).ToList();
        var habitaciones = (await lectura.ReadAsync<OpcionNumeroFila>()).ToList();
        var turnos = (await lectura.ReadAsync<TurnoFila>()).ToList();

        return new CatalogoReserva(
            grupos.Select(fila => new OpcionNumero(fila.Id, fila.Etiqueta ?? string.Empty)).ToList(),
            origenes.Select(fila => new OpcionTexto(fila.Id ?? string.Empty, fila.Etiqueta ?? string.Empty)).ToList(),
            pasajeros.Select(fila => new OpcionNumero(fila.Id, fila.Etiqueta ?? string.Empty)).ToList(),
            habitaciones.Select(fila => new OpcionNumero(fila.Id, fila.Etiqueta ?? string.Empty)).ToList(),
            turnos.Select(fila => new TurnoDisponible(fila.Id, fila.Nombre ?? string.Empty, fila.TipoTurno ?? string.Empty)).ToList());
    }

    private sealed class ReservaFila
    {
        public int Id { get; set; }
        public string? GrupoEmpresa { get; set; }
        public string? Pasajero { get; set; }
        public string? Habitacion { get; set; }
        public string? Estado { get; set; }
        public string? EstadoNumero { get; set; }
        public string? Llegada { get; set; }
        public string? Salida { get; set; }
        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }
    }

    private sealed class OpcionNumeroFila
    {
        public int Id { get; set; }
        public string? Etiqueta { get; set; }
    }

    private sealed class OpcionTextoFila
    {
        public string? Id { get; set; }
        public string? Etiqueta { get; set; }
    }

    private sealed class TurnoFila
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? TipoTurno { get; set; }
    }
}
