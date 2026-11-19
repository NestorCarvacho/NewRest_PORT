import datos from "../../datos/encabezados-tabla-modulo.json" with { type: "json" };

var titulosLargos = {
    censo: {
        "Est. censo": "Estado censo",
        "Est. hab.": "Estado habitación",
        "Camas": "N° camas",
        "Ocupadas": "Camas ocupadas",
        "Ocup. check-in": "Camas ocupadas (check-in)"
    },
    misCensos: {
        "Camas": "N° camas",
        "Ocupadas": "Camas ocupadas"
    }
};

export function encabezadosModulo(modo) {
    var lista = datos[modo];
    return Array.isArray(lista) ? lista.slice() : [];
}

export function tituloLargoEncabezado(modo, corto) {
    var mapa = titulosLargos[modo];
    if (mapa && mapa[corto]) return mapa[corto];
    return corto;
}

export function pintarEncabezadosTabla(tabla, modo) {
    var titulos = encabezadosModulo(modo);
    if (!tabla || !titulos.length) return;
    var celdas = tabla.querySelectorAll("thead th");
    titulos.forEach(function (texto, indice) {
        var th = celdas[indice];
        if (!th) return;
        th.textContent = texto;
        th.title = tituloLargoEncabezado(modo, texto);
    });
}