# Prompt para implementar temas e idiomas

Este documento es autónomo: no requiere acceso a archivos del proyecto original. Copia el prompt y el ejemplo incluidos a continuación y adáptalos a la tecnología y las convenciones del proyecto de destino.

---

Actúa como desarrollador senior de frontend. Implementa un sistema completo de **tema claro/oscuro** y **selección de idioma** en la aplicación existente. Primero revisa su arquitectura, componentes, estilos, configuración de localización y pruebas. Reutiliza sus mecanismos cuando existan, evita dependencias innecesarias y no cambies reglas de negocio.

## Alcance

- Aplica ambos sistemas de forma coherente en el acceso, la navegación principal y todas las pantallas, formularios, paneles, tablas, menús, diálogos, avisos y páginas legales.
- El cambio de tema y de idioma debe reflejarse inmediatamente, sin recargar la página ni perder el estado del formulario.
- Diseña componentes reutilizables para los selectores y un mecanismo compartido de estilos y traducciones.
- Conserva las preferencias al recargar y navegar entre páginas. Documenta el almacenamiento empleado de acuerdo con las políticas de privacidad del proyecto.

## Tema claro y oscuro

1. Ofrece un interruptor visual tipo *switch* con una **luna** a la izquierda para oscuro y un **sol** a la derecha para claro. El indicador se desplaza para señalar el tema activo.
2. Usa el tema oscuro por defecto si no existe una preferencia válida. Guarda la selección en `localStorage` con la clave `sgh.tema` y valores `oscuro` o `claro`; adapta la clave si el proyecto necesita otro prefijo.
3. Aplica la preferencia antes del primer renderizado para evitar un destello del tema incorrecto. Si el almacenamiento no está disponible, mantén el control funcional durante la sesión y registra el problema sin interrumpir la interfaz.
4. El interruptor debe ser un botón accesible con `role="switch"`, nombre accesible traducible y `aria-checked` sincronizado: `true` para claro y `false` para oscuro. Debe funcionar con ratón, táctil, Enter y espacio, y mostrar foco visible.
5. Respeta `prefers-reduced-motion`; evita animaciones del indicador cuando el usuario prefiere movimiento reducido.
6. Cambia también controles nativos, sus opciones desplegadas y complementos como Select2 o DataTables cuando existan. Evita texto blanco sobre fondo blanco o texto oscuro sobre fondo oscuro.

### Paleta base

Usa estas constantes visuales como punto de partida:

| Nombre | Color | Uso de referencia |
|---|---|---|
| Peach | `#ff9853` | Acento y foco claro |
| Caramel | `#db6f16` | Acento cálido |
| Burgundy | `#651c32` | Títulos y enlaces en claro |
| Eggshell | `#fae0c9` | Fondo cálido claro |
| Blizzard | `#e0dfda` | Bordes claros |
| Gray | `#757578` | Texto secundario claro |
| Deep | `#050503` | Texto principal claro |
| White | `#ffffff` | Superficies claras y texto oscuro |
| Green | `#2f3b23` | Acento verde |
| Teal | `#2ca19c` | Acento turquesa |

### Tema oscuro: estilo glass cálido

- Fondo principal `#0f0d0c` con degradados radiales naranja/caramelo, por ejemplo `rgba(175, 97, 22, 0.6)` y `rgba(217, 133, 50, 0.18)`.
- Superficies de navegación y menús: `rgba(40, 32, 28, 0.94)` a `rgba(40, 32, 28, 0.96)`. Panel lateral: `linear-gradient(145deg, rgba(48, 39, 34, 0.97), rgba(22, 18, 16, 0.97))`.
- Tarjetas de vidrio: `linear-gradient(145deg, rgba(255, 255, 255, 0.14), rgba(255, 255, 255, 0.05))`; bordes blancos con opacidad `0.22`, sombra suave y `backdrop-filter: blur(22px) saturate(160%)`. Proporciona un fondo legible si el navegador no soporta el filtro.
- Texto principal `#ffffff`; texto secundario `rgba(255, 255, 255, 0.72)`. Campos editables con fondo blanco al `0.08–0.10` y bordes blancos al `0.22`.
- Botón primario: `linear-gradient(135deg, #d98532, #8e4e12)`. Acento de títulos/secciones `#d98532`; error de campo `#ff8793`.
- Opciones de select nativo y menú Select2: fondo `#28201c` y texto `#ffffff`. Para los select nativos utiliza `color-scheme: dark` cuando corresponda.

