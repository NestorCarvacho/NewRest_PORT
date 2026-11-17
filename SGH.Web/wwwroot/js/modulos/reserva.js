export function crearReserva(dependencias) {
    function filtros() {
        var catalogo = dependencias.obtenerCatalogo() || { grupos: [], origenes: [], pasajeros: [], habitaciones: [], turnos: [] };
        var presets = (dependencias.presetsPeriodo || []).map(function (p) { return { value: p.value, text: p.text }; });
        return [
            dependencias.campo("periodo", "Período (fecha reserva)", "select", presets, { sinVacio: true, valor: "30" }),
            dependencias.campo("reservaDesde", "Desde", "date", [], { requerido: false }),
            dependencias.campo("reservaHasta", "Hasta", "date", [], { requerido: false }),
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
        ];
    }

    function filtroInicial(modo) {
        var rango = dependencias.rangoReservaInicial();
        if (modo === "reserva") return Object.assign({ periodo: "30" }, rango);
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
                var activo = document.querySelector(".modulo:not([hidden])");
                return activo ? dependencias.cargar(activo.dataset.id) : null;
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
            return dependencias.cargar(id).then(function () {
                dependencias.mostrarAviso("Reserva creada N° " + resultado.id + ".", "success");
            });
        });
    }

    function buscar(filtro) {
        var payload = dependencias.normalizarFiltroPeriodo
            ? dependencias.normalizarFiltroPeriodo(filtro)
            : (dependencias.aplicarPresetPeriodo
                ? dependencias.aplicarPresetPeriodo(filtro, filtro && filtro.periodo)
                : filtro);
        return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
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
        });
    }

    function fila(fila) {
        return {
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

    function columnas(modo, crearAcciones) {
        return ["acciones", "numero", "fechaReserva", "grupo", "pasajero", "habitacion", "estado", "llegada", "salida", "checkIn", "checkOut"].map(function (clave) {
            if (clave === "checkIn" || clave === "checkOut") return { data: clave, render: renderizarMovimiento };
            if (clave === "llegada" || clave === "salida") return { data: clave, render: renderizarFechaPrevista };
            if (clave !== "acciones") return { data: clave };
            return {
                data: clave,
                orderable: false,
                createdCell: function (td, fila) {
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
        fila: fila,
        columnas: columnas
    };
}
