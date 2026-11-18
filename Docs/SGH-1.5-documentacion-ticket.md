# Ticket · SGH 1.5 — Autorización por perfiles y menú dinámico

Documento para copiar al gestor de incidencias (Jira, Linear, Azure DevOps).  
Rama de implementación: `cursor/sgh-1-5-menu-autorizacion-f6a4`.

---

## Resumen (descripción del ticket)

**Objetivo:** Alinear NewRest_PORT con el legado HOTELDB en **autorización por perfil** y **menú de escritorio**, usando las tablas `Menus` y `MenusPerfil` junto al `PerfilId` de la sesión (claim `perfilId` tras login).

**Problema que resuelve:** El menú estaba fijo en `_Layout.cshtml` y las políticas HTTP solo distinguían **pasajero (2)** vs **cualquier otro perfil**, sin respetar la matriz menú–perfil de base de datos.

**Solución entregada:** Consulta del menú por perfil, render dinámico en el escritorio y políticas ASP.NET Core que exigen **perfil apto + ítem de menú** para los módulos migrados.

---

## Alcance

| Dentro del ticket | Fuera (seguimiento aparte) |
|-------------------|----------------------------|
| Menú dinámico por `PerfilId` | CRUD pantalla «Perfiles» / administración de menú |
| Autorización en páginas y controladores con políticas existentes + menú | Autorización por `opcion` dentro de cada Mid |
| SP nuevo `he_sp_menu_obtener_por_perfil` | ESS, mensaje autorización, política de aceptación en login |
| Mapeo de `Menus.Link` legado → páginas Razor migradas | Matriz completa de todos los ítems legado (centenares) |
| Respaldo de menú si la BD no responde o no hay filas | i18n automático de textos que vienen de BD |

---

## Modelo de datos (HOTELDB)

| Tabla | Uso |
|-------|-----|
| `Menus` | Ítems: `Nombre`, `Link`, `Ubicacion`, `Orden`, `PadreId`, `IsActive` |
| `MenusPerfil` | Qué `MenuId` puede ver cada `PerfilId` |
| `Perfiles` | Catálogo de perfiles |
| `PerfilesUsuario` | Asignación login (campamento + perfil); ya usada en ingreso |

Detalle columnas: `Docs/datos/diccionario-hoteldb-perfiles-menus.md`  
Diccionario completo: `Docs/datos/Diccionario_de_datos_HotelDB.xlsx`

---

## Implementación técnica

### 1. Base de datos

- Script: `SGH.Infrastructure/Navegacion/Persistence/001_sp_menu.sql`
- Procedimiento: `dbo.he_sp_menu_obtener_por_perfil @PerfilId`
- **Acción de despliegue:** ejecutar el script en HOTELDB (idempotente) en el mismo proceso que el resto de scripts del repo.

### 2. Capas

| Capa | Componente | Función |
|------|------------|---------|
| Domain | `MenuPerfilItem`, `IConsultaMenuPerfil` | Modelo de fila de menú |
| Infrastructure | `ConsultaMenuPerfil` | Dapper → SP |
| Application | `ServicioMenuPerfil` | Caché 10 min por `PerfilId` |
| Application | `ConstruirMenuEscritorio` | Agrupa por `Ubicacion`, ordena |
| Application | `MapeoEnlacesMenu` | `Link` legado → ruta Razor + clave módulo |
| Application | `MenuRespaldoEstatico` | Submenú mínimo si no hay BD/datos |
| Web | `MenuPerfilViewComponent` + `Components/MenuPerfil/Default.cshtml` | Render del menú |
| Web | `MenuAccesoAuthorizationHandler` | Valida acceso por clave de módulo en menú |

### 3. Claves de módulo (`MenuClaves`)

| Clave | Página Razor | Política ASP.NET |
|-------|--------------|------------------|
| `solicitud` | `/Solicitud/Datos` | `GestionarReservas` |
| `reserva` | `/Reserva/Datos` | `GestionarReservas` |
| `censo` | `/Censo/Datos` | `GestionarCensos` |
| `usuarios` | `/Cuenta/Usuarios` | `AdministrarUsuarios` |

**BD vs UI:** en HOTELDB, `nav` = raíz y `side` = hijos (`PadreId`). En NewRest los `side` van al **desplegable bajo cada botón de la barra** (hover/click), no al panel lateral de navegación. Ver perfiles **1** / **137** en `Docs/datos/diccionario-hoteldb-perfiles-menus.md`.

Mapeo de rutas SPA (`/app/gestion/censo`, `/app/gestion/miscensos` → `censo` / `censoQr`). Los informes (`/app/gestion/informe/censo`) quedan deshabilitados hasta migrar. Ampliar `MapeoEnlacesMenu.cs` según nuevos `Link`.

### 4. Políticas (regla combinada)

Cada política operativa exige **dos handlers**:

1. `PerfilAutorizacionHandler` — pasajero vs no pasajero (comportamiento previo).
2. `MenuAccesoAuthorizationHandler` — el perfil debe tener **al menos una** clave de módulo listada en su menú (OR entre claves de la misma política).

Configuración: `SGH.Web/Program.cs`.

### 5. UI

- `_Layout.cshtml` invoca `@await Component.InvokeAsync("MenuPerfil")`.
- Ítems con página migrada: enlace con `data-modulo` (buscador y solapas siguen funcionando).
- Ítems sin mapeo: botón **deshabilitado** («No disponible»).
- Grupos de configuración: `Ubicacion` contiene «config», «campamento» o «administr» → icono engranaje (`menu-config`).

