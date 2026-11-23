export function crearRenderizadorModulos(dependencias) {
    function fila(filaOrigen, modo) {
        if (modo === "solicitud") return dependencias.solicitud.fila(filaOrigen);
        if (modo === "censo") return dependencias.censo.filaCenso(filaOrigen);
        if (modo === "misCensos") return dependencias.censo.filaMisCensos(filaOrigen);
        if (modo === "habitaciones" || modo === "banos" || modo === "tiposcama") {
            return dependencias.habitacion.fila(filaOrigen, modo);
        }
        return dependencias.reserva.fila(filaOrigen);
    }

    function columnas(modo) {
        if (modo === "solicitud") return dependencias.solicitud.columnas();
        if (modo === "censo" || modo === "misCensos") return dependencias.censo.columnas(modo);
        if (modo === "habitaciones" || modo === "banos" || modo === "tiposcama") {
            return dependencias.habitacion.columnas(modo, dependencias.movimientosHabitacion.crearAcciones);
        }
        return dependencias.reserva.columnas(modo, dependencias.movimientosReserva.crearAcciones);
    }

    function pintar(nodo, filas, modo) {
        if (modo === "reserva") nodo._seleccionReserva = new Set();
        dependencias.tablas.pintar(nodo, filas, function (filaOrigen) {
            return fila(filaOrigen, modo);
        });
        if (modo === "censo" && dependencias.censo && dependencias.censo.actualizarBotonGuardar) {
            dependencias.censo.actualizarBotonGuardar(nodo);
        } else if (nodo._guardar) {
            nodo._guardar.hidden = modo !== "censo" || filas.length === 0;
        }
    }

    function widgetAcciones(mapeada, modo) {
        if (modo === "habitaciones" || modo === "banos" || modo === "tiposcama") {
            return dependencias.movimientosHabitacion.crearAcciones(mapeada, modo);
        }
        if (modo === "reserva") {
            return dependencias.movimientosReserva.crearAcciones(mapeada._fila || mapeada, modo);
        }
        return document.createTextNode("");
    }

    function sincronizarFila(nodo, id, filaOrigen, modo) {
        dependencias.tablas.sincronizarFila(nodo, id, filaOrigen, function (origen) {
            return fila(origen, modo);
        }, function (api, indice, mapeada) {
            var celdaAcciones = dependencias.tablas.nodoCeldaDatos(api, indice, "acciones");
            if (celdaAcciones) celdaAcciones.replaceChildren(widgetAcciones(mapeada, modo));
            if (modo === "reserva" && dependencias.reserva.pintarSeleccionMasiva) {
                var celdaSel = dependencias.tablas.nodoCeldaDatos(api, indice, "seleccionMasiva");
                if (celdaSel) dependencias.reserva.pintarSeleccionMasiva(celdaSel, mapeada);
            }
        });
    }

    function quitarFila(nodo, id) {
        dependencias.tablas.quitarFila(nodo, id);
    }

    return { pintar: pintar, sincronizarFila: sincronizarFila, quitarFila: quitarFila, columnas: columnas };
}
