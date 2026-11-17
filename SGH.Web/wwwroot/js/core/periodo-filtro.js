/**
 * Período predefinido: fechas solo lectura con rango calculado.
 * Personalizado: inputs type=month editables.
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
            periodo: form.querySelector("[name='periodo']"),
            desde: form.querySelector("[name='" + nombres.desde + "']"),
            hasta: form.querySelector("[name='" + nombres.hasta + "']")
        };
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

    function aplicarModo(controles) {
        var periodo = controles.periodo ? controles.periodo.value : "30";
        var personalizado = periodo === "custom";

        if (controles.desde) controles.desde.type = personalizado ? "month" : "date";
        if (controles.hasta) controles.hasta.type = personalizado ? "month" : "date";

        if (personalizado) {
            [controles.desde, controles.hasta].forEach(function (input) {
                if (!input) return;
                if (input.value && input.value.length >= 7) input.value = input.value.slice(0, 7);
            });
            marcarCampoSoloLectura(controles.desde, false);
            marcarCampoSoloLectura(controles.hasta, false);
            return;
        }

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
        if (controles.periodo) {
            controles.periodo.addEventListener("change", actualizar);
        }
        actualizar();
    }

    return { enlazarFormulario: enlazarFormulario };
}