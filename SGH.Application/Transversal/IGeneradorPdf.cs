namespace SGH.Application.Transversal;

public interface IGeneradorPdf
{
    byte[] CrearInformeTabular(
        string titulo,
        IReadOnlyList<string> columnas,
        IReadOnlyList<IReadOnlyList<string?>> filas);
}