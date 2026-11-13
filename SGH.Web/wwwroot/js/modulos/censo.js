export function crearCenso(dependencias) {
    function buscarUltimoMes(filtro) {
        var fechas = [];
        var fecha = new Date();
        for (var indice = 0; indice <= 30; indice += 1) {
            var dia = new Date(fecha);
            dia.setDate(fecha.getDate() - indice);
            fechas.push(dia.getFullYear() + "-" + String(dia.getMonth() + 1).padStart(2, "0") + "-" + String(dia.getDate()).padStart(2, "0"));
        }
        return Promise.all(fechas.map(function (dia) {
            return dependencias.enviar("/Censo/Mid/MidCenso.cs", {
                opcion: "listar",
                moduloId: filtro.modulo,
                pisoId: filtro.piso,
                habitacionId: filtro.habitacion,
                fecha: dia,
                turno: filtro.turno,
                EmpresaMandanteId: filtro.EmpresaMandanteId,
                campamentoId: filtro.campamentoId
                
            });
        })).then(function (respuestas) {
            var filas = [];
            respuestas.forEach(function (respuesta) { filas.push.apply(filas, respuesta.filas || []); });
            return filas;
        });
    }

    function buscar(filtro) {
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", {
            opcion: "listar",
            moduloId: filtro.modulo,
            pisoId: filtro.piso,
            habitacionId: filtro.habitacion,
            fecha: filtro.fecha,
            turno: filtro.turno || "D"
        });
    }

    function buscarMios(filtro) {
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", { opcion: "miscensos", fecha: filtro.fecha });
    }

    function filtros() {
        var catalogo = dependencias.obtenerCatalogo() || { modulos: [], pisos: [], habitaciones: [] };
        return [
            dependencias.campo("modulo", "Módulo", "select", dependencias.aOpcion(catalogo.modulos), { requerido: true }),
            dependencias.campo("piso", "Piso", "select", dependencias.aOpcion(catalogo.pisos), { requerido: true }),
            dependencias.campo("habitacion", "Habitación", "select", dependencias.aOpcion(catalogo.habitaciones)),
            dependencias.campo("turno", "Tipo Turno", "select", [{ value: "D", text: "Día" }, { value: "N", text: "Noche" }], { sinVacio: true }),
            dependencias.campo("fecha", "Fecha Censo", "date")
        ];
    }

    function filtrosMisCensos() {
        return [dependencias.campo("fecha", "Fecha de censo", "date", [], { requerido: true })];
    }

    function enlazar(form) {
        var catalogo = dependencias.obtenerCatalogo();
        var modulo = form.querySelector("[name='modulo']");
        var piso = form.querySelector("[name='piso']");
        var habitacion = form.querySelector("[name='habitacion']");
        if (!modulo || !piso || !habitacion || !catalogo) return;
        function llenar(select, padre) {
            var vacio = select.querySelector("option[value='']");
            select.replaceChildren();
            if (vacio) select.append(vacio);
            (select._opciones || []).forEach(function (opcion) {
                if (padre && opcion.padre !== padre) return;
                var item = document.createElement("option");
                item.value = opcion.value;
                item.textContent = opcion.text;
                select.append(item);
            });
        }
        modulo.addEventListener("change", function () {
            llenar(piso, modulo.value);
            llenar(habitacion, "");
        });
        piso.addEventListener("change", function () {
            llenar(habitacion, piso.value);
        });
    }

    function filtroInicial(modo) {
        if (modo === "misCensos") return { fecha: dependencias.fechaActual() };
        if (modo === "censo") return { censoUltimoMes: true, turno: "D" };
        return null;
    }

    function filaCenso(fila) {
        return {
            numero: fila.id,
            fecha: fila.fecha,
            modulo: fila.modulo,
            piso: fila.piso,
            habitacion: fila.habitacion,
            estadoCenso: fila.estadoCenso,
            estadoHabitacion: fila.estadoHabitacion,
            camas: fila.numeroCamas,
            ocupadas: fila.camasOcupadas,
            ocupadasCheckIn: fila.camasCheckIn,
            _fila: fila
        };
    }

    function filaMisCensos(fila) {
        return {
            numero: fila.id,
            fecha: fila.fecha,
            campamento: fila.campamento,
            modulo: fila.modulo,
            piso: fila.piso,
            habitacion: fila.habitacion,
            camas: fila.numeroCamas,
            ocupadas: fila.camasOcupadas,
            turno: fila.turnoNombre || fila.turno,
            _fila: fila
        };
    }

    function columnas(modo) {
        if (modo === "censo") {
            return ["numero", "fecha", "modulo", "piso", "habitacion", "estadoCenso", "estadoHabitacion", "camas", "ocupadas", "ocupadasCheckIn"].map(function (clave) {
                if (clave !== "ocupadas") return { data: clave };
                return {
                    data: clave,
                    createdCell: function (td, valor, row) {
                        var input = document.createElement("input");
                        input.type = "number";
                        input.min = "0";
                        input.className = "cama-ocupada";
                        input.value = row._fila.camasOcupadas || "0";
                        input.addEventListener("input", function () { row._fila.camasOcupadas = input.value; });
                        td.replaceChildren(input);
                    }
                };
            });
        }
        if (modo === "misCensos") {
            return ["numero", "fecha", "campamento", "modulo", "piso", "habitacion", "camas", "ocupadas", "turno"]
                .map(function (clave) { return { data: clave }; });
        }
        return null;
    }

    function validarCamasOcupadas(fila) {
        return fila.camasOcupadas <= fila.numeroCamas;
    }

    function guardar(nodo) {
        // Validar que las camas ocupadas no superen el número de camas y que no sean menor a 0
        // pero pueden ser = 0
        for (var i = 0; i < (nodo._filas || []).length; i++) {
            var fila = nodo._filas[i];
            if (!validarCamasOcupadas(fila) || fila.camasOcupadas < 0) {
                dependencias.mostrarEstado(nodo, "Las camas ocupadas deben estar entre 0 y el número de camas, revisar id: " + (fila.id) + "");
                return Promise.reject(new Error("Las camas ocupadas deben estar entre 0 y el número de camas"));
            }
        }
        var filtro = nodo._filtro || {};
        var filas = (nodo._filas || []).map(function (fila) {
            return {
                id: fila.id,
                habitacionId: dependencias.numero(fila.habitacionId),
                cantidad: fila.camasOcupadas,
                turno: fila.turno || filtro.turno || "D"
            };
        });
        dependencias.mostrarEstado(nodo, "");
        return dependencias.enviar("/Censo/Mid/MidCenso.cs", { opcion: "guardar", fecha: filtro.fecha, filas: filas })
            .then(function () { return dependencias.cargar("censo"); })
            .catch(function (error) { dependencias.mostrarEstado(nodo, error.message); });
    }

    return {
        filtros: filtros,
        filtrosMisCensos: filtrosMisCensos,
        buscarUltimoMes: buscarUltimoMes,
        buscar: buscar,
        buscarMios: buscarMios,
        enlazar: enlazar,
        filtroInicial: filtroInicial,
        filaCenso: filaCenso,
        filaMisCensos: filaMisCensos,
        columnas: columnas,
        guardar: guardar
    };
}
