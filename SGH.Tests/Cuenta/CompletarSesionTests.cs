using Microsoft.Extensions.Logging.Abstractions;
using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Opciones;
using SGH.Application.Cuenta.UseCases;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Entities;
using SGH.Infrastructure.Cuenta.Security;

namespace SGH.Tests.Cuenta;

public class CompletarSesionTests
{
    private readonly RepositorioFalso _repositorio = new();
    private readonly VerificadorContrasena _verificador = new(new OpcionesContrasena { FactorTrabajoBcrypt = 4 });
    private readonly OpcionesSesion _sesion = new() { MinutosPorDefecto = 60, MinutosOperaciones = 480 };
    private const string Clave = "abc123";

    [Fact]
    public async Task SegundoPasoNoRevalidaLaClave()
    {
        Preparar();
        _repositorio.Perfiles.Add(Perfil(8, ReglasCuenta.PerfilOperacionesA));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", "otra123", 8), CancellationToken.None);

        Assert.IsType<ResultadoAutenticacion.Aceptado>(resultado);
        Assert.Equal(1, _repositorio.ConsultasEstado);
    }

    [Fact]
    public async Task PerfilAjenoSeRechazaConElMensajeDeCredenciales()
    {
        Preparar();
        _repositorio.Perfiles.Add(Perfil(8, 2));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", Clave, 99), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoAutenticacion.Rechazado>(resultado);
        Assert.Equal(MotivoRechazo.PerfilNoPermitido, rechazo.Motivo);
        Assert.Equal(ReglasCuenta.CredencialesInvalidas, rechazo.Mensaje);
    }

    [Fact]
    public async Task UnaSolaAsignacionEntraSinElegirCampamento()
    {
        Preparar();
        _repositorio.Perfiles.Add(Perfil(8, ReglasCuenta.PerfilOperacionesA));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", Clave, 0), CancellationToken.None);

        var aceptado = Assert.IsType<ResultadoAutenticacion.Aceptado>(resultado);
        Assert.Equal(8, aceptado.Sesion.AsignacionPerfilId);
        Assert.Equal(20, aceptado.Sesion.CampamentoId);
    }

    [Fact]
    public async Task VariosCampamentosSinSeleccionPidenElegir()
    {
        Preparar();
        _repositorio.Perfiles.Add(Perfil(8, ReglasCuenta.PerfilOperacionesA));
        _repositorio.Perfiles.Add(new PerfilCampamentoUsuario(9, 2, "Otro", 21, "Campamento B", 9, "Mandante", "Campamento B - Otro", 25, "Ana Soto"));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", Clave, 0), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoAutenticacion.Rechazado>(resultado);
        Assert.Equal(MotivoRechazo.PerfilNoPermitido, rechazo.Motivo);
        Assert.Equal(ReglasCuenta.SeleccionePerfil, rechazo.Mensaje);
    }

    [Fact]
    public async Task UnicoCampamentoVariosPerfilesEntraConLaPrimeraAsignacion()
    {
        Preparar();
        _repositorio.Perfiles.Add(Perfil(8, ReglasCuenta.PerfilOperacionesA));
        _repositorio.Perfiles.Add(new PerfilCampamentoUsuario(9, 2, "Supervisor", 20, "Campamento", 9, "Mandante", "Campamento - Supervisor", 25, "Ana Soto"));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", Clave, 0), CancellationToken.None);

        var aceptado = Assert.IsType<ResultadoAutenticacion.Aceptado>(resultado);
        Assert.Equal(8, aceptado.Sesion.AsignacionPerfilId);
    }

    [Fact]
    public async Task SinPerfilesRechazaConMensajeDeCampamento()
    {
        Preparar();

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", Clave, 0), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoAutenticacion.Rechazado>(resultado);
        Assert.Equal(MotivoRechazo.PerfilNoPermitido, rechazo.Motivo);
        Assert.Equal("La sesión no tiene un campamento.", rechazo.Mensaje);
    }

