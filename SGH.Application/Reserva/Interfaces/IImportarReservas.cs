using SGH.Application.Reserva.DTOs;

namespace SGH.Application.Reserva.Interfaces;

public interface IPlanillaReservaExcel
{
    IReadOnlyList<FilaPlanillaReserva> Leer(Stream archivo);

    byte[] CrearPlantilla();
}

public interface IImportarReservas
{
    Task<ResultadoImportacionReservas> EjecutarAsync(
        Stream archivo,
        int empresaMandanteId,
        int campamentoId,
        int usuarioId,
        CancellationToken cancellationToken);
}
