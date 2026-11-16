# Ticket · SGH 1.9 — Gestión de configuración y secretos

Documento para copiar al gestor de incidencias (Jira, Linear, Azure DevOps).  
**Estado:** implementado y mergeado en `main` (commit `6288dcd`, 2026-11-16).  
Rama histórica: `cursor/sgh-1-9-config-secretos-f6a4`.  
**Épica:** `Docs/SGH-1.9-configuracion-secretos.md`

---

## Resumen (descripción del ticket)

**Objetivo:** Retirar **credenciales** del código y de archivos `appsettings` versionados, centralizándolas en un **almacén de secretos por ambiente** (desarrollo, pruebas, staging, producción).

**Problema que resolvía:** Riesgo de filtrar cadenas SQL o SMTP en Git; mezcla de secretos en `appsettings.example.json`; sin convención única de variables de entorno ni validación al desplegar.

**Solución entregada:** Carga escalonada de configuración (`secrets.json` local, User Secrets, directorio Key-per-file, variables de entorno), catálogo de claves, validación al arranque en Production/Staging, enmascaramiento en logs y SMTP enlazado con `OpcionesCorreoSolicitudes` sin password en repos.

---

## Texto estándar para registro de actividad (copiar)

**Descripción:**  
Se elaboró e implementó la gestión de configuración y secretos **SGH 1.9** en NewRest_PORT: extensión `AgregarAlmacenSecretosPorAmbiente`, archivo plantilla `secrets.example.json` (ignorado `secrets.json`), ampliación de `.gitignore`, catálogo `CatalogoSecretos`, enmascaramiento `EnmascaramientoSecretos`, validación de `ConnectionStrings:Hoteldb` al iniciar en ambientes productivos y lectura de credenciales SMTP vía `OpcionesCorreoSolicitudes`. Los `appsettings.json` versionados conservan solo parámetros no sensibles (sesión, logging, bcrypt).

**Objetivo de la documentación:**  
Dar al equipo una guía verificable para instalar localmente y desplegar en servidores sin commitear secretos, con nombres de variables de entorno y checklist QA.

**Resumen de directrices definidas:**

- **Almacén local:** copiar `SGH.Web/secrets.example.json` → `secrets.json` (no versionar).
- **Development:** User Secrets (`UserSecretsId` en `SGH.Web`) o `secrets.json`.
- **Servidor:** `ConnectionStrings__Hoteldb`, `CorreoSolicitudes__Contrasena`, etc. (doble guion bajo `__`).
- **Contenedores:** montar directorio y definir `SGH_SECRETS_DIRECTORY` (Key-per-file).
- **Prefijo opcional:** variables `SGH_*` mapeadas a configuración.
- **Arranque:** Production/Staging fallan si falta HOTELDB; entorno `Testing` excluido (tests automatizados).
- **Logs:** usar `EnmascaramientoSecretos`; prohibido registrar contraseñas o cadenas completas.

**Resultado / impacto esperado:**  
Ticket cerrable con política de secretos alineada a `PROMPT-PRINCIPAL.md`. Despliegues reproducibles por variables del host. Exclusiones en este ticket: Azure Key Vault / Vault SDK, rotación automática y auditoría centralizada de secretos.

---

## Alcance

| Dentro del ticket | Fuera (seguimiento aparte) |
|-------------------|----------------------------|
| Carga `secrets.json` + env + User Secrets | Integración Key Vault Azure |
| Validación cadena HOTELDB al inicio | Secretos de terceros (API keys ajenas) |
| Catálogo y enmascaramiento | Escaneo de repo en CI obligatorio |
| `OpcionesCorreoSolicitudes` en SMTP | Cifrar secretos en reposo en disco |
| Tests archivos sin `Password=` | Rotación automatizada SMTP/SQL |
| Guía `configuracion-secretos.md` | Terraform / IaC de secretos |

---

## Implementación técnica (referencia)

| Componente | Ruta |
|------------|------|
| Catálogo claves / env | `SGH.Application/Configuracion/CatalogoSecretos.cs` |
| Enmascaramiento | `SGH.Application/Configuracion/EnmascaramientoSecretos.cs` |
| Opciones correo | `SGH.Application/Configuracion/OpcionesCorreoSolicitudes.cs` |
| Carga y validación | `SGH.Web/Configuracion/ExtensionesAlmacenSecretos.cs` |
| Registro en host | `SGH.Web/Program.cs` (`AgregarAlmacenSecretosPorAmbiente`, `ValidarSecretosObligatorios`) |
| SMTP | `SGH.Infrastructure/Transversal/EnviadorCorreoSmtp.cs` |
| Plantilla secretos | `SGH.Web/secrets.example.json` |
| Política Git | `.gitignore` (`secrets.json`, `appsettings.*.local.json`) |

### Orden efectivo de configuración (último gana)

1. `appsettings.json` / `appsettings.{Environment}.json`
2. `secrets.json` (raíz contenido `SGH.Web`)
3. Directorio `SGH_SECRETS_DIRECTORY`
4. User Secrets (solo Development)
5. Variables de entorno ASP.NET Core
6. Variables con prefijo `SGH_`

### Variables de entorno principales

| Secreto | Variable |
|---------|----------|
| SQL HOTELDB | `ConnectionStrings__Hoteldb` |
| SMTP password | `CorreoSolicitudes__Contrasena` |

---

## Despliegue / configuración

**Development**

```bash
cp SGH.Web/secrets.example.json SGH.Web/secrets.json
# Editar secrets.json

dotnet user-secrets set "ConnectionStrings:Hoteldb" "Server=...;Database=HOTELDB;..." --project SGH.Web
```

**Production / Staging**

- Definir `ConnectionStrings__Hoteldb` en el proveedor de hosting.
- Configurar SMTP: `CorreoSolicitudes__*` y secretos en el almacén del host.
- Verificar arranque: la app debe iniciar solo si la cadena está presente.

Detalle operativo: `Docs/prompt-proyecto/configuracion-secretos.md`.

---

## Pruebas y criterios de aceptación

```bash
dotnet test SGH.sln -c Release
```

| Criterio | Estado |
|----------|--------|
| `appsettings.json` sin `Password=` / credenciales | Cumplido |
| Carga multi-fuente documentada | Cumplido |
| Falla controlada Production sin cadena | Cumplido |
| `EnmascaramientoSecretosTests` | Cumplido |
| `ArchivosSinCredencialesTests` | Cumplido |
| Suite Release completa | Cumplido (**114** tests) |

### QA manual sugerido

1. Arrancar Web sin `ConnectionStrings:Hoteldb` en Development → log de advertencia, sin crash en Testing/Development.
2. Definir variable `ConnectionStrings__Hoteldb` y reiniciar → log con cadena enmascarada (`Password=***`).
3. Confirmar que `secrets.json` no aparece en `git status` tras crearlo.

---

## Riesgos y seguimiento

| Tema | Nota |
|------|------|
| Olvido de secretos en dev | Primera operación BD fallará; seguir guía local |
| `secrets.json` en CI | No crear en pipelines; solo env vars |
| Ejemplos en docs | Usar `<secreto>` / placeholders, nunca valores reales |

---

## Referencias cruzadas

- Guía equipo: `Docs/prompt-proyecto/configuracion-secretos.md`
- Épica: `Docs/SGH-1.9-configuracion-secretos.md`
- Registro actividad: `Docs/actividades/SGH-1.9-configuracion-secretos.md`
- Correo (SGH 1.8): secretos SMTP en misma convención