    [Fact]
    public async Task VariasAsignacionesSinCampamentoRealPidenElegir()
    {
        Preparar();
        _repositorio.Perfiles.Add(new PerfilCampamentoUsuario(8, ReglasCuenta.PerfilOperacionesA, "Operaciones", 0, "Sin campamento", 9, "Mandante", "A - Operaciones", 25, "Ana Soto"));
        _repositorio.Perfiles.Add(new PerfilCampamentoUsuario(9, 2, "Otro", 0, "Sin campamento", 9, "Mandante", "B - Otro", 25, "Ana Soto"));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", Clave, 0), CancellationToken.None);

        var rechazo = Assert.IsType<ResultadoAutenticacion.Rechazado>(resultado);
        Assert.Equal(ReglasCuenta.SeleccionePerfil, rechazo.Mensaje);
    }

    [Fact]
    public async Task PerfilDeOperacionesUsaLaDuracionLarga()
    {
        Preparar();
        _repositorio.Perfiles.Add(Perfil(8, ReglasCuenta.PerfilOperacionesA));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", Clave, 8), CancellationToken.None);

        var aceptado = Assert.IsType<ResultadoAutenticacion.Aceptado>(resultado);
        Assert.Equal(480, aceptado.Sesion.MinutosVigencia);
        Assert.Equal(ReglasCuenta.RolGeneral, aceptado.Sesion.Rol);
        Assert.Equal(20, aceptado.Sesion.CampamentoId);
        Assert.Equal(25, aceptado.Sesion.UsuarioId);
        Assert.Equal("Ana Soto", aceptado.Sesion.NombreParaMostrar);
    }

    [Fact]
    public async Task RutConocidoFuerzaElPerfilOnce()
    {
        _repositorio.Usuario = UsuarioDe(ReglasCuenta.RutPerfilForzado, "Ana", 2);
        _repositorio.Perfiles.Add(Perfil(8, 2));
        _repositorio.Perfiles.Add(new PerfilCampamentoUsuario(11, ReglasCuenta.PerfilForzado, "Forzado", 30, "Otro", 4, "Mandante", "Otro - Forzado", 42, "Usuario asignado"));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion(ReglasCuenta.RutPerfilForzado, Clave, 8), CancellationToken.None);

        var aceptado = Assert.IsType<ResultadoAutenticacion.Aceptado>(resultado);
        Assert.Equal(ReglasCuenta.PerfilForzado, aceptado.Sesion.PerfilId);
        Assert.Equal(30, aceptado.Sesion.CampamentoId);
        Assert.Equal(42, aceptado.Sesion.UsuarioId);
        Assert.Equal("Usuario asignado", aceptado.Sesion.NombreParaMostrar);
    }

    [Fact]
    public async Task IdeaunoEntraConRolDeIntegracion()
    {
        _repositorio.Usuario = UsuarioDe("123456789", ReglasCuenta.NombreIntegracion, 1);
        _repositorio.Perfiles.Add(Perfil(8, 2));

        var resultado = await Crear().EjecutarAsync(new SolicitudCompletarSesion("123456789", Clave, 0), CancellationToken.None);

        var aceptado = Assert.IsType<ResultadoAutenticacion.Aceptado>(resultado);
        Assert.True(aceptado.Sesion.EsIntegracion);
        Assert.Equal(ReglasCuenta.RolIntegracion, aceptado.Sesion.Rol);
        Assert.Equal(0, aceptado.Sesion.CampamentoId);
    }

    private CompletarSesion Crear()
    {
        var verificacion = new VerificacionClaveUsuario(
            _repositorio,
            _repositorio,
            _verificador,
            NullLogger<VerificacionClaveUsuario>.Instance);
        return new CompletarSesion(verificacion, _repositorio, _sesion);
    }

    private void Preparar() => _repositorio.Usuario = UsuarioDe("123456789", "Ana", 2);

    private UsuarioLogin UsuarioDe(string rut, string nombre, int perfilId) =>
        new(3, rut, nombre, "Soto", _verificador.CrearHashBcrypt(Clave), perfilId, 9, true);

    private static PerfilCampamentoUsuario Perfil(int id, int perfilId) =>
        new(id, perfilId, "Operaciones", 20, "Campamento", 9, "Mandante", "Campamento - Operaciones", 25, "Ana Soto");
}
