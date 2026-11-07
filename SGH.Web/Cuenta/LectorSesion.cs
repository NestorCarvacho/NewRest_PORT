using System.Security.Claims;

namespace SGH.Web.Cuenta;

public readonly record struct ContextoSesion(int UsuarioId, int CampamentoId, int EmpresaMandanteId);

public static class LectorSesion
{
    public static ContextoSesion Leer(ClaimsPrincipal usuario) =>
        new(
            Entero(usuario, ClaimTypes.NameIdentifier),
            Entero(usuario, ClaimsSesion.CampamentoId),
            Entero(usuario, ClaimsSesion.EmpresaMandanteId));

    private static int Entero(ClaimsPrincipal usuario, string tipo)
    {
        var valor = usuario.FindFirstValue(tipo);
        return int.TryParse(valor, out var numero) ? numero : 0;
    }
}