### 6. Respaldo (fallback)

Si el SP falla o `MenusPerfil` no tiene filas para el perfil:

- **Pasajero:** Solicitud + Reserva.
- **Operador:** Solicitud + Reserva + Censo + Usuarios (configuración).

Mientras dure el respaldo, la autorización usa el **mismo** conjunto de claves (no bloquea desarrollo sin HOTELDB).

---

## Actividades (estado sugerido para el ticket)

| ID | Actividad | Estado |
|----|-----------|--------|
| B1 | SP `he_sp_menu_obtener_por_perfil` + consulta infra | Hecho |
| D1 | Servicio + ViewComponent menú | Hecho |
| D2 | Reemplazo menú estático en layout | Hecho |
| C1 | Handler autorización por menú | Hecho |
| C3 | Políticas actualizadas en `Program.cs` | Hecho |
| E1 | Tests `SGH.Tests/Navegacion` | Hecho (7) |
| A1 | Inventario SP menú legado además del nuevo | Pendiente |
| A2 | Matriz `Link` BD vs rutas Razor (todos los perfiles piloto) | Pendiente |
| B3 | Validación manual en LocalDB con perfiles 2, 4, 68, 142 | Pendiente QA |
| C2 | Claims `capacidad` dinámicos desde menú (opcional) | Pendiente / fuera |
| C4 | Mensajes de rechazo alineados AS-IS | Pendiente |
| D3 | Buscador solo ítems visibles (ya depende del DOM renderizado) | Hecho indirectamente |
| E2 | Prueba manual multi-perfil documentada | Pendiente QA |

Epic detallado y criterios históricos: `Docs/SGH-1.5-autorizacion-perfiles-menu-dinamico.md`.

---

## Criterios de aceptación

Marcar en QA según entorno **con HOTELDB actualizada** (`001_sp_menu.sql` aplicado):

- [ ] Tras login con **perfil pasajero**, el menú muestra solo ítems asignados en `MenusPerfil` para ese perfil (o respaldo pasajero si no hay datos).
- [ ] Tras login con **perfil operativo** (p. ej. 4 u 68), el menú difiere entre perfiles si `MenusPerfil` difiere.
- [ ] Abrir **URL directa** a `/Censo/Datos` sin ítem censo en menú → acceso denegado (página o JSON coherente).
- [ ] POST a `Mid/...` con sesión sin menú para el módulo → `registros.resultado = false` por sesión (flujo existente de no autorizado).
- [ ] Ítems legado sin pantalla migrada aparecen deshabilitados, no como enlace roto.
- [ ] Alcance campamento/mandante **no** cambia por el menú (sigue `LectorSesion` / `AlcanceMid`).

---

## Checklist de despliegue

1. [ ] Ejecutar `SGH.Infrastructure/Navegacion/Persistence/001_sp_menu.sql` en HOTELDB.
2. [ ] Verificar que existen datos en `Menus` y `MenusPerfil` para perfiles de prueba.
3. [ ] Desplegar aplicación con rama mergeada.
4. [ ] Smoke: login + menú + abrir Solicitud/Reserva/Censo según perfil.
5. [ ] Revisar logs si el menú muestra solo respaldo (indica fallo de conexión o SP ausente).

---

## Checklist de pruebas manuales

| Paso | Acción | Resultado esperado |
|------|--------|-------------------|
| 1 | Login pasajero (perfil 2) | Menú sin Censo si BD no lo asigna |
| 2 | Login operaciones (perfil 4) | Menú según `MenusPerfil` |
| 3 | Buscar módulo en cabecera | Solo ítems visibles en menú |
| 4 | Abrir solapa desde menú | `data-modulo` abre panel |
| 5 | URL directa a módulo no permitido | Acceso denegado |

---

## Pruebas automatizadas

```bash
dotnet test --filter "FullyQualifiedName~Navegacion"
```

Cubre mapeo de enlaces y servicio de menú (incl. respaldo y acceso por clave).

Documentación RBAC actualizada: `Docs/prompt-proyecto/autorizacion-rbac.md`.

---

## Riesgos y seguimiento

| Riesgo | Mitigación |
|--------|------------|
| `Link` legado con nombres no contemplados | Ampliar `MapeoEnlacesMenu`; completar actividad A2 |
| Sin SP en BD → solo menú respaldo | Checklist de despliegue |
| Muchos ítems BD sin migración | Comportamiento acordado: visible deshabilitado |
| Caché 10 min tras cambio en `MenusPerfil` | Esperar o reiniciar app / invalidar sesión |

---

## Referencias rápidas

- Epic / backlog: `Docs/SGH-1.5-autorizacion-perfiles-menu-dinamico.md`
- Diccionario: `Docs/datos/diccionario-hoteldb-perfiles-menus.md`
- Login / perfiles: `Docs/login-pendiente-revision-base-de-datos.md`

---

## Texto corto para pegar como descripción del ticket

```
SGH 1.5 — Autorización por perfiles y menú dinámico

Implementar menú de escritorio desde HOTELDB (Menus + MenusPerfil) según
PerfilId de sesión, y reforzar autorización HTTP: políticas existentes más
validación de ítem de menú para módulos migrados (solicitud, reserva, censo,
usuarios).

Entregables: he_sp_menu_obtener_por_perfil, menú dinámico en layout, tests Navegacion.
Despliegue: ejecutar 001_sp_menu.sql en HOTELDB.

Doc ticket: Docs/SGH-1.5-documentacion-ticket.md
```