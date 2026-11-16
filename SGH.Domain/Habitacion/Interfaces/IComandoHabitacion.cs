namespace SGH.Domain.Habitacion.Interfaces;

public sealed record DatosGuardarHabitacion(
    int Id,
    int ModuloId,
    string Nombre,
    bool IsActive,
    int CantidadCamas,
    string Sexo,
    int TipoInstalacionId,
    int StandardHabitacionId,
    string? Estado,
    string? ObservacionBloqueo,
    decimal? M2,
    bool IsReservable,
    int PersonasCantidadMin,
    int PersonasCantidadMax,
    int MinutosMin,
    int MinutosMax,
    bool TieneServiciosAlimentacion,
    bool UsoPorTurnos,
    bool UsoExclusivo,
    int UsuarioId);

public sealed record ResultadoGuardarHabitacion(bool Exito, string Mensaje, int Id);

public interface IComandoHabitacion
{
    Task<ResultadoGuardarHabitacion> GuardarAsync(DatosGuardarHabitacion datos, int campamentoId, CancellationToken cancellationToken);
}