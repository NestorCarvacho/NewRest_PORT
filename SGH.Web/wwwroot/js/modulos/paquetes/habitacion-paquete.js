import { crearHabitacion } from "../habitacion.js";

export function definirPantallasHabitacion() {
    return {
        habitaciones: {
            titulo: "Habitaciones",
            modo: "habitaciones",
            tipo: "habitacion",
            tabInicial: "habitaciones",
            columnas: [],
            filtros: [],
            filtroInicial: function () { return {}; }
        },
        banos: {
            titulo: "Baños",
            modo: "banos",
            tipo: "habitacion",
            tabInicial: "banos",
            columnas: [],
            filtros: [],
            filtroInicial: function () { return {}; }
        }
    };
}

export function instalarHabitacion() {
    var habitacion = crearHabitacion();
    return {
        habitacion: habitacion,
        definiciones: definirPantallasHabitacion()
    };
}