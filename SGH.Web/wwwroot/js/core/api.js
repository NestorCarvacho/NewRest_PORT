export function crearApi() {
    var reintentosRed = 2;
    var esperaBaseMs = 400;

    function token() {
        var campo = document.querySelector("#antifalsificacion input[name='__RequestVerificationToken']");
        return campo ? campo.value : "";
    }

    function esFalloRed(error) {
        if (!error) return false;
        if (error.name === "AbortError") return false;
        return error instanceof TypeError;
    }

    function leer(respuesta) {
        return respuesta.text().then(function (texto) {
            var cuerpo = {};
            if (texto) {
                try { cuerpo = JSON.parse(texto); } catch (error) { cuerpo = {}; }
            }
            if (cuerpo && cuerpo.registros && cuerpo.registros.resultado === false) {
                throw new Error(cuerpo.registros.mensaje || "No se pudo completar la operación.");
            }
            if (cuerpo && Object.prototype.hasOwnProperty.call(cuerpo, "registros")) cuerpo = cuerpo.registros;
            if (!respuesta.ok) {
                var mensajeHttp = cuerpo && cuerpo.mensaje;
                if (!mensajeHttp && respuesta.status === 400 && !texto) {
                    mensajeHttp = "La solicitud fue rechazada. Actualice la página e intente nuevamente.";
                }
                throw new Error(mensajeHttp || texto || "No se pudo completar la operación.");
            }
            return cuerpo;
        });
    }

    function pedir(url, opciones, intento) {
        intento = intento || 0;
        return fetch(url, opciones).then(function (respuesta) {
            return leer(respuesta);
        }).catch(function (error) {
            if (error && error.name === "AbortError") throw error;
            if (intento < reintentosRed && esFalloRed(error) && (!opciones.signal || !opciones.signal.aborted)) {
                var espera = esperaBaseMs * (intento + 1);
                return new Promise(function (resolver) {
                    window.setTimeout(resolver, espera);
                }).then(function () {
                    return pedir(url, opciones, intento + 1);
                });
            }
            throw error;
        });
    }

    function enviar(url, cuerpo, extra) {
        extra = extra || {};
        return pedir(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token()
            },
            body: JSON.stringify(cuerpo),
            signal: extra.signal
        });
    }

    function enviarArchivo(url, archivo, datos, extra) {
        extra = extra || {};
        var cuerpo = new FormData();
        cuerpo.append("archivo", archivo);
        if (datos) {
            Object.keys(datos).forEach(function (clave) { cuerpo.append(clave, datos[clave]); });
        }
        return pedir(url, {
            method: "POST",
            headers: { "RequestVerificationToken": token() },
            body: cuerpo,
            signal: extra.signal
        });
    }

    function consulta(parametros) {
        var partes = [];
        Object.keys(parametros).forEach(function (clave) {
            if (parametros[clave] === undefined || parametros[clave] === null || parametros[clave] === "") return;
            partes.push(encodeURIComponent(clave) + "=" + encodeURIComponent(parametros[clave]));
        });
        return partes.length ? "&" + partes.join("&") : "";
    }

    return { pedir: pedir, enviar: enviar, enviarArchivo: enviarArchivo, consulta: consulta };
}