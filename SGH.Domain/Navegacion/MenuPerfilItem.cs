namespace SGH.Domain.Navegacion;

public sealed class MenuPerfilItem
{
    public MenuPerfilItem(
        int id,
        string nombre,
        string link,
        int? padreId,
        string ubicacion,
        int orden,
        string? icono)
    {
        Id = id;
        Nombre = nombre;
        Link = link;
        PadreId = padreId;
        Ubicacion = ubicacion;
        Orden = orden;
        Icono = icono;
    }

    public int Id { get; }
    public string Nombre { get; }
    public string Link { get; }
    public int? PadreId { get; }
    public string Ubicacion { get; }
    public int Orden { get; }
    public string? Icono { get; }
}