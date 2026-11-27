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
        if (fila.estadoNumero === "A" && (fila.checkIn || fila.checkOut)) {
            var quitaCheckOut = !!fila.checkOut;
            acciones.append(crearBotonAccion(fila.id, "reversar", "Anular", function (id) {
                return dependencias.enviar("/Reserva/Mid/MidReservas.cs", { opcion: "reversar", reservaId: id });
            }, quitaCheckOut ? "Check-out anulado." : "Check-in anulado.",
                quitaCheckOut
                    ? "¿Anular el check-out y volver a estado con check-in?"
                    : "¿Anular el check-in y dejar la reserva sin ingreso?"));
        }
        if (fila.estadoNumero === "A" && fila.checkOut) {
            acciones.append(crearBotonAccion(fila.id, "finalizar", "Finalizar", function (id) {
                return dependencias.enviar("/Reserva/Mid/MidReservas.cs", { opcion: "finalizar", reservaId: id });
            }, "Reserva finalizada y habitación liberada.", "¿Finalizar la reserva y liberar la habitación?"));
        }
        var sinMovimiento = !fila.checkIn && !fila.checkOut;
        if (sinMovimiento && (fila.estadoNumero === "A" || fila.estadoNumero === "P")) {
            acciones.append(crearBotonAccion(fila.id, "anular-reserva", "Anular", function (id) {
                return dependencias.enviar("/Reserva/Mid/MidReservas.cs", { opcion: "anular", reservaId: id });
            }, "Reserva anulada.", "¿Anular esta reserva por completo?"));
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
        var boton = dependencias.boton(texto, "accion-fila accion-" + (
            (operacion === "I")
                ? "checkin"
                : (operacion === "O")
                    ? "checkout"
                    : (operacion === "F")
                        ? "finalizar"
                        : "checkout"), function () {
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
                            : (operacion === "O")
                                ? "Check Out registrado correctamente."
                                : "Finalizado correctamente.", "success");
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
            : (operacion === "O")
                ? [["path", "d", "M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"], ["polyline", "points", "16 17 21 12 16 7"], ["path", "d", "M21 12H9"]]
                : [["path", "d", "M12 2a10 10 0 1 0 10 10A10 10 0 0 0 12 2z"], ["polyline", "points", "8 12 12 16 16 12"], ["path", "d", "M12 8v8"]];
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

    function filaPorId(nodo, reservaId) {
        var filas = (nodo && nodo._filas) || [];
        for (var i = 0; i < filas.length; i++) {
            if (Number(filas[i].id) === Number(reservaId)) return filas[i];
        }
        return null;
    }

    function idsSeleccionElegibles(nodo, esElegible) {
        return seleccionReserva(nodo).filter(function (id) {
            var fila = filaPorId(nodo, id);
            return fila && esElegible(fila);
        });
    }

    function botonMovimientoMasivo(nodo) {
        if (!nodo) return null;
        return nodo.querySelector(".modulo-barra [data-accion='movimiento-masivo']");
    }

    function actualizarBotonMovimientoMasivo(nodo) {
        var boton = botonMovimientoMasivo(nodo);
        if (!boton) return;
        var ids = seleccionReserva(nodo);
        var modo = nodo._modoSeleccionMasiva;
        if (!ids.length || !modo) {
            boton.disabled = true;
            boton.textContent = "Check-in masivo";
            boton.dataset.operacionMasiva = "";
            boton.setAttribute("aria-disabled", "true");
            return;
        }
        if (modo === "O") {
            boton.textContent = "Check-out masivo";
            boton.dataset.operacionMasiva = "O";
        } 
        else if (modo === "F") {
            boton.textContent = "Finalizar masivo";
            boton.dataset.operacionMasiva = "F";
        }
        else {
            boton.textContent = "Check-in masivo";
            boton.dataset.operacionMasiva = "I";
        }
        boton.disabled = false;
        boton.removeAttribute("aria-disabled");
    }

    function enlazarBotonMovimientoMasivo(nodo) {
        if (!nodo || nodo._movimientoMasivoEnlazado) return;
        nodo._movimientoMasivoEnlazado = true;
        nodo._actualizarBotonMovimientoMasivo = function () {
            actualizarBotonMovimientoMasivo(nodo);
        };
        actualizarBotonMovimientoMasivo(nodo);
    }

    function limpiarSeleccionMasiva(nodo) {
        if (nodo._seleccionReserva) nodo._seleccionReserva.clear();
        nodo._modoSeleccionMasiva = null;
        actualizarBotonMovimientoMasivo(nodo);
        if (typeof nodo._repintarSeleccionMasiva === "function") nodo._repintarSeleccionMasiva();
        if (typeof nodo._actualizarEncabezadoSeleccionMasiva === "function") nodo._actualizarEncabezadoSeleccionMasiva();
    }

    // Función para manejar el movimiento masivo según el modo seleccionado
    function movimientoMasivo(nodo) {
        var modo = nodo._modoSeleccionMasiva;
        if (modo === "O") checkOutMasivo(nodo);
        else if (modo === "F") finalizarMasivo(nodo);
        else checkInMasivo(nodo);
    }

    function checkInMasivo(nodo) {
        var esElegible = dependencias.elegibleCheckInMasivo || function (fila) {
            return fila && fila.estadoNumero === "A" && !fila.checkIn;
        };
        var ids = idsSeleccionElegibles(nodo, esElegible);
        if (!ids.length) {
            dependencias.mostrarAviso("Seleccione reservas asignadas sin check-in.", "warning");
            return;
        }
        dependencias.abrirPanel("Check-in masivo", dependencias.formulario([
            dependencias.seccion("", [
                dependencias.campo("fecha", "Fecha check-in", "datetime-local", [], {
                    requerido: true,
                    soloLectura: true,
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
                limpiarSeleccionMasiva(nodo);
                if (dependencias.cargar) return dependencias.cargar("reserva", { bloquearPantalla: false, avisarConteoFilas: false });
            });
        }));
    }

    function checkOutMasivo(nodo) {
        var esElegible = dependencias.elegibleCheckOutMasivo || function (fila) {
            return fila && fila.estadoNumero === "A" && fila.checkIn && !fila.checkOut;
        };
        var ids = idsSeleccionElegibles(nodo, esElegible);
        if (!ids.length) {
            dependencias.mostrarAviso("Seleccione reservas con check-in y sin check-out.", "warning");
            return;
        }
        dependencias.abrirPanel("Check-out masivo", dependencias.formulario([
            dependencias.seccion("", [
                dependencias.campo("fecha", "Fecha check-out", "datetime-local", [], {
                    requerido: true,
                    soloLectura: true,
                    valor: dependencias.momentoLocal(true)
                })
            ])
        ], "Registrar", true, function (datos) {
            return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
                opcion: "movimientosmasivo",
                reservaIds: ids,
                operacion: "O",
                fecha: datos.fecha
            }).then(function (resultado) {
                var exitos = (resultado && resultado.exitos) ? resultado.exitos.length : 0;
                var fallos = (resultado && resultado.fallos) ? resultado.fallos.length : 0;
                var texto = "Check-out registrado en " + exitos + " reserva(s).";
                if (fallos) texto += " Sin cambio en " + fallos + ".";
                dependencias.mostrarAviso(texto, fallos ? "warning" : "success");
                if (dependencias.cerrarPanel) dependencias.cerrarPanel();
                limpiarSeleccionMasiva(nodo);
                if (dependencias.cargar) return dependencias.cargar("reserva", { bloquearPantalla: false, avisarConteoFilas: false });
            });
        }));
    }

    function finalizarMasivo(nodo) {
        var esElegible = dependencias.elegibleFinalizarMasivo || function (fila) {
            return fila && fila.estadoNumero === "A" && fila.checkIn && fila.checkOut;
        };
        var ids = idsSeleccionElegibles(nodo, esElegible);
        if (!ids.length) {
            dependencias.mostrarAviso("Seleccione reservas con check-in y check-out hechos", "warning");
            return;
        }
        dependencias.abrirPanel("Finalizar masivo", dependencias.formulario([
            dependencias.seccion("", [
                dependencias.campo("fecha", "Fecha finalización", "datetime-local", [], {
                    requerido: true,
                    soloLectura: true,
                    valor: dependencias.momentoLocal(true)
                })
            ])
        ], "Registrar", true, function (datos) {
            return dependencias.enviar("/Reserva/Mid/MidReservas.cs", {
                opcion: "finalizarmasivo",
                reservaIds: ids,
                operacion: "F",
                fecha: datos.fecha
            }).then(function (resultado) {
                var exitos = (resultado && resultado.exitos) ? resultado.exitos.length : 0;
                var fallos = (resultado && resultado.fallos) ? resultado.fallos.length : 0;
                var texto = "Finalización registrada en " + exitos + " reserva(s).";
                if (fallos) texto += " Sin cambio en " + fallos + ".";
                dependencias.mostrarAviso(texto, fallos ? "warning" : "success");
                if (dependencias.cerrarPanel) dependencias.cerrarPanel();
                limpiarSeleccionMasiva(nodo);
                if (dependencias.cargar) return dependencias.cargar("reserva", { bloquearPantalla: false, avisarConteoFilas: false });
            });
        }));
    }

    return {
        crearAcciones: crearAcciones,
        checkInMasivo: checkInMasivo,
        checkOutMasivo: checkOutMasivo,
        movimientoMasivo: movimientoMasivo,
        enlazarBotonMovimientoMasivo: enlazarBotonMovimientoMasivo,
        actualizarBotonMovimientoMasivo: actualizarBotonMovimientoMasivo,
        finalizarMasivo: finalizarMasivo
    };
}
