namespace SGH.Tests.Configuracion;

public class ArchivosSinCredencialesTests
{
    [Theory]
    [InlineData("SGH.Web/appsettings.json")]
    [InlineData("SGH.Web/appsettings.example.json")]
    public void AppsettingsVersionadosNoContienenPasswordLiteral(string rutaRelativa)
    {
        var ruta = Path.GetFullPath(Path.Combine(EncontrarRaizSolucion(), rutaRelativa));
        Assert.True(File.Exists(ruta), $"No se encontró {ruta}");
        var contenido = File.ReadAllText(ruta);
        Assert.DoesNotContain("Password=", contenido, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Pwd=", contenido, StringComparison.OrdinalIgnoreCase);
    }

  private static string EncontrarRaizSolucion()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (directorio is not null)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "SGH.sln")))
                return directorio.FullName;
            directorio = directorio.Parent;
        }
        throw new InvalidOperationException("No se encontró la raíz de SGH.sln");
    }
}