import { numero } from "../../core/numeros.js";
import { crearHabitacion } from "../habitacion.js";
import { crearMovimientosHabitacion } from "../movimientos-habitacion.js";

export function definirPantallasHabitacion(habitacion) {
    return {
        habitaciones: {
            titulo: "Habitaciones",
            modo: "habitaciones",
            columnas: [
                "Acciones", "Código", "Sexo", "Baño", "Pabellón", "Módulo",
                "Camas", "Hab./total", "Por turnos", "Exclusivo", "Estado"
            ],
            filtros: function () { return habitacion.filtros("habitaciones"); },
            filtroInicial: function () { return habitacion.filtroInicial("habitaciones"); }
        },
        banos: {
            titulo: "Baños",
            modo: "banos",
            columnas: ["Acciones", "Código", "Ubicación", "Sexo", "Estado", "Habitaciones"],
            filtros: function () { return habitacion.filtros("banos"); },
            filtroInicial: function () { return habitacion.filtroInicial("banos"); }
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
        actualizarSelector: base.actualizarSelector
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