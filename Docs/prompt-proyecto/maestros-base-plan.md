# Plan de migración — maestros base (SGH 2.1)

Usar este checklist **por cada** catálogo o entidad. No abrir un solo PR con todos los maestros.

## Antes de codificar (obligatorio)

1. Identificar en HOTELDB: tabla, SP `Obtiene` / `Guarda` / `Elimina` (nombres exactos).
2. Copiar mensajes y validaciones a `Docs/as-is/` o `Reglas*` en Domain.
3. Confirmar ítem de menú (`Menus.Link`) y `PerfilId` que accede.
4. Registrar fila en inventario (`Docs/datos/maestros-listas-inventario.md` cuando exista).

## Archivos (patrón MVC + Mid)

| Pieza | Ruta típica |
|-------|-------------|
| Vista | `SGH.Web/Pages/{Modulo}/Datos.cshtml` |
| Script | `SGH.Web/wwwroot/{Modulo}/Js/datos.js` |
| Controlador | `SGH.Web/Controllers/{Modulo}Controller.cs` |
| Mid | `SGH.Application/Mid/Mid{Modulo}.cs` |
| Repositorio | `SGH.Infrastructure/...` + `AD_Global` |
| Filtro POST | `SGH.Application/.../Filtro{Entidad}.cs` |
| Reglas | `SGH.Domain/.../Reglas*.cs` (si hay invariantes) |
| Tests | Dominio + Mid (sin BD si es posible) |

## Opciones Mid estándar (ajustar según legado)

| opcion | Uso |
|--------|-----|
| `listar` | Grilla inicial |
| `obtener` | Modal editar |
| `crear` | Alta |
| `editar` | Mutación |
| `eliminar` | Baja / soft delete según regla |

## Integración menú

- Agregar constante en `MenuClaves` / mapeo en `MapeoEnlacesMenu` cuando el `Link` legado tenga destino Razor.
- Política `[Authorize]` alineada a perfil administración.

## Lotes de Listas (recomendación)

Agrupar catálogos por **afinidad funcional** (ej. parámetros de cuenta, parámetros de reserva, geografía), **5 a 8 por ticket**, nunca las 41 juntas.

## Definition of Done (sub-ticket)

- [ ] Mid + controlador + vista + JS
- [ ] SP ejecutados sin SQL en C#
- [ ] Alcance mandante/campamento en mutaciones si aplica
- [ ] `dotnet test` Release
- [ ] Registro en `Docs/actividades/`