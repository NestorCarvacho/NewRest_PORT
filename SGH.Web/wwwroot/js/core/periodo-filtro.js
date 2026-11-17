/**
 * Switch «Por rango»: ON → presets de período y fechas bloqueadas.
 * OFF → calendario día a día (desde / hasta) editables.
 */
export function crearPeriodoFiltro(fechas) {
    function camposPeriodo(campo, opciones) {
        opciones = opciones || {};
        var presets = (fechas.presetsPeriodo || []).map(function (p) { return { value: p.value, text: p.text }; });
        var reserva = !!opciones.reservaNombres;
        var nombreDesde = reserva ? "reservaDesde" : "desde";
        var nombreHasta = reserva ? "reservaHasta" : "hasta";
        var defecto = fechas.periodoDefault || "30";
        return [
            campo("periodoPorRango", "Filtrar por rango", "checkbox", [], { interruptor: true, valor: true }),
            campo("periodo", opciones.etiquetaPeriodo || "Período", "select", presets, { sinVacio: true, valor: defecto }),
            campo(nombreDesde, "Desde", "date", [], { requerido: false }),
            campo(nombreHasta, "Hasta", "date", [], { requerido: false })
        ];
    }

    function filtroInicialPeriodo(opciones) {
        opciones = opciones || {};
        var defecto = fechas.periodoDefault || "30";
        var rango = fechas.rangoDesdeDias(parseInt(defecto, 10));
        var base = { periodoPorRango: true, periodo: defecto };
        if (opciones.reservaNombres) {
            return Object.assign(base, { reservaDesde: rango.reservaDesde, reservaHasta: rango.reservaHasta });
        }
        return Object.assign(base, { desde: rango.desde, hasta: rango.hasta });
    }
    function nombresDesdeHasta(form) {
        if (form.querySelector("[name='reservaDesde']")) {
            return { desde: "reservaDesde", hasta: "reservaHasta" };
        }
        return { desde: "desde", hasta: "hasta" };
    }

    function controlesFecha(form, nombres) {
        return {
            porRango: form.querySelector("[name='periodoPorRango']"),
            periodo: form.querySelector("[name='periodo']"),
            desde: form.querySelector("[name='" + nombres.desde + "']"),
            hasta: form.querySelector("[name='" + nombres.hasta + "']")
        };
    }

    function campoDe(input) {
        return input ? input.closest(".campo") : null;
    }

    function marcarCampoSoloLectura(input, soloLectura) {
        if (!input) return;
        input.readOnly = soloLectura;
        var campo = input.closest(".campo");
        if (!campo) return;
        var estado = campo.querySelector(".campo-estado");
        if (soloLectura) {
            if (!estado) {
                estado = document.createElement("span");
                estado.className = "campo-estado";
                var etiqueta = campo.querySelector("span");
                if (etiqueta) etiqueta.append(" ", estado);
            }
            estado.textContent = "Bloqueado";
        } else if (estado) {
            estado.remove();
        }
    }

    function modoPorRango(controles) {
        if (!controles.porRango) return true;
        return controles.porRango.checked;
    }

    function aplicarModo(controles) {
        var porRango = modoPorRango(controles);
        var campoPeriodo = campoDe(controles.periodo);
        if (campoPeriodo) campoPeriodo.hidden = !porRango;

        if (controles.desde) controles.desde.type = "date";
        if (controles.hasta) controles.hasta.type = "date";

        if (!porRango) {
            if (controles.periodo) controles.periodo.disabled = true;
            marcarCampoSoloLectura(controles.desde, false);
            marcarCampoSoloLectura(controles.hasta, false);
            return;
        }

        if (controles.periodo) controles.periodo.disabled = false;
        var periodo = controles.periodo ? controles.periodo.value : (fechas.periodoDefault || "30");
        var dias = parseInt(periodo, 10);
        if (dias > 0) {
            var rango = fechas.rangoDesdeDias(dias);
            if (controles.desde) controles.desde.value = rango.desde;
            if (controles.hasta) controles.hasta.value = rango.hasta;
        }
        marcarCampoSoloLectura(controles.desde, true);
        marcarCampoSoloLectura(controles.hasta, true);
    }

    function enlazarFormulario(form) {
        if (!form || !form.querySelector("[name='periodo']")) return;
        var nombres = nombresDesdeHasta(form);
        var controles = controlesFecha(form, nombres);
        function actualizar() {
            aplicarModo(controles);
        }
        if (controles.porRango) {
            controles.porRango.addEventListener("change", actualizar);
        }
        if (controles.periodo) {
            controles.periodo.addEventListener("change", actualizar);
        }
        actualizar();
    }

    return {
        enlazarFormulario: enlazarFormulario,
        camposPeriodo: camposPeriodo,
        filtroInicialPeriodo: filtroInicialPeriodo
    };
}