import { crearApi } from "./core/api.js";
import { crearFechas } from "./core/fechas.js";
import { inicializarCuenta } from "./modulos/cuenta.js";
import { crearCensoQr } from "./modulos/censo-qr.js";
import { crearCenso } from "./modulos/censo.js";
import { crearReserva } from "./modulos/reserva.js";
import { crearSolicitud } from "./modulos/solicitud.js";
import { crearMovimientosReserva } from "./modulos/movimientos-reserva.js";
import { crearTablas } from "./core/tablas.js";
import { crearFormularios } from "./core/formularios.js";
import { crearNavegacion } from "./core/navegacion.js";
import { crearCatalogos } from "./core/catalogos.js";
import { crearCargador } from "./core/cargador.js";
import { crearAvisos } from "./core/avisos.js";
import { inicializarMenuModulos } from "./core/menu-modulos.js";
import { crearDefinicionPantallas } from "./modulos/definicion-pantallas.js";
import { crearPanelLateral } from "./core/panel-lateral.js";
import { numero } from "./core/numeros.js";
import { crearPanelModulo } from "./core/panel-modulo.js";
import { crearRenderizadorModulos } from "./core/renderizador-modulos.js";

(function () {
    var api = crearApi();
    var fechas = crearFechas();
    var pedir = api.pedir;
    var enviar = api.enviar;
    var consulta = api.consulta;
    var tablas = crearTablas();
    var catalogos = crearCatalogos(pedir);
    var prepararCatalogo = catalogos.preparar;
    var cargador;
    var cargar = function (id) { return cargador.cargar(id); };
    var avisos = crearAvisos();
    var mostrarAviso = avisos.mostrarAviso;
    var mostrarEstado = avisos.mostrarEstado;
    var panelLateral = crearPanelLateral();
    var abrirPanel = panelLateral.abrir;
    var cerrarPanel = panelLateral.cerrar;
    var fechaActual = fechas.fechaActual;
    var momentoLocal = fechas.momentoLocal;
    var rangoReservaInicial = fechas.rangoReservaInicial;
    inicializarCuenta();
    var interfazFormularios = crearFormularios({
        momentoLocal: momentoLocal,
        enlazar: function (form) { censo.enlazar(form); },
        cerrarPanel: cerrarPanel,
        mostrarAviso: mostrarAviso
    });
    var formulario = interfazFormularios.formulario;
    var campo = interfazFormularios.campo;
    var boton = interfazFormularios.boton;
    var seccion = interfazFormularios.seccion;
    var contenido = interfazFormularios.contenido;
    var aOpcion = interfazFormularios.aOpcion;
    var reserva = crearReserva({
        obtenerCatalogo: catalogos.obtenerReserva,
        campo: campo,
        aOpcion: aOpcion,
        seccion: seccion,
        momentoLocal: momentoLocal,
        rangoReservaInicial: rangoReservaInicial,
        formulario: formulario,
        numero: numero,
        enviar: enviar,
        enviarArchivo: api.enviarArchivo,
        abrirPanel: abrirPanel,
        cargar: cargar,
        mostrarAviso: mostrarAviso,
        consulta: consulta
    });
    var solicitud = crearSolicitud({
        pedir: pedir, enviar: enviar, enviarArchivo: api.enviarArchivo, consulta: consulta, campo: campo,
        obtenerCatalogo: catalogos.obtenerSolicitud, prepararCatalogo: prepararCatalogo,
        abrirPanel: abrirPanel, cerrarPanel: cerrarPanel, cargar: cargar,
        mostrarAviso: mostrarAviso, fechaActual: fechaActual, rangoInicial: rangoReservaInicial
    });
    var movimientosReserva = crearMovimientosReserva({
        boton: boton,
        prepararCatalogo: prepararCatalogo,
        abrirEdicion: reserva.abrirEdicion,
        mostrarAviso: mostrarAviso,
        abrirPanel: abrirPanel,
        formulario: formulario,
        seccion: seccion,
        campo: campo,
        momentoLocal: momentoLocal,
        enviar: enviar,
        cargar: cargar
    });
    var censo = crearCenso({
        obtenerCatalogo: catalogos.obtenerCenso,
        campo: campo,
        aOpcion: aOpcion,
        fechaActual: fechaActual,
        pedir: pedir,
        consulta: consulta,
        numero: numero,
        mostrarEstado: mostrarEstado,
        enviar: enviar,
        cargar: cargar
    });
    var censoQr = crearCensoQr({
        formulario: formulario,
        seccion: seccion,
        campo: campo,
        fechaActual: fechaActual,
        enviar: enviar,
        mostrarAviso: mostrarAviso,
        abrirPanel: abrirPanel,
        boton: boton
    });
    var modulos = crearDefinicionPantallas(reserva, censo, solicitud);
    var renderizador = crearRenderizadorModulos({
        tablas: tablas,
        censo: censo,
        reserva: reserva,
        solicitud: solicitud,
        movimientosReserva: movimientosReserva
    });

    var solapas = document.getElementById("solapas");
    var paneles = document.getElementById("paneles");
    var vista = document.getElementById("vista-inicial");
    var velo = panelLateral.velo;
    var crearPanel = crearPanelModulo({
        boton: boton,
        prepararCatalogo: prepararCatalogo,
        abrirPanel: abrirPanel,
        formulario: formulario,
        contenido: contenido,
        cargar: cargar,
        mostrarEstado: mostrarEstado,
        reserva: reserva,
        solicitud: solicitud,
        censo: censo,
        censoQr: censoQr,
        crearTabla: tablas.crear
    });
    var navegacion = crearNavegacion({
        solapas: solapas,
        paneles: paneles,
        vista: vista,
        modulos: modulos,
        crearPanel: crearPanel,
        columnas: renderizador.columnas,
        inicializarTabla: tablas.inicializar,
        destruirTabla: tablas.destruir,
        cargar: cargar
    });
    var menuModulos = inicializarMenuModulos({
        abrirModulo: navegacion.abrir,
        abrirCensoQr: censoQr.abrir
    });
    cargador = crearCargador({
        obtenerNodo: function (id) { return navegacion.paneles.querySelector('[data-id="' + id + '"]'); },
        obtenerModulo: function (id) { return modulos[id]; },
        pedir: pedir,
        censo: censo,
        reserva: reserva,
        solicitud: solicitud,
        mostrarEstado: mostrarEstado,
        pintar: renderizador.pintar
    });

    window.addEventListener("online", censoQr.sincronizarPendientes);
    censoQr.sincronizarPendientes();

    document.getElementById("panel-cerrar").addEventListener("click", cerrarPanel);
    velo.addEventListener("click", cerrarPanel);
    document.addEventListener("keydown", function (evento) {
        if (evento.key === "Escape") {
            cerrarPanel();
            menuModulos.cerrarMenus();
        }
    });
})();
