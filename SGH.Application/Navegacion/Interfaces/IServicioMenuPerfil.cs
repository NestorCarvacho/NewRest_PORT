namespace SGH.Application.Navegacion.Interfaces;

public interface IServicioMenuPerfil
{
    Task<MenuEscritorioVm> ObtenerMenuAsync(int perfilId, CancellationToken cancellationToken);

    Task<bool> TieneAccesoModuloAsync(int perfilId, string claveModulo, CancellationToken cancellationToken);
}