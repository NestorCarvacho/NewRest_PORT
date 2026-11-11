export function crearCensoQr(dependencias) {
    var clavePendientes = "sgh.censos.pendientes";

    function leerPendientes() {
        try {
            return JSON.parse(localStorage.getItem(clavePendientes) || "[]");
        } catch (_) {
            return [];
        }
    }

    function guardarPendiente(payload) {
        var pendientes = leerPendientes();
        pendientes.push({ id: Date.now() + "-" + Math.random().toString(16).slice(2), payload: payload });
        localStorage.setItem(clavePendientes, JSON.stringify(pendientes));
    }

    function sincronizarPendientes() {
        var pendientes = leerPendientes();
        if (!pendientes.length) return Promise.resolve();
        return pendientes.reduce(function (promesa, pendiente) {
            return promesa.then(function () {
                return dependencias.enviar("/Censo/Mid/MidCenso.cs", Object.assign({ opcion: "guardarqr" }, pendiente.payload)).then(function () {
                    var actuales = leerPendientes().filter(function (item) { return item.id !== pendiente.id; });
                    localStorage.setItem(clavePendientes, JSON.stringify(actuales));
                });
            });
        }, Promise.resolve()).catch(function () { });
    }

    function iniciarEscaner(campoQr) {
        if (!("BarcodeDetector" in window) || !navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            dependencias.mostrarAviso("Este navegador no soporta lectura QR por cámara. Use un lector físico.", "error");
            return;
        }
        var video = document.createElement("video");
        video.autoplay = true;
        video.playsInline = true;
        video.className = "lector-qr";
        document.getElementById("panel-cuerpo").append(video);
        navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: "environment" } } }).then(function (stream) {
            video.srcObject = stream;
            var detector = new BarcodeDetector({ formats: ["qr_code"] });
            var leer = function () {
                if (video.readyState < 2) return requestAnimationFrame(leer);
                detector.detect(video).then(function (codigos) {
                    if (codigos.length) {
                        campoQr.value = codigos[0].rawValue;
                        stream.getTracks().forEach(function (track) { track.stop(); });
                        video.remove();
                        return;
                    }
                    requestAnimationFrame(leer);
                }).catch(function () { requestAnimationFrame(leer); });
            };
            leer();
        }).catch(function () {
            video.remove();
            dependencias.mostrarAviso("No fue posible acceder a la cámara.", "error");
        });
    }

    function abrir() {
        var contenidoQr = dependencias.formulario([
            dependencias.seccion("Lectura", [
                dependencias.campo("qr", "Contenido QR", "textarea", [], { requerido: true }),
                dependencias.campo("fecha", "Fecha de censo", "date", [], { requerido: true, valor: dependencias.fechaActual() }),
                dependencias.campo("turno", "Turno", "select", [
                    { value: "D", text: "Día" },
                    { value: "N", text: "Noche" },
                    { value: "A", text: "Ambos" }
                ], { valor: "D", sinVacio: true }),
                dependencias.campo("cantidad", "Camas ocupadas", "number", [], { requerido: true, valor: "0", controlesCantidad: true }),
                dependencias.campo("cantidadNoche", "Camas ocupadas noche", "number", [], { valor: "0", controlesCantidad: true })
            ])
        ], "Guardar", true, function (datos) {
            var qr;
            try {
                qr = JSON.parse(datos.qr);
            } catch (_) {
                return Promise.reject(new Error("El contenido QR no es un JSON válido."));
            }
            if (!qr || !qr.h) return Promise.reject(new Error("El código QR no contiene una habitación."));
            var payload = {
                habitacionId: String(qr.h),
                fecha: datos.fecha,
                turno: datos.turno,
                cantidad: datos.cantidad,
                cantidadNoche: datos.cantidadNoche
            };
            return dependencias.enviar("/Censo/Mid/MidCenso.cs", Object.assign({ opcion: "guardarqr" }, payload)).then(function () {
                dependencias.mostrarAviso("Censo QR guardado correctamente.", "success");
            }).catch(function (error) {
                guardarPendiente(payload);
                throw new Error(error.message + " El censo quedó pendiente para sincronizar.");
            });
        });
        dependencias.abrirPanel("Censo QR", contenidoQr);
        var campoQr = contenidoQr.querySelector("[name='qr']");
        var botonEscanear = dependencias.boton("Usar cámara", "boton-secundario", function () {
            iniciarEscaner(campoQr);
        });
        contenidoQr.insertBefore(botonEscanear, contenidoQr.firstChild);
    }

    return { abrir: abrir, sincronizarPendientes: sincronizarPendientes };
}
