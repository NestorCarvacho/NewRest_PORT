# Arquitectura base para desarrollos SGH / NewRest

Documento de referencia para **nuevos proyectos y nuevos módulos**: misma solución en capas, mismo patrón **MVC + Mid**, mismo contrato JSON y mismas reglas de sesión.

Documentos de este paquete:

| Archivo | Uso |
|---------|-----|
| [`PROMPT-PRINCIPAL.md`](PROMPT-PRINCIPAL.md) | Instrucción corta para IA / equipo. |
| [`flujo-mvc-mid.md`](flujo-mvc-mid.md) | Migración pantalla a pantalla. |
| [`autorizacion-rbac.md`](autorizacion-rbac.md) | Políticas y perfiles. |
| [`directrices-desarrollo.md`](directrices-desarrollo.md) | Capas, datos, estilo, pruebas. |

En el repo de producto: `Docs/as-is/` (comportamiento legado por módulo).

---

## 1. Objetivo

- **Separar** reglas de negocio, orquestación, persistencia y UI.
- **Unificar** la forma de cada pantalla: un POST con `opcion`, respuesta `registros`.
- **Reutilizar** SQL Server y procedimientos existentes sin SQL inline en C#.
- **Acotar datos** por sesión (mandante, campamento, usuario desde el servidor).

---

## 2. Solución .NET (plantilla)

```
{Nombre}.sln
├── {Nombre}.Domain
├── {Nombre}.Application
├── {Nombre}.Infrastructure
├── {Nombre}.Web
└── {Nombre}.Tests
```

**Dependencias:** `Web → Application → Domain`; `Infrastructure → Application + Domain`. Domain sin referencias a otras capas.

**DI:** `Program.cs` → `AddInfraestructura(configuration)` → encadena `AddAplicacion()`.

Stack: **.NET 10**, **C# 14**, **SQL Server**, cookie de autenticación.

---

## 3. Responsabilidad por capa

(Ver también [`directrices-desarrollo.md`](directrices-desarrollo.md).)

- **Domain:** `Reglas*`, entidades, interfaces de dominio; sin SQL ni ASP.NET.
- **Application:** DTOs, casos de uso, **`Mid/`**, `AlcanceSesion`, `AddAplicacion()`. El controlador solo llama al Mid.
- **Infrastructure:** Commands/Queries, Dapper, `AD_Global`, scripts `Persistence/*.sql`.
- **Web:** `Pages/`, `Controllers/`, `RespuestaDeMid`, `LectorSesion`, `wwwroot/js`.

---

## 4. Flujo MVC + Mid

Ver diagrama y detalle en [`flujo-mvc-mid.md`](flujo-mvc-mid.md).

Resumen contrato: POST camelCase con `opcion`; respuesta `{ "registros": … }`; mandante/campamento/usuario solo desde sesión servidor.

---

## 5. Seguridad

[`autorizacion-rbac.md`](autorizacion-rbac.md) + `ReglasAlcance` / `AlcanceMid` para datos por campamento.

---

## 6. Checklist nuevo módulo

1. Domain: `Reglas*`
2. Application: DTOs, use cases, `Mid{Modulo}`
3. Infrastructure: consultas/comandos + SP
4. DI
5. Web: `Datos.cshtml`, Controller, `[Authorize]`
6. wwwroot JS
7. Tests con dobles
8. `Docs/as-is/` si hay legado

---

## 7. Proyecto greenfield

1. Cinco proyectos y referencias correctas.
2. Copiar transversales del repo referencia: `EjecucionMid`, `FiltroPeticion`, `AlcanceMid`, `RespuestaDeMid`, `LectorSesion`, extensiones DI, núcleo JS.
3. Connection string en configuración.
4. Un módulo piloto.
5. CI: build + test.
6. Carpeta **`prompt-proyecto`** en `Docs/` + regla Cursor en `.cursor/rules/`.

---

## 8. Módulos referencia (NewRest_PORT)

| Módulo | Mid | Controller |
|--------|-----|------------|
| Reserva | `MidReservas` | `ReservasController` |
| Censo | `MidCenso` | `CensoController` |
| Solicitud | `MidSolicitudes` | `SolicitudesController` |
| Informes | `MidInformesCenso` | `CensoInformesController` |

---

## 9. Verificación

```powershell
dotnet restore {Nombre}.sln
dotnet build {Nombre}.sln
dotnet test {Nombre}.sln
```
