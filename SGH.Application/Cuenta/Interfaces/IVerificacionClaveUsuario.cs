using SGH.Domain.Cuenta.Entities;

namespace SGH.Application.Cuenta.Interfaces;

public abstract record ResultadoVerificacionClave
{
    private ResultadoVerificacionClave()
    {
    }

    public sealed record Valida(UsuarioLogin Usuario, string Rut) : ResultadoVerificacionClave;

    public sealed record Rechazada(string Mensaje, SGH.Application.Cuenta.DTOs.MotivoRechazo Motivo) : ResultadoVerificacionClave;
}

public interface IVerificacionClaveUsuario
{
    Task<ResultadoVerificacionClave> EjecutarAsync(string rut, string contrasena, int loginInterno, CancellationToken cancellationToken);
}
