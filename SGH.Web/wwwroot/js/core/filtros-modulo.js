import { bloquearBotonSegundos } from "./boton-espera.js";

export function crearFiltrosModulo(dependencias) {
    function catalogoPara(modo) {
        if (modo === "habitaciones" || modo === "banos" || modo === "tiposcama") {
            var hab = dependencias.obtenerCatalogoHabitacion && dependencias.obtenerCatalogoHabitacion();
            return (hab && hab.registros) ? hab.registros : hab;
        }
        if (modo === "censo" || modo === "misCensos") {
            var cen = dependencias.obtenerCatalogoCenso && dependencias.obtenerCatalogoCenso();
            return (cen && cen.registros) ? cen.registros : cen;
        }
        if (modo === "solicitud") {
            var sol = dependencias.obtenerCatalogoSolicitud && dependencias.obtenerCatalogoSolicitud();
            return (sol && sol.registros) ? sol.registros : sol;
        }
        var res = dependencias.obtenerCatalogoReserva && dependencias.obtenerCatalogoReserva();
        return (res && res.registros) ? res.registros : res;
    }

    function llenarSelect(select, opciones, vacio) {
        var valorPrevio = select.value;
        select.replaceChildren();
        if (vacio !== false) {
            var todos = document.createElement("option");
            todos.value = "";
            todos.textContent = vacio === true ? "Todos" : "Seleccione";
            select.append(todos);
        }
        (opciones || []).forEach(function (op) {
            var item = document.createElement("option");
            item.value = String(op.value);
            item.textContent = op.text;
            if (op.padre != null) item.dataset.padre = String(op.padre);
            select.append(item);
        });
        select._opciones = opciones || [];
        if (valorPrevio && Array.from(select.options).some(function (o) { return o.value === valorPrevio; })) {
            select.value = valorPrevio;
        }
    }

    function rellenarCatalogo(form, modo) {
        var catalogo = catalogoPara(modo) || {};
        form.querySelectorAll("[data-catalogo]").forEach(function (select) {
            var clave = select.getAttribute("data-catalogo");
            var lista = catalogo[clave];
            if (!lista) return;
            var opciones = dependencias.aOpcion(lista);
            llenarSelect(select, opciones, true);
        });
    }

    function aplicarFiltro(form, filtro) {
        if (!filtro) return;
        Object.keys(filtro).forEach(function (nombre) {
            var control = form.querySelector("[name='" + nombre + "']");
            if (!control) return;
            if (control.type === "checkbox") control.checked = !!filtro[nombre];
            else control.value = filtro[nombre] == null ? "" : String(filtro[nombre]);
        });
    }

    function enlazarSubmit(form, nodo, id, modo) {
        if (form.dataset.enlazado) return;
        form.dataset.enlazado = "1";
        form.querySelectorAll("[data-accion='cerrar-panel']").forEach(function (btn) {
            btn.addEventListener("click", dependencias.cerrarPanel);
        });
        form.addEventListener("submit", function (evento) {
            evento.preventDefault();
            var datos = dependencias.leerFormulario(form);
            nodo._filtro = dependencias.normalizarFiltroPeriodo
                ? dependencias.normalizarFiltroPeriodo(datos)
                : datos;
            if (modo === "censo" && dependencias.actualizarBotonGuardarCenso) {
                dependencias.actualizarBotonGuardarCenso(nodo);
            }
            dependencias.cerrarPanel();
            dependencias.cargar(id).catch(function (error) {
                dependencias.mostrarEstado(nodo, error.message);
            });
        });
    }

    function abrir(nodo, id, modulo) {
        var form = nodo.querySelector("form.formulario-filtros-modulo");
        if (!form) return abrirDinamico(nodo, id, modulo);

        return dependencias.prepararCatalogo(modulo.modo).then(function () {
            rellenarCatalogo(form, modulo.modo);
            aplicarFiltro(form, nodo._filtro);
            if (modulo.modo === "censo" && dependencias.enlazarFiltrosCenso) {
                dependencias.enlazarFiltrosCenso(form);
            }
            dependencias.enlazarPeriodoFiltro(form);
            if (modulo.modo === "censo" && dependencias.enlazarPeriodoCenso) {
                dependencias.enlazarPeriodoCenso(form);
            }
            dependencias.inicializarSelectores(form);
            if (modulo.modo === "censo") {
                if (dependencias.sincronizarPeriodoCenso) dependencias.sincronizarPeriodoCenso(form);
                if (dependencias.sincronizarFiltrosCenso) dependencias.sincronizarFiltrosCenso(form);
            }
            enlazarSubmit(form, nodo, id, modulo);
            form._contenedorPadre = nodo;
            dependencias.abrirPanel("Filtros de " + modulo.titulo, form);
        }).catch(function (error) {
            dependencias.mostrarEstado(nodo, error.message);
        });
    }

    function abrirDinamico(nodo, id, modulo) {
        return dependencias.prepararCatalogo(modulo.modo).then(function () {
            dependencias.abrirPanel("Filtros de " + modulo.titulo, dependencias.formulario(
                dependencias.contenido(modulo.filtros).map(function (grupo) {
                    return { titulo: grupo.titulo, campos: grupo.campos.map(function (campo) {
                        return Object.assign({}, campo, { valor: nodo._filtro[campo.nombre] });
                    }) };
                }),
                "Aplicar",
                false,
                function (datos) {
                    nodo._filtro = dependencias.normalizarFiltroPeriodo
                        ? dependencias.normalizarFiltroPeriodo(datos)
                        : datos;
                    return dependencias.cargar(id);
                }
            ));
        });
    }

    return { abrir: abrir };
}