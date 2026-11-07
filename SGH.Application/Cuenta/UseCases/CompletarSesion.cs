using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Interfaces;
using SGH.Application.Cuenta.Opciones;
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
        var verificacion = await _verificacion.EjecutarAsync(
            solicitud.Rut,
            solicitud.Contrasena,
            solicitud.LoginInterno,
            cancellationToken);
        if (verificacion is ResultadoVerificacionClave.Rechazada rechazada)
        {
            return new ResultadoAutenticacion.Rechazado(rechazada.Mensaje, rechazada.Motivo);
        }

        var valida = (ResultadoVerificacionClave.Valida)verificacion;
        var usuario = valida.Usuario;
        var perfiles = await _consulta.ObtenerCampamentosYPerfilesAsync(valida.Rut, cancellationToken);

        if (ReglasCuenta.EsIntegracion(usuario.Nombre))
        {
            return new ResultadoAutenticacion.Aceptado(CrearSesion(usuario, usuario.PerfilId, 0, usuario.EmpresaMandanteId ?? 0, 0, true));
        }

        if (ReglasCuenta.FuerzaPerfil(valida.Rut))
        {
            var coincidencia = perfiles.FirstOrDefault(perfil => perfil.PerfilId == ReglasCuenta.PerfilForzado);
            return new ResultadoAutenticacion.Aceptado(CrearSesion(
                usuario,
                ReglasCuenta.PerfilForzado,
                coincidencia?.CampamentoId ?? 0,
                coincidencia?.EmpresaMandanteId ?? usuario.EmpresaMandanteId ?? 0,
                coincidencia?.Id ?? 0,
                false));
        }

        if (solicitud.AsignacionPerfilId == 0 || perfiles.Count == 0)
        {
            return new ResultadoAutenticacion.Aceptado(CrearSesion(
                usuario,
                usuario.PerfilId,
                0,
                usuario.EmpresaMandanteId ?? 0,
                0,
                false));
        }

        var elegido = perfiles.FirstOrDefault(perfil => perfil.Id == solicitud.AsignacionPerfilId);
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
            false));
    }

    private UsuarioSesionDto CrearSesion(
        UsuarioLogin usuario,
        int perfilId,
        int campamentoId,
        int empresaMandanteId,
        int asignacionPerfilId,
        bool esIntegracion)
    {
        return new UsuarioSesionDto(
            usuario.Id,
            usuario.Rut,
            usuario.NombreParaMostrar,
            perfilId,
            campamentoId,
            empresaMandanteId,
            asignacionPerfilId,
            esIntegracion ? ReglasCuenta.RolIntegracion : ReglasCuenta.RolGeneral,
            ReglasCuenta.Sistema,
            esIntegracion,
            _sesion.MinutosPara(perfilId));
    }
}
