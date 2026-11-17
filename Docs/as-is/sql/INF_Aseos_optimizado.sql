/*
    INF_Aseos — versión optimizada (mismo contrato de salida que legado).
    Revisar con Docs/as-is/inf-aseos-optimizacion.md antes de aplicar en HOTELDB.
*/
CREATE OR ALTER PROCEDURE dbo.INF_Aseos
    @empresaMandante  INT,
    @campamento       INT,
    @modulo           INT,
    @piso             INT,
    @habitacion       INT,
    @usuarioCreacion  INT,
    @fechaDesde       DATETIME = NULL,
    @fechaHasta       DATETIME = NULL,
    @minera           INT,
    @tipoModuloId     INT = 0,
    @tipoInstalacion  INT = 0,
    @grupoServicioId  INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @perfilGestionAdminId INT;
    DECLARE @fechaHastaExclusiva DATETIME = CASE WHEN @fechaHasta IS NULL THEN NULL ELSE DATEADD(DAY, 1, @fechaHasta) END;

    SELECT TOP (1) @perfilGestionAdminId = Id
    FROM dbo.Perfiles WITH (NOLOCK)
    WHERE PerfilGestionAdmin = 'S'
      AND MineraId = @minera;

    DECLARE @campamentos TABLE (campamentoId INT NOT NULL PRIMARY KEY);

    IF @campamento > 0
        INSERT INTO @campamentos (campamentoId) VALUES (@campamento);
    ELSE IF @perfilGestionAdminId IS NOT NULL
        INSERT INTO @campamentos (campamentoId)
        SELECT cpg.CampamentoId
        FROM dbo.CampamentosPerfilGestionAdminMinera AS cpg WITH (NOLOCK)
        WHERE cpg.MineraId = @minera
          AND cpg.PerfilGestionId = @perfilGestionAdminId;

    ;WITH AseoFiltrado AS
    (
        SELECT a.*
        FROM dbo.Aseo AS a WITH (NOLOCK)
        INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = a.HabitacionId
        INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
        INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
        INNER JOIN dbo.Campamentos AS c WITH (NOLOCK) ON c.Id = p.CampamentoId
        WHERE (@usuarioCreacion = 0 OR a.UsuarioCreacionId = @usuarioCreacion)
          AND (@grupoServicioId = 0 OR a.GrupoServicioId = @grupoServicioId)
          AND (@tipoModuloId = 0 OR p.TipoModuloId = @tipoModuloId)
          AND (@tipoInstalacion = 0 OR h.TipoInstalacionId = @tipoInstalacion)
          AND (@modulo = 0 OR p.Id = @modulo)
          AND (@piso = 0 OR m.Id = @piso)
          AND (@habitacion = 0 OR a.HabitacionId = @habitacion)
          AND (
                @fechaDesde IS NULL AND @fechaHasta IS NULL
                OR (a.FechaCreacion IS NOT NULL
                    AND (@fechaDesde IS NULL OR a.FechaCreacion >= @fechaDesde)
                    AND (@fechaHastaExclusiva IS NULL OR a.FechaCreacion < @fechaHastaExclusiva))
                OR (a.FechaInicioAseo IS NOT NULL
                    AND (@fechaDesde IS NULL OR a.FechaInicioAseo >= @fechaDesde)
                    AND (@fechaHastaExclusiva IS NULL OR a.FechaInicioAseo < @fechaHastaExclusiva))
              )
          AND EXISTS (SELECT 1 FROM @campamentos AS cp WHERE cp.campamentoId = c.Id)
    )
    SELECT
        a.Id,
        h.Nombre AS HabitacionNombre,
        m.Nombre AS PisoNombre,
        p.Nombre AS ModuloNombre,
        tm.Nombre AS TipoModulo,
        c.Nombre AS CampamentoNombre,
        e.Nombre AS EmpresaMandanteNombre,
        ga.Nombre AS GrupoServicio,
        uc.Nombre + ' ' + uc.Apellidos AS UsuarioCreacionNombre,
        iac.Nombre AS ItemCheckList,
        CASE ac.value WHEN 'true' THEN 'SI' WHEN 'false' THEN 'NO' END AS ValorAseo,
        ac.Cantidad,
        CASE supc.value WHEN 'true' THEN 'SI' WHEN 'false' THEN 'NO' ELSE '-' END AS ValorSupervision,
        supc.observacion AS ObservacionSupervision,
        h.M2 AS M2,
        CASE WHEN a.FechaInicioAseo IS NOT NULL
            THEN CONVERT(VARCHAR(10), a.FechaInicioAseo, 103) + ' ' + CONVERT(VARCHAR(8), a.FechaInicioAseo, 108)
            ELSE NULL END AS FechaInicioAseo,
        CASE WHEN a.FechaCreacion IS NOT NULL
            THEN CONVERT(VARCHAR(10), a.FechaCreacion, 103) + ' ' + CONVERT(VARCHAR(8), a.FechaCreacion, 108)
            ELSE '' END AS FechaTerminoAseo,
        CASE WHEN a.FechaGrabacion IS NOT NULL
            THEN CONVERT(VARCHAR(10), a.FechaGrabacion, 103) + ' ' + CONVERT(VARCHAR(8), a.FechaGrabacion, 108)
            ELSE NULL END AS FechaRegistro
    FROM AseoFiltrado AS a
    INNER JOIN dbo.Habitaciones AS h WITH (NOLOCK) ON h.Id = a.HabitacionId
    INNER JOIN dbo.Modulos AS m WITH (NOLOCK) ON m.Id = h.ModuloId
    INNER JOIN dbo.Pabellones AS p WITH (NOLOCK) ON p.Id = m.PabellonId
    INNER JOIN dbo.Campamentos AS c WITH (NOLOCK) ON c.Id = p.CampamentoId
    LEFT JOIN dbo.Empresas AS e WITH (NOLOCK) ON e.Id = c.EmpresaMandanteId
    LEFT JOIN dbo.AseoCheckList AS ac WITH (NOLOCK) ON ac.AseoId = a.Id
    OUTER APPLY
    (
        SELECT TOP (1) sup.Id
        FROM dbo.SupervisionAseo AS sup WITH (NOLOCK)
        WHERE sup.AseoId = a.Id
        ORDER BY sup.FechaCreacion DESC
    ) AS supUltima
    LEFT JOIN dbo.SupervisionAseoCheckList AS supc WITH (NOLOCK)
        ON supc.SupervisionAseoId = supUltima.Id
       AND supc.ItemCheckListAseoId = ac.ItemCheckListAseoId
    LEFT JOIN dbo.ItemCheckListAseo AS iac WITH (NOLOCK) ON iac.Id = ac.ItemCheckListAseoId
    LEFT JOIN dbo.Usuarios AS uc WITH (NOLOCK) ON uc.Id = a.UsuarioCreacionId
    LEFT JOIN dbo.TiposModulo AS tm WITH (NOLOCK) ON tm.Id = p.TipoModuloId
    LEFT JOIN dbo.GrupoActividadAseo AS ga WITH (NOLOCK) ON ga.Id = a.GrupoServicioId
    ORDER BY ISNULL(a.FechaCreacion, a.FechaInicioAseo);
END
GO