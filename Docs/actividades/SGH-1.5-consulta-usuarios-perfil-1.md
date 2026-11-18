# Registro de actividad — Consulta de usuarios y asignaciones con PerfilId = 1

**Épica / ticket:** SGH 1.5 · Autorización por perfiles y menú dinámico  
**Fecha:** 2026-11-16  
**Rama de referencia:** `cursor/sgh-1-5-menu-autorizacion-f6a4`  
**Referencias en repo:** `Docs/datos/diccionario-hoteldb-perfiles-menus.md`, `Docs/SGH-1.5-documentacion-ticket.md`, `Docs/login-pendiente-revision-base-de-datos.md`

---

## Descripción

Se documentaron consultas T-SQL reutilizables sobre **HOTELDB** para localizar **usuarios y asignaciones campamento–perfil** asociadas al **`PerfilId = 1`** (perfil de administración con menú completo en muestras del diccionario). El registro distingue explícitamente **`dbo.PerfilesUsuario.perfilId`** (origen del segundo paso de login y del claim `perfilId` en sesión) de **`dbo.Usuarios.PerfilId`** (perfil por defecto en la fila de cuenta) y de **`dbo.PerfilesUsuario.id`** (identificador del combo de asignación, no el catálogo de perfil).

Este documento complementa el inventario de tablas y las muestras de menú para perfiles **1** y **137** ya descritas en el diccionario HOTELDB.

## Objetivo de la documentación

Proporcionar a desarrollo, QA y operaciones un marco **único y no ambiguo** para:

- Elegir cuentas de prueba con perfil administración (`PerfilId = 1`).
- Contrastar menú esperado (`MenusPerfil` / `he_sp_menu_obtener_por_perfil`) con usuarios que realmente pueden seleccionar ese perfil al iniciar sesión.
- Evitar errores habituales al filtrar por `id` de asignación en lugar de por `perfilId`.

## Resumen de directrices definidas

- **Modelo legado:** `Perfiles` (catálogo) ← `PerfilesUsuario` (usuario × campamento × perfil) → `Usuarios`; menú vía `MenusPerfil` × `Menus`. Diagrama y columnas en `Docs/datos/diccionario-hoteldb-perfiles-menus.md`.
- **Consulta canónica (login):** listar filas activas en `PerfilesUsuario` con `perfilId = 1`, uniendo nombre de perfil y datos de usuario.
- **Consulta complementaria:** usuarios cuyo **perfil por defecto** en `Usuarios` es `1` (no sustituye a las asignaciones del combo).
- **Menú del perfil:** `EXEC dbo.he_sp_menu_obtener_por_perfil @PerfilId = 1` (script en `SGH.Infrastructure/Navegacion/Persistence/001_sp_menu.sql`) o equivalente con join directo a `Menus` / `MenusPerfil`.
- **Convención de nombres:** en documentación y tickets usar **`PerfilId`** para el catálogo `dbo.Perfiles.Id`; usar **`AsignacionId`** o `PerfilesUsuario.id` solo para la fila del selector de campamento–perfil.

## Resultado / impacto esperado

Queda una **fuente de verdad operativa** para auditoría y pruebas de SGH 1.5: quién puede operar con perfil 1, qué menú debería ver según BD y cómo validar sin mezclar identificadores. Reduce retrabajo en onboarding y en validación manual post-despliegue del SP de menú.

---

## Anexo A — Consultas T-SQL

### A1. Asignaciones activas con PerfilId = 1 (recomendada para QA / login)

```sql
SELECT
    pu.id              AS AsignacionId,
    pu.usuarioId,
    pu.usuarioRut,
    u.Nombre,
    u.Apellidos,
    pu.perfilId,
    p.Nombre           AS PerfilNombre,
    pu.campamentoId,
    pu.empresaMandanteId,
    pu.IsActive
FROM dbo.PerfilesUsuario AS pu
INNER JOIN dbo.Perfiles AS p
    ON p.Id = pu.perfilId
LEFT JOIN dbo.Usuarios AS u
    ON u.Id = pu.usuarioId
WHERE pu.perfilId = 1
  AND pu.IsActive = 1
  AND p.IsActive = 1
ORDER BY u.Nombre, u.Apellidos, pu.campamentoId;
```

### A2. Perfil por defecto en `Usuarios` (solo referencia)

```sql
SELECT
    u.Id,
    u.Rut,
    u.Nombre,
    u.Apellidos,
    u.PerfilId,
    p.Nombre AS PerfilNombre
FROM dbo.Usuarios AS u
INNER JOIN dbo.Perfiles AS p
    ON p.Id = u.PerfilId
WHERE u.PerfilId = 1
  AND u.IsActive = 1
  AND p.IsActive = 1;
```

### A3. Una fila de asignación por `PerfilesUsuario.id` (no es PerfilId)

```sql
SELECT *
FROM dbo.PerfilesUsuario
WHERE id = 1;
```

### A4. Ítems de menú permitidos para el perfil 1

```sql
EXEC dbo.he_sp_menu_obtener_por_perfil @PerfilId = 1;
```

Equivalente sin procedimiento:

```sql
SELECT m.Id, m.Nombre, m.Link, m.PadreId, m.Ubicacion, m.Orden
FROM dbo.MenusPerfil AS mp
INNER JOIN dbo.Menus AS m
    ON m.Id = mp.MenuId
WHERE mp.PerfilId = 1
  AND m.IsActive = 1
ORDER BY m.Ubicacion, m.Orden, m.Id;
```

## Anexo B — Relación con procedimientos de login

El segundo paso de ingreso consume asignaciones expuestas por `dbo.he_sp_usuario_obtener_campamentos_y_perfiles` (envoltorio de `USU_ObtieneCampamentosYPerfilesUsuario`). Los campos `Id`, `PerfilId` y `UsuarioId` de ese resultado deben alinearse con las columnas documentadas en A1. Ver `SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql` y `Docs/login-pendiente-revision-base-de-datos.md`.