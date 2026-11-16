# Registro de actividad — SGH 2.1 Maestros base (PENDIENTE)

**Épica / ticket:** SGH 2.1 · Listas, Empresas, Áreas, Divisiones, Unidades de negocio y Departamentos  
**Fecha:** 2026-11-16  
**Estado:** **PENDIENTE** — documentación y roadmap; implementación **progresiva**  
**Tipo de cierre:** No aplica (épica abierta; cerrar solo sub-épicas)

**Ticket gestor:** [`../SGH-2.1-documentacion-ticket.md`](../SGH-2.1-documentacion-ticket.md)  
**Roadmap:** [`../SGH-2.1-maestros-base.md`](../SGH-2.1-maestros-base.md)

---

## Descripción

Se registró la épica de migración de **maestros base** (41 catálogos `Listas` y entidades Empresas / AreasEmpresa / Divisiones / UnidadesNegocio / Departamentos). En NewRest_PORT las operaciones core ya usan **parte** de la jerarquía organizacional en **solo lectura** (Solicitud); no hay CRUD de mantenimiento. La actividad queda **pendiente** porque el volumen exige **varios tickets** (inventario + lotes de Listas + entidades).

## Objetivo de la documentación

Dejar en el gestor de incidencias un registro honesto: **2.1 no se implementa de una sola vez**, con fases numeradas, plantilla técnica y inventario por completar en HOTELDB.

## Resumen de directrices definidas

- Épica **2.1 abierta**; hijos **2.1.0 … 2.1.5**.
- Primero **inventario** de 41 listas (`Docs/datos/maestros-listas-inventario.md`).
- Cada implementación sigue **flujo MVC + Mid**.
- Lotes de **5–8** catálogos Lista por ticket.
- Cierre global 2.1 solo cuando todos los hijos estén en `main`.

## Resultado / impacto esperado

Planificación compartida; evita un PR monolítico; base para estimar y priorizar Configuración en el menú administración.

---

## Anexo — Bloque para pegar en gestor (estado pendiente)

**SGH 2.1 · Maestros base** — **Estado: PENDIENTE / en curso por fases.** Alcance: CRUD 41 Listas + Empresas + AreasEmpresa + Divisiones + UnidadesNegocio + Departamentos. Hoy: solo lectura parcial (UNG/DIV/DEP) en Solicitud. **No cerrar** el ticket padre hasta sub-épicas 2.1.0–2.1.5. Documentación: `Docs/SGH-2.1-documentacion-ticket.md`, plan `Docs/prompt-proyecto/maestros-base-plan.md`. **Próximo paso:** SGH 2.1.0 inventario en HOTELDB.