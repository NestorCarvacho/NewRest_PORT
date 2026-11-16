using SGH.Application.Sesion;

namespace SGH.Tests.Sesion;

public sealed class AuditoriaPersistenciaTests
{
    [Fact]
    public void UsuarioDeSesion_idPositivo_devuelveMismoId()
    {
        Assert.Equal(42, AuditoriaPersistencia.UsuarioDeSesion(new DatosSesion(42, 3, 9)));
    }

    [Fact]
    public void UsuarioDeSesion_idInvalido_lanza()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => AuditoriaPersistencia.UsuarioDeSesion(new DatosSesion(0, 3, 9)));
        Assert.Equal(AuditoriaPersistencia.MensajeUsuarioInvalido, ex.Message);
    }
}