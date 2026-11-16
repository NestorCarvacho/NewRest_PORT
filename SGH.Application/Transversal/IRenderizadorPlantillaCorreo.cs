namespace SGH.Application.Transversal;

public interface IRenderizadorPlantillaCorreo
{
    Task<string> RenderizarHtmlAsync(string rutaPlantilla, object modelo, CancellationToken cancellationToken);
}