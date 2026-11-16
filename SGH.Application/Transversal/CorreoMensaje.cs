namespace SGH.Application.Transversal;

public sealed class CorreoMensaje
{
    public required string Destinatario { get; init; }
    public required string Asunto { get; init; }
    public required string CuerpoHtml { get; init; }
    public string? CuerpoTexto { get; init; }
}