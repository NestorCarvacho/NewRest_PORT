using SGH.Application.Solicitud.DTOs;

namespace SGH.Application.Solicitud.Interfaces;

public interface IPlanillaPasajeros
{
    IReadOnlyList<FilaCargaPasajeroDto> Leer(Stream archivo);
    byte[] CrearPlantilla();
}
