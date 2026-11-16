# SGH 1.9 · Gestión de configuración y secretos

## Objetivo

Retirar credenciales del código y de archivos versionados, centralizando la carga por ambiente (Development / Staging / Production / Testing).

## Entregables

| Pieza | Ubicación |
|-------|-----------|
| Catálogo de claves | `SGH.Application/Configuracion/CatalogoSecretos.cs` |
| Enmascaramiento logs | `EnmascaramientoSecretos.cs` |
| Carga por ambiente | `SGH.Web/Configuracion/ExtensionesAlmacenSecretos.cs` |
| Plantilla secretos | `SGH.Web/secrets.example.json` |
| Guía equipo | `Docs/prompt-proyecto/configuracion-secretos.md` |
| `.gitignore` | `secrets.json`, `appsettings.*.local.json` |

## Comportamiento

- `appsettings.json` permanece sin connection strings ni contraseñas SMTP.
- Validación al arranque: obligatoria cadena HOTELDB en Production/Staging; warning en Development; omitida en entorno `Testing` (tests).
- Correo: `OpcionesCorreoSolicitudes` enlazada por DI; contraseña solo desde almacén.

## Pruebas

- `EnmascaramientoSecretosTests`
- `ArchivosSinCredencialesTests`
- Suite completa Release