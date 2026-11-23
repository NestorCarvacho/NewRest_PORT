namespace SGH.Application.Solicitud.DTOs;

public sealed record FiltroSolicitudesDto(
    int EmpresaMandanteId, int CampamentoId, DateTime Desde, DateTime Hasta,
    int GrupoId, int SolicitanteId, string Estado, int Numero);

public sealed record DatosSolicitudDto(
    int Id, int EmpresaMandanteId, int CampamentoId, int UsuarioId,
    DateTime? FechaSolicitud, DateTime? FechaCheckIn, DateTime? FechaCheckOut,
    int GrupoId, int? SolicitanteId, bool ConSistemaTurno, int? TurnoId,
    int? TurnoRotacionId, string? Observacion, string? Adc,
    string EmailContactoCampamento, string? NumeroContactoCampamento,
    int? UnidadNegocioId, int? DivisionId, int? DepartamentoId,
    string? NumeroContrato, bool ReservaVisita);

public sealed record SolicitudListadaDepuracionDto(
    int SolicitudId,
    string EstadoCodigo,
    int EstadoTabla,
    int CampamentoId,
    int EmpresaMandanteId,
    int AreaEmpresaSolicitanteId,
    int? UsuarioSolicitanteId,
    int EmpresaContratistaId,
    DateTime FechaCheckIn,
    DateTime? FechaCheckOut,
    int? TurnoId,
    bool ConSistemaTurno,
    int? PrimeraReservaGeneradaId);

public sealed record SolicitudListadaDto(
    int Numero, DateTime Fecha, string Empresa, string? Solicitante, string? Estado,
    SolicitudListadaDepuracionDto Depuracion);

public sealed record PasajeroSolicitudDto(
    int Id, int? UsuarioPasajeroId, string? NombrePasajero, string? Rut,
    string? TipoIdentificador, string? Empresa, string? Cargo, string? CheckIn,
    string? CheckOut, int? HabitacionId, string? Habitacion, int? NumeroReserva);

public sealed record GuardarPasajeroSolicitudDto(
    int SolicitudId, int PasajeroId, int UsuarioPasajeroId, int EmpresaMandanteId,
    int CampamentoId, int UsuarioId, DateTime FechaCheckIn, DateTime? FechaCheckOut);

public sealed record FilaCargaPasajeroDto(
    int NumeroFila, string Identificador, string TipoIdentificador, string Nombres, string Apellidos, string Genero);
public sealed record ResultadoCargaPasajeroDto(int NumeroFila, bool Procesada, string Mensaje);
public sealed record ResultadoProcedimientoCargaDto(bool Procesada, string? Mensaje);

public sealed record HabitacionDisponibleSolicitudDto(
    int PabellonId, string? PabellonName, int ModuloId, string? ModuloName,
    int HabitacionId, string? HabitacionName, int TotalCamas, string? Sexo,
    string? TipoInstalacion, string? StandardHabitacion, int AreaEmpresaContratista,
    int CantidadCamasAsignadas, int CantidadCamasDisponibles);

public sealed record OpcionSolicitudDto(int Id, string Nombre);
public sealed record GrupoSolicitudDto(int Id, string Nombre, int? TurnoId, int? UnidadNegocioId, string? Contrato);
public sealed record EstadoSolicitudDto(string Id, string Nombre);
public sealed record TurnoSolicitudDto(int Id, string Nombre, int DiasTrabajados, int DiasLibres, string? TipoTurno);
public sealed record CatalogoSolicitudDto(
    IReadOnlyList<GrupoSolicitudDto> Grupos, IReadOnlyList<EstadoSolicitudDto> Estados,
    IReadOnlyList<OpcionSolicitudDto> Solicitantes, IReadOnlyList<TurnoSolicitudDto> Turnos,
    string? EmailEmpresa, string? EmailCampamento);
public sealed record CatalogoOrganizacionSolicitudDto(
    IReadOnlyList<OpcionSolicitudDto> UnidadesNegocio, IReadOnlyList<OpcionSolicitudDto> Divisiones,
    IReadOnlyList<OpcionSolicitudDto> Departamentos);

public sealed record SolicitudDetalleDto(
    int Id, DateTime FechaSolicitud, int? UsuarioSolicitanteId, string? UsuarioSolicitante,
    int EmpresaMandanteId, string? EmpresaMandante, int EmpresaContratistaId,
    string? EmpresaContratistaNombre, int AreaEmpresaSolicitanteId, string? AreaEmpresaSolicitante,
    bool ConSistemaTurno, int? Turno, int? TurnoRotacion, string? TipoTurno, string? TipoTurnoRotacion,
    int CampamentoId, string? CampamentoNombre, string? CampamentoEmail, string? Observacion,
    DateTime FechaCheckIn, DateTime? FechaCheckOut, string Estado, string? EstadoNombre,
    int UsuarioCreacionId, int? PrimeraReservaGeneradaId, string? Adc, string? EmailContactoCampamento,
    string? NumeroContactoCampamento, int? UnidadNegocioId, string? UnidadNegocioNombre,
    int? DivisionId, string? DivisionNombre, int? DepartamentoId, string? DepartamentoNombre,
    string? NumeroContrato, bool? ReservaVisita);
