# SGH 1.8 · Servicios transversales (correo, PDF, QR, Excel)

Infraestructura compartida para notificaciones, informes descargables y códigos QR, con librerías validadas en **.NET 10**.

## Objetivo

Centralizar servicios que hoy estaban dispersos (SMTP ad hoc, Excel XML en `Mid`, sin PDF/QR servidor) detrás de interfaces en `SGH.Application.Transversal` e implementaciones en **Infrastructure** y **Web** (plantillas Razor de correo).

## Contratos (`SGH.Application/Transversal`)

| Interfaz | Uso |
|----------|-----|
| `IEnviadorCorreo` | Envío SMTP con sección `appsettings` (`CorreoSolicitudes`, etc.). |
| `IRenderizadorPlantillaCorreo` | HTML desde vistas Razor (`Pages/Correo/`). |
| `IGeneradorCodigoQr` | PNG (`PngByteQRCode`); JSON censo `{"h":id}`. |
| `IGeneradorPdf` | Informes tabulares (QuestPDF, licencia Community). |
| `IExportadorHojaCalculo` | Excel XML Spreadsheet (informes Mid) y **.xlsx** (ClosedXML). |

## Librerías (.NET 10)

| Librería | Versión | Rol |
|----------|---------|-----|
| ClosedXML | 0.105.1 | Import/export `.xlsx` (planillas + exportador). |
| QRCoder | 1.8.1 | PNG sin GDI+ (`PngByteQRCode`). |
| QuestPDF | 2026.9.2 | PDF tabular. |
| `System.Net.Mail` | BCL | SMTP (sin paquete extra). |

Pruebas de humo: `SGH.Tests/Transversal/ServiciosTransversalesNet10Tests.cs`.

## Registro DI

- **Infrastructure:** `AddInfraestructura` registra enviador, QR, PDF y Excel.
- **Web:** `AddServiciosTransversalesWeb` registra `IRenderizadorPlantillaCorreo` y `INotificadorSolicitud` con plantilla Razor.

## Correo con plantilla

- Vista: `Pages/Correo/ReservasGeneradas.cshtml`
- Modelo: `CorreoReservasGeneradasVm`
- Orquestación: `NotificadorSolicitudCorreo` (reemplaza cuerpo texto plano anterior).

Configuración existente: sección `CorreoSolicitudes` en `appsettings`.

## Excel en informes

`MidInformesCenso` inyecta `IExportadorHojaCalculo` y mantiene descargas `.xls` (XML Spreadsheet) sin cambiar contrato Mid.

## QR censo

`IGeneradorCodigoQr.CrearPngCensoHabitacion` alinea el payload con el lector JS (`censo-qr.js`: propiedad `h`).

## Criterios de aceptación

- [x] Interfaces transversales registradas en DI.
- [x] Notificación solicitud usa plantilla Razor + SMTP.
- [x] QR, PDF y Excel generan bytes válidos en tests.
- [x] `dotnet test SGH.sln -c Release` en verde.

## Guía operativa

Ver `Docs/prompt-proyecto/servicios-transversales.md`.