namespace SGH.Application.Transversal;

public interface IGeneradorCodigoQr
{
    byte[] CrearPng(string contenido, int pixelesPorModulo = 10);

    byte[] CrearPngCensoHabitacion(int habitacionId, int pixelesPorModulo = 10);
}