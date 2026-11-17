using SGH.Application.Reserva.DTOs;
using SGH.Application.Reserva.Interfaces;
using SGH.Application.Sesion;
using SGH.Domain.Consulta;

namespace SGH.Application.Mid;

public sealed class MidReservas(
    IBuscarReservas buscar,
    IObtenerCatalogoReserva catalogo,
    ICrearReserva crear,
    IImportarReservas importar,
    IPlanillaReservaExcel planilla,
    IRegistrarMovimientoReserva movimiento,
    IActualizarReserva actualizar)
{
    public const string Ruta = "Mid/MidReservas.cs";

    public static readonly IReadOnlyList<AccionApi> Opciones =
    [
        Accion("listar", "Listar", "sp_Reserva_Buscar", "Devuelve las reservas del campamento de la sesión."),
        Accion("catalogo", "Catálogo", "sp_Reserva_ListarCatalogo", "Devuelve grupos, orígenes, pasajeros, habitaciones y turnos."),
        Accion("crear", "Crear", "dbo.sp_Reserva_Crear", "Inserta una reserva."),
        Accion("editar", "Editar", "dbo.sp_Reserva_Actualizar", "Actualiza una reserva."),
        Accion("movimiento", "Movimiento", "dbo.sp_Reserva_RegistrarMovimiento", "Registra check-in o check-out."),
        Accion("importar", "Importar", "dbo.sp_Reserva_Crear", "Crea reservas desde una planilla."),
        Accion("plantilla", "Plantilla", "planilla", "Devuelve la planilla de importación.")
    ];

    public async Task<EjecucionMid> EjecutarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        switch (filtro.Opcion)
        {
            case "listar":
                return await ListarAsync(filtro, sesion, cancellationToken);
            case "catalogo":
                return await CatalogoAsync(sesion, cancellationToken);
            case "crear":
                return await CrearAsync(filtro, sesion, cancellationToken);
            case "editar":
                return await EditarAsync(filtro, sesion, cancellationToken);
            case "movimiento":
                return await MovimientoAsync(filtro, sesion, cancellationToken);
            case "plantilla":
                return EjecucionMid.Descarga(
                    planilla.CrearPlantilla(),
                    "PlantillaReservas.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            default:
                return EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está implementada en el Mid.");
        }
    }

    public async Task<EjecucionMid> ImportarAsync(Stream archivo, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        var resultado = await importar.EjecutarAsync(archivo, sesion.EmpresaMandanteId, sesion.CampamentoId, sesion.UsuarioId, cancellationToken);
        return EjecucionMid.Ok(resultado);
    }

    private async Task<EjecucionMid> ListarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var fechaReserva = FechaDia(LecturaPeticion.Texto(filtro, "fechaReserva"));
        var fechaReservaDesde = FechaDia(LecturaPeticion.Texto(filtro, "fechaReservaDesde"));
        var fechaReservaHasta = FechaDia(LecturaPeticion.Texto(filtro, "fechaReservaHasta"));
        var fechaLlegada = FechaDia(LecturaPeticion.Texto(filtro, "fechaLlegada"));
        var fechaSalida = FechaDia(LecturaPeticion.Texto(filtro, "fechaSalida"));
        var checkIn = FechaDia(LecturaPeticion.Texto(filtro, "checkIn"));
        var checkOut = FechaDia(LecturaPeticion.Texto(filtro, "checkOut"));

        if (ReglasConsultaListado.RangoReservaVacío(
                fechaReserva, fechaReservaDesde, fechaReservaHasta, fechaLlegada, fechaSalida, checkIn, checkOut)
            || (fechaReservaDesde is null && fechaReservaHasta is null && fechaReserva is null))
        {
            var rango = ReglasConsultaListado.NormalizarRango(null, null);
            fechaReservaDesde = rango.Desde;
            fechaReservaHasta = rango.Hasta;
        }
        else if (fechaReservaDesde is null || fechaReservaHasta is null)
        {
            var rango = ReglasConsultaListado.NormalizarRango(fechaReservaDesde, fechaReservaHasta);
            fechaReservaDesde = rango.Desde;
            fechaReservaHasta = rango.Hasta;
        }

        var resultado = await buscar.EjecutarAsync(new SolicitudBuscarReservas(
            sesion.EmpresaMandanteId,
            sesion.CampamentoId,
            LecturaPeticion.Entero(filtro, "id"),
            LecturaPeticion.Entero(filtro, "areaId"),
            LecturaPeticion.Texto(filtro, "origen") ?? "0",
            LecturaPeticion.Entero(filtro, "solicitudId"),
            LecturaPeticion.Texto(filtro, "estado") ?? "0",
            fechaReserva,
            fechaReservaDesde,
            fechaReservaHasta,
            fechaLlegada,
            fechaSalida,
            checkIn,
            checkOut,
            LecturaPeticion.Entero(filtro, "pasajeroId"),
            LecturaPeticion.Entero(filtro, "habitacionId"),
            string.IsNullOrWhiteSpace(LecturaPeticion.Texto(filtro, "tieneAgua")) ? "0" : LecturaPeticion.Texto(filtro, "tieneAgua")!), cancellationToken);

        return resultado switch
        {
            ResultadoBusquedaReservas.Lista lista => EjecucionMid.Ok(new { filas = lista.Filas }),
            ResultadoBusquedaReservas.Rechazada rechazada => EjecucionMid.Fallo(400, rechazada.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo consultar las reservas.")
        };
    }

    private async Task<EjecucionMid> CatalogoAsync(DatosSesion sesion, CancellationToken cancellationToken)
    {
        var resultado = await catalogo.EjecutarAsync(sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return resultado switch
        {
            ResultadoCatalogoReserva.Listo listo => EjecucionMid.Ok(listo.Catalogo),
            ResultadoCatalogoReserva.Rechazado rechazado => EjecucionMid.Fallo(400, rechazado.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo consultar el catálogo.")
        };
    }

    private async Task<EjecucionMid> CrearAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        if (!FechaHora(LecturaPeticion.Texto(filtro, "fechaReserva"), out var reserva)
            || !FechaHora(LecturaPeticion.Texto(filtro, "fechaLlegada"), out var llegada))
            return EjecucionMid.Fallo(400, "Ingrese la fecha de reserva y la fecha de llegada.");

        var resultado = await crear.EjecutarAsync(new SolicitudCrearReserva(
            sesion.EmpresaMandanteId,
            sesion.CampamentoId,
            sesion.UsuarioId,
            LecturaPeticion.Entero(filtro, "areaId"),
            LecturaPeticion.Entero(filtro, "turnoId"),
            reserva,
            llegada,
            LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fechaSalida")),
            LecturaPeticion.Texto(filtro, "observacion")), cancellationToken);

        return resultado switch
        {
            ResultadoAltaReserva.Creada creada => EjecucionMid.Ok(new { id = creada.Id }),
            ResultadoAltaReserva.Rechazada rechazada => EjecucionMid.Fallo(400, rechazada.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo crear la reserva.")
        };
    }

    private async Task<EjecucionMid> EditarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        if (!FechaHora(LecturaPeticion.Texto(filtro, "fechaReserva"), out var reserva)
            || !FechaHora(LecturaPeticion.Texto(filtro, "fechaLlegada"), out var llegada))
            return EjecucionMid.Fallo(400, "Ingrese las fechas obligatorias.");

        var resultado = await actualizar.EjecutarAsync(new SolicitudActualizarReserva(
            LecturaPeticion.Entero(filtro, "reservaId"),
            sesion.EmpresaMandanteId,
            sesion.CampamentoId,
            LecturaPeticion.Entero(filtro, "areaId"),
            reserva,
            llegada,
            LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fechaSalida")),
            LecturaPeticion.Texto(filtro, "observacion"),
            LecturaPeticion.Entero(filtro, "turnoId"),
            LecturaPeticion.EnteroOpcional(filtro, "pasajeroId"),
            LecturaPeticion.EnteroOpcional(filtro, "habitacionId"),
            LecturaPeticion.Texto(filtro, "estado") ?? "P",
            LecturaPeticion.Bandera(filtro, "tieneAgua"),
            sesion.UsuarioId), cancellationToken);

        return resultado switch
        {
            ResultadoActualizacionReserva.Actualizada => EjecucionMid.Ok(new { listo = true }),
            ResultadoActualizacionReserva.Rechazada rechazada => EjecucionMid.Fallo(400, rechazada.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo actualizar la reserva.")
        };
    }

    private async Task<EjecucionMid> MovimientoAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var fecha = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fecha"));
        if (fecha is null)
            return EjecucionMid.Fallo(400, "Seleccione la fecha.");

        var resultado = await movimiento.EjecutarAsync(new SolicitudMovimientoReserva(
            LecturaPeticion.Entero(filtro, "reservaId"),
            fecha.Value,
            sesion.UsuarioId,
            LecturaPeticion.Texto(filtro, "operacion") ?? string.Empty,
            sesion.EmpresaMandanteId,
            sesion.CampamentoId), cancellationToken);

        return resultado switch
        {
            ResultadoMovimientoReserva.Registrado => EjecucionMid.Ok(new { listo = true }),
            ResultadoMovimientoReserva.Rechazado rechazado => EjecucionMid.Fallo(400, rechazado.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo registrar el movimiento.")
        };
    }

    private static AccionApi Accion(string opcion, string etiqueta, string procedimiento, string descripcion) => new()
    {
        Id = opcion,
        Opcion = opcion,
        Etiqueta = etiqueta,
        Mid = Ruta,
        Procedimiento = procedimiento,
        Descripcion = descripcion
    };

    private static DateTime? FechaDia(string? valor) =>
        DateTime.TryParse(valor, out var fecha) ? fecha.Date : null;

    private static bool FechaHora(string? valor, out DateTime fecha) =>
        DateTime.TryParse(valor, out fecha);
}
