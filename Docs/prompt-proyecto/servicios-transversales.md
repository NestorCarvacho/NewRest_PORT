# Servicios transversales (SGH 1.8)

## Cuándo usar cada servicio

| Necesidad | Servicio | Notas |
|-----------|----------|--------|
| Avisar por correo con HTML homogéneo | `IRenderizadorPlantillaCorreo` + `IEnviadorCorreo` | Nueva plantilla en `Pages/Correo/`; constante en `PlantillasCorreo`. |
| Descarga informe desde Mid (legado) | `IExportadorHojaCalculo.CrearXmlSpreadsheet` | MIME `application/vnd.ms-excel`, extensión `.xls`. |
| Exportación moderna `.xlsx` | `CrearXlsx` | Misma forma de filas que informes. |
| Etiqueta / sticker habitación | `IGeneradorCodigoQr` | JSON `{"h":habitacionId}` como censo QR. |
| Informe imprimible | `IGeneradorPdf` | Tabla simple; extender métodos si el informe crece. |

## Correo

1. Crear vista Razor sin layout (`Layout = null`).
2. Agregar ruta en `PlantillasCorreo`.
3. Componer `CorreoMensaje` y llamar `IEnviadorCorreo.EnviarAsync(..., seccionConfig)`.
4. No incluir contraseñas ni datos sensibles en plantillas.

Sección típica:

```json
"CorreoSolicitudes": {
  "Habilitado": false,
  "Host": "",
  "Puerto": 587,
  "Ssl": true,
  "Remitente": "",
  "Usuario": "",
  "Contrasena": ""
}
```

## Excel

- **Importación** de planillas: seguir `IPlanillaPasajeros` / `IPlanillaReservaExcel` (ClosedXML).
- **Exportación** dinámica desde filas de SP: `IExportadorHojaCalculo`; no duplicar XML Spreadsheet en Application.

## QR y PDF

- QR: contenido UTF-8; validar tamaño `pixelesPorModulo` (4–40).
- PDF: QuestPDF con `LicenseType.Community`; evitar imágenes embebidas hasta definir storage.

## .NET 10

Tras actualizar paquetes, ejecutar `dotnet test SGH.sln -c Release` y revisar `ServiciosTransversalesNet10Tests`.