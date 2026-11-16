# Configuración y secretos (SGH 1.9)

## Principio

Las credenciales **no** van en código ni en `appsettings.json` versionado. Cada ambiente define secretos en su **almacén** (User Secrets, archivo local, variables de entorno o volumen montado).

## Orden de carga (último gana)

1. `appsettings.json` — solo parámetros no sensibles.
2. `appsettings.{Environment}.json` — overrides no sensibles.
3. `secrets.json` — copia local de `secrets.example.json` (**gitignored**).
4. Directorio `SGH_SECRETS_DIRECTORY` (Key-per-file, típico en contenedores).
5. **User Secrets** — solo `Development` (`SGH.Web` tiene `UserSecretsId`).
6. Variables de entorno estándar (`ConnectionStrings__Hoteldb`, etc.).
7. Variables con prefijo `SGH_` (ej. `SGH_ConnectionStrings__Hoteldb`).

Código: `SGH.Web/Configuracion/ExtensionesAlmacenSecretos.cs`.

## Claves secretas

| Secreto | Clave configuración | Variable de entorno |
|---------|---------------------|---------------------|
| SQL Server HOTELDB | `ConnectionStrings:Hoteldb` | `ConnectionStrings__Hoteldb` |
| SMTP contraseña | `CorreoSolicitudes:Contrasena` | `CorreoSolicitudes__Contrasena` |

Catálogo: `SGH.Application/Configuracion/CatalogoSecretos.cs`.

## Desarrollo local

```bash
cp SGH.Web/secrets.example.json SGH.Web/secrets.json
# Editar secrets.json (no commitear)

dotnet user-secrets set "ConnectionStrings:Hoteldb" "Server=...;Database=HOTELDB;..." --project SGH.Web
```

## Producción / staging

- Definir `ConnectionStrings__Hoteldb` en el host (App Service, IIS, Kubernetes Secret → env, etc.).
- Opcional: montar `/run/secrets` y exportar `SGH_SECRETS_DIRECTORY=/run/secrets` con un archivo por clave (`ConnectionStrings__Hoteldb` como nombre de archivo).

La aplicación **falla al iniciar** en Production/Staging si falta la cadena de conexión.

## Correo SMTP

Valores no secretos (host, puerto, remitente) pueden vivir en configuración del host; **Usuario** y **Contrasena** solo en almacén de secretos.

## Logging

Nunca registrar contraseñas ni cadenas completas; usar `EnmascaramientoSecretos` si se diagnostica configuración.