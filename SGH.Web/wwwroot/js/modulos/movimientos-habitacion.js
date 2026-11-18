import { crearAccionesDesplegable } from "../core/acciones-desplegable.js";

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
        var items = [];
        if (modo === "habitaciones") {
            items.push(["Editar", function () {
                dependencias.prepararCatalogo("habitaciones").then(function () {
                    dependencias.habitacion.abrirEdicionHabitacion(fila);
                }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
            }]);
            items.push(["Camas", function () { dependencias.habitacion.abrirCamasHabitacion(fila); }]);
            if (estaActivo(fila)) {
                items.push(["Inactivar", function () {
                    dependencias.habitacion.inactivarHabitacion(fila.id).catch(function (error) {
                        dependencias.mostrarAviso(error.message, "error");
                    });
                }]);
            }
        }
        if (modo === "banos") {
            var activo = estaActivo(fila);
            items.push(["Editar", function () {
                dependencias.prepararCatalogo("banos").then(function () {
                    dependencias.habitacion.abrirEdicionBano(fila);
                }).catch(function (error) { dependencias.mostrarAviso(error.message, "error"); });
            }]);
            items.push([activo ? "Inactivar" : "Activar", function () {
                dependencias.habitacion.alternarEstadoBano(fila).catch(function (error) {
                    dependencias.mostrarAviso(error.message, "error");
                });
            }]);
        }
        if (modo === "tiposcama") {
            var tipoActivo = fila.isActive === true || fila.isActive === 1;
            items.push(["Editar", function () { dependencias.habitacion.abrirEdicionTipoCama(fila); }]);
            items.push([tipoActivo ? "Inactivar" : "Activar", function () {
                dependencias.habitacion.alternarEstadoTipoCama(fila).catch(function (error) {
                    dependencias.mostrarAviso(error.message, "error");
                });
            }]);
        }
        if (!items.length) return document.createTextNode("");
        return crearAccionesDesplegable(dependencias.boton, items);
    }

    return { crearAcciones: crearAcciones };
}