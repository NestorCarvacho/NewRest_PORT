import { numero } from "../../core/numeros.js";
import { encabezadosModulo } from "../../core/encabezados-tabla.js";
import { crearHabitacion } from "../habitacion.js";
import { crearMovimientosHabitacion } from "../movimientos-habitacion.js";

export function definirPantallasHabitacion(habitacion) {
    return {
        habitaciones: {
            titulo: "Habitaciones",
            modo: "habitaciones",
            columnas: encabezadosModulo("habitaciones"),
            filtros: function () { return habitacion.filtros("habitaciones"); },
            filtroInicial: function () { return habitacion.filtroInicial("habitaciones"); }
        },
        banos: {
            titulo: "Baños",
            modo: "banos",
            columnas: encabezadosModulo("banos"),
            filtros: function () { return habitacion.filtros("banos"); },
            filtroInicial: function () { return habitacion.filtroInicial("banos"); }
        },
        tiposcama: {
            titulo: "Tipos de cama",
            modo: "tiposcama",
            columnas: encabezadosModulo("tiposcama"),
            filtros: function () { return habitacion.filtros("tiposcama"); },
            filtroInicial: function () { return habitacion.filtroInicial("tiposcama"); }
        }
    };
}

export function instalarHabitacion(base) {
    var habitacion = crearHabitacion({
        enviar: base.enviar,
        obtenerCatalogoHabitacion: base.catalogos.obtenerHabitacion,
        invalidarCatalogoHabitacion: base.catalogos.invalidarHabitacion,
        campo: base.campo,
        seccion: base.seccion,
        aOpcion: base.aOpcion,
        formulario: base.formulario,
        abrirPanel: base.abrirPanel,
        mostrarAviso: base.mostrarAviso,
        prepararCatalogo: base.prepararCatalogo,
        cargar: base.cargar,
        numero: numero,
        actualizarSelector: base.actualizarSelector,
        boton: base.boton
    });
    var movimientosHabitacion = crearMovimientosHabitacion({
        boton: base.boton,
        prepararCatalogo: base.prepararCatalogo,
        habitacion: habitacion,
        mostrarAviso: base.mostrarAviso
    });
    return {
        habitacion: habitacion,
        movimientosHabitacion: movimientosHabitacion,
        definiciones: definirPantallasHabitacion(habitacion)
    };
}