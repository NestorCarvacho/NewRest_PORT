# Ticket · SGH 1.9 — Gestión de configuración y secretos

**Estado:** implementado en rama `cursor/sgh-1-9-config-secretos-f6a4`.

---

## Resumen

**Objetivo:** Retirar credenciales del código y de `appsettings` versionados, usando un **almacén de secretos por ambiente**.

**Solución:** Carga escalonada (`secrets.json`, Key-per-file, User Secrets, variables de entorno), catálogo de claves, validación al inicio, enmascaramiento en logs y plantillas `secrets.example.json` / guía operativa.

---

## Texto estándar para registro de actividad (copiar)

**Descripción:**  
Se implementó SGH 1.9: extensión `AgregarAlmacenSecretosPorAmbiente`, validación de `ConnectionStrings:Hoteldb` en producción, plantilla `secrets.example.json`, actualización de `.gitignore`, catálogo `CatalogoSecretos` y enmascaramiento para diagnósticos. Las credenciales SMTP y SQL se leen solo desde secretos/entorno; `appsettings.json` queda sin secretos.

**Objetivo de la documentación:**  
Onboarding y despliegue seguro por ambiente sin filtrar credenciales al repositorio.

**Resumen de directrices:**  
(1) No versionar `secrets.json`. (2) Usar `ConnectionStrings__Hoteldb` en servidores. (3) Development: User Secrets o `secrets.json`. (4) Testing: sin validación obligatoria de BD. (5) Logs sin contraseñas.

**Resultado / impacto esperado:**  
Cumplimiento de política de secretos; despliegues reproducibles con variables del host.

---

## Criterios de aceptación

| Criterio | Estado |
|----------|--------|
| `appsettings.json` sin credenciales | Cumplido |
| Almacén por ambiente documentado | Cumplido |
| Falla controlada en Production sin cadena | Cumplido |
| Tests enmascaramiento + archivos limpios | Cumplido |
| `dotnet test SGH.sln -c Release` | Ejecutar en CI |

---

## Referencias

- `Docs/prompt-proyecto/configuracion-secretos.md`
- `Docs/actividades/SGH-1.9-configuracion-secretos.md`