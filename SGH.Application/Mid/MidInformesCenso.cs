using SGH.Application.Censo.Interfaces;
using SGH.Application.Sesion;
using SGH.Application.Transversal;
using SGH.Domain.Censo.Interfaces;

namespace SGH.Application.Mid;

public sealed class MidInformesCenso(
    IConsultarGestionDiaria gestionDiaria,
    IGenerarInformeGeneral informeGeneral,
    IGenerarInformeOcupabilidadCenso ocupabilidad,
    IConsultarEstadoPago estadoPago,
    IConsultarEstadoPagoPantalla estadoPagoPantalla,
    IConsultarEstadoPagoServicio estadoPagoServicio,
    IConsultarEstadoPagoServicioPantalla estadoPagoServicioPantalla,
    IExportadorHojaCalculo exportadorHojaCalculo)
{
    public const string Ruta = "Mid/MidInformesCenso.cs";

    public static readonly IReadOnlyList<AccionApi> Opciones =
    [
        Accion("gestiondiaria", "Gestión diaria", "INF_GestionDia", "Consulta la gestión diaria del censo."),
        Accion("informegeneral", "Informe general", "INF_Censo", "Descarga el informe general en Excel."),
        Accion("ocupabilidad", "Ocupabilidad", "INF_Ocupabilidad", "Consulta la ocupabilidad del censo."),
        Accion("consultar", "Consultar", "INF_EstadoPagoPantalla", "Estado de pago consolidado."),
        Accion("descargar", "Descargar", "INF_EstadoPago", "Excel del estado de pago."),
        Accion("consultarservicio", "Consultar servicio", "INF_EstadoPagoServicioPantalla", "Estado de pago por servicio."),
        Accion("descargarservicio", "Descargar servicio", "INF_EstadoPagoServicio", "Excel por servicio.")
    ];

    public async Task<EjecucionMid> EjecutarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        return filtro.Opcion switch
        {
            "gestiondiaria" => await GestionDiariaAsync(filtro, sesion, cancellationToken),
            "informegeneral" => await InformeGeneralAsync(filtro, sesion, cancellationToken),
            "ocupabilidad" => await OcupabilidadAsync(filtro, sesion, cancellationToken),
            "consultar" => await ConsultarEstadoPagoAsync(filtro, sesion, descargar: false, cancellationToken),
            "descargar" => await ConsultarEstadoPagoAsync(filtro, sesion, descargar: true, cancellationToken),
            "consultarservicio" => await ConsultarEstadoPagoServicioAsync(filtro, sesion, descargar: false, cancellationToken),
            "descargarservicio" => await ConsultarEstadoPagoServicioAsync(filtro, sesion, descargar: true, cancellationToken),
            _ => EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está implementada en el Mid.")
        };
    }

    private async Task<EjecucionMid> GestionDiariaAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var fecha = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fecha"));
        var turno = LecturaPeticion.Texto(filtro, "turno");
        if (fecha is null || turno is not ("D" or "N"))
            return EjecucionMid.Fallo(400, "Debe ingresar una fecha y un turno válido.");

        var resultado = await gestionDiaria.EjecutarAsync(
            new FiltroGestionDiaria(sesion.EmpresaMandanteId, sesion.CampamentoId, turno, fecha.Value.Date), cancellationToken);
        return EjecucionMid.Ok(new { filas = resultado.Filas });
    }

    private async Task<EjecucionMid> InformeGeneralAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var desde = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "desde"));
        var hasta = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "hasta"));
        if (desde is null || hasta is null)
            return EjecucionMid.Fallo(400, "Debe ingresar un período válido.");

        var resultado = await informeGeneral.EjecutarAsync(new FiltroInformeGeneral(
            LecturaPeticion.Texto(filtro, "tipo") ?? "C",
            sesion.CampamentoId,
            LecturaPeticion.Entero(filtro, "moduloId"),
            LecturaPeticion.Entero(filtro, "pisoId"),
            LecturaPeticion.Entero(filtro, "habitacionId"),
            desde.Value.Date,
            hasta.Value.Date,
            LecturaPeticion.Texto(filtro, "tipoTurno") ?? "T",
            sesion.EmpresaMandanteId), cancellationToken);

        return EjecucionMid.Descarga(
            exportadorHojaCalculo.CrearXmlSpreadsheet(resultado.Filas, "Censo"),
            "informe-censo.xls",
            "application/vnd.ms-excel");
    }

    private async Task<EjecucionMid> OcupabilidadAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var desde = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "desde"));
        var hasta = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "hasta"));
        if (desde is null || hasta is null)
            return EjecucionMid.Fallo(400, "Debe ingresar un período válido.");

        var resultado = await ocupabilidad.EjecutarAsync(new FiltroOcupabilidadCenso(
            LecturaPeticion.Texto(filtro, "tipo") ?? "C",
            LecturaPeticion.Texto(filtro, "subtipo") ?? string.Empty,
            LecturaPeticion.Texto(filtro, "nivel") ?? "C",
            desde.Value.Date,
            hasta.Value.Date,
            sesion.EmpresaMandanteId,
            sesion.CampamentoId,
            LecturaPeticion.Entero(filtro, "mineraId")), cancellationToken);

        return EjecucionMid.Ok(new { filas = resultado.Filas });
    }

    private async Task<EjecucionMid> ConsultarEstadoPagoAsync(FiltroPeticion filtro, DatosSesion sesion, bool descargar, CancellationToken cancellationToken)
    {
        var desde = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "desde"));
        var hasta = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "hasta"));
        if (desde is null || hasta is null)
            return EjecucionMid.Fallo(400, "Debe ingresar un período válido.");

        var filtroInforme = new FiltroEstadoPago(
            sesion.EmpresaMandanteId, sesion.CampamentoId, desde, hasta, LecturaPeticion.Entero(filtro, "mineraId"));
        var resultado = await (descargar
            ? estadoPago.EjecutarAsync(filtroInforme, cancellationToken)
            : estadoPagoPantalla.EjecutarAsync(filtroInforme, cancellationToken));

        return descargar
            ? EjecucionMid.Descarga(exportadorHojaCalculo.CrearXmlSpreadsheet(resultado.Filas, "EstadoPago"), "EstadoPago.xls", "application/vnd.ms-excel")
            : EjecucionMid.Ok(new { filas = resultado.Filas });
    }

    private async Task<EjecucionMid> ConsultarEstadoPagoServicioAsync(FiltroPeticion filtro, DatosSesion sesion, bool descargar, CancellationToken cancellationToken)
    {
        var desde = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "desde"));
        var hasta = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "hasta"));
        if (desde is null || hasta is null)
            return EjecucionMid.Fallo(400, "Debe ingresar un período válido.");

        var filtroInforme = new FiltroEstadoPagoServicio(
            sesion.EmpresaMandanteId, sesion.CampamentoId, desde, hasta,
            LecturaPeticion.Entero(filtro, "habitacionId"),
            LecturaPeticion.Entero(filtro, "moduloId"),
            LecturaPeticion.Entero(filtro, "pisoId"),
            LecturaPeticion.Texto(filtro, "servicio") ?? "F",
            LecturaPeticion.Entero(filtro, "mineraId"));

        var resultado = await (descargar
            ? estadoPagoServicio.EjecutarAsync(filtroInforme, cancellationToken)
            : estadoPagoServicioPantalla.EjecutarAsync(filtroInforme, cancellationToken));

        return descargar
            ? EjecucionMid.Descarga(exportadorHojaCalculo.CrearXmlSpreadsheet(resultado.Filas, "EstadoPagoServicio"), "EstadoPagoServicio.xls", "application/vnd.ms-excel")
            : EjecucionMid.Ok(new { filas = resultado.Filas });
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
}
