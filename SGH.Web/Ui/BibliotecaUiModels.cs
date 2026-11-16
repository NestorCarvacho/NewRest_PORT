namespace SGH.Web.Ui;

public sealed class ModuloEscritorioVm(string moduloId, string? tituloBienvenida = null)
{
    public string ModuloId { get; } = moduloId;
    public string TituloBienvenida { get; } = tituloBienvenida ?? "Gestión Hotelera";
}

public sealed class TablaListadoVm
{
    public string ContenedorId { get; init; } = "tabla-listado";
    public IReadOnlyList<string> Columnas { get; init; } = [];
    public string ClaseContenedor { get; init; } = "sgh-ui-tabla";
    public bool IncluirPaginacionCliente { get; init; } = true;
}

public sealed class BarraModuloVm
{
    public string ModuloId { get; init; } = "";
    public IReadOnlyList<BotonBarraVm> Botones { get; init; } = [];
}

public sealed class BotonBarraVm(string texto, string clase, string? dataAccion = null)
{
    public string Texto { get; } = texto;
    public string Clase { get; } = clase;
    public string? DataAccion { get; } = dataAccion;
}

public sealed class ModalDialogoVm
{
    public string ModalId { get; init; } = "sgh-modal-confirmacion";
    public string Titulo { get; init; } = "Confirmar";
    public string TextoConfirmar { get; init; } = "Aceptar";
    public string TextoCancelar { get; init; } = "Cancelar";
}

public sealed class CampoFormularioVm
{
    public string Nombre { get; init; } = "";
    public string Etiqueta { get; init; } = "";
    public string Tipo { get; init; } = "text";
    public bool Requerido { get; init; }
    public string? Valor { get; init; }
    public string? MensajeAyuda { get; init; }
    public int? MaxLength { get; init; }
}

public static class BibliotecaUiClases
{
    public const string Modulo = "modulo";
    public const string Tabla = "sgh-ui-tabla";
    public const string Campo = "sgh-ui-campo";
    public const string Validacion = "sgh-ui-validacion";
    public const string Modal = "sgh-ui-modal";
}