/**
 * Switch «Por rango»: ON → presets de período y fechas bloqueadas.
 * OFF → calendario día a día (desde / hasta) editables.
 */
export function crearPeriodoFiltro(fechas) {
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
        var periodo = controles.periodo ? controles.periodo.value : "30";
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

    return { enlazarFormulario: enlazarFormulario };
}