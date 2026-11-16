namespace SGH.Application.Transversal;

public interface IEnviadorCorreo
{
    Task EnviarAsync(CorreoMensaje mensaje, string seccionConfiguracion, CancellationToken cancellationToken);
}