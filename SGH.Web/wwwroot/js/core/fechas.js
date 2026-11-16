export function crearFechas() {
    function fechaActual() {
        var hoy = new Date();
        return hoy.getFullYear() + "-" + String(hoy.getMonth() + 1).padStart(2, "0") + "-" + String(hoy.getDate()).padStart(2, "0");
    }

    function dosDigitos(numero) {
        return String(numero).padStart(2, "0");
    }

    function momentoLocal(conHora) {
        var ahora = new Date();
        var fecha = ahora.getFullYear() + "-" + dosDigitos(ahora.getMonth() + 1) + "-" + dosDigitos(ahora.getDate());
        if (!conHora) return fecha;
        return fecha + "T" + dosDigitos(ahora.getHours()) + ":" + dosDigitos(ahora.getMinutes());
    }

    /** Convierte dd/MM/yyyy o dd/MM/yyyy HH:mm (p. ej. listado) a yyyy-MM-dd para input[type=date]. */
    function fechaListadoAInput(valor) {
        if (!valor || typeof valor !== "string") return "";
        var texto = valor.trim();
        if (texto.indexOf("<") >= 0) return "";
        var partes = /^(\d{2})\/(\d{2})\/(\d{4})/.exec(texto);
        if (!partes) return "";
        return partes[3] + "-" + partes[2] + "-" + partes[1];
    }

    function rangoReservaInicial() {
        var hoy = new Date();
        var fecha = fechaActual();
        var inicio = new Date(hoy);
        inicio.setDate(inicio.getDate() - 30);
        var desde = inicio.getFullYear() + "-" + String(inicio.getMonth() + 1).padStart(2, "0") + "-" + String(inicio.getDate()).padStart(2, "0");
        return { reservaDesde: desde, reservaHasta: fecha };
    }

    return {
        fechaActual: fechaActual,
        momentoLocal: momentoLocal,
        rangoReservaInicial: rangoReservaInicial,
        fechaListadoAInput: fechaListadoAInput
    };
}
