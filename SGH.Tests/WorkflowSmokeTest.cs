namespace SGH.Tests;

public class WorkflowSmokeTest
{
    [Fact]
    public void SumaDeDosNumerosDevuelveElResultadoEsperado()
    {
        var resultado = 1 + 1;

        Assert.Equal(2, resultado);
    }
}
