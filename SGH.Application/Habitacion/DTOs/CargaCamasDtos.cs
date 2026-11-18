namespace SGH.Application.Habitacion.DTOs;

public sealed record FilaCargaCamaDto(
    int NumeroFila,
    string CodigoHabitacion,
    string CodigoCama,
    int? TipoCamaId,
    bool Habilitada,
    int Orden,
    DateTime? FechaColchon,
    string? ColchonObservacion,
    DateTime? FechaCambioRopa,
    string? RopaCamaObservacion);

public sealed record ResultadoCargaCamaDto(int NumeroFila, bool Procesada, string Mensaje);