### Tema claro

- Fondo de acceso cálido con `linear-gradient(180deg, #fff7f0, #fae0c9)` y un degradado radial `rgba(255, 152, 83, 0.55)`. Para el área de trabajo usa `#f6f3f0`.
- Superficies `#ffffff`, texto `#050503`, títulos y enlaces `#651c32`, bordes `#e0dfda` y acento `#ff9853`.
- Opciones de select nativo: fondo `#ffffff`, texto `#050503` y `color-scheme: light`.
- Mantén la identidad cálida, los bordes redondeados y las sombras suaves sin sacrificar la legibilidad.

### Interruptor y estados de campos

- Interruptor: pista de `3.8rem × 2rem`, radio de píldora, indicador circular de `1.5rem` e iconos de `1rem`. En oscuro usa pista `#352a23` e indicador `#f9e3ce`; en claro, pista `#efe4d9`, borde `#9c8b82` e indicador `#ffda9d`. Desplazamiento aproximado: `1.7rem`; transición de `0.2s`.
- Distingue los campos editables de los bloqueados y de solo lectura con una etiqueta textual, borde discontinuo y textura, no solo con color.
- En claro, para bloqueados/solo lectura usa rayas alternadas `#e5dfdb` y `#d9d1cc`, texto `#514842` y borde `#aa9d96`. En oscuro usa rayas `#51443c` y `#423831`, texto `#f0dfd1` y borde `#ad9687`.
- Aplica estos estados a `input`, `textarea`, `select` y los componentes visuales de los desplegables. No elimines el valor, la semántica `disabled`/`readonly` ni las reglas de validación.
- Mantén contraste suficiente en todas las combinaciones: como mínimo WCAG AA para texto normal (4.5:1), texto grande (3:1) y componentes o indicadores visuales relevantes (3:1), considerando transparencias y fondos efectivos.

## Idiomas

1. Soporta **español (`es-CL`)**, **inglés (`en-US`)**, **portugués (`pt-BR`)** y **francés (`fr-FR`)**.
2. Si existe una elección manual válida en `localStorage` (`sgh.idioma`), úsala. En caso contrario, elige el primer idioma soportado de las preferencias del navegador; si ninguno coincide, usa español.
3. Añade un selector accesible y reutilizable en el acceso y en la aplicación. Los nombres de idioma deben ser `Español`, `English`, `Português` y `Français`.
4. Actualiza inmediatamente los textos y atributos traducibles: títulos, etiquetas, botones, placeholders, mensajes, avisos, contenido legal y nombres accesibles. Incluye elementos insertados dinámicamente y componentes de terceros que tengan mensajes visibles.
5. Evita traducir valores introducidos por el usuario, nombres propios o datos de negocio procedentes de la base de datos. Conserva valores de controles y estado de la pantalla.
6. Al cambiar de idioma, actualiza el atributo `lang` de HTML y sincroniza la cultura con el servidor cuando corresponda. En ASP.NET Core, configura `RequestLocalization` y usa `.AspNetCore.Culture` con las culturas anteriores; marca la cookie `Secure` en HTTPS y utiliza `SameSite=Lax`.
7. Preferir claves de traducción estables y un sistema de recursos apropiado a la tecnología del proyecto. Si ya existe un traductor basado en DOM, intégralo sin romperlo y evita bucles de observación o traducciones ambiguas.
8. Traduce mensajes con parámetros sin modificar números, fechas, identificadores ni la semántica del mensaje. Usa la cultura activa para formato de fechas y números donde corresponda.

## Validación y entrega

- Verifica que el tema por defecto sea oscuro y que ambos temas persistan tras recargar y navegar entre acceso y aplicación.
- Prueba el interruptor con ratón, teclado y lector de pantalla; confirma que `aria-checked`, iconos y tema activo coincidan.
- Revisa los cuatro idiomas, la detección inicial, el cambio manual y la persistencia; comprueba traducciones de elementos estáticos y dinámicos, así como la vuelta a español.
- Comprueba visualmente y mediante estilos calculados el contraste de campos editables, bloqueados, de solo lectura, select nativos abiertos y componentes de terceros en ambos temas.
- Comprueba diseño móvil, foco visible, movimiento reducido, almacenamiento bloqueado y ausencia de pérdida de datos al cambiar preferencias.
- Agrega o adapta pruebas automatizadas siguiendo las herramientas del proyecto; ejecuta la compilación, el análisis y las pruebas pertinentes. Indica con claridad cualquier verificación que no pueda ejecutarse.
- Entrega un resumen de archivos modificados, decisiones, comandos ejecutados y limitaciones.

