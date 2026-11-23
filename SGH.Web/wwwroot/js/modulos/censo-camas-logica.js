/** @typedef {"D" | "N" | "A"} TurnoCenso */

export var MAX_CAMAS_ABSOLUTO = 4;

export function capacidadEfectiva(capacidadTotal) {
    var n = parseInt(String(capacidadTotal), 10);
    if (!Number.isFinite(n) || n < 0) return 0;
    return Math.min(MAX_CAMAS_ABSOLUTO, n);
}

/** Legacy «Ambos»: día deshabilitado si i > capacidadHabitacion */
export function radioDiaDeshabilitado(valor, capacidadTotal) {
    return valor > capacidadEfectiva(capacidadTotal);
}

/** Legacy «Ambos»: noche deshabilitado si i > (capacidadHabitacion - camasDia) */
export function radioNocheDeshabilitado(valor, capacidadTotal, camasDia) {
    var dia = parseInt(String(camasDia), 10) || 0;
    return valor > Math.max(0, capacidadEfectiva(capacidadTotal) - dia);
}

export function ajustarCamasNoche(camasNoche, capacidadTotal, camasDia) {
    var noche = parseInt(String(camasNoche), 10) || 0;
    var dia = parseInt(String(camasDia), 10) || 0;
    var maxNoche = Math.max(0, capacidadEfectiva(capacidadTotal) - dia);
    return Math.min(noche, maxNoche);
}

/**
 * @param {TurnoCenso | string} turno
 */
export function validarOcupacionCamas(capacidadTotal, turno, camasDia, camasNoche) {
    var cap = capacidadEfectiva(capacidadTotal);
    var dia = parseInt(String(camasDia), 10) || 0;
    var noche = parseInt(String(camasNoche), 10) || 0;

    if (turno === "D") {
        if (dia < 0 || dia > cap) return "Las camas ocupadas de día deben estar entre 0 y " + cap + ".";
        return null;
    }
    if (turno === "N") {
        if (noche < 0 || noche > cap) return "Las camas ocupadas de noche deben estar entre 0 y " + cap + ".";
        return null;
    }
    if (turno === "A") {
        if (radioDiaDeshabilitado(dia, capacidadTotal))
            return "Las camas ocupadas de día deben estar entre 0 y " + cap + ".";
        if (radioNocheDeshabilitado(noche, capacidadTotal, dia))
            return "La suma de camas día y noche no puede superar la capacidad de la habitación.";
        if (dia + noche > cap)
            return "La suma de camas día y noche no puede superar la capacidad de la habitación.";
        return null;
    }
    return "Seleccione un turno válido.";
}