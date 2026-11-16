export function crearExportadorTabla() {
    function nombreArchivo(titulo, extension) {
        var base = (titulo || "export").replace(/[^\w\s-]/g, "").trim().replace(/\s+/g, "-") || "export";
        return base + "-" + new Date().toISOString().slice(0, 10) + "." + extension;
    }

    function descargar(nombre, mime, contenido) {
        var blob = new Blob(["\ufeff" + contenido], { type: mime + ";charset=utf-8" });
        var enlace = document.createElement("a");
        enlace.href = URL.createObjectURL(blob);
        enlace.download = nombre;
        enlace.click();
        URL.revokeObjectURL(enlace.href);
    }

    function columnasExportables(api) {
        var indices = [];
        api.columns().every(function (idx) {
            var origen = api.column(idx).dataSrc();
            if (origen !== "acciones") indices.push(idx);
        });
        return indices;
    }

    function filasExportables(nodo, api) {
        var indices = columnasExportables(api);
        var cabeceras = indices.map(function (idx) {
            return api.column(idx).header().textContent.trim();
        });
        var filas = [];
        api.rows({ search: "applied", order: "current" }).every(function () {
            var datos = this.data();
            var celdas = indices.map(function (idx) {
                var clave = api.column(idx).dataSrc();
                var valor = datos[clave];
                if (valor === null || valor === undefined) return "";
                if (typeof valor === "object") return "";
                return String(valor);
            });
            filas.push(celdas);
        });
        if (filas.length === 0 && Array.isArray(nodo._filas) && nodo._filas.length > 0) {
            return { cabeceras: cabeceras, filas: [], sinDatosEnTabla: true };
        }
        return { cabeceras: cabeceras, filas: filas, sinDatosEnTabla: false };
    }

    function escaparCsv(valor) {
        var texto = valor == null ? "" : String(valor);
        if (/[",;\r\n]/.test(texto)) return '"' + texto.replace(/"/g, '""') + '"';
        return texto;
    }

    function exportarCsv(nodo, titulo, sinDatos) {
        var api = nodo._tabla;
        if (!api) return false;
        var datos = filasExportables(nodo, api);
        if (datos.filas.length === 0) {
            if (sinDatos) sinDatos();
            return false;
        }
        var lineas = [datos.cabeceras.map(escaparCsv).join(";")];
        datos.filas.forEach(function (fila) {
            lineas.push(fila.map(escaparCsv).join(";"));
        });
        descargar(nombreArchivo(titulo, "csv"), "text/csv", lineas.join("\r\n"));
        return true;
    }

    function exportarExcel(nodo, titulo, sinDatos) {
        var api = nodo._tabla;
        if (!api) return false;
        var datos = filasExportables(nodo, api);
        if (datos.filas.length === 0) {
            if (sinDatos) sinDatos();
            return false;
        }
        var filasHtml = datos.filas.map(function (fila) {
            return "<tr>" + fila.map(function (celda) {
                return "<td>" + String(celda).replace(/&/g, "&amp;").replace(/</g, "&lt;") + "</td>";
            }).join("") + "</tr>";
        }).join("");
        var cabeceraHtml = datos.cabeceras.map(function (h) {
            return "<th>" + String(h).replace(/&/g, "&amp;").replace(/</g, "&lt;") + "</th>";
        }).join("");
        var html = "<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\">" +
            "<head><meta charset=\"utf-8\"></head><body><table><thead><tr>" + cabeceraHtml + "</tr></thead><tbody>" +
            filasHtml + "</tbody></table></body></html>";
        descargar(nombreArchivo(titulo, "xls"), "application/vnd.ms-excel", html);
        return true;
    }

    return { exportarCsv: exportarCsv, exportarExcel: exportarExcel };
}