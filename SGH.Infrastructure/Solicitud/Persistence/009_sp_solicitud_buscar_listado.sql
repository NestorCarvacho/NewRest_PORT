/*
    Incremental QA — ejecutar después de 001_sp_solicitud.sql si ese archivo ya se aplicó
    sin columnas técnicas en el listado, @MaxFilas, rango por defecto de 30 días ni TOP.

    Agrupa: depuración Mid (columnas extra) + listados con período y TOP 5000.
    Instalación en frío: basta con 001_sp_solicitud.sql (ya incluye este contenido).
*/
CREATE OR ALTER PROCEDURE dbo.he_sp_solicitud_buscar
    @EmpresaMandanteId int,
    @CampamentoId int,
    @Desde date = NULL,
    @Hasta date = NULL,
    @GrupoId int,
    @SolicitanteId int,
    @Estado varchar(1),
    @Numero int,
    @MaxFilas int = 5000
AS
BEGIN
    SET NOCOUNT ON;

    IF @Desde IS NULL AND @Hasta IS NULL
    BEGIN
        SET @Hasta = CAST(GETDATE() AS date);
        SET @Desde = DATEADD(day, -30, @Hasta);
    END
    ELSE IF @Desde IS NULL
        SET @Desde = DATEADD(day, -30, @Hasta);
    ELSE IF @Hasta IS NULL
        SET @Hasta = CAST(GETDATE() AS date);

    SELECT TOP (@MaxFilas) s.Id AS Numero, s.FechaSolicitud AS Fecha,
           a.Nombre + ' - ' + e.Nombre AS Empresa,
           u.Nombre + ' ' + u.Apellidos AS Solicitante,
           l.Descripcion AS Estado,
           s.Id AS SolicitudId,
           s.Estado AS EstadoCodigo,
           s.EstadoTabla,
           s.CampamentoId,
           s.EmpresaMandanteId,
           s.AreaEmpresaSolicitanteId,
           s.UsuarioSolicitanteId,
           a.EmpresaId AS EmpresaContratistaId,
           s.FechaCheckIn,
           s.FechaCheckOut,
           s.TurnoId,
           s.ConSistemaTurno,
           (SELECT TOP (1) r.Id FROM dbo.Reservas AS r WHERE r.SolicitudAlojamientoId = s.Id ORDER BY r.Id) AS PrimeraReservaGeneradaId
    FROM dbo.SolicitudesAlojamiento AS s WITH (NOLOCK)
    INNER JOIN dbo.AreasEmpresa AS a WITH (NOLOCK) ON a.Id = s.AreaEmpresaSolicitanteId
    INNER JOIN dbo.Empresas AS e WITH (NOLOCK) ON e.Id = a.EmpresaId
    LEFT JOIN dbo.Usuarios AS u WITH (NOLOCK) ON u.Id = s.UsuarioSolicitanteId
    LEFT JOIN dbo.Listas AS l WITH (NOLOCK) ON l.IdTabla = s.EstadoTabla AND l.Valor1 = s.Estado AND l.Tipo = 'Detalle'
    WHERE s.EmpresaMandanteId = @EmpresaMandanteId
      AND s.CampamentoId = @CampamentoId
      AND s.FechaSolicitud >= @Desde AND s.FechaSolicitud < DATEADD(day, 1, @Hasta)
      AND (@GrupoId = 0 OR s.AreaEmpresaSolicitanteId = @GrupoId)
      AND (@SolicitanteId = 0 OR s.UsuarioSolicitanteId = @SolicitanteId)
      AND (@Estado = '0' OR s.Estado = @Estado)
      AND (@Numero = 0 OR s.Id = @Numero)
    ORDER BY s.FechaSolicitud, s.Id;
END
GO