# Autorización: ¿tiene RBAC?

## Conclusión

**Parcialmente.** El sistema usa **autorización por políticas de ASP.NET Core** apoyada en el **`PerfilId`** que viene de la base (asignación campamento–perfil al iniciar sesión), no un RBAC clásico con matriz rol→permiso persistida y evaluada por rol.

Desde **SGH 1.5**, las políticas operativas también exigen un ítem en **`MenusPerfil`** para el `PerfilId` de sesión (`MenuAccesoAuthorizationHandler` + `he_sp_menu_obtener_por_perfil`). Si la BD no devuelve menú, se usa un respaldo mínimo equivalente al escritorio migrado.

En la práctica hoy hay **dos bandas** (más el filtro por menú anterior):

| Banda | `PerfilId` | Acceso a módulos operativos |
|--------|------------|-----------------------------|
| Pasajero | `2` (`MatrizPerfiles.Pasajero`) | Solo lo que exige políticas con `PermitePasajero = true` (p. ej. `GestionarReservas`). |
| Operador (cualquier otro perfil de la sesión) | distinto de `2` | Mismas políticas operativas que un operador; **no** se discrimina por perfil 4, 5, 11, 68, 142, etc. |

El **`ClaimTypes.Role`** (`all` / `ideauno`) es legado de integración; **ninguna política registrada lo usa** (`RequireRole` no aparece en el código).

Complemento importante: el **alcance por campamento/mandante** (`AlcanceMid`, `ReglasAlcance`) es control de **contexto de sesión** (tipo ABAC por atributos de sesión), independiente del perfil y de RBAC.

---

## Componentes

### Autenticación

- Cookie (`CookieAuthenticationDefaults.AuthenticationScheme`).
- Tras login, `EmisorCookieSesion` arma el `ClaimsPrincipal` (`SGH.Web/Cuenta/EmisorCookieSesion.cs`).
- Claims relevantes: `perfilId`, `campamentoId`, `empresaMandanteId`, `tipoPerfil` (`pasajero` | `operador`), `ClaimTypes.Role`, y opcionalmente varios `capacidad` (ver abajo).

### “Roles” en dominio vs políticas

| Concepto | Origen | Uso en autorización HTTP |
|----------|--------|---------------------------|
| `PerfilId` | `dbo.Usuarios` / asignación campamento–perfil (`he_sp_usuario_*`) | **Sí** — `PerfilAutorizacionHandler` |
| `Rol` (`all`, `ideauno`) | `CompletarSesion` según integración | **No** en políticas actuales |
| Claims `capacidad` | Emitidos en cookie para no-pasajero | **No** — no hay `RequireClaim` ni handler que los lea |

Constantes de perfiles en código (`SGH.Domain/Cuenta/MatrizPerfiles.cs`): pasajero, operaciones A/B, IdeaUno, gestión TI, configuración. Solo **`EsPasajero`** participa en autorización.

---

## Políticas (`PoliticasAplicacion`)

Registro en `SGH.Web/Program.cs`. Todas exigen usuario autenticado + `PerfilAutorizacionRequirement`.

| Política | `PermitePasajero` | Comportamiento del handler |
|----------|-------------------|----------------------------|
| `AdministrarUsuarios` | `false` | Solo perfiles con `perfilId ≠ 2` |
| `GestionarReservas` | `true` | Pasajero y operadores |
| `EjecutarCheckInOut` | `false` | Solo no-pasajero |
| `GestionarCensos` | `false` | Solo no-pasajero |

Implementación: `SGH.Web/Autorizacion/PoliticasAplicacion.cs` (`PerfilAutorizacionHandler` lee claim `perfilId`).

Pruebas: `SGH.Tests/Cuenta/PoliticasAutorizacionTests.cs`.

---

## Dónde se aplica cada política

| Recurso | Política |
|---------|----------|
| `Pages/Index` | Solo `[Authorize]` (cualquier autenticado) |
| `Pages/Reserva/Datos`, `Pages/Solicitud/Datos` | `GestionarReservas` |
| `Pages/Censo/Datos` | `GestionarCensos` |
| `ReservasController`, `SolicitudesController` | `GestionarReservas` |
| `CensoController`, `CensoInformesController` | `GestionarCensos` |
| `Pages/Cuenta/Usuarios` (convención Razor) | `AdministrarUsuarios` — **página aún no implementada en el repo** |
| Resto de carpetas Razor | `[Authorize]` por convención `AuthorizeFolder("/")` |
| Login, cambio de clave, error, etc. | Anónimo explícito |

### Política sin cablear

`EjecutarCheckInOut` está **definida** en `Program.cs` pero **no** decora ningún controlador ni página. El check-in/out del escritorio hoy pasa por APIs de reservas bajo `GestionarReservas`.

### UI vs servidor

En `_Layout.cshtml` se oculta el ítem “Usuarios” si `tipoPerfil` es `pasajero`. Eso **no sustituye** la política en servidor; es solo presentación.

---

## Comparación con RBAC “textbook”

| Criterio RBAC típico | En NewRest_PORT |
|----------------------|-----------------|
| Roles nombrados asignados al usuario | Perfiles en BD; en HTTP solo se distingue pasajero vs resto |
| Permisos granulares por rol | Tres claims `capacidad` en cookie; **sin enforcement** |
| Matriz rol→permiso (p. ej. en BD o config) | No existe en aplicación |
| Separación admin / operador / solo lectura por `PerfilId` | No implementada (68, 142, 4, 5 comparten las mismas políticas operativas) |
| Autorización en cada endpoint | Sí, en Mid migrados + páginas `Datos` |
| Alcance de datos (campamento) | Sí, por sesión — no es RBAC |

---

## Relación con el legado

`Docs/login-pendiente-revision-base-de-datos.md` (en el repo de producto) describe perfiles, asignaciones y procedimientos `he_sp_usuario_*`. La autorización migrada **reutiliza `PerfilId` de sesión** pero **no** replica aún una matriz fina de permisos del sistema legacy si existía en menús o SPs.

---

## Recomendaciones si se quiere RBAC completo

1. Decidir permisos atómicos (p. ej. `reservas.leer`, `censo.guardar`, `usuarios.administrar`).
2. Mapear cada `PerfilId` (o grupo) → permisos en dominio (`MatrizPerfiles` o tabla/config).
3. Sustituir o complementar `PerfilAutorizacionRequirement` por `RequireClaim("capacidad", …)` o requirements por permiso.
4. Cablear `EjecutarCheckInOut` a los endpoints/páginas de movimiento de reserva.
5. Aplicar `AdministrarUsuarios` cuando exista el módulo de usuarios.
6. Mantener **alcance de campamento** como capa aparte (siempre desde `LectorSesion`, nunca desde el cliente).

---

## Referencias de código

- Políticas y handler: `SGH.Web/Autorizacion/PoliticasAplicacion.cs`
- Registro: `SGH.Web/Program.cs` (`AddAuthorization`)
- Emisión de claims: `SGH.Web/Cuenta/EmisorCookieSesion.cs`
- Perfiles: `SGH.Domain/Cuenta/MatrizPerfiles.cs`
- Alcance de datos: `SGH.Domain/Alcance/ReglasAlcance.cs`, `SGH.Application/Mid/AlcanceMid.cs`
