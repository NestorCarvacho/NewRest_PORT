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
        bool md5Retirado)
    {
        Id = id;
        Rut = rut;
        Nombre = nombre;
        Apellidos = apellidos;
        HashContrasena = hashContrasena;
        PerfilId = perfilId;
        EmpresaMandanteId = empresaMandanteId;
        Md5Retirado = md5Retirado;
    }

    public int Id { get; }
    public string Rut { get; }
    public string Nombre { get; }
    public string Apellidos { get; }
    public string HashContrasena { get; }
    public int PerfilId { get; }
    public int? EmpresaMandanteId { get; }
    public bool Md5Retirado { get; }

    public string NombreParaMostrar
    {
        get
        {
            var nombre = $"{Nombre} {Apellidos}".Trim();
            return nombre.Length == 0 ? Rut : nombre;
        }
    }
}
