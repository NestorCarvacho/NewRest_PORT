import { crearApi } from "./api.js";
import { crearFechas } from "./fechas.js";
import { crearPeriodoFiltro } from "./periodo-filtro.js";
import { crearTablas } from "./tablas.js";
import { crearFormularios } from "./formularios.js";
import { crearCatalogos } from "./catalogos.js";
import { crearAvisos } from "./avisos.js";
import { crearPanelLateral } from "./panel-lateral.js";
import { crearCargarDeferido } from "./cargar-deferido.js";
import { crearExportadorTabla } from "./exportar-tabla.js";

export function crearEntornoBase() {
    var api = crearApi();
    var fechas = crearFechas();
    var periodoFiltro = crearPeriodoFiltro(fechas);
    var tablas = crearTablas();
    var catalogos = crearCatalogos(api.enviar);
    var avisos = crearAvisos();
    var panelLateral = crearPanelLateral();
    var cargarDeferido = crearCargarDeferido();
    var exportarTabla = crearExportadorTabla();
    var enlazarFormulario = function () { };

    var interfazFormularios = crearFormularios({
        momentoLocal: fechas.momentoLocal,
        enlazar: function (form) {
            enlazarFormulario(form);
            periodoFiltro.enlazarFormulario(form);
        },
        cerrarPanel: panelLateral.cerrar,
        mostrarAviso: avisos.mostrarAviso
    });

    return {
        api: api,
        pedir: api.pedir,
        enviar: api.enviar,
        consulta: api.consulta,
        tablas: tablas,
        catalogos: catalogos,
        prepararCatalogo: catalogos.preparar,
        avisos: avisos,
        mostrarAviso: avisos.mostrarAviso,
        mostrarEstado: avisos.mostrarEstado,
        panelLateral: panelLateral,
        abrirPanel: panelLateral.abrir,
        cerrarPanel: panelLateral.cerrar,
        fechas: fechas,
        fechaActual: fechas.fechaActual,
        momentoLocal: fechas.momentoLocal,
        rangoReservaInicial: fechas.rangoReservaInicial,
        rangoDesdeDias: fechas.rangoDesdeDias,
        presetsPeriodo: fechas.presetsPeriodo,
        aplicarPresetPeriodo: fechas.aplicarPresetPeriodo,
        normalizarFiltroPeriodo: fechas.normalizarFiltroPeriodo,
        enlazarPeriodoFiltro: periodoFiltro.enlazarFormulario,
        refrescarPeriodoFiltro: periodoFiltro.refrescarFormulario,
        camposPeriodoFiltro: periodoFiltro.camposPeriodo,
        filtroInicialPeriodo: periodoFiltro.filtroInicialPeriodo,
        fechasEntre: fechas.fechasEntre,
        fechaListadoAInput: fechas.fechaListadoAInput,
        bloquearPantalla: avisos.bloquearPantalla,
        desbloquearPantalla: avisos.desbloquearPantalla,
        exportarTabla: exportarTabla,
        cargar: cargarDeferido.cargar,
        cargarDeferido: cargarDeferido,
        formulario: interfazFormularios.formulario,
        campo: interfazFormularios.campo,
        boton: interfazFormularios.boton,
        seccion: interfazFormularios.seccion,
        contenido: interfazFormularios.contenido,
        aOpcion: interfazFormularios.aOpcion,
        inicializarSelectores: interfazFormularios.inicializarSelectores,
        actualizarSelector: interfazFormularios.actualizarSelector,
        leerFormulario: interfazFormularios.leer,
        setEnlazarFormulario: function (fn) { enlazarFormulario = fn; }
    };
}
