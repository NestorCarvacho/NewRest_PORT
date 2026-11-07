using SGH.Application.Cuenta.DTOs;
using SGH.Application.Cuenta.Interfaces;
using SGH.Domain.Cuenta;
using SGH.Domain.Cuenta.Interfaces;

namespace SGH.Application.Cuenta.UseCases;

public sealed class PreautenticarUsuario : IPreautenticarUsuario
{
    private readonly IVerificacionClaveUsuario _verificacion;
    private readonly IConsultaAutenticacion _consulta;

    public PreautenticarUsuario(IVerificacionClaveUsuario verificacion, IConsultaAutenticacion consulta)
    {
        _verificacion = verificacion;
        _consulta = consulta;
    }

    public async Task<ResultadoPreautenticacion> EjecutarAsync(SolicitudPreautenticacion solicitud, CancellationToken cancellationToken)
    {
        var verificacion = await _verificacion.EjecutarAsync(solicitud.Rut, solicitud.Contrasena, solicitud.LoginInterno, cancellationToken);
        if (verificacion is ResultadoVerificacionClave.Rechazada rechazada)
        {
            return new ResultadoPreautenticacion.Rechazado(rechazada.Mensaje, rechazada.Motivo);
        }

        var valida = (ResultadoVerificacionClave.Valida)verificacion;
        var perfiles = await _consulta.ObtenerCampamentosYPerfilesAsync(valida.Rut, cancellationToken);
        var opciones = perfiles
            .Select(perfil => new OpcionPerfilDto(
                perfil.Id,
                perfil.PerfilId,
                perfil.PerfilNombre,
                perfil.CampamentoId,
                perfil.CampamentoNombre,
                perfil.EmpresaMandanteId,
                perfil.EmpresaMandanteNombre,
                perfil.Etiqueta))
            .ToList();

        return new ResultadoPreautenticacion.Listo(
            valida.Rut,
            valida.Usuario.Nombre,
            ReglasCuenta.EsIntegracion(valida.Usuario.Nombre),
            opciones);
    }
}
