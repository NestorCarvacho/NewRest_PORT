using SGH.Domain.Consulta;

namespace SGH.Application.Mid;

public static class RespuestaListado
{
    public static object ConTope<T>(IReadOnlyList<T> filas)
    {
        var (recorte, truncado) = ReglasConsultaListado.AplicarTopeFilas(filas);
        return new
        {
            filas = recorte,
            truncado,
            maxFilas = ReglasConsultaListado.MaxFilas
        };
    }
}