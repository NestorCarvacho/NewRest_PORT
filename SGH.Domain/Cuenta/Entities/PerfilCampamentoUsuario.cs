namespace SGH.Domain.Cuenta.Entities;

public sealed class PerfilCampamentoUsuario
{
    public PerfilCampamentoUsuario(
        int id,
        int perfilId,
        string perfilNombre,
        int campamentoId,
        string campamentoNombre,
        int empresaMandanteId,
        string empresaMandanteNombre,
        string campamentoPerfil,
        int usuarioId,
        string usuarioNombre)
    {
        Id = id;
        PerfilId = perfilId;
        PerfilNombre = perfilNombre;
        CampamentoId = campamentoId;
        CampamentoNombre = campamentoNombre;
        EmpresaMandanteId = empresaMandanteId;
        EmpresaMandanteNombre = empresaMandanteNombre;
        CampamentoPerfil = campamentoPerfil;
        UsuarioId = usuarioId;
        UsuarioNombre = usuarioNombre;
    }

    public int Id { get; }
    public int PerfilId { get; }
    public string PerfilNombre { get; }
    public int CampamentoId { get; }
    public string CampamentoNombre { get; }
    public int EmpresaMandanteId { get; }
    public string EmpresaMandanteNombre { get; }
    public string CampamentoPerfil { get; }
    public int UsuarioId { get; }
    public string UsuarioNombre { get; }

    public string Etiqueta =>
        string.IsNullOrWhiteSpace(CampamentoPerfil)
            ? $"{CampamentoNombre} / {PerfilNombre}"
            : CampamentoPerfil;
}
