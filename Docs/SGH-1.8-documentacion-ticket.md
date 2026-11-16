# Ticket · SGH 1.8 — Servicios transversales: correo, PDF, QR y Excel

Documento para copiar al gestor de incidencias.  
**Estado:** implementado en rama `cursor/sgh-1-8-servicios-transversales-f6a4`.  
**Épica:** `Docs/SGH-1.8-servicios-transversales.md`

---

## Resumen (descripción del ticket)

**Objetivo:** Implementar y probar servicios transversales — **correo con plantillas Razor**, **PDF**, **códigos QR** y **exportación Excel** — verificando compatibilidad con **.NET 10**.

**Problema:** SMTP de solicitud armaba texto plano en código; Excel de informes vivía en una clase estática; no había abstracciones ni pruebas de QR/PDF en el stack actual.

**Solución:** Capa `SGH.Application.Transversal` + implementaciones (`EnviadorCorreoSmtp`, `GeneradorCodigoQr`, `GeneradorPdfDocumento`, `ExportadorHojaCalculo`) y plantilla `ReservasGeneradas.cshtml` renderizada por `RenderizadorPlantillaCorreoRazor`.

---

## Texto estándar para registro de actividad (copiar)

**Descripción:**  
Se implementaron los servicios transversales SGH 1.8 en NewRest_PORT: envío SMTP genérico, renderizado de plantillas de correo Razor, generación de PNG QR (formato censo habitación), informes PDF tabulares con QuestPDF y exportación Excel (XML Spreadsheet legado + `.xlsx` ClosedXML). La notificación de reservas generadas en solicitud migró a plantilla HTML. Se validaron **QRCoder 1.8.1**, **QuestPDF 2026.9.2** y **ClosedXML 0.105.1** en **net10.0**.

**Objetivo de la documentación:**  
Unificar criterios para nuevos correos/informes y dejar evidencia de compatibilidad de librerías con .NET 10.

**Resumen de directrices definidas:**

- Correo: `IEnviadorCorreo` + vistas en `Pages/Correo/`; configuración por sección (`CorreoSolicitudes`).
- Excel informes Mid: `IExportadorHojaCalculo.CrearXmlSpreadsheet` (sin romper `.xls` actuales).
- Excel `.xlsx`: `CrearXlsx` para exportaciones nuevas.
- QR: payload JSON `{"h":id}` coherente con `censo-qr.js`.
- PDF: QuestPDF Community; tablas simples vía `IGeneradorPdf`.
- Tests de humo en `SGH.Tests/Transversal/`.

**Resultado / impacto esperado:**  
Menos duplicación, correos presentables, base para módulos de códigos QR e informes PDF. Exclusiones: plantillas de correo masivas, SMTP centralizado único, generación PDF desde todos los informes Mid.

---

## Alcance

| Dentro | Fuera |
|--------|--------|
| Interfaces + DI transversal | Cola de correo / Hangfire |
| Plantilla solicitud reservas generadas | Todas las notificaciones del legado |
| QR PNG servidor | UI de impresión masiva QR |
| PDF tabular básico | Diseño gráfico corporativo PDF |
| Tests net10 + integración Razor/SMTP | Pruebas contra SMTP productivo |

---

## Implementación técnica

| Componente | Ruta |
|------------|------|
| Contratos | `SGH.Application/Transversal/*.cs` |
| SMTP | `Infrastructure/Transversal/EnviadorCorreoSmtp.cs` |
| QR | `Infrastructure/Transversal/GeneradorCodigoQr.cs` |
| PDF | `Infrastructure/Transversal/GeneradorPdfDocumento.cs` |
| Excel | `Infrastructure/Transversal/ExportadorHojaCalculo.cs` |
| Razor correo | `Web/Pages/Correo/ReservasGeneradas.cshtml` |
| Renderizado | `Web/Transversal/RenderizadorPlantillaCorreoRazor.cs` |
| Notificador | `Web/Solicitud/NotificadorSolicitudCorreo.cs` |
| Mid informes | `MidInformesCenso` usa `IExportadorHojaCalculo` |

---

## Pruebas

```bash
dotnet test SGH.sln -c Release
```

| Criterio | Estado |
|----------|--------|
| 110 tests Release | Cumplido |
| PNG QR válido | Cumplido |
| PDF cabecera `%PDF` | Cumplido |
| Xlsx abre en ClosedXML | Cumplido |
| Plantilla Razor renderiza solicitud id | Cumplido |
| SMTP prueba local con plantilla | Cumplido |

---

## Referencias

- `Docs/prompt-proyecto/servicios-transversales.md`
- `Docs/actividades/SGH-1.8-servicios-transversales.md`