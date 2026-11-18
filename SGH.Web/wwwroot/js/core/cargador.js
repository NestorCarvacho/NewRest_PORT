export function crearCargador(dependencias) {
  function cargar(id) {
    var nodo = dependencias.obtenerNodo(id);
    var modulo = dependencias.obtenerModulo(id);
    if (!nodo || !modulo) return Promise.resolve();
    if (nodo._cargando) return nodo._cargaPendiente || Promise.resolve();
    var filtro = nodo._filtro || {};
    dependencias.mostrarEstado(nodo, "Cargando datos...");
    if (dependencias.bloquearPantalla) {
      dependencias.bloquearPantalla("Cargando datos, espere…");
    }
    nodo._cargando = true;
    var solicitud = obtenerListado(modulo.modo, filtro);
    nodo._cargaPendiente = solicitud;
    return solicitud.then(function (resultado) {
      if (!nodo.isConnected) return;
      dependencias.mostrarEstado(nodo, "");
      var filas = Array.isArray(resultado) ? resultado : resultado.filas || [];
      if (filas.length > 1000 && dependencias.mostrarAviso) {
        dependencias.mostrarAviso("Se muestran " + filas.length + " filas (máximo " + 5000 + ").", "info");
      }
      dependencias.pintar(nodo, filas, modulo.modo);
    }).catch(function (error) {
      if (!nodo.isConnected) return;
      dependencias.pintar(nodo, [], modulo.modo);
      dependencias.mostrarEstado(nodo, error.message);
    }).finally(function () {
      nodo._cargando = false;
      nodo._cargaPendiente = null;
      if (dependencias.desbloquearPantalla) dependencias.desbloquearPantalla();
    });
  }

    function obtenerListado(modo, filtro) {
        if (modo === "solicitud") return dependencias.solicitud.buscar(filtro);
        if (modo === "censo") return dependencias.censo.buscarRango(filtro);
        if (modo === "misCensos") return dependencias.censo.buscarMiosRango(filtro);
        if (modo === "habitaciones" || modo === "banos" || modo === "tiposcama") {
            return dependencias.habitacion.buscar(filtro, modo);
        }
        return dependencias.reserva.buscar(filtro);
    }

    return { cargar: cargar };
}
