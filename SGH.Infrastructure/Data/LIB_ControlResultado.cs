namespace SGH.Infrastructure.Data;

public sealed class LIB_ControlResultado
{
    public LIB_ControlResultado()
    {
        Resultado = true;
    }

    public bool Resultado { get; set; }
    public string? Descripcion { get; set; }
    public string? Detalle { get; set; }
    public string? Codigo { get; set; }

    public string Mensaje
    {
        get
        {
            if (string.IsNullOrEmpty(Detalle) || Descripcion == Detalle)
                return Descripcion ?? "";

            return $"{Descripcion}. {Detalle}";
        }
    }

    public void SetBadResult(string descripcion, string? detalle = null)
    {
        Resultado = false;
        Descripcion = descripcion;
        Detalle = detalle ?? descripcion;
    }
}
