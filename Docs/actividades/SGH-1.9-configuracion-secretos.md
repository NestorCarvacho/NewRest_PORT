# Registro de actividad — SGH 1.9 Configuración y secretos

**Épica / ticket:** SGH 1.9 · Gestión de configuración y secretos  
**Fecha:** 2026-11-16  
**Rama / merge:** `cursor/sgh-1-9-config-secretos-f6a4` → `main` (`6288dcd`, docs `1a909d4`)  
**Estado:** Implementada, mergeada y verificada en local (Windows / PowerShell)

**Ticket gestor:** [`../SGH-1.9-documentacion-ticket.md`](../SGH-1.9-documentacion-ticket.md)

---

## Descripción

Se implementó el almacén de secretos por ambiente: carga desde `secrets.json`, User Secrets, variables de entorno y directorio Key-per-file; validación de cadena HOTELDB al arranque en producción; catálogo de claves y enmascaramiento en diagnósticos; retiro de credenciales de archivos versionados y opciones tipadas para SMTP.

## Objetivo de la documentación

Registrar el cierre formal de la actividad y ofrecer texto listo para Jira/Linear/DevOps, onboarding de desarrolladores y checklist de despliegue sin exposición de secretos en Git.

## Resumen de directrices definidas

- **No versionar:** `secrets.json`, `appsettings.*.local.json`.
- **Desarrollo:** `secrets.example.json` + User Secrets (`SGH.Web`).
- **Servidor:** `ConnectionStrings__Hoteldb`, `CorreoSolicitudes__Contrasena`.
- **Validación:** obligatoria en Production/Staging; omisión en entorno `Testing`.
- **Logs:** `EnmascaramientoSecretos` para cadenas de conexión.
- **Referencia:** `Docs/prompt-proyecto/configuracion-secretos.md`.

## Resultado / impacto esperado

Política de secretos unificada con el resto de épicas SGH; 114 tests Release en verde; documentación de ticket actualizada para copiar al gestor de incidencias.

---

## Anexo — Bloque único para pegar en gestor

**SGH 1.9 · Gestión de configuración y secretos** — Secretos fuera del repositorio: `AgregarAlmacenSecretosPorAmbiente`, `secrets.example.json`, catálogo `CatalogoSecretos`, validación HOTELDB en prod, enmascaramiento y guía `configuracion-secretos.md`. Merge `6288dcd` en `main`. Pruebas: `dotnet test SGH.sln -c Release` (114 passed). Documentación: `Docs/SGH-1.9-documentacion-ticket.md`.