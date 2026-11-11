export function crearApi() {
    function token() {
        var campo = document.querySelector("#antifalsificacion input[name='__RequestVerificationToken']");
        return campo ? campo.value : "";
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

    function pedir(url, opciones) {
        return fetch(url, opciones).then(leer);
    }

    function enviar(url, cuerpo) {
        return pedir(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token()
            },
            body: JSON.stringify(cuerpo)
        });
    }

    function enviarArchivo(url, archivo, datos) {
        var cuerpo = new FormData();
        cuerpo.append("archivo", archivo);
        if (datos) {
            Object.keys(datos).forEach(function (clave) { cuerpo.append(clave, datos[clave]); });
        }
        return pedir(url, {
            method: "POST",
            headers: { "RequestVerificationToken": token() },
            body: cuerpo
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
