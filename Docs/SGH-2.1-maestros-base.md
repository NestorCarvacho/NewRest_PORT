# SGH 2.1 · Maestros base (Listas, Empresas, Áreas, Divisiones, UN, Departamentos)

## Estado

| Campo | Valor |
|-------|--------|
| **Estado global** | **PENDIENTE** — no cerrable en una sola actividad |
| **Estrategia** | Migración **progresiva** por sub-épica / ticket |
| **Patrón** | Pantalla **MVC + Mid** (`Docs/prompt-proyecto/flujo-mvc-mid.md`) |
| **Dependencias previas** | SGH 1.5 (menú), 1.6 (layout/errores), 1.7 (UI), 1.9 (secretos) |

## Objetivo

Migrar el **CRUD** de:

1. **41 catálogos** de la entidad **`Listas`** (parámetros y tablas maestras de HOTELDB; ej. `usrpasshist` ya consumido desde política de contraseña).
2. **Empresas** (mandantes / empresas alojamiento según modelo legado).
3. **AreasEmpresa**
4. **Divisiones**
5. **UnidadesNegocio**
6. **Departamentos**

## Situación actual en NewRest_PORT (as-is)

| Dominio | En NewRest hoy | Notas |
|---------|----------------|-------|
| Listas | No hay pantallas Mid | Valores leídos en BD / reglas puntuales (`Listas.*` en SP) |
| Empresas | No migrado CRUD | Alcance por `EmpresaMandanteId` en sesión |
| AreasEmpresa | No migrado | — |
| Divisiones | **Solo lectura** en solicitud | `DIV_ObtieneDivisiones` (combo organización) |
| UnidadesNegocio | **Solo lectura** en solicitud | `UNG_ObtieneUnidadesNegocio` |
| Departamentos | **Solo lectura** en solicitud | `DEP_ObtieneDepartamentos` |
| Menú | Entradas legado **no migradas** | p. ej. Configuración → Listas / empresas (contrastar `Menus` en HOTELDB) |

No inventar contratos: cada sub-módulo exige inventario de SP legado, mensajes en `Docs/as-is/` y reglas en `SGH.Domain` cuando aplique.

## Por qué no es un solo ticket

- **Volumen:** 41 listas + 5 entidades relacionales ⇒ decenas de pantallas, SP y pruebas.
- **Riesgo:** mezclar catálogos heterogéneos en un único Mid rompe mantenibilidad y QA.
- **Paralelismo:** equipos pueden avanzar por **lotes de Listas** mientras Empresas define el árbol organizacional.
- **Menú:** cada pantalla requiere entrada en `Menus`/`MapeoEnlacesMenu` y política (`SGH 1.5`).

## Roadmap propuesto (sub-épicas)

| Fase | ID sugerido | Alcance | Entregable mínimo |
|------|-------------|---------|-------------------|
| **0 — Inventario** | SGH 2.1.0 | Matriz 41 listas + SP por tabla | `Docs/datos/maestros-listas-inventario.md` |
| **1 — Listas (lotes)** | SGH 2.1.1a…n | 5–8 catálogos Lista por ticket | Módulo `Listas/{Codigo}` MVC+Mid |
| **2 — Empresas** | SGH 2.1.2 | CRUD Empresas + alcance mandante | `/Empresas/Datos` + `MidEmpresas` |
| **3 — Áreas** | SGH 2.1.3 | CRUD AreasEmpresa (depende Empresa) | Mid + reglas jerarquía |
| **4 — Organización** | SGH 2.1.4 | Divisiones, UN, Departamentos (CRUD completo, no solo combo) | Extiende SP existentes con CUD |
| **5 — Cierre** | SGH 2.1.5 | Menú Configuración completo, regresión escritorio | Checklist QA global |

Cada fase cierra con: `dotnet test`, prueba manual escritorio, registro en `Docs/actividades/`.

## Plantilla técnica por pantalla

Ver `Docs/prompt-proyecto/maestros-base-plan.md`.

## Criterios de aceptación de la épica 2.1 (cuando esté completa)

- [ ] 41 catálogos Lista con CRUD Mid según legado.
- [ ] CRUD Empresas / Areas / Divisiones / UN / Departamentos.
- [ ] Autorización por perfil-menú; alcance campamento/mandante en mutaciones.
- [ ] Sin SQL en C#; SP existentes respetados o nuevos documentados.
- [ ] Documentación `as-is` y actividades por sub-épica.

**Hasta entonces:** la actividad **SGH 2.1** permanece **abierta / pendiente**; solo se marcan **cerradas** las sub-épicas individuales.