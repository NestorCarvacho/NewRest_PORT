export function crearMovimientosHabitacion(dependencias) {
    function crearAcciones(fila, modo) {
        var acciones = document.createElement("span");
        acciones.className = "acciones-fila";
        if (modo === "habitaciones") {
            acciones.append(dependencias.boton("Editar", "accion-fila", function () {
                dependencias.prepararCatalogo("habitaciones").then(function () {
                    dependencias.habitacion.abrirEdicionHabitacion(fila._fila);
                }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
            }));
            if (fila._fila.isActive) {
                acciones.append(dependencias.boton("Inactivar", "accion-fila", function () {
                    dependencias.habitacion.inactivarHabitacion(fila._fila.id).catch(function (error) {
                        dependencias.mostrarAviso(error.message, "error");
                    });
                }));
            }
        }
        if (modo === "banos") {
            acciones.append(dependencias.boton("Editar", "accion-fila", function () {
                dependencias.prepararCatalogo("banos").then(function () {
                    dependencias.habitacion.abrirEdicionBano(fila._fila);
                }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
            }));
            if (fila._fila.isActive) {
                acciones.append(dependencias.boton("Inactivar", "accion-fila", function () {
                    dependencias.habitacion.inactivarBano(fila._fila.id).catch(function (error) {
                        dependencias.mostrarAviso(error.message, "error");
                    });
                }));
            }
        }
        return acciones;
    }

    return { crearAcciones: crearAcciones };
}