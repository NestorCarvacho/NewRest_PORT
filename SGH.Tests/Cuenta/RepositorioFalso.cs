using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Entities;
using SGH.Domain.Cuenta.Interfaces;

namespace SGH.Tests.Cuenta;

internal sealed class RepositorioFalso : IConsultaAutenticacion, IComandoAutenticacion
{
    public UsuarioLogin? Usuario { get; set; }
    public int Estado { get; set; }
    public List<PerfilCampamentoUsuario> Perfiles { get; } = [];
    public int CodigoActualizacion { get; set; }
    public int CodigoReemplazo { get; set; } = -1;
    public int ConsultasEstado { get; private set; }
    public int Reemplazos { get; private set; }
    public int Retiros { get; private set; }
    public int Actualizaciones { get; private set; }
    public int Intentos { get; private set; }
    public int Reinicios { get; private set; }
    public int LoginInternoRecibido { get; private set; }
    public string? HashGuardado { get; private set; }
    public string? HuellaGuardada { get; private set; }

    public Task<UsuarioLogin?> ObtenerParaLoginAsync(string rut, int loginInterno, CancellationToken cancellationToken)
    {
        LoginInternoRecibido = loginInterno;
        var coincide = Usuario is not null && string.Equals(Usuario.Rut, rut, StringComparison.Ordinal);
        return Task.FromResult(coincide ? Usuario : null);
    }

    public Task<int> ObtenerEstadoAsync(string rut, CancellationToken cancellationToken)
    {
        ConsultasEstado++;
        return Task.FromResult(Estado);
    }

    public Task<IReadOnlyList<PerfilCampamentoUsuario>> ObtenerCampamentosYPerfilesAsync(string rut, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<PerfilCampamentoUsuario>>(Perfiles);
    }

    public Task<ResultadoIntentoClave> RegistrarIntentoFallidoAsync(string rut, CancellationToken cancellationToken)
    {
        Intentos++;
        return Task.FromResult(new ResultadoIntentoClave(Intentos, Intentos >= ReglasCuenta.IntentosAntesDeBloquear));
    }

    public Task ReiniciarIntentosAsync(string rut, CancellationToken cancellationToken)
    {
        Reinicios++;
        Intentos = 0;
        return Task.CompletedTask;
    }

    public Task<int> ActualizarPasswordAsync(string rut, string hashBcrypt, string? huellaMd5, CancellationToken cancellationToken)
    {
        Actualizaciones++;
        HashGuardado = hashBcrypt;
        HuellaGuardada = huellaMd5;
        return Task.FromResult(CodigoActualizacion);
    }

    public Task<int> ReemplazarHashAsync(string rut, string hashBcrypt, CancellationToken cancellationToken)
    {
        Reemplazos++;
        HashGuardado = hashBcrypt;
        return Task.FromResult(CodigoReemplazo);
    }

    public Task<int> RetirarMd5Async(string rut, CancellationToken cancellationToken)
    {
        Retiros++;
        return Task.FromResult(CodigoReemplazo);
    }
}
