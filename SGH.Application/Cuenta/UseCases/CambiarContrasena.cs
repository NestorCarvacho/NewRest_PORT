using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Interfaces;
using SGH.Application.Cuenta.Validators;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Interfaces;

namespace SGH.Application.Cuenta.UseCases;

public sealed class CambiarContrasena : ICambiarContrasena
{
    private readonly IConsultaAutenticacion _consulta;
    private readonly IComandoAutenticacion _comando;
    private readonly IVerificadorContrasena _verificador;

    public CambiarContrasena(
        IConsultaAutenticacion consulta,
        IComandoAutenticacion comando,
        IVerificadorContrasena verificador)
    {
        _consulta = consulta;
        _comando = comando;
        _verificador = verificador;
    }

    public async Task<ResultadoCambioContrasena> EjecutarAsync(SolicitudCambioContrasena solicitud, CancellationToken cancellationToken)
    {
        var mensaje = ValidadorCambioContrasena.Validar(solicitud.ContrasenaActual, solicitud.ContrasenaNueva, solicitud.Confirmacion);
        if (mensaje is not null)
            return new ResultadoCambioContrasena.Rechazado(mensaje, MotivoRechazo.FormatoContrasena);

        var rut = NormalizadorRut.Normalizar(solicitud.Rut);
        var usuario = await _consulta.ObtenerParaLoginAsync(rut, loginInterno: 0, cancellationToken);
        if (usuario is null)
            return new ResultadoCambioContrasena.Rechazado(ReglasCuenta.CredencialesInvalidas, MotivoRechazo.CredencialesInvalidas);

        if (usuario.Bloqueada)
            return new ResultadoCambioContrasena.Rechazado(ReglasCuenta.MensajeCuentaBloqueada, MotivoRechazo.CuentaBloqueada);

        if (!_verificador.Coincide(solicitud.ContrasenaActual, usuario.HashContrasena, usuario.Md5Retirado))
        {
            var intento = await _comando.RegistrarIntentoFallidoAsync(rut, cancellationToken);
            var aviso = intento.Bloqueada
                ? ReglasCuenta.MensajeCuentaBloqueada
                : ReglasCuenta.MensajeIntentosRestantes(ReglasCuenta.IntentosAntesDeBloquear - intento.IntentosFallidos);
            var motivo = intento.Bloqueada ? MotivoRechazo.CuentaBloqueada : MotivoRechazo.CredencialesInvalidas;
            return new ResultadoCambioContrasena.Rechazado(aviso, motivo);
        }

        await _comando.ReiniciarIntentosAsync(rut, cancellationToken);

        if (string.Equals(solicitud.ContrasenaActual, solicitud.ContrasenaNueva, StringComparison.Ordinal))
        {
            return new ResultadoCambioContrasena.Rechazado(ReglasCuenta.ClaveReutilizada, MotivoRechazo.ClaveReutilizada);
        }

        var historial = await _consulta.ObtenerHistorialContrasenasAsync(rut, cancellationToken);
        if (historial.Any(hash => _verificador.Coincide(solicitud.ContrasenaNueva, hash, md5Retirado: false)))
        {
            return new ResultadoCambioContrasena.Rechazado(ReglasCuenta.ClaveReutilizada, MotivoRechazo.ClaveReutilizada);
        }

        var huellaMd5 = !usuario.Md5Retirado && _verificador.EsHashMd5Legado(usuario.HashContrasena)
            ? _verificador.CrearHuellaMd5Legado(solicitud.ContrasenaNueva)
            : null;

        var codigo = await _comando.ActualizarPasswordAsync(
            rut,
            _verificador.CrearHashBcrypt(solicitud.ContrasenaNueva),
            huellaMd5,
            cancellationToken);

        if (codigo == CodigoCambioClave.Exito)
            return new ResultadoCambioContrasena.Actualizada();

        if (codigo == CodigoCambioClave.Reutilizada)
            return new ResultadoCambioContrasena.Rechazado(ReglasCuenta.ClaveReutilizada, MotivoRechazo.ClaveReutilizada);

        return new ResultadoCambioContrasena.Rechazado(ReglasCuenta.CredencialesInvalidas, MotivoRechazo.CredencialesInvalidas);
    }
}
