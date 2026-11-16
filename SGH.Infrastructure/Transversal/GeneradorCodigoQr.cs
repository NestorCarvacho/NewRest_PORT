using System.Text.Json;
using QRCoder;
using SGH.Application.Transversal;

namespace SGH.Infrastructure.Transversal;

public sealed class GeneradorCodigoQr : IGeneradorCodigoQr
{
    public byte[] CrearPng(string contenido, int pixelesPorModulo = 10)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            throw new ArgumentException("El contenido del QR no puede estar vacío.", nameof(contenido));
        if (pixelesPorModulo is < 4 or > 40)
            throw new ArgumentOutOfRangeException(nameof(pixelesPorModulo), "Use un tamaño entre 4 y 40 píxeles por módulo.");

        using var generador = new QRCodeGenerator();
        var datos = generador.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.M);
        return new PngByteQRCode(datos).GetGraphic(pixelesPorModulo);
    }

    public byte[] CrearPngCensoHabitacion(int habitacionId, int pixelesPorModulo = 10)
    {
        if (habitacionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(habitacionId), "La habitación debe ser mayor que cero.");
        var json = JsonSerializer.Serialize(new { h = habitacionId });
        return CrearPng(json, pixelesPorModulo);
    }
}