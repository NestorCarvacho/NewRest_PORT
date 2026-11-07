using Microsoft.Extensions.Logging;
using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Interfaces;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Entities;
using SGH.Domain.Cuenta.Interfaces;

namespace SGH.Application.Cuenta.UseCases;

public sealed class VerificacionClaveUsuario : IVerificacionClaveUsuario
{
    private readonly IConsultaAutenticacion _consulta;
    private readonly IComandoAutenticacion _comando;
    private readonly IVerificadorContrasena _verificador;
    private readonly ILogger<VerificacionClaveUsuario> _logger;

    public VerificacionClaveUsuario(
        IConsultaAutenticacion consulta,
        IComandoAutenticacion comando,
        IVerificadorContrasena verificador,
        ILogger<VerificacionClaveUsuario> logger)
    {
        _consulta = consulta;
        _comando = comando;
        _verificador = verificador;
        _logger = logger;
    }

    public async Task<ResultadoVerificacionClave> EjecutarAsync(
        string rut,
        string contrasena,
        int loginInterno,
        CancellationToken cancellationToken)
    {
        var rutNormalizado = NormalizadorRut.Normalizar(rut);
        if (rutNormalizado.Length == 0 || string.IsNullOrEmpty(contrasena))
        {
            return new ResultadoVerificacionClave.Rechazada(ReglasCuenta.CredencialesInvalidas, MotivoRechazo.CredencialesInvalidas);
        }

        var usuario = await _consulta.ObtenerParaLoginAsync(rutNormalizado, loginInterno, cancellationToken);
        var rechazo = await RechazoDeClaveAsync(
            usuario,
            usuario is not null && _verificador.Coincide(contrasena, usuario.HashContrasena, usuario.Md5Retirado),
            rutNormalizado,
            cancellationToken);
        if (rechazo is not null || usuario is null)
            return rechazo ?? new ResultadoVerificacionClave.Rechazada(ReglasCuenta.CredencialesInvalidas, MotivoRechazo.CredencialesInvalidas);

        var estado = await _consulta.ObtenerEstadoAsync(rutNormalizado, cancellationToken);
        if (estado == CodigoEstadoUsuario.ClaveCaducada)
        {
            return new ResultadoVerificacionClave.Rechazada(ReglasCuenta.ClaveCaducada, MotivoRechazo.ClaveCaducada);
        }

        if (estado != CodigoEstadoUsuario.Vigente)
        {
            return new ResultadoVerificacionClave.Rechazada(ReglasCuenta.ErrorEstado, MotivoRechazo.EstadoInvalido);
        }

        if (!usuario.Md5Retirado && _verificador.EsHashMd5Legado(usuario.HashContrasena))
        {
            await ReemplazarHashLegadoAsync(usuario.Id, rutNormalizado, contrasena, cancellationToken);
        }
        else if (!usuario.Md5Retirado && _verificador.EsHashBcrypt(usuario.HashContrasena))
        {
            await MarcarMd5RetiradoAsync(usuario.Id, rutNormalizado, cancellationToken);
        }

        return new ResultadoVerificacionClave.Valida(usuario, rutNormalizado);
    }

    private async Task<ResultadoVerificacionClave?> RechazoDeClaveAsync(
        UsuarioLogin? usuario,
        bool claveCoincide,
        string rut,
        CancellationToken cancellationToken)
    {
        if (usuario is null)
            return new ResultadoVerificacionClave.Rechazada(ReglasCuenta.CredencialesInvalidas, MotivoRechazo.CredencialesInvalidas);

        if (usuario.Bloqueada)
            return new ResultadoVerificacionClave.Rechazada(ReglasCuenta.MensajeCuentaBloqueada, MotivoRechazo.CuentaBloqueada);

        if (!claveCoincide)
        {
            var intento = await _comando.RegistrarIntentoFallidoAsync(rut, cancellationToken);
            var mensaje = intento.Bloqueada
                ? ReglasCuenta.MensajeCuentaBloqueada
                : ReglasCuenta.MensajeIntentosRestantes(ReglasCuenta.IntentosAntesDeBloquear - intento.IntentosFallidos);
            var motivo = intento.Bloqueada ? MotivoRechazo.CuentaBloqueada : MotivoRechazo.CredencialesInvalidas;
            return new ResultadoVerificacionClave.Rechazada(mensaje, motivo);
        }

        await _comando.ReiniciarIntentosAsync(rut, cancellationToken);
        return null;
    }

    private async Task MarcarMd5RetiradoAsync(int usuarioId, string rut, CancellationToken cancellationToken)
    {
        try
        {
            var codigo = await _comando.RetirarMd5Async(rut, cancellationToken);
            if (codigo != CodigoCambioClave.Exito)
            {
                _logger.LogWarning(
                    "No se marcó MD5 como retirado para el usuario {UsuarioId}. Código {Codigo}.",
                    usuarioId,
                    codigo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se marcó MD5 como retirado para el usuario {UsuarioId}.", usuarioId);
        }
    }

    private async Task ReemplazarHashLegadoAsync(int usuarioId, string rut, string contrasena, CancellationToken cancellationToken)
    {
        try
        {
            var codigo = await _comando.ReemplazarHashAsync(rut, _verificador.CrearHashBcrypt(contrasena), cancellationToken);
            if (codigo != CodigoCambioClave.Exito)
            {
                _logger.LogWarning(
                    "No se reemplazó el hash MD5 del usuario {UsuarioId}. Código {Codigo}.",
                    usuarioId,
                    codigo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se reemplazó el hash MD5 del usuario {UsuarioId}.", usuarioId);
        }
    }
}
