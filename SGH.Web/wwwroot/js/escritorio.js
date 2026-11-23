import { inicializarCuenta } from "./modulos/cuenta.js";
import { crearNavegacion } from "./core/navegacion.js";
import { inicializarMenuModulos } from "./core/menu-modulos.js";
import { crearEntornoEscritorio } from "./core/fabrica-escritorio.js";
import { instalarColaOffline } from "./core/colaOffline.js";

(function () {
    function enviarColaOffline(url, cuerpo) {
        return fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(cuerpo)
        }).then(function (respuesta) {
            if (!respuesta.ok) throw new Error("Error de sincronización");
            return respuesta.json();
        });
    }
    instalarColaOffline(enviarColaOffline);
    inicializarCuenta();
    var entorno = crearEntornoEscritorio();
    var solapas = document.getElementById("solapas");
    var paneles = document.getElementById("paneles");
    var vista = document.getElementById("vista-inicial");
    var navegacion = null;
    var menuModulos;

    if (!solapas || !paneles) {
        menuModulos = inicializarMenuModulos({
            abrirModulo: function (modulo) {
                if (!modulo) return;
                window.location.assign("/Index?modulo=" + encodeURIComponent(modulo));
            },
            abrirCensoQr: entorno.censoQr ? entorno.censoQr.abrir : function () { },
            esModuloSolapa: function (id) { return !!entorno.modulos[id]; }
        });
        entorno.enlazarPanelesLaterales();
        document.addEventListener("keydown", function (evento) {
            if (evento.key === "Escape") {
                entorno.cerrarPanel();
                menuModulos.cerrarMenus();
            }
        });
        return;
    }

    navegacion = crearNavegacion({
        solapas: solapas,
        paneles: paneles,
        vista: vista,
        modulos: entorno.modulos,
        crearPanel: entorno.crearPanel,
        columnas: entorno.renderizador.columnas,
        prepararCatalogo: entorno.prepararCatalogo,
        inicializarTabla: entorno.tablas.inicializar,
        destruirTabla: entorno.tablas.destruir,
        cargar: entorno.cargar,
        cancelarCargaNodo: entorno.cancelarCargaNodo,
        mostrarEstado: entorno.mostrarEstado
    });

    entorno.enlazarCargador(function (id) {
        return navegacion.paneles.querySelector('[data-id="' + id + '"]');
    });

    menuModulos = inicializarMenuModulos({
        abrirModulo: navegacion.abrir,
        abrirCensoQr: entorno.censoQr ? entorno.censoQr.abrir : function () { },
        esModuloSolapa: function (id) { return !!entorno.modulos[id]; }
    });

    entorno.enlazarPanelesLaterales();
    if (entorno.censoQr) {
        window.addEventListener("online", entorno.censoQr.sincronizarPendientes);
        entorno.censoQr.sincronizarPendientes();
    }

    var inicial = document.getElementById("modulo-inicial");
    if (inicial) navegacion.abrir(inicial.dataset.moduloInicial);

    document.addEventListener("keydown", function (evento) {
        if (evento.key === "Escape") {
            entorno.cerrarPanel();
            menuModulos.cerrarMenus();
        }
    });
})();
