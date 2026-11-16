/**
 * SGH 1.7 — API cliente de la biblioteca UI (plantillas Razor + data-sgh-*).
 */

export function clonarModuloShell(id) {
    var plantilla = document.getElementById("sgh-ui-plantilla-modulo");
    if (!plantilla) return null;
    var fragmento = plantilla.content.cloneNode(true);
    var nodo = fragmento.querySelector("[data-sgh-modulo-shell]");
    if (!nodo) return null;
    nodo.classList.add("modulo-" + id);
    nodo.dataset.id = id;
    nodo.removeAttribute("data-sgh-modulo-shell");
    return nodo;
}

export function crearBibliotecaUi() {
  var modalActivo = null;
  var resolverModal = null;

  function modalPorDefecto() {
    return document.querySelector("[data-sgh-modal]");
  }

  function cerrarModal(modal) {
    if (!modal) return;
    modal.hidden = true;
    if (resolverModal) {
      resolverModal(false);
      resolverModal = null;
    }
    modalActivo = null;
  }

  function abrirModal(modal, mensaje, titulo) {
    var cuerpo = modal.querySelector("[data-sgh-modal-mensaje]");
    var cabecera = modal.querySelector(".sgh-ui-modal-cabeza h2");
    if (cuerpo) cuerpo.textContent = mensaje;
    if (titulo && cabecera) cabecera.textContent = titulo;
    modal.hidden = false;
    modalActivo = modal;
    var aceptar = modal.querySelector("[data-sgh-modal-aceptar]");
    if (aceptar) aceptar.focus();
  }

  function enlazarModal(modal) {
    if (!modal || modal.dataset.sghModalEnlazado === "1") return;
    modal.dataset.sghModalEnlazado = "1";
    modal.querySelectorAll("[data-sgh-modal-cerrar], [data-sgh-modal-cancelar]").forEach(function (boton) {
      boton.addEventListener("click", function () { cerrarModal(modal); });
    });
    modal.querySelector("[data-sgh-modal-aceptar]")?.addEventListener("click", function () {
      if (resolverModal) resolverModal(true);
      resolverModal = null;
      modal.hidden = true;
      modalActivo = null;
    });
    modal.addEventListener("click", function (evento) {
      if (evento.target === modal) cerrarModal(modal);
    });
  }

  function confirmar(mensaje, opciones) {
    opciones = opciones || {};
    var modal = opciones.modalId
      ? document.getElementById(opciones.modalId)
      : modalPorDefecto();
    if (!modal) return Promise.resolve(window.confirm(mensaje));
    enlazarModal(modal);
    return new Promise(function (resolver) {
      resolverModal = resolver;
      abrirModal(modal, mensaje, opciones.titulo || modal.querySelector("h2")?.textContent);
    });
  }

  function marcarCampo(control, mensaje) {
    if (!control) return;
    var envoltorio = control.closest(".sgh-ui-campo, .campo");
    if (envoltorio) envoltorio.classList.add("campo-error");
    control.setAttribute("aria-invalid", "true");
    var aviso = envoltorio?.querySelector("[data-sgh-validacion]");
    if (aviso) {
      aviso.textContent = mensaje || "Complete este campo.";
      aviso.hidden = false;
    }
  }

  function limpiarCampo(control) {
    if (!control) return;
    var envoltorio = control.closest(".sgh-ui-campo, .campo");
    if (envoltorio) envoltorio.classList.remove("campo-error");
    control.setAttribute("aria-invalid", "false");
    var aviso = envoltorio?.querySelector("[data-sgh-validacion]");
    if (aviso) {
      aviso.textContent = "";
      aviso.hidden = true;
    }
  }

  function validarFormulario(form) {
    var valido = true;
    form.querySelectorAll("[data-sgh-campo]").forEach(function (control) {
      var requerido = control.required || control.getAttribute("required") !== null;
      var completo = String(control.value || "").trim() !== "";
      if (requerido && !completo) {
        marcarCampo(control, "Este campo es obligatorio.");
        valido = false;
      } else {
        limpiarCampo(control);
      }
    });
    return valido;
  }

  function inicializarModales() {
    document.querySelectorAll("[data-sgh-modal]").forEach(enlazarModal);
  }

  return {
    clonarModuloShell: clonarModuloShell,
    confirmar: confirmar,
    marcarCampo: marcarCampo,
    limpiarCampo: limpiarCampo,
    validarFormulario: validarFormulario,
    inicializarModales: inicializarModales,
    cerrarModalActivo: function () { if (modalActivo) cerrarModal(modalActivo); }
  };
}