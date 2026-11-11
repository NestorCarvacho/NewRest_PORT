# Directrices de Copilot para SGH

## Prompt principal del equipo

Toda la arquitectura y el prompt compartible viven en **[`Docs/prompt-proyecto/`](Docs/prompt-proyecto/README.md)**.

1. Leé primero [`Docs/prompt-proyecto/PROMPT-PRINCIPAL.md`](Docs/prompt-proyecto/PROMPT-PRINCIPAL.md).
2. Migrar pantallas: [`Docs/prompt-proyecto/flujo-mvc-mid.md`](Docs/prompt-proyecto/flujo-mvc-mid.md) (y regla Cursor en `Docs/prompt-proyecto/cursor/`).
3. Capas y checklists: [`Docs/prompt-proyecto/arquitectura-base.md`](Docs/prompt-proyecto/arquitectura-base.md).
4. Autorización: [`Docs/prompt-proyecto/autorizacion-rbac.md`](Docs/prompt-proyecto/autorizacion-rbac.md).
5. Estilo y datos: [`Docs/prompt-proyecto/directrices-desarrollo.md`](Docs/prompt-proyecto/directrices-desarrollo.md).

## Reglas de producto

- Antes de cambiar comportamiento funcional, consultá `Docs/as-is/` del repo.
- Reserva, Censo, Solicitud e informes usan controladores Mid; cuenta sigue en Razor Pages hasta migrar.
- `ConnectionStrings:Hoteldb`, procedimientos existentes y `Reglas*` en Domain se conservan.

## Verificación

```shell
dotnet build SGH.sln
dotnet test SGH.sln
```
