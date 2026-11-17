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

    function rangoDesdeDias(dias) {
        var hoy = new Date();
        var hasta = fechaActual();
        var inicio = new Date(hoy);
        inicio.setDate(inicio.getDate() - (dias || 30));
        var desde = inicio.getFullYear() + "-" + String(inicio.getMonth() + 1).padStart(2, "0") + "-" + String(inicio.getDate()).padStart(2, "0");
        return { desde: desde, hasta: hasta, reservaDesde: desde, reservaHasta: hasta };
    }

    function rangoReservaInicial() {
        return rangoDesdeDias(30);
    }

    var presetsPeriodo = [
        { value: "30", text: "Último mes" },
        { value: "90", text: "Últimos 3 meses" },
        { value: "180", text: "Últimos 6 meses" },
        { value: "365", text: "Último año" },
        { value: "custom", text: "Personalizado (por mes)" }
    ];

    function ultimoDiaDelMes(anioMes) {
        if (!anioMes || anioMes.length < 7) return anioMes;
        var partes = anioMes.split("-");
        var anio = parseInt(partes[0], 10);
        var mes = parseInt(partes[1], 10);
        if (!anio || !mes) return anioMes;
        var ultimo = new Date(anio, mes, 0);
        return anio + "-" + dosDigitos(mes) + "-" + dosDigitos(ultimo.getDate());
    }

    function primerDiaDelMes(anioMes) {
        if (!anioMes || anioMes.length < 7) return anioMes;
        var partes = anioMes.split("-");
        return partes[0] + "-" + partes[1] + "-01";
    }

    /** Acepta yyyy-MM-dd o yyyy-MM (personalizado por mes). */
    function normalizarFiltroPeriodo(datos) {
        if (!datos) return datos;
        var periodo = datos.periodo || "30";
        if (periodo === "custom") {
            var desdeMes = datos.desde || datos.reservaDesde || "";
            var hastaMes = datos.hasta || datos.reservaHasta || "";
            var desde = desdeMes.length === 7 ? primerDiaDelMes(desdeMes) : desdeMes;
            var hasta = hastaMes.length === 7 ? ultimoDiaDelMes(hastaMes) : hastaMes;
            return Object.assign({}, datos, {
                desde: desde,
                hasta: hasta,
                reservaDesde: desde,
                reservaHasta: hasta
            });
        }
        return aplicarPresetPeriodo(datos, periodo);
    }

    function aplicarPresetPeriodo(datos, dias) {
        if (!datos || dias === "custom") return datos;
        var n = parseInt(dias, 10);
        if (!n) return datos;
        var rango = rangoDesdeDias(n);
        return Object.assign({}, datos, {
            periodo: String(dias),
            desde: rango.desde,
            hasta: rango.hasta,
            reservaDesde: rango.reservaDesde,
            reservaHasta: rango.reservaHasta
        });
    }

    return {
        fechaActual: fechaActual,
        momentoLocal: momentoLocal,
        rangoReservaInicial: rangoReservaInicial,
        rangoDesdeDias: rangoDesdeDias,
        presetsPeriodo: presetsPeriodo,
        aplicarPresetPeriodo: aplicarPresetPeriodo,
        normalizarFiltroPeriodo: normalizarFiltroPeriodo,
        fechaListadoAInput: fechaListadoAInput
    };
}
