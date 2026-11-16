using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SGH.Application.Transversal;

namespace SGH.Infrastructure.Transversal;

public sealed class GeneradorPdfDocumento : IGeneradorPdf
{
    public GeneradorPdfDocumento()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] CrearInformeTabular(
        string titulo,
        IReadOnlyList<string> columnas,
        IReadOnlyList<IReadOnlyList<string?>> filas)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("El título del informe es obligatorio.", nameof(titulo));
        if (columnas.Count == 0)
            throw new ArgumentException("Debe indicar al menos una columna.", nameof(columnas));

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Margin(32);
                pagina.Size(PageSizes.A4);
                pagina.Header().Text(titulo).Bold().FontSize(16);
                pagina.Content().Table(tabla =>
                {
                    tabla.ColumnsDefinition(def =>
                    {
                        for (var i = 0; i < columnas.Count; i++)
                            def.RelativeColumn();
                    });

                    tabla.Header(fila =>
                    {
                        foreach (var columna in columnas)
                            fila.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(columna).SemiBold();
                    });

                    foreach (var filaDatos in filas)
                    {
                        for (var i = 0; i < columnas.Count; i++)
                        {
                            var valor = i < filaDatos.Count ? filaDatos[i] ?? string.Empty : string.Empty;
                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(valor);
                        }
                    }
                });
                pagina.Footer().AlignRight().Text(texto =>
                {
                    texto.Span("SGH · ").FontSize(8);
                    texto.CurrentPageNumber().FontSize(8);
                });
            });
        });

        return documento.GeneratePdf();
    }
}