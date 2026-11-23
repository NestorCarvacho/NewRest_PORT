export function detenerEscaner(envoltorio) {
    if (!envoltorio) return;
    var video = envoltorio.querySelector("video");
    if (video && video.srcObject) {
        video.srcObject.getTracks().forEach(function (track) { track.stop(); });
    }
    envoltorio.remove();
}

export function iniciarEscaner(contenedor, alLeer, mostrarAviso) {
    if (!("BarcodeDetector" in window) || !navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        if (mostrarAviso) mostrarAviso("Este navegador no soporta lectura QR por cámara.", "error");
        return null;
    }
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

    var esMovil = window.matchMedia("(max-width: 768px)").matches;
    navigator.mediaDevices.getUserMedia({
        video: {
            facingMode: { ideal: "environment" },
            width: { ideal: esMovil ? 640 : 1280 },
            height: { ideal: esMovil ? 480 : 720 }
        }
    }).then(function (stream) {
        video.srcObject = stream;
        var detector = new BarcodeDetector({ formats: ["qr_code"] });
        var leer = function () {
            if (!envoltorio.isConnected) return;
            if (video.readyState < 2) return requestAnimationFrame(leer);
            detector.detect(video).then(function (codigos) {
                if (codigos.length) {
                    detenerEscaner(envoltorio);
                    alLeer(codigos[0].rawValue);
                    return;
                }
                requestAnimationFrame(leer);
            }).catch(function () { requestAnimationFrame(leer); });
        };
        leer();
    }).catch(function () {
        detenerEscaner(envoltorio);
        if (mostrarAviso) mostrarAviso("No fue posible acceder a la cámara.", "error");
    });
    return envoltorio;
}