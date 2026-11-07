using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Opciones;
using SGH.Application.Cuenta.UseCases;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Entities;
using SGH.Infrastructure.Cuenta.Security;

namespace SGH.Tests.Cuenta;

public class CambiarContrasenaTests
{
    private readonly RepositorioFalso _repositorio = new();
    private readonly VerificadorContrasena _verificador = new(new OpcionesContrasena { FactorTrabajoBcrypt = 4 });

    [Fact]
    public async Task FormatoInvalidoNoConsultaAlUsuario()
    {
        var resultado = await Crear().EjecutarAsync(
            new SolicitudCambioContrasena("123456789", "abc123", "aa", "aa"),
            CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoCambioContrasena.Rechazado>(resultado);
        Assert.Equal("Clave debe ser mayor a 6", rechazo.Mensaje);
        Assert.Equal(0, _repositorio.Actualizaciones);
    }

    [Fact]
    public async Task ConfirmacionDistintaUsaElMensajeDeLaPantallaLegada()
    {
        var resultado = await Crear().EjecutarAsync(
            new SolicitudCambioContrasena("123456789", "abc123", "abcd1234", ""),
            CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoCambioContrasena.Rechazado>(resultado);
        Assert.Equal("Password debe ser la misma", rechazo.Mensaje);
    }

    [Fact]
    public async Task ClaveActualIncorrectaSeRechaza()
    {
        _repositorio.Usuario = Usuario("abc123");

        var resultado = await Crear().EjecutarAsync(
            new SolicitudCambioContrasena("123456789", "zzzz123", "abcd1234", "abcd1234"),
            CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoCambioContrasena.Rechazado>(resultado);
        Assert.Equal(ReglasCuenta.CredencialesInvalidas, rechazo.Mensaje);
    }

    [Fact]
    public async Task RepetirLaClaveVigenteInformaReutilizacionSinLlamarAlProcedimiento()
    {
        _repositorio.Usuario = Usuario("abc123");

        var resultado = await Crear().EjecutarAsync(
            new SolicitudCambioContrasena("123456789", "abc123", "abc123", "abc123"),
            CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoCambioContrasena.Rechazado>(resultado);
        Assert.Equal(MotivoRechazo.ClaveReutilizada, rechazo.Motivo);
        Assert.Equal(0, _repositorio.Actualizaciones);
    }

    [Fact]
    public async Task CodigoMenosTresInformaClaveReutilizada()
    {
        _repositorio.Usuario = Usuario("abc123");
        _repositorio.CodigoActualizacion = CodigoCambioClave.Reutilizada;

        var resultado = await Crear().EjecutarAsync(
            new SolicitudCambioContrasena("123456789", "abc123", "abcd1234", "abcd1234"),
            CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoCambioContrasena.Rechazado>(resultado);
        Assert.Equal(ReglasCuenta.ClaveReutilizada, rechazo.Mensaje);
    }

    [Fact]
    public async Task CambioValidoGuardaBcryptYLaHuellaMd5ParaElHistorico()
    {
        _repositorio.Usuario = Usuario("abc123");

        var resultado = await Crear().EjecutarAsync(
            new SolicitudCambioContrasena("12345678-9", "abc123", "abcd1234", "abcd1234"),
            CancellationToken.None);

        Assert.IsType<ResultadoCambioContrasena.Actualizada>(resultado);
        Assert.StartsWith("$2", _repositorio.HashGuardado, StringComparison.Ordinal);
        Assert.Equal(_verificador.CrearHuellaMd5Legado("abcd1234"), _repositorio.HuellaGuardada);
    }

    [Fact]
    public async Task CambioConMd5RetiradoNoEnviaHuellaMd5()
    {
        var hash = _verificador.CrearHashBcrypt("abc123");
        _repositorio.Usuario = new UsuarioLogin(3, "123456789", "Ana", "Soto", hash, 2, 9, true);

        var resultado = await Crear().EjecutarAsync(
            new SolicitudCambioContrasena("123456789", "abc123", "abcd1234", "abcd1234"),
            CancellationToken.None);

        Assert.IsType<ResultadoCambioContrasena.Actualizada>(resultado);
        Assert.Null(_repositorio.HuellaGuardada);
    }

    private CambiarContrasena Crear() => new(_repositorio, _repositorio, _verificador);

    private UsuarioLogin Usuario(string clave) =>
        new(3, "123456789", "Ana", "Soto", _verificador.CrearHuellaMd5Legado(clave), 2, 9, false);
}
