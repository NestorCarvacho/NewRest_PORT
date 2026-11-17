namespace SGH.Domain.Cuenta.Entities;

public sealed class UsuarioLogin
{
    public UsuarioLogin(
        int id,
        string rut,
        string nombre,
        string apellidos,
        string hashContrasena,
        int perfilId,
        int? empresaMandanteId,
        bool md5Retirado,
        bool bloqueada = false,
        string hashContrasenaV2 = "")
    {
        Id = id;
        Rut = rut;
        Nombre = nombre;
        Apellidos = apellidos;
        HashContrasena = hashContrasena;
        HashContrasenaV2 = hashContrasenaV2 ?? string.Empty;
        PerfilId = perfilId;
        EmpresaMandanteId = empresaMandanteId;
        Md5Retirado = md5Retirado;
        Bloqueada = bloqueada;
    }

    public int Id { get; }
    public string Rut { get; }
    public string Nombre { get; }
    public string Apellidos { get; }
    /// <summary>Campo legado <c>Password</c> (MD5 o estado histórico).</summary>
    public string HashContrasena { get; }

    /// <summary>BCrypt del sistema nuevo; no altera el MD5 legado en <see cref="HashContrasena"/>.</summary>
    public string HashContrasenaV2 { get; }
    public int PerfilId { get; }
    public int? EmpresaMandanteId { get; }
    public bool Md5Retirado { get; }
    public bool Bloqueada { get; }

    public string NombreParaMostrar
    {
        get
        {
            var nombre = $"{Nombre} {Apellidos}".Trim();
            return nombre.Length == 0 ? Rut : nombre;
        }
    }
}
