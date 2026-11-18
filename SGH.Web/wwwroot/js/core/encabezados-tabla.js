import datos from "../../datos/encabezados-tabla-modulo.json" with { type: "json" };

export function encabezadosModulo(modo) {
    var lista = datos[modo];
    return Array.isArray(lista) ? lista.slice() : [];
}