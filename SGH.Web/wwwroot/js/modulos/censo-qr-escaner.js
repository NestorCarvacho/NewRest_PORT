var jsQrCarga = null;

function cargarJsQr() {
    if (typeof window.jsQR === "function") return Promise.resolve(window.jsQR);
    if (jsQrCarga) return jsQrCarga;
    jsQrCarga = new Promise(function (resolver, rechazar) {
        var existente = document.querySelector("script[data-jsqr]");
        if (existente) {
            existente.addEventListener("load", function () { resolver(window.jsQR); });
            existente.addEventListener("error", rechazar);
            return;
        }
        var script = document.createElement("script");
        script.src = "/lib/jsqr/jsQR.min.js";
        script.async = true;
        script.dataset.jsqr = "1";
        script.onload = function () {
            if (typeof window.jsQR === "function") resolver(window.jsQR);
            else rechazar(new Error("jsQR no disponible"));
        };
        script.onerror = function () { rechazar(new Error("No se pudo cargar el lector QR.")); };
        document.head.append(script);
    });
    return jsQrCarga;
}

export function detenerEscaner(envoltorio) {
    if (!envoltorio) return;
    if (envoltorio._detenerLectura) {
        envoltorio._detenerLectura();
        envoltorio._detenerLectura = null;
    }
    var video = envoltorio.querySelector("video");
    if (video && video.srcObject) {
        video.srcObject.getTracks().forEach(function (track) { track.stop(); });
    }
    envoltorio.remove();
}

function construirVisor(contenedor) {
    var previo = contenedor.querySelector(".lector-qr-envoltorio");
    if (previo) detenerEscaner(previo);

    var envoltorio = document.createElement("div");
    envoltorio.className = "lector-qr-envoltorio censo-qr-wizard-visor";
    var video = document.createElement("video");
    video.autoplay = true;
    video.playsInline = true;
    video.muted = true;
    video.className = "lector-qr";
    video.setAttribute("aria-label", "Vista previa de cámara para leer código QR");
    var marco = document.createElement("div");
    marco.className = "censo-qr-marco-escaneo";
    marco.setAttribute("aria-hidden", "true");
    envoltorio.append(video, marco);
    contenedor.append(envoltorio);
    return { envoltorio: envoltorio, video: video };
}

function constraintsVideo() {
    var esMovil = window.matchMedia("(max-width: 768px)").matches;
    return {
        video: {
            facingMode: { ideal: "environment" },
            width: { ideal: esMovil ? 480 : 960 },
            height: { ideal: esMovil ? 360 : 540 }
        }
    };
}

function iniciarLecturaBarcodeDetector(video, envoltorio, alLeer) {
    var detector = new BarcodeDetector({ formats: ["qr_code"] });
    var activo = true;
    envoltorio._detenerLectura = function () { activo = false; };
    var leer = function () {
        if (!activo || !envoltorio.isConnected) return;
        if (video.readyState < 2) return requestAnimationFrame(leer);
        detector.detect(video).then(function (codigos) {
            if (!activo) return;
            if (codigos.length) {
                detenerEscaner(envoltorio);
                alLeer(codigos[0].rawValue);
                return;
            }
            requestAnimationFrame(leer);
        }).catch(function () { requestAnimationFrame(leer); });
    };
    leer();
}

function iniciarLecturaJsQr(video, envoltorio, alLeer, jsQR) {
    var canvas = document.createElement("canvas");
    var ctx = canvas.getContext("2d", { willReadFrequently: true });
    var activo = true;
    envoltorio._detenerLectura = function () { activo = false; };
    var leer = function () {
        if (!activo || !envoltorio.isConnected) return;
        if (video.readyState >= 2 && video.videoWidth > 0) {
            var ancho = video.videoWidth;
            var alto = video.videoHeight;
            if (canvas.width !== ancho) canvas.width = ancho;
            if (canvas.height !== alto) canvas.height = alto;
            ctx.drawImage(video, 0, 0, ancho, alto);
            var imagen = ctx.getImageData(0, 0, ancho, alto);
            var codigo = jsQR(imagen.data, imagen.width, imagen.height, { inversionAttempts: "dontInvert" });
            if (!codigo) {
                codigo = jsQR(imagen.data, imagen.width, imagen.height, { inversionAttempts: "attemptBoth" });
            }
            if (codigo && codigo.data) {
                detenerEscaner(envoltorio);
                alLeer(codigo.data);
                return;
            }
        }
        requestAnimationFrame(leer);
    };
    leer();
}

export function iniciarEscaner(contenedor, alLeer, mostrarAviso) {
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        if (mostrarAviso) mostrarAviso("Este navegador no permite acceder a la cámara.", "error");
        return null;
    }
    if (!window.isSecureContext && location.protocol !== "https:" && location.hostname !== "localhost") {
        if (mostrarAviso) mostrarAviso("La cámara requiere HTTPS (o localhost).", "error");
        return null;
    }

    var visor = construirVisor(contenedor);
    var envoltorio = visor.envoltorio;
    var video = visor.video;
    var usaBarcodeNativo = typeof window.BarcodeDetector === "function";

    navigator.mediaDevices.getUserMedia(constraintsVideo()).then(function (stream) {
        video.srcObject = stream;
        if (usaBarcodeNativo) {
            iniciarLecturaBarcodeDetector(video, envoltorio, alLeer);
            return;
        }
        cargarJsQr().then(function (jsQR) {
            iniciarLecturaJsQr(video, envoltorio, alLeer, jsQR);
        }).catch(function () {
            detenerEscaner(envoltorio);
            if (mostrarAviso) mostrarAviso("No se pudo inicializar el lector QR.", "error");
        });
    }).catch(function () {
        detenerEscaner(envoltorio);
        if (mostrarAviso) mostrarAviso("No fue posible acceder a la cámara.", "error");
    });

    return envoltorio;
}