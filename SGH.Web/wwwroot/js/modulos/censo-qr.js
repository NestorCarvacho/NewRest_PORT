import { sincronizarTodo, actualizarBannerOffline } from "../core/colaOffline.js";
import { encolarCensoQr } from "../core/colaCensoQr.js";
import { iniciarEscaner, detenerEscaner } from "./censo-qr-escaner.js";
import { capacidadEfectiva, validarOcupacionCamas } from "./censo-camas-logica.js";
import { montarSelectorCamasCenso } from "./censo-camas-selector.js";

export function crearCensoQr(dependencias) {
    function guardarPendiente(payload, meta) {
        encolarCensoQr(Object.assign({ opcion: "guardarqr" }, payload), meta);
    }

    function sincronizarPendientes() {
        return sincronizarTodo(dependencias.enviar);
    }

    function usarEscanerEnFormulario(campoQr, formulario) {
        var ancla = formulario.querySelector(".lector-qr-ancla");
        if (!ancla) return;
        iniciarEscaner(ancla, function (texto) {
            campoQr.value = texto;
            campoQr.dispatchEvent(new Event("input", { bubbles: true }));
        }, dependencias.mostrarAviso);
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
                guardarPendiente(payload, {
                    habitacion: qr.n || qr.habitacion || ("Habitación " + qr.h),
                    camasOcupadas: camasDia || camasNoche,
                    turnoNombre: datos.turno
                });
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
            usarEscanerEnFormulario(campoQr, contenidoQr);
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
