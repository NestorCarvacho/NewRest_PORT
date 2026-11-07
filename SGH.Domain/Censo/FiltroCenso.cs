namespace SGH.Domain.Censo;

public sealed record FiltroCenso(
    int EmpresaMandanteId,
    int CampamentoId,
    int ModuloId,
    int PisoId,
    int HabitacionId,
    DateTime Fecha,
    string Turno);
