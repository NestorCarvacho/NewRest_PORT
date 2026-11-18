import { bloquearBotonSegundos } from "./boton-espera.js";

export function crearPanelModulo(dependencias) {
    function enlazarExportar(barra, nodo, tituloModulo) {
        var envoltorio = barra.querySelector(".exportar-desplegable");
        if (!envoltorio || envoltorio.dataset.enlazado) return;
        envoltorio.dataset.enlazado = "1";
        var dispara = envoltorio.querySelector(".exportar-desplegable-dispara");
        dispara.addEventListener("click", function (evento) {
            evento.stopPropagation();
            var abierto = envoltorio.classList.toggle("abierto");
            dispara.setAttribute("aria-expanded", abierto ? "true" : "false");
        });
        document.addEventListener("click", function (evento) {
            if (!envoltorio.classList.contains("abierto")) return;
            if (!envoltorio.contains(evento.target)) {
                envoltorio.classList.remove("abierto");
                dispara.setAttribute("aria-expanded", "false");
            }
        });
        envoltorio.querySelectorAll("[data-exportar]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                envoltorio.classList.remove("abierto");
                dispara.setAttribute("aria-expanded", "false");
                if (!nodo._tabla) {
                    dependencias.mostrarAviso("No hay datos para exportar.", "warning");
                    return;
                }
                var avisoVacio = function () { dependencias.mostrarAviso("No hay datos para exportar.", "warning"); };
                var tipo = boton.getAttribute("data-exportar");
                if (tipo === "excel") dependencias.exportarTabla.exportarExcel(nodo, tituloModulo, avisoVacio);
                else dependencias.exportarTabla.exportarCsv(nodo, tituloModulo, avisoVacio);
            });
        });
    }

    function enlazarBarra(nodo, id, modulo) {
        var barra = nodo.querySelector(".modulo-barra");
        if (!barra || barra.dataset.enlazado) return;
        barra.dataset.enlazado = "1";

        var ultimaActualizacion = 0;
        var intervaloMinimoMs = 5000;
        var btnActualizar = barra.querySelector("[data-accion='actualizar']");
        if (btnActualizar) {
            btnActualizar.addEventListener("click", function () {
                if (btnActualizar.disabled) return;
                var ahora = Date.now();
                var transcurrido = ahora - ultimaActualizacion;
                if (transcurrido < intervaloMinimoMs) {
                    bloquearBotonSegundos(btnActualizar, Math.ceil((intervaloMinimoMs - transcurrido) / 1000));
                    return;
                }
                ultimaActualizacion = ahora;
                bloquearBotonSegundos(btnActualizar, Math.ceil(intervaloMinimoMs / 1000));
                dependencias.cargar(id, { bloquearPantalla: false, avisarConteoFilas: false }).catch(function (error) {
                    dependencias.mostrarEstado(nodo, error.message);
                });
            });
        }

        enlazarExportar(barra, nodo, modulo.titulo);

        var btnFiltros = barra.querySelector("[data-accion='filtros']");
        if (btnFiltros && dependencias.abrirFiltrosModulo) {
            btnFiltros.addEventListener("click", function () {
                dependencias.abrirFiltrosModulo(nodo, id, modulo);
            });
        }

        var acciones = {
            "nueva-solicitud": function () { dependencias.solicitud.abrirNueva(); },
            "censo-qr": function () { dependencias.censoQr.abrir(); },
            "guardar-censo": function () { dependencias.censo.guardar(nodo); },
            "nueva-habitacion": function () { dependencias.habitacion.abrirNuevaHabitacion(); },
            "nuevo-bano": function () { dependencias.habitacion.abrirNuevoBano(); },
            "nuevo-tipo-cama": function () { dependencias.habitacion.abrirNuevoTipoCama(); },
            "cargar-camas": function () { dependencias.habitacion.abrirImportacionCamas(); }
        };
        Object.keys(acciones).forEach(function (clave) {
            var boton = barra.querySelector("[data-accion='" + clave + "']");
            if (boton) boton.addEventListener("click", acciones[clave]);
        });

        var guardarCenso = barra.querySelector("[data-rol='guardar-censo']");
        if (guardarCenso) nodo._guardar = guardarCenso;
    }

    function obtenerPanel(id) {
        var nodo = document.querySelector('.modulo-anclado[data-id="' + id + '"]');
        if (nodo) return nodo;
        var plantilla = document.getElementById("plantilla-modulo-" + id);
        if (!plantilla || !plantilla.content) return null;
        var dentro = plantilla.content.querySelector(".modulo-anclado");
        return dentro ? dentro.cloneNode(true) : null;
    }

    function crear(id, modulo) {
        var nodo = obtenerPanel(id);
        if (!nodo) {
            throw new Error("No hay markup Razor para el módulo '" + id + "'. Agregue partial en la página o plantilla.");
        }

        nodo.classList.add("modulo", "modulo-" + id);
        nodo.dataset.id = id;
        nodo.hidden = true;
        nodo._filtro = modulo.filtroInicial();
        enlazarBarra(nodo, id, modulo);
        return nodo;
    }

    return crear;
}