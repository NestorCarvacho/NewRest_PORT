using System.Net.Mail;
using SGH.Application.Solicitud.DTOs;
using SGH.Application.Solicitud.Interfaces;

namespace SGH.Application.Solicitud.UseCases;

public sealed class GestionSolicitudes(IConsultaSolicitudAlojamiento consulta, IComandoSolicitudAlojamiento comando)
    : IGestionSolicitudes
{
    public async Task<IReadOnlyList<SolicitudListadaDto>> BuscarAsync(FiltroSolicitudesDto filtro, CancellationToken cancellationToken)
    {
        ValidarSesion(filtro.EmpresaMandanteId, filtro.CampamentoId);
        if (filtro.Desde.Date > filtro.Hasta.Date)
            throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta.");
        if (filtro.Hasta.Date == DateTime.MaxValue.Date)
            throw new ArgumentException("La fecha hasta está fuera del rango permitido.");
        if (filtro.GrupoId < 0 || filtro.SolicitanteId < 0 || filtro.Numero < 0
            || string.IsNullOrWhiteSpace(filtro.Estado) || filtro.Estado.Length != 1)
            throw new ArgumentException("Los filtros de solicitud no son válidos.");
        return await consulta.BuscarAsync(filtro with { Desde = filtro.Desde.Date, Hasta = filtro.Hasta.Date }, cancellationToken);
    }

    public async Task<SolicitudDetalleDto?> ObtenerAsync(int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0)
            throw new ArgumentException("Seleccione una solicitud válida.");
        return await consulta.ObtenerAsync(id, empresaMandanteId, campamentoId, cancellationToken);
    }

    public async Task<IReadOnlyList<PasajeroSolicitudDto>> ListarPasajerosAsync(
        int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0)
            throw new ArgumentException("Seleccione una solicitud válida.");
        return await consulta.ListarPasajerosAsync(id, empresaMandanteId, campamentoId, cancellationToken);
    }

    public async Task<IReadOnlyList<HabitacionDisponibleSolicitudDto>> HabitacionesDisponiblesAsync(
        int id, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0)
            throw new ArgumentException("Seleccione una solicitud válida.");
        return await consulta.HabitacionesDisponiblesAsync(id, empresaMandanteId, campamentoId, cancellationToken);
    }

    public Task AgregarCuposAsync(int id, int empresaMandanteId, int campamentoId, int usuarioId, int cantidad, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0 || usuarioId <= 0 || cantidad is < 1 or > 999)
            throw new ArgumentException("Indique una solicitud, un usuario y una cantidad de cupos válidos.");
        return comando.AgregarCuposAsync(id, empresaMandanteId, campamentoId, usuarioId, cantidad, cancellationToken);
    }

    public Task EliminarPasajeroAsync(int id, int pasajeroId, int empresaMandanteId, int campamentoId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0 || pasajeroId <= 0)
            throw new ArgumentException("Seleccione una solicitud y un pasajero válidos.");
        return comando.EliminarPasajeroAsync(id, pasajeroId, empresaMandanteId, campamentoId, cancellationToken);
    }

    public Task<int> GuardarPasajeroAsync(GuardarPasajeroSolicitudDto pasajero, CancellationToken cancellationToken)
    {
        ValidarSesion(pasajero.EmpresaMandanteId, pasajero.CampamentoId);
        if (pasajero.SolicitudId <= 0 || pasajero.PasajeroId < 0 ||
            pasajero.UsuarioPasajeroId <= 0 || pasajero.UsuarioId <= 0 || pasajero.FechaCheckIn == default)
            throw new ArgumentException("Indique una solicitud, un pasajero y una fecha de check-in válidos.");
        if (pasajero.FechaCheckOut.HasValue && pasajero.FechaCheckOut.Value.Date < pasajero.FechaCheckIn.Date)
            throw new ArgumentException("La fecha de check-out no puede ser anterior a la de check-in.");
        return comando.GuardarPasajeroAsync(pasajero with
        {
            FechaCheckIn = pasajero.FechaCheckIn.Date,
            FechaCheckOut = pasajero.FechaCheckOut?.Date
        }, cancellationToken);
    }

    public Task<int> GenerarReservasAsync(int id, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0 || usuarioId <= 0)
            throw new ArgumentException("Indique una solicitud y un usuario válidos.");
        return comando.GenerarReservasAsync(id, empresaMandanteId, campamentoId, usuarioId, cancellationToken);
    }

    public async Task<IReadOnlyList<ResultadoCargaPasajeroDto>> CargarPasajerosAsync(
        int id, int empresaMandanteId, int campamentoId, int usuarioId, IReadOnlyList<FilaCargaPasajeroDto> filas, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0 || usuarioId <= 0)
            throw new ArgumentException("Indique una solicitud y un usuario válidos.");
        var resultados = new List<ResultadoCargaPasajeroDto>();
        foreach (var fila in filas)
        {
            var tipo = fila.TipoIdentificador.Trim().ToLowerInvariant();
            var identificador = fila.Identificador.Trim();
            if (tipo == "rut")
                identificador = identificador.Replace(".", "").Replace("-", "").TrimStart('0');
            var genero = fila.Genero.Trim().ToUpperInvariant();
            var error = ValidarFilaCarga(fila, tipo, identificador, genero);
            if (error is not null)
            {
                resultados.Add(new(fila.NumeroFila, false, error));
                continue;
            }
            var resultado = await comando.CargarPasajeroAsync(id, empresaMandanteId, campamentoId, usuarioId,
                fila with { TipoIdentificador = tipo, Identificador = identificador, Genero = genero }, cancellationToken);
            resultados.Add(new(fila.NumeroFila, resultado.Procesada,
                resultado.Mensaje ?? (resultado.Procesada ? "Procesada" : "El procedimiento rechazó la fila sin indicar el motivo.")));
        }
        return resultados;
    }

    private static string? ValidarFilaCarga(FilaCargaPasajeroDto fila, string tipo, string identificador, string genero)
    {
        if (tipo is not ("rut" or "dni"))
            return "Tipo de identificador inválido.";
        if (tipo == "rut" ? !RutValido(identificador) : identificador.Length is < 2 or > 25)
            return "Identificador inválido.";
        if (string.IsNullOrWhiteSpace(fila.Nombres) || fila.Nombres.Length > 100 ||
            string.IsNullOrWhiteSpace(fila.Apellidos) || fila.Apellidos.Length > 100)
            return "Nombres y apellidos son obligatorios y no pueden superar 100 caracteres.";
        return genero is "M" or "F" ? null : "Género debe ser M o F.";
    }

    private static bool RutValido(string rut)
    {
        if (rut.Length is < 8 or > 9)
            return false;
        var suma = 0;
        var multiplicador = 2;
        for (var i = rut.Length - 2; i >= 0; i--)
        {
            if (rut[i] is < '0' or > '9')
                return false;
            suma += (rut[i] - '0') * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }
        var digito = 11 - suma % 11;
        var esperado = digito == 11 ? '0' : digito == 10 ? 'K' : (char)('0' + digito);
        return char.ToUpperInvariant(rut[^1]) == esperado;
    }

    public Task AsignarHabitacionAsync(int id, int pasajeroId, int habitacionId, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0 || pasajeroId <= 0 || habitacionId <= 0 || usuarioId <= 0)
            throw new ArgumentException("Indique una solicitud, un pasajero y una habitación válidos.");
        return comando.AsignarHabitacionAsync(id, pasajeroId, habitacionId, empresaMandanteId, campamentoId, usuarioId, cancellationToken);
    }

    public async Task<CatalogoSolicitudDto> CatalogoAsync(
        int empresaMandanteId, int campamentoId, int grupoId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (grupoId < 0)
            throw new ArgumentException("Seleccione un grupo válido.");
        return await consulta.CatalogoAsync(empresaMandanteId, grupoId, cancellationToken);
    }

    public async Task<CatalogoOrganizacionSolicitudDto> OrganizacionAsync(
        int empresaMandanteId, int campamentoId, int unidadId, int divisionId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (unidadId < 0 || divisionId < 0 || (divisionId > 0 && unidadId == 0))
            throw new ArgumentException("Seleccione una unidad de negocio y división válidas.");
        return await consulta.OrganizacionAsync(empresaMandanteId, unidadId, divisionId, cancellationToken);
    }

    public Task<int> GuardarAsync(DatosSolicitudDto solicitud, CancellationToken cancellationToken)
    {
        ValidarSesion(solicitud.EmpresaMandanteId, solicitud.CampamentoId);
        if (solicitud.Id < 0 || solicitud.UsuarioId <= 0)
            throw new ArgumentException("La solicitud no tiene datos de sesión válidos.");
        if (solicitud.SolicitanteId < 0 || solicitud.TurnoRotacionId < 0
            || solicitud.UnidadNegocioId < 0 || solicitud.DivisionId < 0 || solicitud.DepartamentoId < 0)
            throw new ArgumentException("Los identificadores de la solicitud no son válidos.");
        if (solicitud.Id == 0)
        {
            if (solicitud.GrupoId <= 0)
                throw new ArgumentException("Seleccione el grupo de la empresa solicitante.");
            if (solicitud.TurnoId is null or <= 0)
                throw new ArgumentException("Seleccione el turno principal del grupo.");
            if (!solicitud.FechaSolicitud.HasValue || !solicitud.FechaCheckIn.HasValue)
                throw new ArgumentException("Ingrese la fecha de solicitud y de check-in.");
            if (solicitud.FechaCheckOut.HasValue && solicitud.FechaCheckOut.Value.Date < solicitud.FechaCheckIn.Value.Date)
                throw new ArgumentException("La fecha de check-out no puede ser anterior a la de check-in.");
            if (solicitud.ConSistemaTurno && !solicitud.FechaCheckOut.HasValue)
                throw new ArgumentException("Para generar reservas con turnos debe indicar fecha de check-out.");
        }
        if (string.IsNullOrWhiteSpace(solicitud.EmailContactoCampamento)
            || !MailAddress.TryCreate(solicitud.EmailContactoCampamento, out _))
            throw new ArgumentException("Ingrese un correo de contacto válido.");
        if (solicitud.EmailContactoCampamento.Length > 100 || solicitud.Adc?.Length > 100
            || solicitud.NumeroContactoCampamento?.Length > 20 || solicitud.NumeroContrato?.Length > 100
            || solicitud.Observacion?.Length > 200)
            throw new ArgumentException("Un campo supera la longitud permitida.");
        return comando.GuardarAsync(solicitud with
        {
            SolicitanteId = solicitud.SolicitanteId == 0 ? null : solicitud.SolicitanteId,
            TurnoRotacionId = solicitud.TurnoRotacionId == 0 ? null : solicitud.TurnoRotacionId,
            UnidadNegocioId = solicitud.UnidadNegocioId == 0 ? null : solicitud.UnidadNegocioId,
            DivisionId = solicitud.DivisionId == 0 ? null : solicitud.DivisionId,
            DepartamentoId = solicitud.DepartamentoId == 0 ? null : solicitud.DepartamentoId
        }, cancellationToken);
    }

    public Task AnularAsync(int id, int empresaMandanteId, int campamentoId, int usuarioId, CancellationToken cancellationToken)
    {
        ValidarSesion(empresaMandanteId, campamentoId);
        if (id <= 0 || usuarioId <= 0)
            throw new ArgumentException("La solicitud no tiene datos de sesión válidos.");
        return comando.AnularAsync(id, empresaMandanteId, campamentoId, usuarioId, cancellationToken);
    }

    private static void ValidarSesion(int empresaMandanteId, int campamentoId)
    {
        if (empresaMandanteId <= 0 || campamentoId <= 0)
            throw new ArgumentException("La sesión no tiene empresa mandante y campamento válidos.");
    }
}
