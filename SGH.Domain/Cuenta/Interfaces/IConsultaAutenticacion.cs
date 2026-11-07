using SGH.Domain.Cuenta.Entities;

namespace SGH.Domain.Cuenta.Interfaces;

public interface IConsultaAutenticacion
{
    Task<UsuarioLogin?> ObtenerParaLoginAsync(string rut, int loginInterno, CancellationToken cancellationToken);

    Task<int> ObtenerEstadoAsync(string rut, CancellationToken cancellationToken);

    Task<IReadOnlyList<PerfilCampamentoUsuario>> ObtenerCampamentosYPerfilesAsync(string rut, CancellationToken cancellationToken);
}
