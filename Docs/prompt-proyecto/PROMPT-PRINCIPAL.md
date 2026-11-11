# Prompt principal — desarrollo SGH / NewRest

Usá este texto como **instrucción de sistema** o primer mensaje en cada proyecto que siga la arquitectura unificada. Detalle ampliado en los demás archivos **de esta misma carpeta**.

---

## Mandato

Desarrollás en **.NET 10 / C# 14**, **ASP.NET Core** (Razor Pages + controladores), **SQL Server** solo vía **procedimientos almacenados** (sin SQL en C#). Arquitectura en cinco proyectos: **Domain → Application → Infrastructure** y **Web**; **Domain** no referencia otras capas.

Cada pantalla de negocio migrada sigue **MVC + Mid**:

1. El navegador hace **un POST** JSON con propiedad **`opcion`** (camelCase) y antiforgery.
2. El **controlador** valida la opción contra el catálogo del Mid y pasa **`DatosSesion`** leída del servidor (**nunca** mandante/campamento/usuario desde el JSON del cliente).
3. **`Mid{Modulo}.EjecutarAsync`** empieza con **`AlcanceMid.BloquearSiSinAlcance`**; hace `switch (opcion)` y llama casos de uso + **`Reglas*`** en Domain.
4. Infrastructure ejecuta SP (Dapper lectura; `AD_Global` escritura donde aplica legado).
5. Respuesta HTTP **200** con `{ "registros": … }` o `{ "registros": { "resultado": false, "mensaje": "…" } }`.

El **controlador no llama casos de uso directamente**. Las reglas de negocio **no** van en JS, controlador ni repositorio: viven en **`SGH.Domain`** (`ReglasCuenta`, `ReglasReserva`, `ReglasAlcance`, …).

## Orden en cada operación

1. Alcance de sesión (`AlcanceSesion` / `ReglasAlcance`).
2. Regla de dominio (mensaje tal cual si falla).
3. Persistencia.
4. En mutaciones con `id`: comprobar que el registro pertenece al campamento de sesión.
5. `EjecucionMid.Ok` o `Fallo`.

## Autorización y datos

- Endpoints con `[Authorize(Policy = …)]`; hoy se distingue sobre todo **pasajero (perfil 2)** vs operador (ver `autorizacion-rbac.md`).
- Alcance por **campamento/mandante** es independiente del perfil y obligatorio en Mid y casos de uso.

## Al migrar o crear un módulo

Seguí **`flujo-mvc-mid.md`** (archivos, rutas, contrato, JS). Checklist en **`arquitectura-base.md`**. Antes de cambiar reglas funcionales, consultá **`Docs/as-is/`** del producto si existe.

## Calidad

- `record` para DTOs; `async`/`await`; pruebas unitarias sin BD real para dominio y casos de uso.
- BCrypt para contraseñas nuevas; secretos fuera del repo.
- Cambios mínimos; `dotnet build` y `dotnet test` antes de cerrar.

## No hacer

- SQL ad hoc en C#; saltarse el Mid; copiar `campamentoId` del cliente; inventar mensajes que contradigan `Docs/as-is`.
- Traer del ejemplo `testMVCCsharp` MySQL ni SP de productos: solo el **patrón** Mid.

---

**Siguiente paso:** abrí `arquitectura-base.md` y `flujo-mvc-mid.md` en este mismo directorio para el detalle completo.
