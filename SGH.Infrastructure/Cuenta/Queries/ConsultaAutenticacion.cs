using System.Data;
using Dapper;
using SGH.Domain.Cuenta.Entities;
using SGH.Domain.Cuenta.Interfaces;
using SGH.Infrastructure.Persistence;

namespace SGH.Infrastructure.Cuenta.Queries;

public sealed class ConsultaAutenticacion : IConsultaAutenticacion
{
    private readonly IFabricaConexionSql _fabrica;

    public ConsultaAutenticacion(IFabricaConexionSql fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<UsuarioLogin?> ObtenerParaLoginAsync(string rut, int loginInterno, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var fila = await conexion.QueryFirstOrDefaultAsync<UsuarioLoginFila>(
            new CommandDefinition(
                "sp_Usuario_ObtenerParaLogin",
                new { Rut = rut, LoginInterno = loginInterno },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        if (fila is null || fila.Id == 0)
            return null;

        return new UsuarioLogin(
            fila.Id,
            fila.Rut ?? rut,
            fila.Nombre ?? string.Empty,
            fila.Apellidos ?? string.Empty,
            fila.HashContrasena ?? string.Empty,
            fila.PerfilId,
            fila.EmpresaMandanteId,
            fila.Md5Retirado == true,
            fila.Bloqueada == true);
    }

    public async Task<int> ObtenerEstadoAsync(string rut, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var fila = await conexion.QueryFirstOrDefaultAsync<EstadoFila>(
            new CommandDefinition(
                "sp_Usuario_ValidarEstado",
                new { Rut = rut },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        if (fila is null)
            throw new InvalidOperationException("sp_Usuario_ValidarEstado no devolvió un estado.");

        return fila.Estado;
    }

    public async Task<IReadOnlyList<PerfilCampamentoUsuario>> ObtenerCampamentosYPerfilesAsync(string rut, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var filas = await conexion.QueryAsync<PerfilCampamentoFila>(
            new CommandDefinition(
                "sp_Usuario_ObtenerCampamentosYPerfiles",
                new { Rut = rut },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return filas.Select(fila => new PerfilCampamentoUsuario(
            fila.Id,
            fila.PerfilId,
            fila.PerfilNombre ?? string.Empty,
            fila.CampamentoId,
            fila.CampamentoNombre ?? string.Empty,
            fila.EmpresaMandanteId,
            fila.EmpresaMandanteNombre ?? string.Empty,
            fila.CampamentoPerfil ?? string.Empty,
            fila.UsuarioId,
            fila.UsuarioNombre ?? string.Empty)).ToList();
    }

    public async Task<IReadOnlyList<string>> ObtenerHistorialContrasenasAsync(string rut, CancellationToken cancellationToken)
    {
        using var conexion = _fabrica.Crear();
        var hashes = await conexion.QueryAsync<string>(
            new CommandDefinition(
                "sp_Usuario_ObtenerHistorialContrasenas",
                new { Rut = rut },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return hashes.AsList();
    }

    private sealed class UsuarioLoginFila
    {
        public int Id { get; set; }
        public string? Rut { get; set; }
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? HashContrasena { get; set; }
        public int PerfilId { get; set; }
        public int? EmpresaMandanteId { get; set; }
        public bool? Md5Retirado { get; set; }
        public bool? Bloqueada { get; set; }
    }

    private sealed class EstadoFila
    {
        public int Estado { get; set; }
    }

    private sealed class PerfilCampamentoFila
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
        public int PerfilId { get; set; }
        public string? PerfilNombre { get; set; }
        public int CampamentoId { get; set; }
        public string? CampamentoNombre { get; set; }
        public int EmpresaMandanteId { get; set; }
        public string? EmpresaMandanteNombre { get; set; }
        public string? CampamentoPerfil { get; set; }
    }
}
