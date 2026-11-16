# Registro de actividad — SGH 1.9 Configuración y secretos

**Fecha:** 2026-11-16  
**Rama:** `cursor/sgh-1-9-config-secretos-f6a4`  
**Ticket:** [`../SGH-1.9-documentacion-ticket.md`](../SGH-1.9-documentacion-ticket.md)

## Descripción

Almacén de secretos por ambiente, validación al arranque y retiro de credenciales de archivos versionados.

## Objetivo de la documentación

Despliegue e instalación local sin exponer secretos en Git.

## Resumen de directrices definidas

- `secrets.json` local + User Secrets + env vars + directorio Key-per-file.
- Catálogo de claves y enmascaramiento en logs.

## Resultado / impacto esperado

Fuente de verdad en `configuracion-secretos.md`; appsettings solo parámetros operativos.