namespace SGH.Domain.Navegacion.Interfaces;

public interface IConsultaMenuPerfil
{
    Task<IReadOnlyList<MenuPerfilItem>> ObtenerPorPerfilAsync(int perfilId, CancellationToken cancellationToken);
}