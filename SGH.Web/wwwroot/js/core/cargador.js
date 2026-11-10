export function crearCargador(dependencias) {
    function cargar(id) {
        var nodo = dependencias.obtenerNodo(id);
        var modulo = dependencias.obtenerModulo(id);
        var filtro = nodo._filtro || {};
        dependencias.mostrarEstado(nodo, "Cargando datos...");
        var solicitud = modulo.modo === "censo" && filtro.censoUltimoMes
            ? dependencias.censo.buscarUltimoMes(filtro)
            : dependencias.pedir(obtenerUrl(modulo.modo, filtro));
        return solicitud.then(function (resultado) {
            dependencias.mostrarEstado(nodo, "");
            var filas = Array.isArray(resultado) ? resultado : resultado.filas || [];
            dependencias.pintar(nodo, filas, modulo.modo);
        }).catch(function (error) {
            dependencias.pintar(nodo, [], modulo.modo);
            dependencias.mostrarEstado(nodo, error.message);
        });
    }

    function obtenerUrl(modo, filtro) {
        if (modo === "solicitud") return dependencias.solicitud.urlBuscar(filtro);
        if (modo === "censo") return dependencias.censo.urlBuscar(filtro);
        if (modo === "misCensos") return dependencias.censo.urlMisCensos(filtro);
        return dependencias.reserva.urlBuscar(filtro);
    }

    return { cargar: cargar };
}
