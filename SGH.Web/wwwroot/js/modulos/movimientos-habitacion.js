export function crearMovimientosHabitacion(dependencias) {
    /** En createdCell, DataTables entrega el valor de la columna (registro API), no la fila mapeada. */
    function origen(dato) {
        if (!dato) return null;
        return dato._fila != null ? dato._fila : dato;
    }

    function estaActivo(registro) {
        if (!registro) return false;
        var estadoActivo = String(registro.estado || "").toUpperCase() === "ACTIVO";
        if (registro.isActive === true || registro.isActive === 1) return true;
        if (registro.isActive === false || registro.isActive === 0) return estadoActivo;
        return estadoActivo;
    }

    function crearAcciones(dato, modo) {
        var fila = origen(dato);
        if (!fila) return document.createTextNode("");
        var acciones = document.createElement("span");
        acciones.className = "acciones-fila";
        if (modo === "habitaciones") {
            acciones.append(dependencias.boton("Editar", "accion-fila", function () {
                dependencias.prepararCatalogo("habitaciones").then(function () {
                    dependencias.habitacion.abrirEdicionHabitacion(fila);
                }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
            }));
            if (estaActivo(fila)) {
                acciones.append(dependencias.boton("Inactivar", "accion-fila", function () {
                    dependencias.habitacion.inactivarHabitacion(fila.id).catch(function (error) {
                        dependencias.mostrarAviso(error.message, "error");
                    });
                }));
            }
        }
        if (modo === "banos") {
            acciones.append(dependencias.boton("Editar", "accion-fila", function () {
                dependencias.prepararCatalogo("banos").then(function () {
                    dependencias.habitacion.abrirEdicionBano(fila);
                }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
            }));
            if (estaActivo(fila)) {
                acciones.append(dependencias.boton("Inactivar", "accion-fila", function () {
                    dependencias.habitacion.inactivarBano(fila.id).catch(function (error) {
                        dependencias.mostrarAviso(error.message, "error");
                    });
                }));
            }
        }
        return acciones;
    }

    return { crearAcciones: crearAcciones };
}