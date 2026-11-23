export function crearMovimientosReserva(dependencias) {
    function crearAcciones(fila, modo) {
        var acciones = document.createElement("span");
        acciones.className = "acciones-fila";
        if (modo === "reserva") {
            acciones.append(dependencias.boton("Editar", "accion-fila", function () {
                dependencias.prepararCatalogo("reserva").then(function () {
                    dependencias.abrirEdicion(fila);
                }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
            }));
        }
        if (fila.estadoNumero === "A" && !fila.checkIn) acciones.append(crearBotonMovimiento(fila.id, "I", "Check In"));
        if (fila.estadoNumero === "A" && fila.checkIn && !fila.checkOut) acciones.append(crearBotonMovimiento(fila.id, "O", "Check Out"));
        if (fila.estadoNumero === "A" && fila.checkOut) {
            acciones.append(crearBotonAccion(fila.id, "finalizar", "Finalizar", function (id) {
                return dependencias.enviar("/Reserva/Mid/MidReservas.cs", { opcion: "finalizar", reservaId: id });
            }, "Reserva finalizada y habitación liberada.", "¿Finalizar la reserva y liberar la habitación?"));
        }
        if (fila.estadoNumero === "A" || fila.estadoNumero === "P") {
            acciones.append(crearBotonAccion(fila.id, "anular", "Anular", function (id) {
                return dependencias.enviar("/Reserva/Mid/MidReservas.cs", { opcion: "anular", reservaId: id });
            }, "Reserva anulada.", "¿Anular esta reserva?"));
        }
        return acciones;
    }

    function crearBotonAccion(reservaId, clase, texto, enviarAccion, mensajeExito, confirmacion) {
        var boton = dependencias.boton(texto, "accion-fila accion-" + clase, function () {
            if (!window.confirm(confirmacion)) return;
            enviarAccion(reservaId).then(function () {
                var actualizar = dependencias.refrescarFilaReserva
                    ? dependencias.refrescarFilaReserva(reservaId)
                    : (dependencias.cargar
                        ? dependencias.cargar("reserva", { bloquearPantalla: false, avisarConteoFilas: false })
                        : Promise.resolve());
                return actualizar.then(function () { dependencias.mostrarAviso(mensajeExito, "success"); });
            }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
        });
        boton.setAttribute("aria-label", texto);
        boton.setAttribute("title", texto);
        return boton;
    }

    function crearBotonMovimiento(reservaId, operacion, texto) {
        var boton = dependencias.boton(texto, "accion-fila accion-" + (operacion === "I" ? "checkin" : "checkout"), function () {
            dependencias.abrirPanel(texto, dependencias.formulario([
                dependencias.seccion("", [
                    dependencias.campo("fecha", "Fecha", "datetime-local", [], { requerido: true, valor: dependencias.momentoLocal(true) })
                ])
            ], "Continuar", true, function (datos) {
                return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
                    opcion: "movimiento",
                    reservaId: reservaId,
                    fecha: datos.fecha,
                    operacion: operacion
                }).then(function () {
                    var actualizar = dependencias.refrescarFilaReserva
                        ? dependencias.refrescarFilaReserva(reservaId)
                        : (dependencias.cargar
                            ? dependencias.cargar("reserva", { bloquearPantalla: false, avisarConteoFilas: false })
                            : Promise.resolve());
                    return actualizar.then(function () {
                        if (dependencias.cerrarPanel) dependencias.cerrarPanel();
                        dependencias.mostrarAviso(operacion === "I"
                            ? "Check In registrado correctamente."
                            : "Check Out registrado correctamente.", "success");
                    });
                });
            }));
        });
        var svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        svg.setAttribute("viewBox", "0 0 24 24");
        svg.setAttribute("fill", "none");
        svg.setAttribute("stroke", "currentColor");
        svg.setAttribute("stroke-width", "2");
        svg.setAttribute("stroke-linecap", "round");
        svg.setAttribute("stroke-linejoin", "round");
        svg.setAttribute("aria-hidden", "true");
        svg.setAttribute("focusable", "false");
        var partes = operacion === "I"
            ? [["path", "d", "M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"], ["polyline", "points", "10 17 15 12 10 7"], ["path", "d", "M15 12H3"]]
            : [["path", "d", "M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"], ["polyline", "points", "16 17 21 12 16 7"], ["path", "d", "M21 12H9"]];
        partes.forEach(function (parte) {
            var elemento = document.createElementNS("http://www.w3.org/2000/svg", parte[0]);
            elemento.setAttribute(parte[1], parte[2]);
            svg.append(elemento);
        });
        boton.setAttribute("aria-label", texto);
        boton.setAttribute("title", texto);
        boton.replaceChildren(svg);
        return boton;
    }

    function seleccionReserva(nodo) {
        return nodo && nodo._seleccionReserva ? Array.from(nodo._seleccionReserva) : [];
    }

    function checkInMasivo(nodo) {
        var ids = seleccionReserva(nodo);
        if (!ids.length) {
            dependencias.mostrarAviso("Seleccione reservas asignadas sin check-in.", "warning");
            return;
        }
        dependencias.abrirPanel("Check-in masivo", dependencias.formulario([
            dependencias.seccion("", [
                dependencias.campo("fecha", "Fecha check-in", "datetime-local", [], {
                    requerido: true,
                    valor: dependencias.momentoLocal(true)
                })
            ])
        ], "Registrar", true, function (datos) {
            return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
                opcion: "movimientosmasivo",
                reservaIds: ids,
                operacion: "I",
                fecha: datos.fecha
            }).then(function (resultado) {
                var exitos = (resultado && resultado.exitos) ? resultado.exitos.length : 0;
                var fallos = (resultado && resultado.fallos) ? resultado.fallos.length : 0;
                var texto = "Check-in registrado en " + exitos + " reserva(s).";
                if (fallos) texto += " Sin cambio en " + fallos + ".";
                dependencias.mostrarAviso(texto, fallos ? "warning" : "success");
                if (dependencias.cerrarPanel) dependencias.cerrarPanel();
                if (dependencias.cargar) return dependencias.cargar("reserva", { bloquearPantalla: false, avisarConteoFilas: false });
            });
        }));
    }

    return { crearAcciones: crearAcciones, checkInMasivo: checkInMasivo };
}
