export function crearCargarDeferido() {
    var ejecutar = function () {
        return Promise.reject(new Error("El cargador aún no está enlazado."));
    };
    return {
        cargar: function (id) { return ejecutar(id); },
        enlazar: function (fn) { ejecutar = fn; }
    };
}
