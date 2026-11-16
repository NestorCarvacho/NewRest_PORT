export function crearCargador(dependencias) {
    function cargar(id) {
        var nodo = dependencias.obtenerNodo(id);
        var modulo = dependencias.obtenerModulo(id);
        var filtro = nodo._filtro || {};
        dependencias.mostrarEstado(nodo, "Cargando datos...");
        var solicitud = modulo.modo === "censo" && filtro.censoUltimoMes
            ? dependencias.censo.buscarUltimoMes(filtro)
            : obtenerListado(modulo.modo, filtro);
        return solicitud.then(function (resultado) {
            if (!nodo.isConnected) return;
            dependencias.mostrarEstado(nodo, "");
            var filas = Array.isArray(resultado) ? resultado : resultado.filas || [];
            dependencias.pintar(nodo, filas, modulo.modo);
        }).catch(function (error) {
            if (!nodo.isConnected) return;
            dependencias.pintar(nodo, [], modulo.modo);
            dependencias.mostrarEstado(nodo, error.message);
        });
    }

    function obtenerListado(modo, filtro) {
        if (modo === "solicitud") return dependencias.solicitud.buscar(filtro);
        if (modo === "censo") return dependencias.censo.buscar(filtro);
        if (modo === "misCensos") return dependencias.censo.buscarMios(filtro);
        if (modo === "habitaciones" || modo === "banos") return dependencias.habitacion.buscar(filtro, modo);
        return dependencias.reserva.buscar(filtro);
    }

    return { cargar: cargar };
}
