using SGH.Application.Habitacion.DTOs;
using SGH.Domain.Habitacion.Interfaces;

namespace SGH.Application.Habitacion.Interfaces;

public interface IPlanillaCamas
{
    byte[] CrearPlantilla(int campamentoId, IReadOnlyList<TipoCamaListado> tiposCama);
    IReadOnlyList<FilaCargaCamaDto> Leer(Stream archivo);
}