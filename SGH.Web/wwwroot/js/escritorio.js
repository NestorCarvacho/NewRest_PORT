(function () {
    var modulos = {
        reserva: pantalla(
            "Reserva",
            ["Acciones", "N°", "Grupo Empresa", "Pasajero", "Habitación", "Estado", "Llegada", "Salida"],
            filtrosReserva,
            formularioReserva,
            { titulo: "Nueva reserva", boton: "Nueva reserva", confirmar: "Guardar" },
            "reserva"
        ),
        checkin: pantalla(
            "Check In",
            ["Acciones", "N°", "Grupo Empresa", "Pasajero", "Habitación", "Estado", "Llegada", "Salida"],
            filtrosReserva,
            null,
            null,
            "checkin"
        ),
        checkout: pantalla(
            "Check Out",
            ["Acciones", "N°", "Grupo Empresa", "Pasajero", "Habitación", "Estado", "Llegada", "Salida"],
            filtrosReserva,
            null,
            null,
            "checkout"
        ),
        censo: pantalla(
            "Censo",
            ["Nº", "Fecha", "Módulo", "Piso", "Habitación", "Estado Censo", "Estado Habitación", "N° Camas", "Camas ocupadas", "Camas ocupadas (CheckIn)"],
            filtrosCenso,
            null,
            { titulo: "Censo", boton: "Guardar", confirmar: "Guardar" },
            "censo"
        )
    };

    function normalizar(texto) {
        return texto.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    }

    var solapas = document.getElementById("solapas");
    var paneles = document.getElementById("paneles");
    var vista = document.getElementById("vista-inicial");
    var panel = document.getElementById("panel-lateral");
    var velo = document.getElementById("velo");
    var orden = [];
    var arrastrada = null;

    document.querySelectorAll("[data-modulo]").forEach(function (boton) {
        boton.addEventListener("click", function () {
            abrir(boton.getAttribute("data-modulo"));
            cerrarMenus();
        });
    });

    document.querySelectorAll(".menu-item").forEach(function (item) {
        var temporizador;
        item.addEventListener("mouseenter", function () {
            window.clearTimeout(temporizador);
            abrirMenu(item);
        });
        item.addEventListener("mouseleave", function () {
            temporizador = window.setTimeout(function () { cerrarMenu(item); }, 180);
        });
    });

    var usuario = document.querySelector(".usuario-boton");
    var menuUsuario = document.querySelector(".usuario-menu");
    if (usuario && menuUsuario) {
        usuario.addEventListener("click", function () {
            var abierto = menuUsuario.hidden;
            menuUsuario.hidden = !abierto;
            usuario.setAttribute("aria-expanded", abierto ? "true" : "false");
        });
        document.addEventListener("click", function (evento) {
            if (menuUsuario.hidden) return;
            if (evento.target.closest(".usuario")) return;
            menuUsuario.hidden = true;
            usuario.setAttribute("aria-expanded", "false");
        });
    }

    var buscador = document.getElementById("buscar-modulo");
    var resultados = document.getElementById("buscar-resultados");
    if (buscador) {
        buscador.addEventListener("input", function () {
            var texto = buscador.value.trim().toLowerCase();
            resultados.replaceChildren();
            if (!texto) {
                resultados.hidden = true;
                return;
            }
            var consulta = normalizar(texto);
            document.querySelectorAll(".menu-columna button").forEach(function (origen) {
                if (normalizar(origen.textContent).indexOf(consulta) < 0) return;
                var item = document.createElement("li");
                var boton = document.createElement("button");
                boton.type = "button";
                boton.textContent = origen.textContent;
                boton.addEventListener("click", function () {
                    buscador.value = "";
                    resultados.hidden = true;
                    var id = origen.getAttribute("data-modulo");
                    if (id) abrir(id);
                });
                item.append(boton);
                resultados.append(item);
            });
            resultados.hidden = resultados.childElementCount === 0;
        });
    }

    document.getElementById("panel-cerrar").addEventListener("click", cerrarPanel);
    velo.addEventListener("click", cerrarPanel);
    document.addEventListener("keydown", function (evento) {
        if (evento.key === "Escape") {
            cerrarPanel();
            cerrarMenus();
        }
    });

    solapas.addEventListener("dragstart", function (evento) {
        var solapa = evento.target.closest(".solapa");
        if (!solapa) return;
        arrastrada = solapa;
        solapa.classList.add("arrastrando");
        evento.dataTransfer.effectAllowed = "move";
    });
    solapas.addEventListener("dragend", function () {
        if (arrastrada) arrastrada.classList.remove("arrastrando");
        arrastrada = null;
    });
    solapas.addEventListener("dragover", function (evento) {
        evento.preventDefault();
        var destino = evento.target.closest(".solapa");
        if (!destino || destino === arrastrada) return;
        var rect = destino.getBoundingClientRect();
        var despues = evento.clientX > rect.left + rect.width / 2;
        solapas.insertBefore(arrastrada, despues ? destino.nextSibling : destino);
    });

    function abrir(id) {
        var modulo = modulos[id];
        if (!modulo) return;
        if (orden.indexOf(id) < 0) {
            orden.push(id);
            solapas.append(crearSolapa(id, modulo.titulo));
            paneles.append(crearPanel(id, modulo));
        }
        activar(id);
    }

    function crearSolapa(id, titulo) {
        var solapa = document.createElement("button");
        solapa.type = "button";
        solapa.className = "solapa";
        solapa.draggable = true;
        solapa.dataset.id = id;
        var texto = document.createElement("span");
        texto.textContent = titulo;
        var cerrar = document.createElement("span");
        cerrar.className = "solapa-cerrar";
        cerrar.setAttribute("role", "button");
        cerrar.setAttribute("aria-label", "Cerrar " + titulo);
        cerrar.textContent = "×";
        cerrar.addEventListener("click", function (evento) {
            evento.stopPropagation();
            cerrarSolapa(id);
        });
        solapa.append(texto, cerrar);
        solapa.addEventListener("click", function () { activar(id); });
        return solapa;
    }

    function crearPanel(id, modulo) {
        var seccion = document.createElement("section");
        seccion.className = "modulo";
        seccion.dataset.id = id;
        seccion.hidden = true;
        seccion._filtro = filtroInicial(modulo.modo);
        var estado = document.createElement("p");
        estado.className = "modulo-estado";
        estado.hidden = true;
        var barra = document.createElement("div");
        barra.className = "modulo-barra";
        barra.append(boton("Filtros", "boton-secundario", function () {
            prepararCatalogo(modulo.modo).then(function () {
                abrirPanel("Filtros de " + modulo.titulo, formulario(contenido(modulo.filtros), "Aplicar", false, function (datos) {
                    seccion._filtro = datos;
                    return cargar(id);
                }));
            }).catch(function (error) { mostrarEstado(seccion, error.message); });
        }));
        if (modulo.accion && modulo.modo === "reserva") {
            barra.append(boton(modulo.accion.boton, "boton-primario", function () {
                prepararCatalogo(modulo.modo).then(function () {
                    abrirPanel(modulo.accion.titulo, formulario(contenido(modulo.secciones), modulo.accion.confirmar, true, function (datos) {
                        return enviar("/Reserva/Datos?handler=Crear", {
                            areaId: numero(datos.grupo),
                            turnoId: numero(datos.turno),
                            fechaReserva: datos.reserva,
                            fechaLlegada: datos.llegada,
                            fechaSalida: datos.salida,
                            observacion: datos.observaciones
                        }).then(function () { return cargar(id); });
                    }));
                }).catch(function (error) { mostrarEstado(seccion, error.message); });
            }));
        }
        if (modulo.modo === "censo") {
            var guardar = boton("Guardar", "boton-primario", function () { guardarCenso(seccion); });
            guardar.hidden = true;
            seccion._guardar = guardar;
            barra.append(guardar);
        }
        var tabla = document.createElement("table");
        tabla.className = "tabla";
        var cabecera = document.createElement("thead");
        var fila = document.createElement("tr");
        modulo.columnas.forEach(function (columna) {
            var th = document.createElement("th");
            th.textContent = columna;
            fila.append(th);
        });
        cabecera.append(fila);
        tabla.append(cabecera, document.createElement("tbody"));
        seccion.append(estado, barra, tabla);
        return seccion;
    }

    function activar(id) {
        solapas.hidden = false;
        vista.hidden = true;
        solapas.querySelectorAll(".solapa").forEach(function (solapa) {
            solapa.classList.toggle("activa", solapa.dataset.id === id);
        });
        paneles.querySelectorAll(".modulo").forEach(function (nodo) {
            var visible = nodo.dataset.id === id;
            nodo.hidden = !visible;
            if (!visible || !window.DataTable) return;
            if (!nodo.dataset.lista) {
                nodo.dataset.lista = "1";
                nodo._tabla = new DataTable(nodo.querySelector("table"), {
                    columns: columnasDe(modulos[id].modo),
                    language: {
                        emptyTable: "No hay registros para mostrar",
                        search: "Buscar:",
                        lengthMenu: "Mostrar _MENU_",
                        info: "_START_ a _END_ de _TOTAL_",
                        infoEmpty: "0 registros",
                        zeroRecords: "Sin resultados",
                        paginate: { previous: "Anterior", next: "Siguiente" }
                    }
                });
                nodo._tabla.on("draw", function () { completarFilas(nodo._tabla); });
                completarFilas(nodo._tabla);
                if (modulos[id].modo !== "censo") cargar(id);
            }
        });
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
            var fila = document.createElement("tr");
            fila.className = "fila-relleno";
            for (var columna = 0; columna < columnas; columna++) {
                var celda = document.createElement("td");
                celda.textContent = "\u00a0";
                fila.append(celda);
            }
            cuerpo.append(fila);
        }
    }

    function cerrarSolapa(id) {
        var indice = orden.indexOf(id);
        if (indice < 0) return;
        orden.splice(indice, 1);
        solapas.querySelector('[data-id="' + id + '"]').remove();
        var nodo = paneles.querySelector('[data-id="' + id + '"]');
        if (nodo._tabla) nodo._tabla.destroy();
        nodo.remove();
        if (orden.length === 0) {
            solapas.hidden = true;
            vista.hidden = false;
            return;
        }
        activar(orden[Math.max(0, indice - 1)]);
    }

    function abrirPanel(titulo, contenido) {
        document.getElementById("panel-titulo").textContent = titulo;
        var cuerpo = document.getElementById("panel-cuerpo");
        cuerpo.replaceChildren(contenido);
        panel.hidden = false;
        velo.hidden = false;
    }

    function cerrarPanel() {
        panel.hidden = true;
        velo.hidden = true;
    }

    function formulario(secciones, accion, esAlta, alEnviar) {
        var form = document.createElement("form");
        form.className = "formulario";
        var error = document.createElement("p");
        error.className = "formulario-error";
        error.hidden = true;
        form.append(error);
        secciones.forEach(function (grupo) {
            if (grupo.titulo) {
                var titulo = document.createElement("h3");
                titulo.className = "formulario-titulo";
                titulo.textContent = grupo.titulo;
                form.append(titulo);
            }
            var grilla = document.createElement("div");
            grilla.className = "formulario-grilla";
            grupo.campos.forEach(function (definicion) {
                grilla.append(campoVisual(definicion, esAlta ? "Seleccione" : "Todos"));
            });
            form.append(grilla);
        });
        enlazarCenso(form);
        var pie = document.createElement("div");
        pie.className = "formulario-pie";
        pie.append(
            boton("Cancelar", "boton-secundario", cerrarPanel),
            boton(accion, "boton-primario", null)
        );
        form.append(pie);
        form.addEventListener("submit", function (evento) {
            evento.preventDefault();
            var promesa = alEnviar ? alEnviar(leerFormulario(form)) : null;
            if (!promesa || !promesa.then) {
                cerrarPanel();
                return;
            }
            promesa.then(function () { cerrarPanel(); }).catch(function (fallo) {
                error.hidden = false;
                error.textContent = fallo.message;
            });
        });
        return form;
    }

    function momentoLocal(conHora) {
        var ahora = new Date();
        var fecha = ahora.getFullYear() + "-" + dosDigitos(ahora.getMonth() + 1) + "-" + dosDigitos(ahora.getDate());
        if (!conHora) return fecha;
        return fecha + "T" + dosDigitos(ahora.getHours()) + ":" + dosDigitos(ahora.getMinutes());
    }

    function dosDigitos(numero) {
        return String(numero).padStart(2, "0");
    }

    function campoVisual(definicion, vacio) {
        var etiqueta = document.createElement("label");
        etiqueta.className = "campo" + (definicion.ancho ? " campo-ancho" : "");
        var texto = document.createElement("span");
        texto.textContent = definicion.etiqueta;
        var control;
        if (definicion.tipo === "select") {
            control = document.createElement("select");
            control._opciones = opcionesDe(definicion);
            if (!definicion.sinVacio) {
                var todos = document.createElement("option");
                todos.value = "";
                todos.textContent = vacio;
                control.append(todos);
            }
            control._opciones.forEach(function (opcion) {
                var item = document.createElement("option");
                item.value = opcion.value;
                item.textContent = opcion.text;
                control.append(item);
            });
        } else if (definicion.tipo === "textarea") {
            control = document.createElement("textarea");
        } else {
            control = document.createElement("input");
            control.type = definicion.tipo || "text";
        }
        if (definicion.soloLectura) control.readOnly = true;
        if (definicion.deshabilitado) control.disabled = true;
        if (definicion.nombre) control.name = definicion.nombre;
        if (definicion.valor) control.value = definicion.valor;
        if ((definicion.tipo === "date" || definicion.tipo === "datetime-local") && !definicion.soloLectura && !definicion.deshabilitado) {
            var linea = document.createElement("span");
            linea.className = "campo-fecha";
            var ahora = document.createElement("button");
            ahora.type = "button";
            ahora.className = "campo-ahora";
            var conHora = definicion.tipo === "datetime-local";
            ahora.textContent = conHora ? "Ahora" : "Hoy";
            ahora.addEventListener("click", function () {
                control.value = momentoLocal(conHora);
            });
            linea.append(control, ahora);
            etiqueta.append(texto, linea);
            return etiqueta;
        }
        etiqueta.append(texto, control);
        return etiqueta;
    }

    function boton(texto, clase, alClick) {
        var elemento = document.createElement("button");
        elemento.type = alClick ? "button" : "submit";
        elemento.className = clase;
        elemento.textContent = texto;
        if (alClick) elemento.addEventListener("click", alClick);
        return elemento;
    }

    function abrirMenu(item) {
        cerrarMenus();
        item.classList.add("abierto");
        item.querySelector(".menu-boton").setAttribute("aria-expanded", "true");
    }

    function cerrarMenu(item) {
        item.classList.remove("abierto");
        item.querySelector(".menu-boton").setAttribute("aria-expanded", "false");
    }

    function cerrarMenus() {
        document.querySelectorAll(".menu-item.abierto").forEach(cerrarMenu);
        var menu = document.querySelector(".usuario-menu");
        if (menu) menu.hidden = true;
        if (resultados) resultados.hidden = true;
    }

    function pantalla(titulo, columnas, filtros, secciones, accion, modo) {
        return { titulo: titulo, columnas: columnas, filtros: filtros, secciones: secciones, accion: accion, modo: modo };
    }

    function seccion(titulo, campos) {
        return { titulo: titulo, campos: campos };
    }

    function campo(nombre, etiqueta, tipo, opciones, extra) {
        extra = extra || {};
        return {
            nombre: nombre,
            etiqueta: etiqueta,
            tipo: tipo || "text",
            opciones: opciones || [],
            ancho: tipo === "textarea" || extra.ancho,
            sinVacio: extra.sinVacio,
            soloLectura: extra.soloLectura,
            deshabilitado: extra.deshabilitado,
            valor: extra.valor
        };
    }

    function filtrosReserva() {
        var catalogo = catalogoReserva || { grupos: [], origenes: [], pasajeros: [], habitaciones: [], turnos: [] };
        return [
            campo("grupo", "Grupo Empresa", "select", aOpcion(catalogo.grupos)),
            campo("origen", "Origen reserva", "select", aOpcion(catalogo.origenes)),
            campo("solicitud", "# Solicitud"),
            campo("estado", "Estado", "select", [
                { value: "A", text: "Asignada" },
                { value: "I", text: "Asignada con Checkin" },
                { value: "O", text: "Asignada con Checkout" },
                { value: "N", text: "Anulada" },
                { value: "F", text: "Finalizada" }
            ]),
            campo("reserva", "Fecha Reserva", "date"),
            campo("llegada", "Fecha Llegada", "date"),
            campo("salida", "Fecha Salida", "date"),
            campo("checkin", "Check In", "date"),
            campo("checkout", "Check Out", "date"),
            campo("pasajero", "Pasajero", "select", aOpcion(catalogo.pasajeros)),
            campo("habitacion", "Habitación", "select", aOpcion(catalogo.habitaciones)),
            campo("agua", "Tiene Agua?", "select", [{ value: "1", text: "SI" }, { value: "2", text: "NO" }])
        ];
    }

    function filtrosCenso() {
        var catalogo = catalogoCenso || { modulos: [], pisos: [], habitaciones: [] };
        return [
            campo("modulo", "Módulo", "select", aOpcion(catalogo.modulos)),
            campo("piso", "Piso", "select", aOpcion(catalogo.pisos)),
            campo("habitacion", "Habitación", "select", aOpcion(catalogo.habitaciones)),
            campo("turno", "Tipo Turno", "select", [{ value: "D", text: "Día" }, { value: "N", text: "Noche" }], { sinVacio: true }),
            campo("fecha", "Fecha Censo", "date")
        ];
    }

    function formularioReserva() {
        var catalogo = catalogoReserva || { grupos: [], origenes: [], turnos: [] };
        var origen = (catalogo.origenes || []).filter(function (opcion) { return opcion.id === "R"; })[0];
        return [
            seccion("Datos", [
                campo("numero", "Número", "text", [], { soloLectura: true }),
                campo("grupo", "Grupo Empresa", "select", aOpcion(catalogo.grupos)),
                campo("origen", "Origen reserva", "text", [], { soloLectura: true, valor: origen ? origen.etiqueta : "R" }),
                campo("solicitud", "# Solicitud", "text", [], { soloLectura: true })
            ]),
            seccion("Estadía", [
                campo("reserva", "Fecha Reserva", "date"),
                campo("llegada", "Fecha Llegada", "date"),
                campo("salida", "Fecha Salida", "date"),
                campo("turno", "Turno", "select", aOpcion(catalogo.turnos)),
                campo("estado", "Estado", "text", [], { soloLectura: true, valor: "P" })
            ]),
            seccion("", [
                campo("observaciones", "Observaciones", "textarea")
            ])
        ];
    }

    var catalogoReserva = null;
    var catalogoCenso = null;

    function contenido(origen) {
        var valor = typeof origen === "function" ? origen() : origen;
        if (!valor) return [];
        if (valor.length && valor[0].campos) return valor;
        return [seccion("Criterios", valor)];
    }

    function opcionesDe(definicion) {
        return (definicion.opciones || []).map(function (opcion) {
            if (typeof opcion === "string") return { value: opcion, text: opcion };
            return { value: String(opcion.value), text: opcion.text, padre: opcion.padre == null ? "" : String(opcion.padre) };
        });
    }

    function aOpcion(lista) {
        return (lista || []).map(function (opcion) {
            return { value: opcion.id, text: opcion.etiqueta, padre: opcion.padreId };
        });
    }

    function leerFormulario(form) {
        var datos = {};
        form.querySelectorAll("input, select, textarea").forEach(function (control) {
            if (!control.name || control.disabled) return;
            datos[control.name] = control.value;
        });
        return datos;
    }

    function enlazarCenso(form) {
        var modulo = form.querySelector("[name='modulo']");
        var piso = form.querySelector("[name='piso']");
        var habitacion = form.querySelector("[name='habitacion']");
        if (!modulo || !piso || !habitacion || !catalogoCenso) return;
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
        if (modo === "checkin") return { estado: "A" };
        if (modo === "checkout") return { estado: "I" };
        return {};
    }

    function token() {
        var campo = document.querySelector("#antifalsificacion input[name='__RequestVerificationToken']");
        return campo ? campo.value : "";
    }

    function pedir(url, opciones) {
        return fetch(url, opciones).then(function (respuesta) {
            return respuesta.text().then(function (texto) {
                var cuerpo = {};
                if (texto) {
                    try { cuerpo = JSON.parse(texto); } catch (error) { cuerpo = {}; }
                }
                if (!respuesta.ok) throw new Error(cuerpo.mensaje || "No se pudo completar la operación.");
                return cuerpo;
            });
        });
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

    function prepararCatalogo(modo) {
        if (modo === "censo") {
            if (catalogoCenso) return Promise.resolve(catalogoCenso);
            return pedir("/Censo/Datos?handler=Catalogo").then(function (datos) {
                catalogoCenso = datos;
                return datos;
            });
        }
        if (catalogoReserva) return Promise.resolve(catalogoReserva);
        return pedir("/Reserva/Datos?handler=Catalogo").then(function (datos) {
            catalogoReserva = datos;
            return datos;
        });
    }

    function numero(valor) {
        var convertido = parseInt(valor, 10);
        return isNaN(convertido) ? 0 : convertido;
    }

    function consulta(parametros) {
        var partes = [];
        Object.keys(parametros).forEach(function (clave) {
            if (parametros[clave] === undefined || parametros[clave] === null || parametros[clave] === "") return;
            partes.push(encodeURIComponent(clave) + "=" + encodeURIComponent(parametros[clave]));
        });
        return partes.length ? "&" + partes.join("&") : "";
    }

    function cargar(id) {
        var nodo = paneles.querySelector('[data-id="' + id + '"]');
        var modulo = modulos[id];
        var filtro = nodo._filtro || {};
        mostrarEstado(nodo, "");
        var url = modulo.modo === "censo"
            ? "/Censo/Datos?handler=Buscar" + consulta({
                moduloId: filtro.modulo,
                pisoId: filtro.piso,
                habitacionId: filtro.habitacion,
                fecha: filtro.fecha,
                turno: filtro.turno || "D"
            })
            : "/Reserva/Datos?handler=Buscar" + consulta({
                areaId: filtro.grupo,
                origen: filtro.origen,
                solicitudId: filtro.solicitud,
                estado: filtro.estado,
                fechaReserva: filtro.reserva,
                fechaLlegada: filtro.llegada,
                fechaSalida: filtro.salida,
                checkIn: filtro.checkin,
                checkOut: filtro.checkout,
                pasajeroId: filtro.pasajero,
                habitacionId: filtro.habitacion,
                tieneAgua: filtro.agua
            });
        return pedir(url).then(function (datos) {
            pintar(nodo, datos.filas || [], modulo.modo);
        }).catch(function (error) {
            pintar(nodo, [], modulo.modo);
            mostrarEstado(nodo, error.message);
        });
    }

    function pintar(nodo, filas, modo) {
        nodo._filas = filas;
        if (nodo._guardar) nodo._guardar.hidden = modo !== "censo" || filas.length === 0;
        var api = nodo._tabla;
        api.clear();
        filas.forEach(function (fila) { api.row.add(filaDe(fila, modo)); });
        api.draw();
    }

    function filaDe(fila, modo) {
        if (modo === "censo") {
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
        return {
            acciones: fila,
            numero: fila.id,
            grupo: fila.grupoEmpresa,
            pasajero: fila.pasajero,
            habitacion: fila.habitacion,
            estado: fila.estado,
            llegada: fila.llegada,
            salida: fila.salida,
            _fila: fila
        };
    }

    function columnasDe(modo) {
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
        return ["acciones", "numero", "grupo", "pasajero", "habitacion", "estado", "llegada", "salida"].map(function (clave) {
            if (clave !== "acciones") return { data: clave };
            return {
                data: clave,
                orderable: false,
                createdCell: function (td, fila) {
                    td.replaceChildren(accionMovimiento(fila));
                }
            };
        });
    }

    function accionMovimiento(fila) {
        if (fila.estadoNumero === "A" && !fila.checkIn) return botonMovimiento(fila.id, "I", "Check In");
        if (fila.estadoNumero === "A" && fila.checkIn && !fila.checkOut) return botonMovimiento(fila.id, "O", "Check Out");
        return document.createTextNode("");
    }

    function botonMovimiento(reservaId, operacion, texto) {
        return boton(texto, "accion-fila", function () {
            abrirPanel("Seleccionar Fecha", formulario([seccion("", [campo("fecha", "Fecha", "datetime-local")])], "Continuar", true, function (datos) {
                return enviar("/Reserva/Datos?handler=Movimiento", {
                    reservaId: reservaId,
                    fecha: datos.fecha,
                    operacion: operacion
                }).then(function () {
                    var activo = document.querySelector(".modulo:not([hidden])");
                    return activo ? cargar(activo.dataset.id) : null;
                });
            }));
        });
    }

    function guardarCenso(nodo) {
        var filtro = nodo._filtro || {};
        var filas = (nodo._filas || []).map(function (fila) {
            return {
                id: fila.id,
                habitacionId: numero(fila.habitacionId),
                cantidad: fila.camasOcupadas,
                turno: fila.turno || filtro.turno || "D"
            };
        });
        mostrarEstado(nodo, "");
        enviar("/Censo/Datos?handler=Guardar", { fecha: filtro.fecha, filas: filas })
            .then(function () { return cargar("censo"); })
            .catch(function (error) { mostrarEstado(nodo, error.message); });
    }

    function mostrarEstado(nodo, mensaje) {
        var estado = nodo.querySelector(".modulo-estado");
        if (!estado) return;
        estado.hidden = !mensaje;
        estado.textContent = mensaje || "";
    }
})();
