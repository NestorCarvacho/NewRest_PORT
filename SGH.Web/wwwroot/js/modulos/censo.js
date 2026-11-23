import { renderizadorCeldaTruncada } from "../core/celda-truncada.js";

export function crearCenso(dependencias) {
    function combinarFilas(respuestas) {
        var filas = [];
        respuestas.forEach(function (respuesta) { filas.push.apply(filas, respuesta.filas || []); });
        return filas;
    }

    function esVerdadero(valor) {
        return valor === true || valor === "true" || valor === "on" || valor === 1 || valor === "1";
    }

    function normalizarFiltroRango(filtro) {
        var base = dependencias.normalizarFiltroPeriodo
            ? dependencias.normalizarFiltroPeriodo(filtro)
            : filtro || {};
        if (!esVerdadero(base.censoSoloHoy)) return base;
        var hoy = dependencias.fechaActual();
        return Object.assign({}, base, {
            censoSoloHoy: true,
            periodoPorRango: false,
            desde: hoy,
            hasta: hoy
        });
    }

    function payloadRangoCenso(filtro) {
        var payload = normalizarFiltroRango(filtro);
        return {
            opcion: "listarrango",
            moduloId: payload.modulo,
            pisoId: payload.piso,
            habitacionId: payload.habitacion,
            desde: payload.desde,
            hasta: payload.hasta,
            turno: payload.turno || "D"
        };
    }

    function payloadRangoMisCensos(filtro) {
        var payload = normalizarFiltroRango(filtro);
        return {
            opcion: "miscensosrango",
            desde: payload.desde,
            hasta: payload.hasta
        };
    }

    function buscarRango(filtro, enviarMid) {
        var enviar = enviarMid || dependencias.enviar;
        return enviar("/Censo/Mid/MidCenso.cs", payloadRangoCenso(filtro));
    }

    function buscarMiosRango(filtro, enviarMid) {
        var enviar = enviarMid || dependencias.enviar;
        return enviar("/Censo/Mid/MidCenso.cs", payloadRangoMisCensos(filtro));
    }

    function buscar(filtro) {
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", {
            opcion: "listar",
            moduloId: filtro.modulo,
            pisoId: filtro.piso,
            habitacionId: filtro.habitacion,
            fecha: filtro.fecha,
            turno: filtro.turno || "D"
        });
    }

    function buscarMios(filtro) {
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", { opcion: "miscensos", fecha: filtro.fecha });
    }

    function filtros() {
        var catalogo = dependencias.obtenerCatalogo() || { modulos: [], pisos: [], habitaciones: [] };
        return dependencias.camposPeriodoFiltro(dependencias.campo, { etiquetaPeriodo: "Período (fecha censo)" }).concat([
            dependencias.campo("modulo", "Módulo", "select", dependencias.aOpcion(catalogo.modulos), { requerido: true }),
            dependencias.campo("piso", "Piso", "select", dependencias.aOpcion(catalogo.pisos), { requerido: true }),
            dependencias.campo("habitacion", "Habitación", "select", dependencias.aOpcion(catalogo.habitaciones)),
            dependencias.campo("turno", "Tipo Turno", "select", [{ value: "D", text: "Día" }, { value: "N", text: "Noche" }], { sinVacio: true })
        ]);
    }

    function filtrosMisCensos() {
        return dependencias.camposPeriodoFiltro(dependencias.campo, { etiquetaPeriodo: "Período (fecha censo)" });
    }

    function opcionesFiltradas(select, padre) {
        var todas = select._opciones || [];
        if (!padre) return todas;
        return todas.filter(function (opcion) {
            return String(opcion.padre) === String(padre);
        });
    }

    function llenarSelectDependiente(select, opciones) {
        var valorPrevio = select.value;
        var vacio = document.createElement("option");
        vacio.value = "";
        vacio.textContent = "Todos";
        select.replaceChildren(vacio);
        (opciones || []).forEach(function (opcion) {
            var item = document.createElement("option");
            item.value = String(opcion.value);
            item.textContent = opcion.text;
            select.append(item);
        });
        if (valorPrevio && Array.from(select.options).some(function (o) { return o.value === valorPrevio; })) {
            select.value = valorPrevio;
        }
        if (dependencias.actualizarSelector) dependencias.actualizarSelector(select);
    }

    function sincronizarFiltros(form) {
        var modulo = form.querySelector("[name='modulo']");
        var piso = form.querySelector("[name='piso']");
        var habitacion = form.querySelector("[name='habitacion']");
        if (!modulo || !piso || !habitacion) return;
        llenarSelectDependiente(piso, opcionesFiltradas(piso, modulo.value));
        llenarSelectDependiente(habitacion, opcionesFiltradas(habitacion, piso.value));
    }

    function camposPeriodoCenso(form) {
        return [
            form.querySelector("[name='periodoPorRango']"),
            form.querySelector("[name='periodo']"),
            form.querySelector("[name='desde']"),
            form.querySelector("[name='hasta']")
        ].map(function (control) {
            return control ? control.closest(".campo") : null;
        });
    }

    function enlazarPeriodoCenso(form) {
        if (form.dataset.enlazadoPeriodoCenso === "1") return;
        form.dataset.enlazadoPeriodoCenso = "1";
        var soloHoy = form.querySelector("[name='censoSoloHoy']");
        if (!soloHoy) return;
        var porRango = form.querySelector("[name='periodoPorRango']");
        var periodo = form.querySelector("[name='periodo']");
        var desde = form.querySelector("[name='desde']");
        var hasta = form.querySelector("[name='hasta']");

        function aplicarModoSoloHoy() {
            var hoy = dependencias.fechaActual();
            var activo = soloHoy.checked;
            camposPeriodoCenso(form).forEach(function (campo) {
                if (campo) campo.hidden = activo;
            });
            if (activo) {
                if (desde) {
                    desde.value = hoy;
                    desde.readOnly = true;
                }
                if (hasta) {
                    hasta.value = hoy;
                    hasta.readOnly = true;
                }
                if (porRango) {
                    porRango.checked = false;
                    porRango.disabled = true;
                }
                if (periodo) periodo.disabled = true;
                return;
            }
            if (porRango) porRango.disabled = false;
            if (periodo) periodo.disabled = false;
            if (desde) desde.readOnly = false;
            if (hasta) hasta.readOnly = false;
            if (dependencias.refrescarPeriodoFiltro) dependencias.refrescarPeriodoFiltro(form);
        }

        soloHoy.addEventListener("change", aplicarModoSoloHoy);
        form._aplicarCensoSoloHoy = aplicarModoSoloHoy;
        aplicarModoSoloHoy();
    }

    function sincronizarPeriodoCenso(form) {
        if (form && typeof form._aplicarCensoSoloHoy === "function") {
            form._aplicarCensoSoloHoy();
        }
    }

    function permiteGuardarCenso(filtro) {
        return !!fechaUnicaCenso(filtro);
    }

    function actualizarBotonGuardar(nodo) {
        if (!nodo || !nodo._guardar) return;
        var filas = nodo._filas || [];
        var filtro = normalizarFiltroRango(nodo._filtro || {});
        var puedeGuardar = permiteGuardarCenso(filtro);
        nodo._guardar.hidden = filas.length === 0;
        nodo._guardar.disabled = !puedeGuardar;
        nodo._guardar.title = puedeGuardar
            ? ""
            : "Para guardar, filtre un solo día (active «Solo hoy» o iguale desde y hasta).";
    }

    function enlazar(form) {
        if (form.dataset.enlazadoCenso === "1") return;
        form.dataset.enlazadoCenso = "1";
        var modulo = form.querySelector("[name='modulo']");
        var piso = form.querySelector("[name='piso']");
        var habitacion = form.querySelector("[name='habitacion']");
        if (!modulo || !piso || !habitacion) return;
        modulo.addEventListener("change", function () {
            llenarSelectDependiente(piso, opcionesFiltradas(piso, modulo.value));
            llenarSelectDependiente(habitacion, []);
        });
        piso.addEventListener("change", function () {
            llenarSelectDependiente(habitacion, opcionesFiltradas(habitacion, piso.value));
        });
    }

    function filtroInicial(modo) {
        if (modo === "misCensos") return dependencias.filtroInicialPeriodo();
        if (modo === "censo") {
            var hoy = dependencias.fechaActual();
            return {
                censoSoloHoy: true,
                periodoPorRango: false,
                desde: hoy,
                hasta: hoy,
                turno: "D"
            };
        }
        return null;
    }

    function filaCenso(fila) {
        return {
            numero: fila.id,
            fecha: fila.fecha,
            modulo: fila.modulo,
            piso: fila.piso,
            habitacion: fila.habitacion,
            estadoCenso: fila.estadoCenso,
            estadoHabitacion: fila.estadoHabitacion,
            camas: fila.numeroCamas,
            ocupadas: fila.camasOcupadas,
            ocupadasCheckIn: fila.camasCheckIn,
            _fila: fila
        };
    }

    function filaMisCensos(fila) {
        return {
            numero: fila.id,
            fecha: fila.fecha,
            campamento: fila.campamento,
            modulo: fila.modulo,
            piso: fila.piso,
            habitacion: fila.habitacion,
            camas: fila.numeroCamas,
            ocupadas: fila.camasOcupadas,
            turno: fila.turnoNombre || fila.turno,
            _fila: fila
        };
    }

    function prioridadColumnaCenso(clave) {
        if (clave === "habitacion") return 1;
        if (clave === "ocupadas") return 2;
        if (clave === "camas") return 3;
        if (clave === "ocupadasCheckIn") return 4;
        if (clave === "fecha") return 10099;
        if (clave === "turno") return 10008;
        if (clave === "numero") return 10001;
        if (clave === "modulo" || clave === "campamento") return 10002;
        if (clave === "piso") return 10003;
        if (clave === "estadoCenso") return 10005;
        if (clave === "estadoHabitacion") return 10006;
        return 10000;
    }

    function columnas(modo) {
        var renderTruncada = renderizadorCeldaTruncada();
        var celdaTextoAncho = { modulo: true, piso: true, habitacion: true, campamento: true };
        var celdaCompacta = { numero: true, camas: true, estadoCenso: true, estadoHabitacion: true, ocupadasCheckIn: true };

        if (modo === "censo") {
            return ["numero", "fecha", "modulo", "piso", "habitacion", "estadoCenso", "estadoHabitacion", "camas", "ocupadas", "ocupadasCheckIn"].map(function (clave) {
                if (clave === "ocupadas") {
                    return {
                        data: clave,
                        className: "col-ocupadas-censo",
                        responsivePriority: prioridadColumnaCenso(clave),
                        createdCell: function (td, valor, row) {
                            var input = document.createElement("input");
                            input.type = "number";
                            input.min = "0";
                            input.className = "cama-ocupada";
                            input.inputMode = "numeric";
                            input.value = row._fila.camasOcupadas || "0";
                            input.addEventListener("input", function () { row._fila.camasOcupadas = input.value; });
                            td.replaceChildren(input);
                        }
                    };
                }
                var columna = {
                    data: clave,
                    responsivePriority: prioridadColumnaCenso(clave)
                };
                if (celdaTextoAncho[clave]) columna.render = renderTruncada;
                if (clave === "fecha") columna.className = "col-fecha-censo";
                else if (clave === "habitacion") columna.className = "col-habitacion-censo";
                else if (celdaCompacta[clave]) columna.className = "col-compacta-censo";
                return columna;
            });
        }
        if (modo === "misCensos") {
            return ["numero", "fecha", "campamento", "modulo", "piso", "habitacion", "camas", "ocupadas", "turno"]
                .map(function (clave) {
                    var columna = {
                        data: clave,
                        responsivePriority: prioridadColumnaCenso(clave)
                    };
                    if (celdaTextoAncho[clave]) columna.render = renderTruncada;
                    if (clave === "fecha") columna.className = "col-fecha-censo";
                    else if (clave === "habitacion") columna.className = "col-habitacion-censo";
                    else if (clave === "camas" || clave === "ocupadas" || clave === "turno") {
                        columna.className = "col-compacta-censo";
                    }
                    return columna;
                });
        }
        return null;
    }

    function configurarOpcionesTabla(opciones) {
        if (!opciones || !opciones.responsive) return;
        opciones.responsive.details = {
            type: "column",
            target: "tr"
        };
    }

    function filaResponsiveAbierta(tr) {
        if (!tr || tr.classList.contains("child")) return false;
        if (tr.classList.contains("dtr-expanded")) return true;
        var siguiente = tr.nextElementSibling;
        return !!(siguiente && siguiente.classList.contains("child"));
    }

    function enlazarTablaResponsiveCenso(nodo, api) {
        if (!api || !nodo || nodo._censoResponsiveEnlazado) return;
        nodo._censoResponsiveEnlazado = true;
        api.on("responsive-display", function (_evento, dt, row, showHide) {
            if (!showHide) return;
            var filaAbierta = row.node();
            if (!filaAbierta) return;
            dt.rows({ page: "current", search: "applied" }).every(function () {
                var tr = this.node();
                if (!tr || tr === filaAbierta) return;
                if (!filaResponsiveAbierta(tr)) return;
                tr.click();
            });
        });
    }

    function camasComoEntero(valor) {
        var convertido = parseInt(String(valor).trim(), 10);
        return Number.isFinite(convertido) ? convertido : NaN;
    }

    function validarCamasOcupadas(fila) {
        var ocupadas = camasComoEntero(fila.camasOcupadas);
        var total = camasComoEntero(fila.numeroCamas);
        if (!Number.isFinite(ocupadas) || !Number.isFinite(total)) return false;
        return ocupadas >= 0 && ocupadas <= total;
    }

    function fechaUnicaCenso(filtro) {
        if (!filtro) return null;
        var normalizado = normalizarFiltroRango(filtro);
        if (esVerdadero(normalizado.censoSoloHoy)) return dependencias.fechaActual();
        if (normalizado.fecha) return String(normalizado.fecha).slice(0, 10);
        var desde = (normalizado.desde || "").slice(0, 10);
        var hasta = (normalizado.hasta || "").slice(0, 10);
        if (desde && desde === hasta) return desde;
        return null;
    }

    function guardar(nodo) {
        for (var i = 0; i < (nodo._filas || []).length; i++) {
            var fila = nodo._filas[i];
            if (!validarCamasOcupadas(fila)) {
                dependencias.mostrarEstado(nodo, "Las camas ocupadas deben estar entre 0 y el número de camas, revisar id: " + (fila.id) + "");
                return Promise.reject(new Error("Las camas ocupadas deben estar entre 0 y el número de camas"));
            }
        }
        var filtro = nodo._filtro || {};
        var fecha = fechaUnicaCenso(filtro);
        if (!fecha) {
            var mensaje = "Para guardar, el período del filtro debe ser un solo día de censo.";
            dependencias.mostrarEstado(nodo, mensaje);
            return Promise.reject(new Error(mensaje));
        }
        var filas = (nodo._filas || []).map(function (fila) {
            return {
                id: fila.id,
                habitacionId: dependencias.numero(fila.habitacionId),
                cantidad: camasComoEntero(fila.camasOcupadas),
                turno: fila.turno || filtro.turno || "D"
            };
        });
        dependencias.mostrarEstado(nodo, "");
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", { opcion: "guardar", fecha: fecha, filas: filas })
            .then(function () { return dependencias.cargar("censo", { bloquearPantalla: false }); })
            .catch(function (error) { dependencias.mostrarEstado(nodo, error.message); });
    }

    return {
        filtros: filtros,
        filtrosMisCensos: filtrosMisCensos,
        buscarRango: buscarRango,
        buscarMiosRango: buscarMiosRango,
        buscar: buscar,
        buscarMios: buscarMios,
        enlazar: enlazar,
        enlazarPeriodoCenso: enlazarPeriodoCenso,
        sincronizarPeriodoCenso: sincronizarPeriodoCenso,
        sincronizarFiltros: sincronizarFiltros,
        actualizarBotonGuardar: actualizarBotonGuardar,
        permiteGuardarCenso: permiteGuardarCenso,
        filtroInicial: filtroInicial,
        filaCenso: filaCenso,
        filaMisCensos: filaMisCensos,
        columnas: columnas,
        configurarOpcionesTabla: configurarOpcionesTabla,
        enlazarTablaResponsiveCenso: enlazarTablaResponsiveCenso,
        guardar: guardar
    };
}