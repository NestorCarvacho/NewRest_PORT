using System.Text.Json;
using SGH.Application.Censo.DTOs;
using SGH.Application.Censo.Interfaces;
using SGH.Application.Sesion;

namespace SGH.Application.Mid;

public sealed class MidCenso(
    IBuscarCensos buscar,
    IBuscarMisCensos buscarMios,
    IObtenerCatalogoCenso catalogo,
    IGuardarCensos guardar)
{
    public const string Ruta = "Mid/MidCenso.cs";

    public static readonly IReadOnlyList<AccionApi> Opciones =
    [
        Accion("listar", "Listar", "sp_Censo_Buscar", "Devuelve el censo del campamento."),
        Accion("miscensos", "Mis censos", "sp_Censo_BuscarPorUsuario", "Devuelve los censos del usuario."),
        Accion("catalogo", "Catálogo", "sp_Censo_ListarCatalogo", "Devuelve módulos, pisos y habitaciones."),
        Accion("guardar", "Guardar", "sp_Censo_Guardar", "Guarda las filas del censo."),
        Accion("guardarqr", "Guardar QR", "sp_Censo_Guardar", "Guarda el censo leído desde un código QR.")
    ];

    public async Task<EjecucionMid> EjecutarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var bloqueo = AlcanceMid.BloquearSiSinAlcance(sesion);
        if (bloqueo is not null)
            return bloqueo;

        return filtro.Opcion switch
        {
            "listar" => await ListarAsync(filtro, sesion, cancellationToken),
            "miscensos" => await MiosAsync(filtro, sesion, cancellationToken),
            "catalogo" => await CatalogoAsync(sesion, cancellationToken),
            "guardar" => await GuardarAsync(filtro, sesion, cancellationToken),
            "guardarqr" => await GuardarQrAsync(filtro, sesion, cancellationToken),
            _ => EjecucionMid.Fallo(400, $"La opción '{filtro.Opcion}' no está implementada en el Mid.")
        };
    }

    private async Task<EjecucionMid> ListarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var resultado = await buscar.EjecutarAsync(new SolicitudBuscarCensos(
            sesion.EmpresaMandanteId,
            sesion.CampamentoId,
            LecturaPeticion.Entero(filtro, "moduloId"),
            LecturaPeticion.Entero(filtro, "pisoId"),
            LecturaPeticion.Entero(filtro, "habitacionId"),
            LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fecha"))?.Date,
            LecturaPeticion.Texto(filtro, "turno")), cancellationToken);

        return resultado switch
        {
            ResultadoBusquedaCenso.Lista lista => EjecucionMid.Ok(new { filas = lista.Filas }),
            ResultadoBusquedaCenso.Rechazada rechazada => EjecucionMid.Fallo(400, rechazada.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo consultar el censo.")
        };
    }

    private async Task<EjecucionMid> MiosAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var fecha = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fecha"));
        if (fecha is null)
            return EjecucionMid.Fallo(400, "Debe ingresar la fecha de censo.");

        var resultado = await buscarMios.EjecutarAsync(new SolicitudBuscarMisCensos(
            sesion.EmpresaMandanteId, sesion.CampamentoId, fecha.Value.Date, sesion.UsuarioId), cancellationToken);

        return resultado switch
        {
            ResultadoBusquedaMisCensos.Lista lista => EjecucionMid.Ok(new { filas = lista.Filas }),
            ResultadoBusquedaMisCensos.Rechazada rechazada => EjecucionMid.Fallo(400, rechazada.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudieron consultar mis censos.")
        };
    }

    private async Task<EjecucionMid> CatalogoAsync(DatosSesion sesion, CancellationToken cancellationToken)
    {
        var resultado = await catalogo.EjecutarAsync(sesion.EmpresaMandanteId, sesion.CampamentoId, cancellationToken);
        return resultado switch
        {
            ResultadoCatalogoCenso.Listo listo => EjecucionMid.Ok(listo.Catalogo),
            ResultadoCatalogoCenso.Rechazado rechazado => EjecucionMid.Fallo(400, rechazado.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo consultar el catálogo.")
        };
    }

    private async Task<EjecucionMid> GuardarAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var fecha = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fecha"));
        if (fecha is null)
            return EjecucionMid.Fallo(400, "Debe ingresar la fecha de censo.");

        var filas = LeerFilas(filtro);
        var resultado = await guardar.EjecutarAsync(
            new SolicitudGuardarCensos(sesion.UsuarioId, sesion.EmpresaMandanteId, sesion.CampamentoId, fecha.Value, filas), cancellationToken);

        return resultado switch
        {
            ResultadoGuardadoCenso.Guardado => EjecucionMid.Ok(new { listo = true }),
            ResultadoGuardadoCenso.Rechazado rechazado => EjecucionMid.Fallo(400, rechazado.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo guardar el censo.")
        };
    }

    private Task<EjecucionMid> GuardarQrAsync(FiltroPeticion filtro, DatosSesion sesion, CancellationToken cancellationToken)
    {
        var habitacion = LecturaPeticion.Texto(filtro, "habitacionId");
        var cantidad = LecturaPeticion.Texto(filtro, "cantidad");
        var turno = LecturaPeticion.Texto(filtro, "turno");
        if (!int.TryParse(habitacion, out var habitacionId) || habitacionId <= 0)
            return Task.FromResult(EjecucionMid.Fallo(400, "El código QR no contiene una habitación válida."));
        if (!int.TryParse(cantidad, out var camas) || camas < 0)
            return Task.FromResult(EjecucionMid.Fallo(400, "La cantidad de camas no es válida."));
        if (turno is not ("D" or "N" or "A"))
            return Task.FromResult(EjecucionMid.Fallo(400, "Seleccione un turno válido."));

        var noche = LecturaPeticion.Texto(filtro, "cantidadNoche");
        if (turno == "A" && (!int.TryParse(noche, out var cantidadNoche) || cantidadNoche < 0))
            return Task.FromResult(EjecucionMid.Fallo(400, "La cantidad de camas nocturnas no es válida."));

        var filas = new List<FilaGuardarCensoDto>
        {
            new(string.Empty, habitacionId, camas.ToString(), turno == "A" ? "D" : turno)
        };
        if (turno == "A")
            filas.Add(new FilaGuardarCensoDto(string.Empty, habitacionId, noche ?? "0", "N"));

        var fecha = LecturaPeticion.Fecha(LecturaPeticion.Texto(filtro, "fecha"));
        if (fecha is null)
            return Task.FromResult(EjecucionMid.Fallo(400, "Debe ingresar la fecha de censo."));

        return GuardarListaAsync(sesion, fecha.Value, filas, cancellationToken);
    }

    private async Task<EjecucionMid> GuardarListaAsync(DatosSesion sesion, DateTime fecha, IReadOnlyList<FilaGuardarCensoDto> filas, CancellationToken cancellationToken)
    {
        var resultado = await guardar.EjecutarAsync(
            new SolicitudGuardarCensos(sesion.UsuarioId, sesion.EmpresaMandanteId, sesion.CampamentoId, fecha, filas), cancellationToken);
        return resultado switch
        {
            ResultadoGuardadoCenso.Guardado => EjecucionMid.Ok(new { listo = true }),
            ResultadoGuardadoCenso.Rechazado rechazado => EjecucionMid.Fallo(400, rechazado.Mensaje),
            _ => EjecucionMid.Fallo(400, "No se pudo guardar el censo.")
        };
    }

    private static List<FilaGuardarCensoDto> LeerFilas(FiltroPeticion filtro)
    {
        var lista = LecturaPeticion.Lista(filtro, "filas");
        if (lista is null)
            return [];

        var filas = new List<FilaGuardarCensoDto>();
        foreach (var item in lista.Value.EnumerateArray())
        {
            filas.Add(new FilaGuardarCensoDto(
                Texto(item, "id"),
                Entero(item, "habitacionId"),
                Texto(item, "cantidad"),
                Texto(item, "turno")));
        }

        return filas;
    }

    private static string? Texto(JsonElement item, string nombre)
    {
        if (!item.TryGetProperty(nombre, out var valor) || valor.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        return valor.ValueKind == JsonValueKind.String ? valor.GetString() : valor.ToString();
    }

    private static int Entero(JsonElement item, string nombre) =>
        item.TryGetProperty(nombre, out var valor) && valor.TryGetInt32(out var numero) ? numero : 0;

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
