namespace SGH.Domain.Cuenta.Interfaces;

public interface IComandoAutenticacion
{
    Task<int> ActualizarPasswordAsync(string rut, string hashBcrypt, string? huellaMd5, CancellationToken cancellationToken);

    Task<int> ReemplazarHashAsync(string rut, string hashBcrypt, CancellationToken cancellationToken);

    Task<int> RetirarMd5Async(string rut, CancellationToken cancellationToken);
}
