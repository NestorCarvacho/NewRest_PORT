using BCryptNet = BCrypt.Net.BCrypt;
using Xunit;

namespace SGH.Tests.Cuenta;

public sealed class GenerarHashTests
{
    [Fact]
    public void GeneraHashBcrypt()
    {
        const string contrasena = "Test12345";
        var hash = BCryptNet.HashPassword(contrasena);

        Console.WriteLine(hash);

        Assert.StartsWith("$2", hash);
        Assert.True(BCryptNet.Verify(contrasena, hash));
    }
}