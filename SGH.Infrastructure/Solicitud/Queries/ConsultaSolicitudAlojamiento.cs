using System.Data;
using Dapper;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Solicitud.Queries;

public sealed class ConsultaSolicitudAlojamiento(IFabricaConexionSql fabrica) : IConsultaSolicitudAlojamiento
{
    public async Task<IReadOnlyList<HabitacionDisponibleSolicitudDto>> HabitacionesDisponiblesAsync(
        int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<HabitacionDisponibleSolicitudDto>(new CommandDefinition(
            "dbo.sp_Solicitud_HabitacionesDisponibles",
            new { SolicitudId = id, empresaMandanteId, campamentoId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return filas.AsList();
    }

    public async Task<IReadOnlyList<PasajeroSolicitudDto>> ListarPasajerosAsync(
        int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<PasajeroFila>(new CommandDefinition(
            "dbo.sp_Solicitud_ListarPasajeros",
            new { SolicitudId = id, empresaMandanteId, campamentoId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return filas.Select(p => new PasajeroSolicitudDto(
            p.Id, p.UsuarioPasajeroId, p.NombrePasajero, p.Rut, p.TipoIdentificador,
            p.Empresa, p.Cargo, p.CheckIn, p.CheckOut, p.HabitacionId, p.Habitacion)).ToList();
    }

    private sealed class PasajeroFila
    {
        public int Id { get; set; }
        public int? UsuarioPasajeroId { get; set; }
        public string? NombrePasajero { get; set; }
        public string? Rut { get; set; }
        public string? TipoIdentificador { get; set; }
        public string? Empresa { get; set; }
        public string? Cargo { get; set; }
        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }
        public int? HabitacionId { get; set; }
        public string? Habitacion { get; set; }
    }

    public async Task<IReadOnlyList<SolicitudListadaDto>> BuscarAsync(FiltroSolicitudesDto filtro, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var filas = await conexion.QueryAsync<SolicitudListadaFila>(new CommandDefinition(
            "dbo.sp_Solicitud_Buscar",
            new { filtro.EmpresaMandanteId, filtro.CampamentoId, filtro.Desde, filtro.Hasta,
                filtro.GrupoId, filtro.SolicitanteId, filtro.Estado, filtro.Numero },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return filas.Select(MapearListada).ToList();
    }

    private static SolicitudListadaDto MapearListada(SolicitudListadaFila fila) =>
        new(fila.Numero, fila.Fecha, fila.Empresa, fila.Solicitante, fila.Estado,
            new SolicitudListadaDepuracionDto(
                fila.SolicitudId, fila.EstadoCodigo, fila.EstadoTabla, fila.CampamentoId, fila.EmpresaMandanteId,
                fila.AreaEmpresaSolicitanteId, fila.UsuarioSolicitanteId, fila.EmpresaContratistaId,
                fila.FechaCheckIn, fila.FechaCheckOut, fila.TurnoId, fila.ConSistemaTurno, fila.PrimeraReservaGeneradaId));

    private sealed class SolicitudListadaFila
    {
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string Empresa { get; set; } = "";
        public string? Solicitante { get; set; }
        public string? Estado { get; set; }
        public int SolicitudId { get; set; }
        public string EstadoCodigo { get; set; } = "";
        public int EstadoTabla { get; set; }
        public int CampamentoId { get; set; }
        public int EmpresaMandanteId { get; set; }
        public int AreaEmpresaSolicitanteId { get; set; }
        public int? UsuarioSolicitanteId { get; set; }
        public int EmpresaContratistaId { get; set; }
        public DateTime FechaCheckIn { get; set; }
        public DateTime? FechaCheckOut { get; set; }
        public int? TurnoId { get; set; }
        public bool ConSistemaTurno { get; set; }
        public int? PrimeraReservaGeneradaId { get; set; }
    }

    public async Task<SolicitudDetalleDto?> ObtenerAsync(int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        var detalle = await conexion.QuerySingleOrDefaultAsync<SolicitudDetalleFila>(new CommandDefinition(
            "dbo.SOL_ObtieneSolicitudAlojamiento", new { id },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        if (detalle is null || detalle.EmpresaMandanteId != empresaMandanteId || detalle.CampamentoId != campamentoId)
            return null;
        var primeraReservaId = detalle.Estado == "R"
            ? await conexion.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
                "dbo.sp_Solicitud_ObtenerPrimeraReserva", new { id, empresaMandanteId, campamentoId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
            : null;
        return new SolicitudDetalleDto(detalle.Id, detalle.FechaSolicitud, detalle.UsuarioSolicitanteId,
            detalle.UsuarioSolicitante, detalle.EmpresaMandanteId, detalle.EmpresaMandante, detalle.EmpresaContratistaId,
            detalle.EmpresaContratistaNombre, detalle.AreaEmpresaSolicitanteId, detalle.AreaEmpresaSolicitante,
            detalle.ConSistemaTurno, detalle.Turno, detalle.TurnoRotacion, detalle.TipoTurno, detalle.TipoTurnoRotacion,
            detalle.CampamentoId, detalle.CampamentoNombre, detalle.CampamentoEmail, detalle.Observacion,
            detalle.FechaCheckIn, detalle.FechaCheckOut, detalle.Estado, detalle.EstadoNombre, detalle.UsuarioCreacionId,
            primeraReservaId, detalle.Adc, detalle.EmailContactoCampamento,
            detalle.NumeroContactoCampamento, detalle.UnidadNegocioId, detalle.UnidadNegocioNombre, detalle.DivisionId,
            detalle.DivisionNombre, detalle.DepartamentoId, detalle.DepartamentoNombre, detalle.NumeroContrato, detalle.ReservaVisita);
    }

    public async Task<CatalogoSolicitudDto> CatalogoAsync(int empresaMandanteId, int grupoId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        using var multi = await conexion.QueryMultipleAsync(new CommandDefinition(
            "dbo.sp_Solicitud_ListarCatalogo", new { empresaMandanteId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        var grupos = (await multi.ReadAsync<GrupoSolicitudDto>()).AsList();
        var estados = (await multi.ReadAsync<EstadoSolicitudDto>()).AsList();
        var turnos = (await multi.ReadAsync<TurnoSolicitudDto>()).AsList();
        multi.Dispose();
        if (grupoId != 0 && !grupos.Any(grupo => grupo.Id == grupoId))
            throw new ArgumentException("El grupo no corresponde a la empresa de la sesión.");
        var usuarios = await conexion.QueryAsync<UsuarioSolicitante>(new CommandDefinition(
            "dbo.USU_ObtieneUsuarios",
            new { perfil = 2, empresaMandante = empresaMandanteId, empresaContratista = grupoId,
                rut = "", nombre = "", apellidos = "", cargo = "", activo = 1, origen = "standard" },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return new CatalogoSolicitudDto(grupos, estados,
            usuarios.Select(u => new OpcionSolicitudDto(u.Id, $"{u.Nombre} {u.Apellidos}".Trim())).ToList(), turnos);
    }

    private sealed class UsuarioSolicitante
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
    }

    public async Task<CatalogoOrganizacionSolicitudDto> OrganizacionAsync(
        int empresaMandanteId, int unidadId, int divisionId, CancellationToken cancellationToken)
    {
        using var conexion = fabrica.Crear();
        using var multi = await conexion.QueryMultipleAsync(new CommandDefinition(
            "dbo.sp_Solicitud_ListarOrganizacion", new { empresaMandanteId, unidadId, divisionId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        var unidades = (await multi.ReadAsync<OpcionFila>()).Select(x => new OpcionSolicitudDto(x.Id, x.Nombre)).ToList();
        var divisiones = (await multi.ReadAsync<OpcionFila>()).Select(x => new OpcionSolicitudDto(x.Id, x.Nombre)).ToList();
        var departamentos = (await multi.ReadAsync<OpcionFila>()).Select(x => new OpcionSolicitudDto(x.Id, x.Nombre)).ToList();
        return new CatalogoOrganizacionSolicitudDto(unidades, divisiones, departamentos);
    }

    private sealed class OpcionFila
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }

    private sealed class SolicitudDetalleFila
    {
        public int Id { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int? UsuarioSolicitanteId { get; set; }
        public string? UsuarioSolicitante { get; set; }
        public int EmpresaMandanteId { get; set; }
        public string? EmpresaMandante { get; set; }
        public int EmpresaContratistaId { get; set; }
        public string? EmpresaContratistaNombre { get; set; }
        public int AreaEmpresaSolicitanteId { get; set; }
        public string? AreaEmpresaSolicitante { get; set; }
        public bool ConSistemaTurno { get; set; }
        public int? Turno { get; set; }
        public int? TurnoRotacion { get; set; }
        public string? TipoTurno { get; set; }
        public string? TipoTurnoRotacion { get; set; }
        public int CampamentoId { get; set; }
        public string? CampamentoNombre { get; set; }
        public string? CampamentoEmail { get; set; }
        public string? Observacion { get; set; }
        public DateTime FechaCheckIn { get; set; }
        public DateTime? FechaCheckOut { get; set; }
        public string Estado { get; set; } = "";
        public string? EstadoNombre { get; set; }
        public int UsuarioCreacionId { get; set; }
        public string? Adc { get; set; }
        public string? EmailContactoCampamento { get; set; }
        public string? NumeroContactoCampamento { get; set; }
        public int? UnidadNegocioId { get; set; }
        public string? UnidadNegocioNombre { get; set; }
        public int? DivisionId { get; set; }
        public string? DivisionNombre { get; set; }
        public int? DepartamentoId { get; set; }
        public string? DepartamentoNombre { get; set; }
        public string? NumeroContrato { get; set; }
        public bool? ReservaVisita { get; set; }
    }
}
