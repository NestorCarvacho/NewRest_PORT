export function crearRenderizadorModulos(dependencias) {
    function fila(filaOrigen, modo) {
        if (modo === "solicitud") return dependencias.solicitud.fila(filaOrigen);
        if (modo === "censo") return dependencias.censo.filaCenso(filaOrigen);
        if (modo === "misCensos") return dependencias.censo.filaMisCensos(filaOrigen);
        return dependencias.reserva.fila(filaOrigen);
    }

    function columnas(modo) {
        if (modo === "solicitud") return dependencias.solicitud.columnas();
        if (modo === "censo" || modo === "misCensos") return dependencias.censo.columnas(modo);
        return dependencias.reserva.columnas(modo, dependencias.movimientosReserva.crearAcciones);
    }

    function pintar(nodo, filas, modo) {
        if (nodo._guardar) nodo._guardar.hidden = modo !== "censo" || filas.length === 0;
        dependencias.tablas.pintar(nodo, filas, function (filaOrigen) {
            return fila(filaOrigen, modo);
        });
    }

    return { pintar: pintar, columnas: columnas };
}
