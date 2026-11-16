# Ticket · SGH 2.1 — Maestros base (Listas, Empresas, Áreas, Divisiones, UN, Departamentos)

Documento para copiar al gestor de incidencias.  
**Estado:** **PENDIENTE** — épica **no implementable en un solo cierre**; ejecutar por **fases** (ver roadmap).  
**Plan:** `Docs/SGH-2.1-maestros-base.md`

---

## Resumen (descripción del ticket)

**Objetivo:** Migrar el **CRUD** de los **41 catálogos Listas** y de **Empresas**, **AreasEmpresa**, **Divisiones**, **UnidadesNegocio** y **Departamentos** al patrón NewRest **MVC + Mid**.

**Situación:** Operaciones migradas (Solicitud, Reserva, Censo) **consumen** algunos maestros en **solo lectura** (`UNG_/DIV_/DEP_Obtiene*`). No existen aún pantallas de mantenimiento en NewRest.

**Decisión de gestión:** La épica **2.1 queda registrada como pendiente** y se subdivide en tickets hijos (inventario, lotes de Listas, Empresas, Áreas, organización).

---

## Texto estándar para registro de actividad (copiar)

**Descripción:**  
Se documentó la épica **SGH 2.1 · Maestros base** en NewRest_PORT: alcance (41 catálogos `Listas` + cinco entidades organizacionales), estado actual (solo lectura parcial en Solicitud), dependencias con épicas 1.5–1.9 y **plan de migración progresivo** por sub-épicas. **No se implementó código** en esta actividad; se deja constancia formal de pendiente y desglose para no bloquear el roadmap con un ticket monolítico.

**Objetivo de la documentación:**  
Alinear al equipo y al gestor de incidencias en un cierre **por fases**, con inventario previo de HOTELDB y patrón técnico repetible por catálogo.

**Resumen de directrices definidas:**

- **Un módulo / un Mid** por dominio o lote de Listas (no un único `MidMaestros` genérico).
- **Inventario primero** (SGH 2.1.0): lista de 41 códigos Lista, SP, pantallas legado.
- **Cada sub-ticket:** vista Razor vacía + POST Mid + repositorio SP + tests + menú.
- **Jerarquía:** Empresas → Areas → (Divisiones, UN, Departamentos) respetando FK legado.
- **Reutilizar** SP `*_Obtiene*` existentes; agregar CUD solo con contrato documentado.
- **Cierre 2.1:** cuando todas las sub-épicas estén en `main`, no antes.

**Resultado / impacto esperado:**  
Épica 2.1 **abierta** con roadmap claro; tickets hijos acotados y estimables. Reducción de riesgo de regresión en Solicitud/Reserva al mantener combos de organización hasta migrar CRUD completo.

---

## Alcance de la épica (completa, futura)

| Incluido | Excluido de 2.1 (otras épicas) |
|----------|--------------------------------|
| CRUD Listas (41) | Usuarios / perfiles (otro módulo) |
| CRUD Empresas y áreas | Solicitud Empresa (gestión operativa) |
| CRUD Div / UN / Dep | Hotel / habitaciones físicas (`Modulos` campamento) |
| Menú Configuración maestros | Migración masiva de informes |

---

## Estado actual vs objetivo

| Entidad | Hoy | Objetivo 2.1 |
|---------|-----|--------------|
| Listas (41) | Sin UI NewRest | CRUD por catálogo |
| Empresas | Sin UI | CRUD |
| AreasEmpresa | Sin UI | CRUD |
| Divisiones | Combo solicitud | CRUD + combo |
| UnidadesNegocio | Combo solicitud | CRUD + combo |
| Departamentos | Combo solicitud | CRUD + combo |

---

## Sub-tickets sugeridos (backlog)

1. **SGH 2.1.0** — Inventario 41 listas + matriz SP / mensajes / menú legado.  
2. **SGH 2.1.1** — Lote 1 Listas (definir códigos tras inventario).  
3. **SGH 2.1.2** — Empresas.  
4. **SGH 2.1.3** — AreasEmpresa.  
5. **SGH 2.1.4** — Divisiones, UnidadesNegocio, Departamentos (CRUD).  
6. **SGH 2.1.5** — Integración menú + QA cruzada.

---

## Prueba de cierre (épica completa, futura)

```bash
dotnet test SGH.sln -c Release
```

Manual: cada maestro desde menú Configuración; crear/editar/eliminar (o baja lógica) con perfil autorizado; comprobar combos en Solicitud.

---

## Referencias

- `Docs/SGH-2.1-maestros-base.md`
- `Docs/prompt-proyecto/maestros-base-plan.md`
- `Docs/actividades/SGH-2.1-maestros-base-pendiente.md`
- `Docs/prompt-proyecto/flujo-mvc-mid.md`