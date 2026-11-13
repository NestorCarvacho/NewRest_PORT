export function crearTablas() {
    function crear(columnas) {
        var tabla = document.createElement("table");
        tabla.className = "tabla";
        var cabecera = document.createElement("thead");
        var fila = document.createElement("tr");
        columnas.forEach(function (columna) {
            var th = document.createElement("th");
            th.textContent = columna;
            fila.append(th);
        });
        cabecera.append(fila);
        tabla.append(cabecera, document.createElement("tbody"));
        return tabla;
    }

    function inicializar(nodo, columnas, ordenInicial) {
        function t(texto) {
            return window.SGHI18n ? window.SGHI18n.traducir(texto) : texto;
        }

        var opciones = {
            columns: columnas,
            language: {
                emptyTable: t("No hay registros para mostrar"),
                search: t("Buscar:"),
                lengthMenu: t("Mostrar _MENU_"),
                info: t("_START_ a _END_ de _TOTAL_"),
                infoEmpty: t("0 registros"),
                zeroRecords: t("Sin resultados"),
                paginate: { previous: t("Anterior"), next: t("Siguiente") }
            }
        };
        if (ordenInicial) opciones.order = ordenInicial;
        var tabla = new DataTable(nodo.querySelector("table"), opciones);
        tabla.on("draw", function () { completarFilas(tabla); });
        completarFilas(tabla);
        return tabla;
    }

    function pintar(nodo, filas, mapear) {
        nodo._filas = filas;
        var api = nodo._tabla;
        api.clear();
        filas.forEach(function (fila) { api.row.add(mapear(fila)); });
        api.draw();
    }

    function completarFilas(api) {
        var cuerpo = api.table().body();
        if (!cuerpo) return;
        var cantidad = api.page.len();
        if (cantidad < 1) return;
        cuerpo.querySelectorAll("tr").forEach(function (fila) {
            if (fila.querySelector(".dt-empty") || fila.classList.contains("fila-vacia")) fila.remove();
        });
        cuerpo.querySelectorAll(".fila-relleno").forEach(function (fila) { fila.remove(); });
        var columnas = api.columns().count();
        if (cuerpo.rows.length === 0) {
            var vacia = document.createElement("tr");
            vacia.className = "fila-vacia";
            var celda = document.createElement("td");
            celda.colSpan = columnas;
            celda.innerHTML = '<div class="tabla-vacia" style="--filas:' + cantidad + '"><img src="/img/estado/tabla-vacia.svg" alt="" /><p>No hay registros para mostrar</p></div>';
            vacia.append(celda);
            cuerpo.append(vacia);
            return;
        }
        var faltan = cantidad - cuerpo.rows.length;
        for (var i = 0; i < faltan; i++) {
            var filaRelleno = document.createElement("tr");
            filaRelleno.className = "fila-relleno";
            for (var columna = 0; columna < columnas; columna++) {
                var celdaRelleno = document.createElement("td");
                celdaRelleno.textContent = "\u00a0";
                filaRelleno.append(celdaRelleno);
            }
            cuerpo.append(filaRelleno);
        }
    }

    function destruir(nodo) {
        if (nodo._tabla) nodo._tabla.destroy();
    }

    return { crear: crear, inicializar: inicializar, pintar: pintar, destruir: destruir };
}
