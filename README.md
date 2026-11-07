# SGH26 — Sistema de Gestión de Hotelería

SGH es el desarrollo de una plataforma para gestionar la operación hotelera de campamentos: su infraestructura y ocupación, las solicitudes y reservas de alojamiento, los servicios operativos y los recursos asociados. El alcance funcional se basa en los módulos documentados del sistema actual y busca conservar sus reglas y flujos relevantes mientras se construye una solución con arquitectura desacoplada.

El alcance funcional previsto toma como base el comportamiento del sistema hotelero existente y considera 50 módulos. Ese alcance orienta el desarrollo, pero no significa que todos los módulos ya estén implementados en esta nueva solución.

## Alcance funcional previsto

Los módulos documentados se agrupan en las siguientes áreas:

- **Administración y organización:** campamentos; empresas, áreas y mandantes; departamentos, divisiones y unidades de negocio; pabellones, módulos, tipos de módulo, habitaciones, tipos de instalación y plantillas de ubicación; proyectos, grupos y actividades.
- **Acceso y configuración:** autenticación, usuarios, perfiles y menús, módulos, listas maestras, turnos y política de aceptación.
- **Alojamiento y ocupación:** solicitudes de alojamiento y sus pasajeros, reservas, reserva de instalaciones, censos, informes de disponibilidad y ocupabilidad, y desalojos con sus objetos y documentos.
- **Aseo y operación de habitaciones:** checklists, grupos y actividades de aseo, supervisión de aseo y estándares de validación de habitación.
- **Activos, artículos e inventarios:** activos; artículos, grupos, estados y precios; registro de artículos y registro por habitación; inventarios de habitación e ítems de inventario.
- **Elementos de protección personal (EPP):** casilleros, sus áreas y secciones, e inventario EPP.
- **Mantención, calidad y servicios:** mantenciones, órdenes de servicio y de trabajo; desviaciones, no conformidades, propuestas y seguimiento; comentarios y observaciones ESS; valijas y entregas de valija.

## Estado y enfoque técnico

La solución se organiza con Clean Architecture en .NET 10, C# 14, ASP.NET Core Razor Pages y SQL Server:

```
SGH.sln
├── SGH.Domain           Entidades, value objects, enums, interfaces y excepciones de negocio
├── SGH.Application      DTOs, interfaces de servicios, casos de uso y validadores
├── SGH.Infrastructure   Persistencia, repositorios, seguridad e inyección de dependencias
├── SGH.Web              Interfaz Razor Pages, ViewModels y middlewares
└── SGH.Tests            Pruebas automatizadas
```

La arquitectura separa las reglas de negocio, los casos de uso, los adaptadores técnicos y la presentación. La solución usa procedimientos almacenados para interactuar con SQL Server: EF Core para escrituras y Dapper para lecturas. Las contraseñas nuevas o actualizadas se almacenan con BCrypt.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)

## Comandos

```powershell
dotnet restore SGH.sln
dotnet build SGH.sln
dotnet test SGH.sln
dotnet run --project SGH.Web
```

En GitHub Actions, `CI` compila la solución y `Tests` ejecuta las pruebas por separado. El workflow de tests se activa en pushes, pull requests y manualmente; incluye una prueba temporal sencilla para comprobar su funcionamiento.
