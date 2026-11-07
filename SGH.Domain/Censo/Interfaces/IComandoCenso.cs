using SGH.Domain.Censo.Entities;

namespace SGH.Domain.Censo.Interfaces;

public interface IComandoCenso
{
    Task<ResultadoFilaCenso> GuardarAsync(
        FilaCensoGuardar fila,
        DateTime fecha,
        int usuarioId,
        CancellationToken cancellationToken);
}
