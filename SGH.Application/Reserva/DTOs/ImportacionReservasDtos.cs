namespace SGH.Application.Reserva.DTOs;

public sealed record FilaPlanillaReserva(
    int NumeroFila,
    string GrupoEmpresa,
    string Turno,
    DateTime? FechaReserva,
    DateTime? FechaLlegada,
    DateTime? FechaSalida,
    string? Observacion,
    IReadOnlyList<string> Errores);

public sealed record ResultadoFilaImportacionReserva(int NumeroFila, int? ReservaId, string? Error);

public sealed record ResultadoImportacionReservas(
    int FilasLeidas,
    IReadOnlyList<ResultadoFilaImportacionReserva> Filas,
    string? Error);
