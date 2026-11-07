using System.Data;
using Microsoft.Data.SqlClient;

namespace SGH.Infrastructure.Persistence;

public interface IFabricaConexionSql
{
    IDbConnection Crear();
}

public sealed class FabricaConexionSql : IFabricaConexionSql
{
    private readonly string? _cadena;

    public FabricaConexionSql(string? cadena)
    {
        _cadena = cadena;
    }

    public IDbConnection Crear()
    {
        if (string.IsNullOrWhiteSpace(_cadena))
            throw new InvalidOperationException("Falta ConnectionStrings:Hoteldb.");

        return new SqlConnection(_cadena);
    }
}