---

## Ejemplo autónomo

El siguiente ejemplo contiene HTML, CSS y JavaScript en un solo archivo. Guárdalo como `ejemplo-temas-idiomas.html` y ábrelo en el navegador; no necesita archivos externos ni librerías. Es una **base demostrativa**, no reemplaza la integración completa ni las pruebas solicitadas en el prompt.

```html
<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Temas e idiomas</title>
  <script>
    // Ejecutar antes de pintar la página para evitar un destello del tema.
    try {
      var temaInicial = localStorage.getItem("sgh.tema");
      document.documentElement.dataset.tema =
        temaInicial === "claro" ? "claro" : "oscuro";
    } catch (error) {
      console.warn("No se pudo leer la preferencia de tema.", error);
      document.documentElement.dataset.tema = "oscuro";
    }
  </script>
  <style>
    :root {
      font-family: Inter, system-ui, sans-serif;
      --texto: #fff;
      --texto-secundario: #e8dfd8;
      --borde: rgba(255, 255, 255, .22);
      --superficie: linear-gradient(145deg, rgba(255,255,255,.14), rgba(255,255,255,.05));
      --campo: #352a23;
      --opcion: #28201c;
      --acento: #d98532;
      --bloqueado-1: #51443c;
      --bloqueado-2: #423831;
      --texto-bloqueado: #f0dfd1;
      --borde-bloqueado: #ad9687;
      color-scheme: dark;
    }
    html[data-tema="claro"] {
      --texto: #050503;
      --texto-secundario: #514842;
      --borde: #e0dfda;
      --superficie: #fff;
      --campo: #fff;
      --opcion: #fff;
      --acento: #651c32;
      --bloqueado-1: #e5dfdb;
      --bloqueado-2: #d9d1cc;
      --texto-bloqueado: #514842;
      --borde-bloqueado: #aa9d96;
      color-scheme: light;
    }
    * { box-sizing: border-box; }
    body {
      min-height: 100vh;
      margin: 0;
      display: grid;
      place-items: center;
      padding: 1.5rem;
      background: radial-gradient(900px 650px at 0% 0%, rgba(175,97,22,.6), transparent 62%),
                  radial-gradient(800px 700px at 100% 100%, rgba(175,97,22,.4), transparent 62%),
                  #0f0d0c;
      color: var(--texto);
    }
    html[data-tema="claro"] body {
      background: radial-gradient(ellipse at 50% -10%, rgba(255,152,83,.55), transparent 52%),
                  linear-gradient(180deg, #fff7f0, #fae0c9);
    }
    .tarjeta {
      width: min(100%, 28rem);
      padding: 1.75rem;
      border: 1px solid var(--borde);
      border-radius: 1.5rem;
      background: #28201c;
      background: var(--superficie);
      box-shadow: 0 24px 60px rgba(0,0,0,.25);
      -webkit-backdrop-filter: blur(22px) saturate(160%);
      backdrop-filter: blur(22px) saturate(160%);
    }
    html[data-tema="claro"] .tarjeta { box-shadow: 0 24px 60px rgba(5,5,3,.12); }
    .preferencias { display: flex; justify-content: flex-end; align-items: center; gap: .5rem; }
    .idioma, .campo {
      border: 1px solid var(--borde);
      background: var(--campo);
      color: var(--texto);
      font: inherit;
    }
    .idioma { height: 2rem; padding: 0 .55rem; border-radius: 999px; }
    select option { background: var(--opcion); color: var(--texto); }
    h1 { color: var(--texto); font-size: 1.4rem; }
    p { color: var(--texto-secundario); }
    label { display: block; margin-top: 1rem; font-size: .9rem; font-weight: 600; }
    .campo { display: block; width: 100%; margin-top: .35rem; padding: .65rem; border-radius: .65rem; }
    .campo[readonly], .campo:disabled {
      border: 1px dashed var(--borde-bloqueado);
      background: repeating-linear-gradient(135deg, var(--bloqueado-1), var(--bloqueado-1) 6px,
                                           var(--bloqueado-2) 6px, var(--bloqueado-2) 12px);
      color: var(--texto-bloqueado);
      opacity: 1;
    }
    .estado {
      display: inline-block;
      margin-left: .3rem;
      padding: .1rem .35rem;
      border: 1px dashed currentColor;
      border-radius: .3rem;
      color: var(--texto-secundario);
      font-size: .7rem;
    }
    .interruptor {
      position: relative;
      display: inline-flex;
      align-items: center;
      justify-content: space-around;
      width: 3.8rem;
      height: 2rem;
      padding: 0 .15rem;
      border: 1px solid #ad9687;
      border-radius: 999px;
      background: #352a23;
      color: #e8cdb5;
      cursor: pointer;
    }
    .indicador {
      position: absolute;
      top: .2rem;
      left: .2rem;
      width: 1.5rem;
      height: 1.5rem;
      border-radius: 50%;
      background: #f9e3ce;
      transition: transform .2s ease;
    }
    .icono { position: relative; z-index: 1; width: 1rem; height: 1rem; }
    .luna { color: #492d1b; }
    html[data-tema="claro"] .interruptor { background: #efe4d9; border-color: #9c8b82; }
    html[data-tema="claro"] .indicador { background: #ffda9d; transform: translateX(1.7rem); }
    html[data-tema="claro"] .luna { color: #665348; }
    html[data-tema="claro"] .sol { color: #492d1b; }
    :focus-visible { outline: 2px solid #d98532; outline-offset: 2px; }
    @media (prefers-reduced-motion: reduce) { .indicador { transition: none; } }
  </style>
</head>
<body>
  <main class="tarjeta">
    <div class="preferencias">
      <select class="idioma" data-idioma data-i18n-aria-label="idioma" aria-label="Idioma">
        <option value="es">Español</option>
        <option value="en">English</option>
        <option value="pt">Português</option>
        <option value="fr">Français</option>
      </select>
      <button class="interruptor" data-tema-switch type="button" role="switch"
              aria-checked="false" aria-label="Tema claro" data-i18n-aria-label="temaClaro">
        <span class="indicador" aria-hidden="true"></span>
        <svg class="icono luna" viewBox="0 0 24 24" fill="none" stroke="currentColor"
             stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M20.9 13A9 9 0 0 1 11 3.1 9 9 0 1 0 20.9 13Z"></path>
        </svg>
        <svg class="icono sol" viewBox="0 0 24 24" fill="none" stroke="currentColor"
             stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <circle cx="12" cy="12" r="4"></circle>
          <path d="M12 2v2m0 16v2M4.93 4.93l1.42 1.42m11.3 11.3 1.42 1.42M2 12h2m16 0h2M4.93 19.07l1.42-1.42m11.3-11.3 1.42-1.42"></path>
        </svg>
      </button>
    </div>
    <h1 data-i18n="titulo">Gestión Hotelera</h1>
    <p data-i18n="descripcion">Seleccione su idioma y tema.</p>
    <label for="nombre"><span data-i18n="nombre">Nombre</span></label>
    <input id="nombre" class="campo" data-i18n-placeholder="escribaNombre">
    <label for="numero">
      <span data-i18n="numero">Número</span>
      <span class="estado" data-i18n="soloLectura">Solo lectura</span>
    </label>
    <input id="numero" class="campo" value="123" readonly>
    <label for="perfil"><span data-i18n="perfil">Perfil</span></label>
    <select id="perfil" class="campo">
      <option value="" data-i18n="seleccione">Seleccione</option>
      <option value="operador" data-i18n="operador">Operador</option>
    </select>
  </main>
  <script>
    (function () {
      "use strict";
      const textos = {
        es: { idioma: "Idioma", temaClaro: "Tema claro", titulo: "Gestión Hotelera",
              descripcion: "Seleccione su idioma y tema.", nombre: "Nombre",
              escribaNombre: "Escriba su nombre", numero: "Número",
              soloLectura: "Solo lectura", perfil: "Perfil", seleccione: "Seleccione",
              operador: "Operador" },
        en: { idioma: "Language", temaClaro: "Light theme", titulo: "Hotel Management",
              descripcion: "Select your language and theme.", nombre: "Name",
              escribaNombre: "Enter your name", numero: "Number",
              soloLectura: "Read-only", perfil: "Profile", seleccione: "Select",
              operador: "Operator" },
        pt: { idioma: "Idioma", temaClaro: "Tema claro", titulo: "Gestão Hoteleira",
              descripcion: "Selecione seu idioma e tema.", nombre: "Nome",
              escribaNombre: "Digite seu nome", numero: "Número",
              soloLectura: "Somente leitura", perfil: "Perfil", seleccione: "Selecione",
              operador: "Operador" },
        fr: { idioma: "Langue", temaClaro: "Thème clair", titulo: "Gestion Hôtelière",
              descripcion: "Sélectionnez votre langue et votre thème.", nombre: "Nom",
              escribaNombre: "Saisissez votre nom", numero: "Numéro",
              soloLectura: "Lecture seule", perfil: "Profil", seleccione: "Sélectionnez",
              operador: "Opérateur" }
      };
      const culturas = { es: "es-CL", en: "en-US", pt: "pt-BR", fr: "fr-FR" };
      const selector = document.querySelector("[data-idioma]");
      const interruptor = document.querySelector("[data-tema-switch]");

      function leer(clave) {
        try { return localStorage.getItem(clave); }
        catch (error) { console.warn("No se pudo leer la preferencia.", error); return null; }
      }
      function guardar(clave, valor) {
        try { localStorage.setItem(clave, valor); }
        catch (error) { console.warn("No se pudo guardar la preferencia.", error); }
      }
      function idiomaInicial() {
        const guardado = leer("sgh.idioma");
        if (Object.hasOwn(textos, guardado)) return guardado;
        const candidatos = navigator.languages && navigator.languages.length
          ? navigator.languages : [navigator.language || "es"];
        return candidatos.map(valor => valor.slice(0, 2).toLowerCase())
          .find(valor => Object.hasOwn(textos, valor)) || "es";
      }
      let idioma = idiomaInicial();
      function traducir() {
        document.documentElement.lang = culturas[idioma];
        document.querySelectorAll("[data-i18n]").forEach(elemento => {
          const valor = textos[idioma][elemento.dataset.i18n];
          if (valor !== undefined) elemento.textContent = valor;
        });
        document.querySelectorAll("[data-i18n-placeholder]").forEach(elemento => {
          const valor = textos[idioma][elemento.dataset.i18nPlaceholder];
          if (valor !== undefined) elemento.placeholder = valor;
        });
        document.querySelectorAll("[data-i18n-aria-label]").forEach(elemento => {
          const valor = textos[idioma][elemento.dataset.i18nAriaLabel];
          if (valor !== undefined) elemento.setAttribute("aria-label", valor);
        });
        selector.value = idioma;
      }
      function actualizarInterruptor() {
        interruptor.setAttribute("aria-checked",
          document.documentElement.dataset.tema === "claro" ? "true" : "false");
      }
      selector.addEventListener("change", function () {
        idioma = selector.value;
        guardar("sgh.idioma", idioma);
        traducir();
      });
      interruptor.addEventListener("click", function () {
        const tema = document.documentElement.dataset.tema === "claro" ? "oscuro" : "claro";
        document.documentElement.dataset.tema = tema;
        guardar("sgh.tema", tema);
        actualizarInterruptor();
      });
      traducir();
      actualizarInterruptor();
    })();
  </script>
</body>
</html>
```

### Integración posterior

- El diccionario del ejemplo solo cubre los textos de demostración. Amplíalo con claves y traducciones para todas las pantallas, o conecta los recursos de localización existentes.
- Para contenido dinámico, invoca la traducción al montar componentes nuevos o usa el mecanismo del framework; el ejemplo no observa cambios del DOM automáticamente.
- El ejemplo funciona únicamente en el navegador y no sincroniza la cultura con el servidor. Si el destino usa ASP.NET Core, configura las culturas y el middleware de localización, y sincroniza la cookie `.AspNetCore.Culture` según el flujo y la política del proyecto.
- La apariencia de los menús nativos depende del navegador y sistema operativo. Comprueba los desplegables abiertos en las plataformas objetivo y valida los contrastes efectivos antes de publicar.
