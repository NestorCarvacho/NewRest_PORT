using Microsoft.Extensions.Logging.Abstractions;
using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Opciones;
using SGH.Application.Cuenta.UseCases;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Entities;
using SGH.Infrastructure.Cuenta.Security;

namespace SGH.Tests.Cuenta;

public class PreautenticarUsuarioTests
{
    private readonly RepositorioFalso _repositorio = new();
    private readonly VerificadorContrasena _verificador = new(new OpcionesContrasena { FactorTrabajoBcrypt = 4 });

    [Fact]
    public async Task ClaveIncorrectaNoConsultaElEstado()
    {
        _repositorio.Usuario = Usuario("abc123");

        var resultado = await Crear().EjecutarAsync(new SolicitudPreautenticacion("12345678-9", "otra123"), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoPreautenticacion.Rechazado>(resultado);
        Assert.Equal(ReglasCuenta.MensajeIntentosRestantes(4), rechazo.Mensaje);
        Assert.Equal(0, _repositorio.ConsultasEstado);
        Assert.Equal(1, _repositorio.Intentos);
    }

    [Fact]
    public async Task EstadoMenosDosInformaClaveCaducadaYNoReemplazaElHash()
    {
        _repositorio.Usuario = Usuario("abc123");
        _repositorio.Estado = CodigoEstadoUsuario.ClaveCaducada;

        var resultado = await Crear().EjecutarAsync(new SolicitudPreautenticacion("123456789", "abc123"), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoPreautenticacion.Rechazado>(resultado);
        Assert.Equal(MotivoRechazo.ClaveCaducada, rechazo.Motivo);
        Assert.Equal(0, _repositorio.Reemplazos);
    }

    [Fact]
    public async Task OtroEstadoDistintoDeCeroEsError()
    {
        _repositorio.Usuario = Usuario("abc123");
        _repositorio.Estado = 7;

        var resultado = await Crear().EjecutarAsync(new SolicitudPreautenticacion("123456789", "abc123"), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoPreautenticacion.Rechazado>(resultado);
        Assert.Equal(MotivoRechazo.EstadoInvalido, rechazo.Motivo);
    }

    [Fact]
    public async Task HashMd5ValidoSigueAunqueElReemplazoFalle()
    {
        _repositorio.Usuario = Usuario("abc123");
        _repositorio.CodigoReemplazo = CodigoCambioClave.NoActualizada;
        _repositorio.Perfiles.Add(Perfil(4, 15));

        var resultado = await Crear().EjecutarAsync(new SolicitudPreautenticacion("12345678-9", "abc123", 0), CancellationToken.None);

        var listo = Assert.IsType<ResultadoPreautenticacion.Listo>(resultado);
        Assert.Equal("123456789", listo.Rut);
        Assert.Single(listo.Perfiles);
        Assert.Equal(1, _repositorio.Reemplazos);
        Assert.StartsWith("$2", _repositorio.HashGuardado, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HashBcryptNoSeReemplaza()
    {
        var hash = _verificador.CrearHashBcrypt("abc123");
        _repositorio.Usuario = new UsuarioLogin(3, "123456789", "Ana", "Soto", hash, 2, 9, false);

        var resultado = await Crear().EjecutarAsync(new SolicitudPreautenticacion("123456789", "abc123"), CancellationToken.None);

        Assert.IsType<ResultadoPreautenticacion.Listo>(resultado);
        Assert.Equal(0, _repositorio.Reemplazos);
        Assert.Equal(1, _repositorio.Retiros);
    }

    [Fact]
    public async Task HashBcryptConFlagRetiradaNoTocaMd5()
    {
        var hash = _verificador.CrearHashBcrypt("abc123");
        _repositorio.Usuario = new UsuarioLogin(3, "123456789", "Ana", "Soto", hash, 2, 9, true);

        var resultado = await Crear().EjecutarAsync(new SolicitudPreautenticacion("123456789", "abc123"), CancellationToken.None);

        Assert.IsType<ResultadoPreautenticacion.Listo>(resultado);
        Assert.Equal(0, _repositorio.Reemplazos);
        Assert.Equal(0, _repositorio.Retiros);
        Assert.Equal(1, _repositorio.Reinicios);
    }

    [Fact]
    public async Task RutDesconocidoNoCuentaIntento()
    {
        var resultado = await Crear().EjecutarAsync(new SolicitudPreautenticacion("999999999", "abc123"), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoPreautenticacion.Rechazado>(resultado);
        Assert.Equal(ReglasCuenta.CredencialesInvalidas, rechazo.Mensaje);
        Assert.Equal(0, _repositorio.Intentos);
    }

    [Fact]
    public async Task QuintoIntentoBloqueaLaCuenta()
    {
        _repositorio.Usuario = Usuario("abc123");

        ResultadoPreautenticacion? ultimo = null;
        for (var i = 0; i < ReglasCuenta.IntentosAntesDeBloquear; i++)
            ultimo = await Crear().EjecutarAsync(new SolicitudPreautenticacion("123456789", "otra123"), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoPreautenticacion.Rechazado>(ultimo);
        Assert.Equal(ReglasCuenta.MensajeCuentaBloqueada, rechazo.Mensaje);
        Assert.Equal(MotivoRechazo.CuentaBloqueada, rechazo.Motivo);
    }

    [Fact]
    public async Task CuentaBloqueadaNoSigueContando()
    {
        _repositorio.Usuario = new UsuarioLogin(3, "123456789", "Ana", "Soto", _verificador.CrearHuellaMd5Legado("abc123"), 2, 9, false, true);

        var resultado = await Crear().EjecutarAsync(new SolicitudPreautenticacion("123456789", "abc123"), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoPreautenticacion.Rechazado>(resultado);
        Assert.Equal(ReglasCuenta.MensajeCuentaBloqueada, rechazo.Mensaje);
        Assert.Equal(0, _repositorio.Intentos);
        Assert.Equal(0, _repositorio.ConsultasEstado);
    }

    private PreautenticarUsuario Crear() =>
        new(new VerificacionClaveUsuario(_repositorio, _repositorio, _verificador, NullLogger<VerificacionClaveUsuario>.Instance), _repositorio);

    private UsuarioLogin Usuario(string clave) =>
        new(3, "123456789", "Ana", "Soto", _verificador.CrearHuellaMd5Legado(clave), 2, 9, false);

    private static PerfilCampamentoUsuario Perfil(int id, int perfilId) =>
        new(id, perfilId, "Operaciones", 20, "Campamento", 9, "Mandante", "Campamento - Operaciones");
}
