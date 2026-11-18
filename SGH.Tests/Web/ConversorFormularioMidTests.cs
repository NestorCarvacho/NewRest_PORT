using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SGH.Application.Mid;
using SGH.Web.Pages.Modulos;

namespace SGH.Tests.Web;

public class ConversorFormularioMidTests
{
    [Fact]
    public void DesdeFormulario_mapea_opcion_y_campos()
    {
        var form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["opcion"] = "guardarcama",
            ["habitacionId"] = "10",
            ["codigoCama"] = "Cama 1",
            ["habilitada"] = "true",
            ["__RequestVerificationToken"] = "token"
        });

        var filtro = ConversorFormularioMid.DesdeFormulario(form);

        Assert.Equal("guardarcama", filtro.Opcion);
        Assert.NotNull(filtro.Resto);
        Assert.Equal(10, LecturaPeticion.Entero(filtro, "habitacionId"));
        Assert.Equal("Cama 1", LecturaPeticion.Texto(filtro, "codigoCama"));
        Assert.False(filtro.Resto!.ContainsKey("__RequestVerificationToken"));
    }
}