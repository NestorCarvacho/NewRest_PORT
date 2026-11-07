using System.Data;
using Dapper;
using SGH.Domain.Censo.Entities;
using SGH.Domain.Censo.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Censo.Commands;

public sealed class ComandoCenso : IComandoCenso
{
    private readonly IFabricaConexionSql _fabrica;

    public ComandoCenso(IFabricaConexionSql fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<ResultadoFilaCenso> GuardarAsync(
        FilaCensoGuardar fila,
        DateTime fecha,
        int usuarioId,
        int campamentoId,
        CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var resultado = await conexion.QueryFirstOrDefaultAsync<ResultadoFila>(
            new CommandDefinition(
                "sp_Censo_Guardar",
                new
                {
                    fila.Id,
                    fila.HabitacionId,
                    fila.Cantidad,
                    Fecha = fecha,
                    fila.Turno,
                    UsuarioId = usuarioId,
                    CampamentoId = campamentoId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return resultado is null
            ? new ResultadoFilaCenso(1, "No se pudo guardar el censo.")
            : new ResultadoFilaCenso(resultado.Resultado, resultado.Mensaje);
    }

    private sealed class ResultadoFila
    {
        public int Resultado { get; set; }
        public string? Mensaje { get; set; }
    }
}
