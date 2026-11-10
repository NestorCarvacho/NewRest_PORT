using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SGH.Web.Autorizacion;

namespace SGH.Tests.Cuenta;

public sealed class PoliticasAutorizacionTests
{
    [Fact]
    public async Task PasajeroNoPuedeCumplirPoliticaOperativa()
    {
        var contexto = new AuthorizationHandlerContext(
            [new PerfilAutorizacionRequirement(false)],
            new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("perfilId", "2")],
                "test")),
            null);

        await new PerfilAutorizacionHandler().HandleAsync(contexto);

        Assert.False(contexto.HasSucceeded);
    }

    [Fact]
    public async Task PerfilOperativoPuedeCumplirPoliticaOperativa()
    {
        var contexto = new AuthorizationHandlerContext(
            [new PerfilAutorizacionRequirement(false)],
            new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("perfilId", "19")],
                "test")),
            null);

        await new PerfilAutorizacionHandler().HandleAsync(contexto);

        Assert.True(contexto.HasSucceeded);
    }
}
