using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Interfaces;
using SGH.Application.Cuenta.Opciones;
using SGH.Domain.Alcance;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Entities;
using SGH.Domain.Cuenta.Interfaces;

namespace SGH.Application.Cuenta.UseCases;

public sealed class CompletarSesion : ICompletarSesion
{
    private readonly IVerificacionClaveUsuario _verificacion;
    private readonly IConsultaAutenticacion _consulta;
    private readonly OpcionesSesion _sesion;

    public CompletarSesion(IVerificacionClaveUsuario verificacion, IConsultaAutenticacion consulta, OpcionesSesion sesion)
    {
        _verificacion = verificacion;
        _consulta = consulta;
        _sesion = sesion;
    }

    public async Task<ResultadoAutenticacion> EjecutarAsync(SolicitudCompletarSesion solicitud, CancellationToken cancellationToken)
    {
        var rutNormalizado = NormalizadorRut.Normalizar(solicitud.Rut);
        var usuario = await _consulta.ObtenerParaLoginAsync(
            rutNormalizado,
            solicitud.LoginInterno,
            cancellationToken);
        if (usuario is null)
            return new ResultadoAutenticacion.Rechazado(ReglasCuenta.CredencialesInvalidas, MotivoRechazo.CredencialesInvalidas);

        var estado = await _consulta.ObtenerEstadoAsync(rutNormalizado, cancellationToken);
        if (estado != CodigoEstadoUsuario.Vigente)
            return new ResultadoAutenticacion.Rechazado(ReglasCuenta.ErrorEstado, MotivoRechazo.EstadoInvalido);

        var perfiles = await _consulta.ObtenerCampamentosYPerfilesAsync(rutNormalizado, cancellationToken);

        if (ReglasCuenta.EsIntegracion(usuario.Nombre))
        {
            return new ResultadoAutenticacion.Aceptado(CrearSesion(usuario, usuario.PerfilId, 0, usuario.EmpresaMandanteId ?? 0, 0, true));
        }

        if (ReglasCuenta.FuerzaPerfil(rutNormalizado))
        {
            var coincidencia = perfiles.FirstOrDefault(perfil => perfil.PerfilId == ReglasCuenta.PerfilForzado);
            return new ResultadoAutenticacion.Aceptado(CrearSesion(
                usuario,
                ReglasCuenta.PerfilForzado,
                coincidencia?.CampamentoId ?? 0,
                coincidencia?.EmpresaMandanteId ?? usuario.EmpresaMandanteId ?? 0,
                coincidencia?.Id ?? 0,
                false,
                coincidencia));
        }

        if (perfiles.Count == 0)
        {
            return new ResultadoAutenticacion.Rechazado(
                ReglasAlcance.SinCampamento,
                MotivoRechazo.PerfilNoPermitido);
        }

        var asignacionId = solicitud.AsignacionPerfilId;
        if (asignacionId == 0)
        {
            var automatica = ReglasCuenta.ResolverAsignacionAutomatica(
                perfiles.Count,
                perfiles.Select(perfil => perfil.CampamentoId),
                perfiles[0].Id);
            if (automatica is int id)
                asignacionId = id;
        }

        if (asignacionId == 0)
            return new ResultadoAutenticacion.Rechazado(ReglasCuenta.SeleccionePerfil, MotivoRechazo.PerfilNoPermitido);

        var elegido = perfiles.FirstOrDefault(perfil => perfil.Id == asignacionId);
        if (elegido is null)
        {
            return new ResultadoAutenticacion.Rechazado(ReglasCuenta.CredencialesInvalidas, MotivoRechazo.PerfilNoPermitido);
        }

        return new ResultadoAutenticacion.Aceptado(CrearSesion(
            usuario,
            elegido.PerfilId,
            elegido.CampamentoId,
            elegido.EmpresaMandanteId,
            elegido.Id,
            false,
            elegido));
    }

    private UsuarioSesionDto CrearSesion(
        UsuarioLogin usuario,
        int perfilId,
        int campamentoId,
        int empresaMandanteId,
        int asignacionPerfilId,
        bool esIntegracion,
        PerfilCampamentoUsuario? asignacion = null)
    {
        return new UsuarioSesionDto(
            asignacion?.UsuarioId > 0 ? asignacion.UsuarioId : usuario.Id,
            usuario.Rut,
            string.IsNullOrWhiteSpace(asignacion?.UsuarioNombre)
                ? usuario.NombreParaMostrar
                : asignacion.UsuarioNombre,
            perfilId,
            campamentoId,
            empresaMandanteId,
            asignacion?.EmpresaMandanteNombre ?? string.Empty,
            asignacionPerfilId,
            esIntegracion ? ReglasCuenta.RolIntegracion : ReglasCuenta.RolGeneral,
            ReglasCuenta.Sistema,
            esIntegracion,
            _sesion.MinutosPara(perfilId));
    }
}
