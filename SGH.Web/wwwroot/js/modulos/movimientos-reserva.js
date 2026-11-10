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
        return acciones;
    }

    function crearBotonMovimiento(reservaId, operacion, texto) {
        return dependencias.boton(texto, "accion-fila", function () {
            dependencias.abrirPanel("Seleccionar Fecha", dependencias.formulario([
                dependencias.seccion("", [
                    dependencias.campo("fecha", "Fecha", "datetime-local", [], { requerido: true, valor: dependencias.momentoLocal(true) })
                ])
            ], "Continuar", true, function (datos) {
                return dependencias.enviar("/Reserva/Datos?handler=Movimiento", {
                    reservaId: reservaId,
                    fecha: datos.fecha,
                    operacion: operacion
                }).then(function () {
                    var activo = document.querySelector(".modulo:not([hidden])");
                    return (activo ? dependencias.cargar(activo.dataset.id) : Promise.resolve()).then(function () {
                        dependencias.mostrarAviso(operacion === "I"
                            ? "Check In registrado correctamente."
                            : "Check Out registrado correctamente.", "success");
                    });
                });
            }));
        });
    }

    return { crearAcciones: crearAcciones };
}
