import { crearEntornoBase } from "./entorno-base.js";
import { crearCargador } from "./cargador.js";
import { crearPanelModulo } from "./panel-modulo.js";
import { crearRenderizadorModulos } from "./renderizador-modulos.js";
import { paquetesParaPantallas, INSTALADORES, PROPIEDADES_PAQUETE } from "./registro-paquetes.js";

function elegirDefiniciones(idsPantalla, definiciones) {
    var modulos = {};
    idsPantalla.forEach(function (id) {
        if (definiciones[id]) modulos[id] = definiciones[id];
    });
    return modulos;
}

function volcarPaqueteEnInstancia(instancia, paquete) {
    PROPIEDADES_PAQUETE.forEach(function (propiedad) {
        if (paquete[propiedad]) instancia[propiedad] = paquete[propiedad];
    });
}

export function ensamblarEntorno(idsPantalla) {
    var base = crearEntornoBase();
    var paquetes = paquetesParaPantallas(idsPantalla);
    var instancia = {
        reserva: null,
        censo: null,
        solicitud: null,
        movimientosReserva: null,
        censoQr: null,
        habitacion: null,
        movimientosHabitacion: null
    };
    var modulos = {};

    paquetes.forEach(function (nombre) {
        var instalar = INSTALADORES[nombre];
        if (!instalar) return;
        var paquete = instalar(base);
        volcarPaqueteEnInstancia(instancia, paquete);
        Object.assign(modulos, elegirDefiniciones(idsPantalla, paquete.definiciones));
    });

    var renderizador = crearRenderizadorModulos({
        tablas: base.tablas,
        censo: instancia.censo,
        reserva: instancia.reserva,
        solicitud: instancia.solicitud,
        movimientosReserva: instancia.movimientosReserva,
        habitacion: instancia.habitacion,
        movimientosHabitacion: instancia.movimientosHabitacion
    });
    var crearPanel = crearPanelModulo({
        boton: base.boton,
        prepararCatalogo: base.prepararCatalogo,
        abrirPanel: base.abrirPanel,
        formulario: base.formulario,
        contenido: base.contenido,
        cargar: base.cargar,
        mostrarEstado: base.mostrarEstado,
        mostrarAviso: base.mostrarAviso,
        exportarTabla: base.exportarTabla,
        reserva: instancia.reserva,
        solicitud: instancia.solicitud,
        censo: instancia.censo,
        censoQr: instancia.censoQr,
        habitacion: instancia.habitacion,
        crearTabla: base.tablas.crear
    });

    function enlazarCargador(obtenerNodo) {
        var cargador = crearCargador({
            obtenerNodo: obtenerNodo,
            obtenerModulo: function (id) { return modulos[id]; },
            pedir: base.pedir,
            censo: instancia.censo,
            reserva: instancia.reserva,
            solicitud: instancia.solicitud,
            habitacion: instancia.habitacion,
            mostrarEstado: base.mostrarEstado,
            mostrarAviso: base.mostrarAviso,
            bloquearPantalla: base.bloquearPantalla,
            desbloquearPantalla: base.desbloquearPantalla,
            pintar: renderizador.pintar
        });
        base.cargarDeferido.enlazar(function (id) { return cargador.cargar(id); });
    }

    function enlazarPanelesLaterales() {
        var cerrar = document.getElementById("panel-cerrar");
        var velo = base.panelLateral.velo;
        if (cerrar) cerrar.addEventListener("click", base.cerrarPanel);
        if (velo) velo.addEventListener("click", base.cerrarPanel);
    }

    return Object.assign(instancia, {
        modulos: modulos,
        renderizador: renderizador,
        crearPanel: crearPanel,
        tablas: base.tablas,
        prepararCatalogo: base.prepararCatalogo,
        cargar: base.cargar,
        enlazarCargador: enlazarCargador,
        mostrarEstado: base.mostrarEstado,
        cerrarPanel: base.cerrarPanel,
        panelLateral: base.panelLateral,
        enlazarPanelesLaterales: enlazarPanelesLaterales
    });
}
