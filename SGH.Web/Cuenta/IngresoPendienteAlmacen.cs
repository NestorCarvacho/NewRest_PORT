using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SGH.Web.Cuenta;

public sealed class OpcionPerfilVista
{
    public int Id { get; set; }

    public string Etiqueta { get; set; } = string.Empty;
}

public sealed class IngresoPendiente
{
    public string Rut { get; set; } = string.Empty;
    public DateTime ExpiraUtc { get; set; }
    public bool EsIntegracion { get; set; }
    public List<OpcionPerfilVista> Perfiles { get; set; } = [];
}

public static class IngresoPendienteAlmacen
{
    public const string Clave = "IngresoPendiente";
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);

    public static void Guardar(ITempDataDictionary tempData, IngresoPendiente pendiente)
    {
        tempData[Clave] = JsonSerializer.Serialize(pendiente, Opciones);
    }

    public static IngresoPendiente? Leer(ITempDataDictionary tempData)
    {
        if (tempData.Peek(Clave) is not string json)
            return null;

        var pendiente = JsonSerializer.Deserialize<IngresoPendiente>(json, Opciones);
        if (pendiente is null || pendiente.ExpiraUtc < DateTime.UtcNow)
        {
            tempData.Remove(Clave);
            return null;
        }

        tempData.Keep(Clave);
        return pendiente;
    }

    public static void Descartar(ITempDataDictionary tempData) => tempData.Remove(Clave);
}
