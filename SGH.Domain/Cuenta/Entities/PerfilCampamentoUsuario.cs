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
        string campamentoPerfil)
    {
        Id = id;
        PerfilId = perfilId;
        PerfilNombre = perfilNombre;
        CampamentoId = campamentoId;
        CampamentoNombre = campamentoNombre;
        EmpresaMandanteId = empresaMandanteId;
        EmpresaMandanteNombre = empresaMandanteNombre;
        CampamentoPerfil = campamentoPerfil;
    }

    public int Id { get; }
    public int PerfilId { get; }
    public string PerfilNombre { get; }
    public int CampamentoId { get; }
    public string CampamentoNombre { get; }
    public int EmpresaMandanteId { get; }
    public string EmpresaMandanteNombre { get; }
    public string CampamentoPerfil { get; }

    public string Etiqueta =>
        string.IsNullOrWhiteSpace(CampamentoPerfil)
            ? $"{CampamentoNombre} / {PerfilNombre}"
            : CampamentoPerfil;
}
