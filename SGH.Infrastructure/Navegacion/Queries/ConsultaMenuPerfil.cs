using System.Data;
using Dapper;
using SGH.Domain.Navegacion;
using SGH.Domain.Navegacion.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Navegacion.Queries;

public sealed class ConsultaMenuPerfil : IConsultaMenuPerfil
{
    private readonly IFabricaConexionSql _fabrica;

    public ConsultaMenuPerfil(IFabricaConexionSql fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<IReadOnlyList<MenuPerfilItem>> ObtenerPorPerfilAsync(int perfilId, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var filas = await conexion.QueryAsync<MenuPerfilFila>(
            new CommandDefinition(
                "sp_Menu_ObtenerPorPerfil",
                new { PerfilId = perfilId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return filas
            .Select(fila => new MenuPerfilItem(
                fila.Id,
                fila.Nombre ?? string.Empty,
                fila.Link ?? string.Empty,
                fila.PadreId,
                fila.Ubicacion ?? string.Empty,
                fila.Orden,
                fila.Icono))
            .ToList();
    }

    private sealed class MenuPerfilFila
    {
        public int Id { get; init; }
        public string? Nombre { get; init; }
        public string? Link { get; init; }
        public int? PadreId { get; init; }
        public string? Ubicacion { get; init; }
        public int Orden { get; init; }
        public string? Icono { get; init; }
    }
}