import { encabezadosModulo } from "./encabezados-tabla.js";
import { enlazarTooltipsCelda, ocultarTooltipCelda } from "./celda-truncada.js";

export function crearTablas() {
    /** Fallback cuando el panel no viene anclado desde Razor: arma la misma tabla “tonta” que _TablaModulo.cshtml. */
    function crear(encabezados, modo) {
        var titulos = encabezados;
        if ((!titulos || !titulos.length) && modo) titulos = encabezadosModulo(modo);
        var tabla = document.createElement("table");
        tabla.className = "tabla modulo-tabla";
        if (modo) tabla.dataset.modulo = modo;
        var cabecera = document.createElement("thead");
        var fila = document.createElement("tr");
        (titulos || []).forEach(function (columna) {
            var th = document.createElement("th");
            th.scope = "col";
            th.textContent = columna;
            fila.append(th);
        });
        cabecera.append(fila);
        tabla.append(cabecera, document.createElement("tbody"));
        var envoltorio = document.createElement("div");
        envoltorio.className = "modulo-tabla-contenedor";
        envoltorio.append(tabla);
        return envoltorio;
    }

    function inicializar(nodo, columnas, ordenInicial) {
        function t(texto) {
            return window.SGHI18n ? window.SGHI18n.traducir(texto) : texto;
        }

        var opciones = {
            columns: columnas,
            autoWidth: false,
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
        if (window.DataTable && DataTable.Responsive) {
            opciones.responsive = {
                breakpoints: [
                    { name: "desktop", width: Infinity },
                    { name: "tablet", width: 1024 },
                    { name: "mobile", width: 768 }
                ],
                details: {
                    type: "column",
                    target: "tr"
                }
            };
            opciones.columnDefs = (columnas || []).map(function (columna, indice) {
                var prioridad = columna && columna.responsivePriority != null
                    ? columna.responsivePriority
                    : (indice < 2 ? indice + 1 : 100 + indice);
                return { targets: indice, responsivePriority: prioridad };
            });
        }
        if (ordenInicial) opciones.order = ordenInicial;
        var elementoTabla = nodo.querySelector("table.modulo-tabla, table.tabla");
        if (!elementoTabla) {
            throw new Error("No hay tabla en el panel del módulo.");
        }
        var tabla = new DataTable(elementoTabla, opciones);
        tabla.on("draw", function () {
            completarFilas(tabla);
            enlazarTooltipsCelda(nodo);
            ocultarTooltipCelda();
        });
        completarFilas(tabla);
        enlazarTooltipsCelda(nodo);
        return tabla;
    }

    function idRegistro(fila) {
        if (!fila) return null;
        var id = fila.id != null ? fila.id : fila.Id;
        return id == null ? null : Number(id);
    }

    function idDesdeDatosFila(datos) {
        if (!datos) return null;
        if (datos._fila != null) return idRegistro(datos._fila);
        if (datos.acciones != null && typeof datos.acciones === "object") return idRegistro(datos.acciones);
        return idRegistro(datos);
    }

    function indiceEnFilas(filas, id) {
        for (var i = 0; i < filas.length; i++) {
            if (idRegistro(filas[i]) === id) return i;
        }
        return -1;
    }

    function indiceColumnaDatos(api, nombreDato) {
        var ajustes = api.settings()[0];
        var columnas = ajustes.aoColumns || [];
        for (var indice = 0; indice < columnas.length; indice++) {
            var dato = columnas[indice].mData;
            if (typeof dato === "string" && dato === nombreDato) return indice;
        }
        return -1;
    }

    function filaApiPorId(api, id) {
        var numeroId = Number(id);
        var encontrada = null;
        api.rows().every(function () {
            if (idDesdeDatosFila(this.data()) === numeroId) {
                encontrada = this;
                return false;
            }
        });
        return encontrada;
    }

    function pintar(nodo, filas, mapear) {
        nodo._filas = filas;
        var api = nodo._tabla;
        api.clear();
        filas.forEach(function (fila) { api.row.add(mapear(fila)); });
        api.draw();
    }

    function quitarFila(nodo, id) {
        if (id == null) return;
        var numeroId = Number(id);
        var filas = nodo._filas || [];
        var idx = indiceEnFilas(filas, numeroId);
        if (idx >= 0) filas.splice(idx, 1);
        nodo._filas = filas;
        var api = nodo._tabla;
        if (!api) return;
        var fila = filaApiPorId(api, numeroId);
        if (fila) fila.remove();
        api.draw(false);
    }

  /** Actualiza una fila en memoria y en DataTables sin repintar todo el listado. */
    function sincronizarFila(nodo, id, filaOrigen, mapear, repintarAcciones) {
        if (id == null || !filaOrigen) return;
        var numeroId = Number(id);
        var filas = nodo._filas || [];
        var idx = indiceEnFilas(filas, numeroId);
        if (idx >= 0) filas[idx] = filaOrigen;
        else filas.push(filaOrigen);
        nodo._filas = filas;
        var api = nodo._tabla;
        if (!api) return;
        var mapeada = mapear(filaOrigen);
        var fila = filaApiPorId(api, numeroId);
        if (fila) {
            fila.data(mapeada);
            fila.invalidate();
            api.draw(false);
            var indice = fila.index();
            if (repintarAcciones) {
                var columnaAcciones = indiceColumnaDatos(api, "acciones");
                if (columnaAcciones >= 0) {
                    var celdaAcciones = api.cell(indice, columnaAcciones).node();
                    if (celdaAcciones) celdaAcciones.replaceChildren(repintarAcciones(mapeada));
                }
            }
            return;
        }
        api.row.add(mapeada).draw(false);
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

    return {
        crear: crear,
        inicializar: inicializar,
        pintar: pintar,
        sincronizarFila: sincronizarFila,
        quitarFila: quitarFila,
        destruir: destruir
    };
}
