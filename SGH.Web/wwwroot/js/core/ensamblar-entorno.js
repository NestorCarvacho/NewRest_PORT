import { crearEntornoBase } from "./entorno-base.js";
import { crearCargador } from "./cargador.js";
import { crearPanelModulo } from "./panel-modulo.js";
import { crearFiltrosModulo } from "./filtros-modulo.js";
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
    var abrirFiltrosModulo = crearFiltrosModulo({
        prepararCatalogo: base.prepararCatalogo,
        abrirPanel: base.abrirPanel,
        cerrarPanel: base.cerrarPanel,
        formulario: base.formulario,
        contenido: base.contenido,
        cargar: base.cargar,
        mostrarEstado: base.mostrarEstado,
        normalizarFiltroPeriodo: base.normalizarFiltroPeriodo,
        aOpcion: base.aOpcion,
        leerFormulario: base.leerFormulario,
        enlazarPeriodoFiltro: base.enlazarPeriodoFiltro,
        inicializarSelectores: base.inicializarSelectores,
        obtenerCatalogoHabitacion: base.catalogos.obtenerHabitacion,
        obtenerCatalogoReserva: base.catalogos.obtenerReserva,
        obtenerCatalogoCenso: base.catalogos.obtenerCenso,
        obtenerCatalogoSolicitud: base.catalogos.obtenerSolicitud,
        enlazarFiltrosCenso: instancia.censo ? instancia.censo.enlazar.bind(instancia.censo) : null,
        sincronizarFiltrosCenso: instancia.censo ? instancia.censo.sincronizarFiltros.bind(instancia.censo) : null,
        enlazarPeriodoCenso: instancia.censo ? instancia.censo.enlazarPeriodoCenso.bind(instancia.censo) : null,
        sincronizarPeriodoCenso: instancia.censo ? instancia.censo.sincronizarPeriodoCenso.bind(instancia.censo) : null,
        actualizarBotonGuardarCenso: instancia.censo ? instancia.censo.actualizarBotonGuardar.bind(instancia.censo) : null,
        refrescarPeriodoFiltro: base.refrescarPeriodoFiltro,
        actualizarSelector: base.actualizarSelector
    }).abrir;

    var crearPanel = crearPanelModulo({
        prepararCatalogo: base.prepararCatalogo,
        abrirPanel: base.abrirPanel,
        cargar: base.cargar,
        mostrarEstado: base.mostrarEstado,
        mostrarAviso: base.mostrarAviso,
        exportarTabla: base.exportarTabla,
        solicitud: instancia.solicitud,
        censo: instancia.censo,
        censoQr: instancia.censoQr,
        habitacion: instancia.habitacion,
        movimientosReserva: instancia.movimientosReserva,
        abrirFiltrosModulo: abrirFiltrosModulo
    });

    function enlazarCargador(obtenerNodo) {
        var cargador = crearCargador({
            obtenerNodo: obtenerNodo,
            obtenerModulo: function (id) { return modulos[id]; },
            enviar: base.enviar,
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
        return cargador;
    }

    function enlazarPanelesLaterales() {
        var cerrar = document.getElementById("panel-cerrar");
        var velo = base.panelLateral.velo;
        if (cerrar) cerrar.addEventListener("click", base.cerrarPanel);
        if (velo) velo.addEventListener("click", base.cerrarPanel);
    }

    var cancelarCargaNodo = function () { };

    var enlazarSincronizacion = {
        sincronizarFila: renderizador.sincronizarFila.bind(renderizador),
        quitarFila: renderizador.quitarFila.bind(renderizador)
    };
    if (instancia.habitacion && typeof instancia.habitacion.enlazarSincronizacionListado === "function") {
        instancia.habitacion.enlazarSincronizacionListado(enlazarSincronizacion);
    }
    if (instancia.reserva && typeof instancia.reserva.enlazarSincronizacionListado === "function") {
        instancia.reserva.enlazarSincronizacionListado(enlazarSincronizacion);
    }

    function inicializarTablaModulo(nodo, columnas, ordenInicial) {
        var ajustarOpciones = null;
        if (nodo.dataset.id === "reserva" && instancia.reserva) {
            ajustarOpciones = instancia.reserva.configurarOpcionesTabla;
        } else if ((nodo.dataset.id === "censo" || nodo.dataset.id === "misCensos") && instancia.censo) {
            ajustarOpciones = instancia.censo.configurarOpcionesTabla;
        }
        var api = base.tablas.inicializar(nodo, columnas, ordenInicial, ajustarOpciones);
        if (nodo && nodo.dataset && nodo.dataset.id === "reserva"
            && instancia.reserva && typeof instancia.reserva.enlazarTablaDraw === "function"
            && instancia.movimientosReserva) {
            instancia.reserva.enlazarTablaDraw(nodo, api, {
                nodoCeldaDatos: base.tablas.nodoCeldaDatos,
                crearAcciones: instancia.movimientosReserva.crearAcciones
            });
            if (instancia.movimientosReserva.enlazarBotonMovimientoMasivo) {
                instancia.movimientosReserva.enlazarBotonMovimientoMasivo(nodo);
            }
        }
        return api;
    }

    var tablas = Object.assign({}, base.tablas, {
        inicializar: inicializarTablaModulo
    });

    return Object.assign(instancia, {
        modulos: modulos,
        renderizador: renderizador,
        crearPanel: crearPanel,
        tablas: tablas,
        prepararCatalogo: base.prepararCatalogo,
        cargar: base.cargar,
        enlazarCargador: function (obtenerNodo) {
            var cargador = enlazarCargador(obtenerNodo);
            cancelarCargaNodo = cargador.cancelarCargaNodo;
        },
        cancelarCargaNodo: function (nodo) { cancelarCargaNodo(nodo); },
        mostrarEstado: base.mostrarEstado,
        cerrarPanel: base.cerrarPanel,
        panelLateral: base.panelLateral,
        enlazarPanelesLaterales: enlazarPanelesLaterales
    });
}
