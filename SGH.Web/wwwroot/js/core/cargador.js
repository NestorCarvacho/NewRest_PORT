var MAX_FILAS_UI = 5000;

export function crearCargador(dependencias) {
  function cancelarCargaNodo(nodo) {
    if (!nodo) return;
    if (nodo._abortCarga) {
      nodo._abortCarga.abort();
      nodo._abortCarga = null;
    }
    nodo._cargaGeneracion = (nodo._cargaGeneracion || 0) + 1;
    nodo._cargando = false;
    nodo._cargaPendiente = null;
    dependencias.mostrarEstado(nodo, "");
  }

  function avisarTruncado(nodo, filas, resultado) {
    if (!dependencias.mostrarAviso) return;
    if (resultado && resultado.truncado) {
      var tope = resultado.maxFilas || MAX_FILAS_UI;
      dependencias.mostrarAviso("Se muestran " + tope + " filas como máximo. Acote el período o los filtros.", "info");
      return;
    }
    if (filas.length >= MAX_FILAS_UI) {
      dependencias.mostrarAviso("El listado alcanzó el tope de " + MAX_FILAS_UI + " filas. Acote los criterios.", "info");
    } else if (filas.length > 1000) {
      dependencias.mostrarAviso("Se muestran " + filas.length + " filas.", "info");
    }
  }

  function cargar(id, opciones) {
    opciones = opciones || {};
    var nodo = dependencias.obtenerNodo(id);
    var modulo = dependencias.obtenerModulo(id);
    if (!nodo || !modulo) return Promise.resolve();

    if (nodo._cargando) {
      cancelarCargaNodo(nodo);
    }

    var filtro = nodo._filtro || {};
    var bloquearPantalla = opciones.bloquearPantalla !== false && !!dependencias.bloquearPantalla;
    dependencias.mostrarEstado(nodo, bloquearPantalla ? "Cargando datos..." : "");
    if (bloquearPantalla) {
      dependencias.bloquearPantalla("Cargando datos, espere…");
    }

    var controlador = new AbortController();
    nodo._abortCarga = controlador;
    nodo._cargando = true;
    nodo._cargaGeneracion = (nodo._cargaGeneracion || 0) + 1;
    var generacion = nodo._cargaGeneracion;

  function enviarConSenal(url, cuerpo) {
      return dependencias.enviar(url, cuerpo, { signal: controlador.signal });
    }

    var solicitud = obtenerListado(modulo.modo, filtro, enviarConSenal);
    nodo._cargaPendiente = solicitud;

    return solicitud.then(function (resultado) {
      if (generacion !== nodo._cargaGeneracion || !nodo.isConnected) return;
      dependencias.mostrarEstado(nodo, "");
      var filas = Array.isArray(resultado) ? resultado : resultado.filas || [];
      var avisarConteo = opciones.avisarConteoFilas;
      if (avisarConteo === undefined) avisarConteo = bloquearPantalla;
      if (avisarConteo) avisarTruncado(nodo, filas, resultado);
      dependencias.pintar(nodo, filas, modulo.modo);
    }).catch(function (error) {
      if (generacion !== nodo._cargaGeneracion) return;
      if (error && error.name === "AbortError") return;
      if (!nodo.isConnected) return;
      dependencias.pintar(nodo, [], modulo.modo);
      dependencias.mostrarEstado(nodo, error.message);
    }).finally(function () {
      if (bloquearPantalla && dependencias.desbloquearPantalla) {
        dependencias.desbloquearPantalla();
      }
      if (generacion === nodo._cargaGeneracion) {
        nodo._cargando = false;
        nodo._cargaPendiente = null;
        if (nodo._abortCarga === controlador) nodo._abortCarga = null;
      }
    });
  }

    function obtenerListado(modo, filtro, enviarLista) {
        var enviarMid = enviarLista || dependencias.enviar;
        if (modo === "solicitud") return dependencias.solicitud.buscar(filtro, enviarMid);
        if (modo === "censo") return dependencias.censo.buscarRango(filtro, enviarMid);
        if (modo === "misCensos") return dependencias.censo.buscarMiosRango(filtro, enviarMid);
        if (modo === "habitaciones" || modo === "banos" || modo === "tiposcama") {
            return dependencias.habitacion.buscar(filtro, modo, enviarMid);
        }
        return dependencias.reserva.buscar(filtro, enviarMid);
    }

    return { cargar: cargar, cancelarCargaNodo: cancelarCargaNodo };
}