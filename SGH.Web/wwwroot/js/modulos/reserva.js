export function crearReserva(dependencias) {
  var sincronizacionListado = null;

  function enlazarSincronizacionListado(deps) {
        sincronizacionListado = deps;
    }

    function filasDeRespuesta(respuesta) {
        if (!respuesta) return [];
        if (Array.isArray(respuesta)) return respuesta;
        if (Array.isArray(respuesta.filas)) return respuesta.filas;
        return [];
    }

    function contextoReservaVisible() {
        var activo = document.querySelector('.modulo[data-id="reserva"]:not([hidden])');
        if (!activo) activo = document.querySelector(".modulo:not([hidden])");
        if (!activo || activo.dataset.id !== "reserva") return null;
        return { nodo: activo };
    }

    function filtros() {
        var catalogo = dependencias.obtenerCatalogo() || { grupos: [], origenes: [], pasajeros: [], habitaciones: [], turnos: [] };
        return dependencias.camposPeriodoFiltro(dependencias.campo, {
            etiquetaPeriodo: "Período (fecha reserva)",
            reservaNombres: true
        }).concat([
            dependencias.campo("grupo", "Grupo Empresa", "select", dependencias.aOpcion(catalogo.grupos)),
            dependencias.campo("origen", "Origen reserva", "select", dependencias.aOpcion(catalogo.origenes)),
            dependencias.campo("solicitud", "# Solicitud"),
            dependencias.campo("estado", "Estado", "select", [
                { value: "A", text: "Asignada" },
                { value: "I", text: "Asignada con Checkin" },
                { value: "O", text: "Asignada con Checkout" },
                { value: "N", text: "Anulada" },
                { value: "F", text: "Finalizada" }
            ]),
            dependencias.campo("reserva", "Fecha Reserva", "date"),
            dependencias.campo("llegada", "Fecha Llegada", "date"),
            dependencias.campo("salida", "Fecha Salida", "date"),
            dependencias.campo("checkin", "Check In", "date"),
            dependencias.campo("checkout", "Check Out", "date"),
            dependencias.campo("pasajero", "Pasajero", "select", dependencias.aOpcion(catalogo.pasajeros)),
            dependencias.campo("habitacion", "Habitación", "select", dependencias.aOpcion(catalogo.habitaciones)),
            dependencias.campo("agua", "Tiene Agua?", "select", [{ value: "1", text: "SI" }, { value: "2", text: "NO" }])
        ]);
    }

    function filtroInicial(modo) {
        if (modo === "reserva") return dependencias.filtroInicialPeriodo({ reservaNombres: true });
        return null;
    }

    function formulario() {
        var catalogo = dependencias.obtenerCatalogo() || { grupos: [], origenes: [], pasajeros: [], habitaciones: [], turnos: [] };
        var origen = (catalogo.origenes || []).filter(function (opcion) { return opcion.id === "R"; })[0];
        return [
            dependencias.seccion("Datos", [
                dependencias.campo("numero", "Número", "text", [], { soloLectura: true }),
                dependencias.campo("grupo", "Grupo Empresa", "select", dependencias.aOpcion(catalogo.grupos), { requerido: true }),
                dependencias.campo("origen", "Origen reserva", "text", [], { soloLectura: true, valor: origen ? origen.etiqueta : "R" }),
                dependencias.campo("solicitud", "# Solicitud", "text", [], { soloLectura: true, valor: "0" })
            ]),
            dependencias.seccion("Estadía", [
                dependencias.campo("reserva", "Fecha Reserva", "datetime-local", [], { requerido: true, valor: dependencias.momentoLocal(true) }),
                dependencias.campo("llegada", "Fecha Llegada", "datetime-local", [], { requerido: true, valor: dependencias.momentoLocal(true) }),
                dependencias.campo("salida", "Fecha Salida", "datetime-local"),
                dependencias.campo("turno", "Turno", "select", dependencias.aOpcion(catalogo.turnos), { requerido: true }),
                dependencias.campo("estado", "Estado", "text", [], { soloLectura: true, valor: "P" })
            ]),
            dependencias.seccion("", [
                dependencias.campo("observaciones", "Observaciones", "textarea")
            ])
        ];
    }

    function abrirEdicion(fila) {
        var catalogo = dependencias.obtenerCatalogo() || { grupos: [], pasajeros: [], habitaciones: [], turnos: [] };
        var fechaReservaInicial = dependencias.fechaListadoAInput(fila.fechaReserva) || dependencias.momentoLocal(false);
        var fechaLlegadaInicial = dependencias.fechaListadoAInput(fila.llegada) || dependencias.fechaListadoAInput(fila.fechaLlegada);
        var fechaSalidaInicial = dependencias.fechaListadoAInput(fila.salida) || dependencias.fechaListadoAInput(fila.fechaSalida);
        var bloqueada = fila.estadoNumero === "F" || fila.estadoNumero === "N";
        var habitaciones = dependencias.aOpcion(catalogo.habitaciones);
        if (habitaciones.length === 1) {
            habitaciones.push({ value: "", text: "No hay habitaciones reservables", deshabilitado: true });
        }
        var form = dependencias.formulario([
            dependencias.seccion("Asignación", [
                dependencias.campo("areaId", "Grupo Empresa", "select", dependencias.aOpcion(catalogo.grupos), { sinVacio: false, deshabilitado: true, valor: fila.areaId }),
                dependencias.campo("pasajeroId", "Nombre", "select", dependencias.aOpcion(catalogo.pasajeros), { requerido: true, deshabilitado: bloqueada, valor: fila.pasajeroId || "" }),
                dependencias.campo("habitacionId", "Habitación", "select", habitaciones, { deshabilitado: fila.estadoNumero !== "P" && catalogo.habitaciones.length > 0, valor: fila.habitacionId || "" }),
                dependencias.campo("turnoId", "Turno", "select", dependencias.aOpcion(catalogo.turnos), { deshabilitado: true, valor: fila.turnoId || "" })
            ]),
            dependencias.seccion("Estado", [
                dependencias.campo("estado", "Estado", "select", [
                    { value: "P", text: "Pre-reserva" },
                    { value: "A", text: "Asignada" },
                    { value: "F", text: "Finalizada" },
                    { value: "N", text: "Anulada" }
                ], { deshabilitado: true, valor: fila.estadoNumero || "P" }),
                dependencias.campo("fechaReserva", "Fecha Reserva", "date", [], { valor: fechaReservaInicial }),
                dependencias.campo("fechaLlegada", "Fecha Llegada", "date", [], { valor: fechaLlegadaInicial }),
                dependencias.campo("fechaSalida", "Fecha Salida", "date", [], { valor: fechaSalidaInicial }),
                dependencias.campo("observacion", "Observación", "textarea")
            ])
        ], "Guardar", true, function (datos) {
            return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
                opcion: "editar",
                reservaId: fila.id,
                areaId: dependencias.numero(form.querySelector("[name='areaId']").value),
                turnoId: dependencias.numero(datos.turnoId) || fila.turnoId,
                fechaReserva: datos.fechaReserva || fechaReservaInicial,
                fechaLlegada: datos.fechaLlegada || fechaLlegadaInicial,
                fechaSalida: datos.fechaSalida || fechaSalidaInicial || null,
                observacion: datos.observacion,
                pasajeroId: dependencias.numero(form.querySelector("[name='pasajeroId']").value) || null,
                habitacionId: dependencias.numero(datos.habitacionId) || null,
                estado: datos.estado || fila.estadoNumero || "P",
                tieneAgua: false
            }).then(function () {
                return refrescarFilaEnListado(fila.id);
            });
        });
        dependencias.abrirPanel("Editar reserva " + fila.id, form);
    }

    function crear(datos, id) {
        return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
            opcion: "crear",
            areaId: dependencias.numero(datos.grupo),
            turnoId: dependencias.numero(datos.turno),
            fechaReserva: datos.reserva,
            fechaLlegada: datos.llegada,
            fechaSalida: datos.salida,
            observacion: datos.observaciones
        }).then(function (resultado) {
            return refrescarFilaEnListado(resultado.id).then(function () {
                dependencias.mostrarAviso("Reserva creada N° " + resultado.id + ".", "success");
            });
        });
    }

    function payloadListar(filtro, reservaId) {
        var payload = dependencias.normalizarFiltroPeriodo
            ? dependencias.normalizarFiltroPeriodo(filtro)
            : (dependencias.aplicarPresetPeriodo
                ? dependencias.aplicarPresetPeriodo(filtro, filtro && filtro.periodo)
                : filtro);
        var cuerpo = {
            opcion: "listar",
            areaId: payload.grupo,
            origen: payload.origen,
            solicitudId: payload.solicitud,
            estado: payload.estado,
            fechaReserva: payload.reserva,
            fechaReservaDesde: payload.reservaDesde,
            fechaReservaHasta: payload.reservaHasta,
            fechaLlegada: payload.llegada,
            fechaSalida: payload.salida,
            checkIn: payload.checkin,
            checkOut: payload.checkout,
            pasajeroId: payload.pasajero,
            habitacionId: payload.habitacion,
            tieneAgua: payload.agua
        };
        if (reservaId) cuerpo.id = reservaId;
        return cuerpo;
    }

    function buscar(filtro, enviarMid) {
        var enviar = enviarMid || dependencias.enviar;
        return enviar("/Reserva/Mid/MidReservas.cs", payloadListar(filtro, 0));
    }

    function buscarUnaReserva(reservaId, enviarMid) {
        var ctx = contextoReservaVisible();
        var filtro = ctx && ctx.nodo._filtro ? ctx.nodo._filtro : {};
        var enviar = enviarMid || dependencias.enviar;
        return enviar("/Reserva/Mid/MidReservas.cs", payloadListar(filtro, reservaId)).then(filasDeRespuesta).then(function (filas) {
            return filas[0] || null;
        });
    }

    function refrescarFilaEnListado(reservaId) {
        var ctx = contextoReservaVisible();
        if (!ctx || !sincronizacionListado) {
            return dependencias.cargar("reserva", { bloquearPantalla: false, avisarConteoFilas: false });
        }
        return buscarUnaReserva(reservaId).then(function (fila) {
            if (!fila) return;
            sincronizacionListado.sincronizarFila(ctx.nodo, reservaId, fila, "reserva");
        }).catch(function () {
            return dependencias.cargar("reserva", { bloquearPantalla: false, avisarConteoFilas: false });
        });
    }

    function fila(fila) {
        return {
            seleccionMasiva: fila,
            acciones: fila,
            numero: fila.id,
            fechaReserva: fila.fechaReserva,
            grupo: fila.grupoEmpresa,
            pasajero: fila.pasajero,
            habitacion: fila.habitacion,
            estado: fila.estado,
            llegada: fila.llegada,
            salida: fila.salida,
            checkIn: fila.checkIn || "",
            checkOut: fila.checkOut || "",
            _fila: fila
        };
    }

    function renderizarMovimiento(valor, tipo) {
        if (!valor) return "";
        if (tipo === "sort" || tipo === "type") {
            var partes = /^(\d{2})\/(\d{2})\/(\d{4}) (\d{2}:\d{2})$/.exec(valor);
            if (partes) return partes[3] + "-" + partes[2] + "-" + partes[1] + " " + partes[4];
        }
        return DataTable.render.text().display(valor);
    }

    function renderizarFechaPrevista(valor, tipo) {
        if (!valor) return "";
        var partes = /^(\d{2})\/(\d{2})\/(\d{4})(?: \d{2}:\d{2})?$/.exec(valor);
        if (!partes) return DataTable.render.text().display(valor);
        if (tipo === "sort" || tipo === "type") return partes[3] + "-" + partes[2] + "-" + partes[1];
        return partes[1] + "/" + partes[2] + "/" + partes[3];
    }

    function textoMovimiento(valor) {
        return valor != null && String(valor).trim() !== "";
    }

    function registroDesdeMapeada(mapeada, celda) {
        if (mapeada && mapeada._fila) return mapeada._fila;
        if (mapeada && mapeada.estadoNumero !== undefined) return mapeada;
        if (celda && celda.estadoNumero !== undefined) return celda;
        return null;
    }

    function elegibleCheckInMasivo(fila) {
        return !!(fila && fila.estadoNumero === "A" && !textoMovimiento(fila.checkIn));
    }

    function elegibleCheckOutMasivo(fila) {
        return !!(fila && fila.estadoNumero === "A" && textoMovimiento(fila.checkIn) && !textoMovimiento(fila.checkOut));
    }

    function elegibleSeleccionMasiva(fila) {
        return elegibleCheckInMasivo(fila) || elegibleCheckOutMasivo(fila);
    }

    function moduloReservaDesde(td) {
        return td.closest('.modulo[data-id="reserva"]') || td.closest(".modulo-reserva");
    }

    function pintarSeleccionMasiva(td, mapeada, celda) {
        td.replaceChildren();
        var fila = registroDesdeMapeada(mapeada, celda);
        if (!elegibleSeleccionMasiva(fila)) return;
        var modulo = moduloReservaDesde(td);
        if (!modulo) return;
        if (!modulo._seleccionReserva) modulo._seleccionReserva = new Set();
        var checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.className = "seleccion-reserva";
        checkbox.setAttribute("aria-label", "Seleccionar reserva " + fila.id);
        checkbox.checked = modulo._seleccionReserva.has(fila.id);
        checkbox.addEventListener("change", function () {
            if (checkbox.checked) modulo._seleccionReserva.add(fila.id);
            else modulo._seleccionReserva.delete(fila.id);
        });
        td.append(checkbox);
    }

    function enlazarTablaDraw(nodo, api, herramientas) {
        if (!api || !herramientas || nodo._reservaTablaDraw) return;
        nodo._reservaTablaDraw = true;
        function repintarCeldasReserva() {
            api.rows({ page: "current" }).every(function () {
                var mapeada = this.data();
                var indice = this.index();
                var celdaSel = herramientas.nodoCeldaDatos(api, indice, "seleccionMasiva");
                if (celdaSel) pintarSeleccionMasiva(celdaSel, mapeada);
                if (!herramientas.crearAcciones) return;
                var celdaAcciones = herramientas.nodoCeldaDatos(api, indice, "acciones");
                if (!celdaAcciones) return;
                var fila = registroDesdeMapeada(mapeada);
                celdaAcciones.replaceChildren(herramientas.crearAcciones(fila, "reserva"));
            });
        }
        api.on("draw", repintarCeldasReserva);
        repintarCeldasReserva();
    }

    function columnaSeleccionMasiva() {
        return {
            data: "seleccionMasiva",
            orderable: false,
            className: "col-seleccion-masiva",
            responsivePriority: 1,
            defaultContent: "",
            render: function () { return ""; },
            createdCell: function (td, celda, filaMapeada) {
                pintarSeleccionMasiva(td, filaMapeada, celda);
            }
        };
    }

    function columnas(modo, crearAcciones) {
        var claves = ["seleccionMasiva", "acciones", "numero", "fechaReserva", "grupo", "pasajero", "habitacion", "estado", "llegada", "salida", "checkIn", "checkOut"];
        return claves.map(function (clave) {
            if (clave === "seleccionMasiva") return columnaSeleccionMasiva();
            if (clave === "checkIn" || clave === "checkOut") return { data: clave, render: renderizarMovimiento };
            if (clave === "llegada" || clave === "salida") {
                return {
                    data: clave,
                    className: "col-fecha-prevista",
                    render: renderizarFechaPrevista
                };
            }
            if (clave !== "acciones") return { data: clave };
            return {
                data: clave,
                orderable: false,
                className: "col-acciones-reserva",
                defaultContent: "",
                render: function () { return ""; },
                createdCell: function (td, celda) {
                    var fila = celda && celda.estadoNumero !== undefined ? celda : (celda && celda._fila) || celda;
                    td.replaceChildren(crearAcciones(fila, modo));
                }
            };
        });
    }

    return {
        filtros: filtros,
        filtroInicial: filtroInicial,
        formulario: formulario,
        abrirEdicion: abrirEdicion,
        crear: crear,
        buscar: buscar,
        refrescarFilaEnListado: refrescarFilaEnListado,
        enlazarSincronizacionListado: enlazarSincronizacionListado,
        fila: fila,
        columnas: columnas,
        pintarSeleccionMasiva: pintarSeleccionMasiva,
        enlazarTablaDraw: enlazarTablaDraw,
        elegibleCheckInMasivo: elegibleCheckInMasivo,
        elegibleCheckOutMasivo: elegibleCheckOutMasivo
    };
}
