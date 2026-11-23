import { encolarOperacion, sincronizarTodo, actualizarBannerOffline } from "../core/colaOffline.js";
import { capacidadEfectiva, validarOcupacionCamas } from "./censo-camas-logica.js";
import { montarSelectorCamasCenso } from "./censo-camas-selector.js";

export function crearCensoQr(dependencias) {
    function guardarPendiente(payload) {
        encolarOperacion({
            url: "/Censo/Mid/MidCenso.cs",
            cuerpo: Object.assign({ opcion: "guardarqr" }, payload)
        });
    }

    function sincronizarPendientes() {
        return sincronizarTodo(dependencias.enviar);
    }

    function detenerEscaner(envoltorio) {
        if (!envoltorio) return;
        var video = envoltorio.querySelector("video");
        if (video && video.srcObject) {
            video.srcObject.getTracks().forEach(function (track) { track.stop(); });
        }
        envoltorio.remove();
    }

    function iniciarEscaner(campoQr, formulario) {
        if (!("BarcodeDetector" in window) || !navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            dependencias.mostrarAviso("Este navegador no soporta lectura QR por cámara. Use un lector físico.", "error");
            return;
        }
        var previo = formulario.querySelector(".lector-qr-envoltorio");
        if (previo) detenerEscaner(previo);

        var envoltorio = document.createElement("div");
        envoltorio.className = "lector-qr-envoltorio";
        var video = document.createElement("video");
        video.autoplay = true;
        video.playsInline = true;
        video.muted = true;
        video.className = "lector-qr";
        video.setAttribute("aria-label", "Vista previa de cámara para leer código QR");
        envoltorio.append(video);
        var ancla = formulario.querySelector(".lector-qr-ancla");
        if (ancla) ancla.after(envoltorio);
        else formulario.insertBefore(envoltorio, formulario.firstChild);

        var esMovil = window.matchMedia("(max-width: 768px)").matches;
        var restricciones = {
            video: {
                facingMode: { ideal: "environment" },
                width: { ideal: esMovil ? 640 : 1280 },
                height: { ideal: esMovil ? 480 : 720 }
            }
        };

        navigator.mediaDevices.getUserMedia(restricciones).then(function (stream) {
            video.srcObject = stream;
            var detector = new BarcodeDetector({ formats: ["qr_code"] });
            var leer = function () {
                if (!envoltorio.isConnected) return;
                if (video.readyState < 2) return requestAnimationFrame(leer);
                detector.detect(video).then(function (codigos) {
                    if (codigos.length) {
                        campoQr.value = codigos[0].rawValue;
                        detenerEscaner(envoltorio);
                        return;
                    }
                    requestAnimationFrame(leer);
                }).catch(function () { requestAnimationFrame(leer); });
            };
            leer();
        }).catch(function () {
            detenerEscaner(envoltorio);
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
                dependencias.campo("cantidad", "Camas ocupadas", "hidden", [], { valor: "0" }),
                dependencias.campo("cantidadNoche", "Camas ocupadas noche", "hidden", [], { valor: "0" }),
                dependencias.campo("capacidadHabitacion", "Capacidad habitación", "hidden", [], { valor: "4" })
            ])
        ], "Guardar", true, function (datos) {
            var qr;
            try {
                qr = JSON.parse(datos.qr);
            } catch (_) {
                return Promise.reject(new Error("El contenido QR no es un JSON válido."));
            }
            if (!qr || !qr.h) return Promise.reject(new Error("El código QR no contiene una habitación."));
            var capacidad = capacidadEfectiva(
                datos.capacidadHabitacion != null ? datos.capacidadHabitacion : (qr.c != null ? qr.c : qr.nc)
            );
            var camasDia = datos.turno === "N" ? 0 : parseInt(datos.cantidad, 10) || 0;
            var camasNoche = datos.turno === "D" ? 0 : (datos.turno === "N"
                ? parseInt(datos.cantidad, 10) || 0
                : parseInt(datos.cantidadNoche, 10) || 0);
            var errorCamas = validarOcupacionCamas(capacidad, datos.turno, camasDia, camasNoche);
            if (errorCamas) return Promise.reject(new Error(errorCamas));
            var payload = {
                habitacionId: String(qr.h),
                fecha: datos.fecha,
                turno: datos.turno,
                cantidad: datos.cantidad,
                cantidadNoche: datos.cantidadNoche,
                capacidadHabitacion: String(capacidad)
            };
            return dependencias.enviar("/Censo/Mid/MidCenso.cs", Object.assign({ opcion: "guardarqr" }, payload)).then(function () {
                dependencias.mostrarAviso("Censo QR guardado correctamente.", "success");
            }).catch(function (error) {
                guardarPendiente(payload);
                actualizarBannerOffline();
                throw new Error(error.message + " El censo quedó pendiente para sincronizar.");
            });
        });
        dependencias.abrirPanel("Censo QR", contenidoQr);
        var campoQr = contenidoQr.querySelector("[name='qr']");
        var ancla = document.createElement("div");
        ancla.className = "lector-qr-ancla";
        contenidoQr.insertBefore(ancla, contenidoQr.firstChild);
        var botonEscanear = dependencias.boton("Usar cámara", "boton-secundario", function () {
            iniciarEscaner(campoQr, contenidoQr);
        });
        ancla.append(botonEscanear);

        var selectTurno = contenidoQr.querySelector("[name='turno']");
        var inputCantidad = contenidoQr.querySelector("[name='cantidad']");
        var inputNoche = contenidoQr.querySelector("[name='cantidadNoche']");
        var inputCapacidad = contenidoQr.querySelector("[name='capacidadHabitacion']");
        var anclaCamas = document.createElement("div");
        anclaCamas.className = "censo-camas-ancla";
        var grilla = contenidoQr.querySelector(".formulario-grilla");
        if (grilla) grilla.append(anclaCamas);

        var selectorCamas = null;
        function actualizarCapacidadDesdeQr() {
            var capacidad = 4;
            try {
                var qr = JSON.parse(campoQr.value || "{}");
                if (qr && (qr.c != null || qr.nc != null))
                    capacidad = capacidadEfectiva(qr.c != null ? qr.c : qr.nc);
            } catch (_) { /* texto aún no es JSON */ }
            if (inputCapacidad) inputCapacidad.value = String(capacidad);
            if (selectorCamas) selectorCamas.setCapacidad(capacidad);
        }
        if (selectTurno && inputCantidad && inputNoche && inputCapacidad) {
            selectorCamas = montarSelectorCamasCenso({
                capacidadTotal: parseInt(inputCapacidad.value, 10) || 4,
                contenedor: anclaCamas,
                selectTurno: selectTurno,
                inputCantidad: inputCantidad,
                inputCantidadNoche: inputNoche
            });
            campoQr.addEventListener("input", actualizarCapacidadDesdeQr);
            actualizarCapacidadDesdeQr();
        }
    }

    return { abrir: abrir, sincronizarPendientes: sincronizarPendientes };
}
