#!/usr/bin/env python3
"""
Genera Docs/datos/paquetes-ssms/Habitacion_ejecutar_HOTELDB.sql desde los 7 archivos de Persistence.
Orden: DDL (002, 005, 004 opcional) luego SP (003, 006, 007, 001).
"""
from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PERSIST = ROOT / "SGH.Infrastructure" / "Habitacion" / "Persistence"
OUT = ROOT / "Docs" / "datos" / "paquetes-ssms" / "Habitacion_ejecutar_HOTELDB.sql"

DDL_FILES = (
    "002_hotldb_cu_hab_extension.sql",
    "005_hotldb_tipo_cama_inventario.sql",
    "004_banos_isactive_estado.sql",
)

SP_FILES = (
    "003_sp_bano_cama.sql",
    "006_sp_tipo_cama_inventario.sql",
    "007_sp_habitacion_resolver_nombre.sql",
    "001_sp_habitacion.sql",
)

AUTHOR = "Nestor Carvacho"
DATE = "2026-11-20"

DESCRIPTIONS: dict[str, str] = {
    "he_sp_bano_listar": "Lista baños del campamento y cantidad de habitaciones vinculadas.",
    "he_sp_bano_habitaciones_activas_por_bano": "Habitaciones activas asociadas a un baño en el campamento.",
    "he_sp_bano_guardar": "Alta y edición de baño; aplica RN-01 inactivación y RN-02 compatibilidad de sexo.",
    "he_sp_cama_listar_por_habitacion": "Camas de una habitación con tipo e inventario (CU-HAB-02).",
    "he_sp_cama_guardar": "Alta y edición de cama; actualiza conteos en Habitaciones.",
    "he_sp_habitacion_sincronizar_camas": "Crea o ajusta filas en Camas según CantidadCamas de la habitación.",
    "he_sp_tipo_cama_listar": "Catálogo de tipos de cama por campamento.",
    "he_sp_tipo_cama_guardar": "Alta y edición de tipo de cama.",
    "he_sp_habitacion_catalogo": "Result sets de catálogo para formularios (pabellones, listas, baños, tipos).",
    "he_sp_habitacion_id_por_nombre": "Obtiene Id de habitación por nombre en el campamento (carga masiva).",
    "he_sp_habitacion_listar": "Listado de habitaciones con baño y flags de uso CU-HAB.",
    "he_sp_habitacion_obtener": "Detalle de habitación para edición.",
    "he_sp_habitacion_guardar": "Alta y edición de habitación; legado HAB_* o guardado nativo.",
}

AUDIT_RE = re.compile(
    r"(CREATE OR ALTER PROCEDURE dbo\.(?P<name>he_sp_\w+)\s.*?AS\s*BEGIN)\s*"
    r"(?!(\s*--=+\s*\n\s*--\s+author:))",
    re.DOTALL | re.IGNORECASE,
)

PROC_BEGIN = re.compile(
    r"(CREATE OR ALTER PROCEDURE dbo\.(?P<name>he_sp_\w+)\s.*?AS\s*BEGIN)\s*"
    r"(?P<body>(?:SET NOCOUNT ON;\s*)?)",
    re.DOTALL | re.IGNORECASE,
)


def audit_block(proc_name: str) -> str:
    desc = DESCRIPTIONS.get(
        proc_name,
        f"Procedimiento {proc_name} del módulo habitación.",
    )
    return (
        f"    --======================================================\n"
        f"    --  author: {AUTHOR}\n"
        f"    --  date: {DATE}\n"
        f"    --  description: {desc}\n"
        f"    --======================================================\n\n"
    )


def inject_audit(sql: str) -> str:
    def repl(match: re.Match[str]) -> str:
        body = match.group("body")
        if "--  author:" in body[:400]:
            return match.group(0)
        proc = match.group("name")
        prefix = match.group(1)
        set_nc = "SET NOCOUNT ON;\n\n" if "SET NOCOUNT ON" not in body[:80] else ""
        return f"{prefix}\n{audit_block(proc)}{set_nc}{body.lstrip()}"

    return PROC_BEGIN.sub(repl, sql)


def read_file(name: str) -> str:
    path = PERSIST / name
    return path.read_text(encoding="utf-8")


def write_sources() -> None:
    for name in SP_FILES:
        path = PERSIST / name
        original = path.read_text(encoding="utf-8")
        updated = inject_audit(original)
        if updated != original:
            path.write_text(updated, encoding="utf-8")
            print(f"Actualizado {path.name}")


def main() -> None:
    header = """/*
================================================================================
  MÓDULO: Habitación (CU-HAB) — paquete único para QA / SSMS
  Base:   HOTELDB
  Origen: SGH.Infrastructure/Habitacion/Persistence/*.sql (7 archivos, este orden)
  Uso:    Abrir en SSMS, seleccionar HOTELDB, Ejecutar (F5) una sola vez.
  Instrucciones: Docs/datos/paquetes-ssms/LEEME.md
  Regenerar: python3 tools/empaquetar-habitacion-ssms.py
================================================================================
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

"""

    section1 = "/* ========== SECCIÓN 1 — Tablas, ALTER y datos (orden fijo) ========== */\n\n"
    for name in DDL_FILES:
        block = read_file(name).strip()
        if name.startswith("004"):
            section1 += f"/* --- {name} (opcional: corrección de datos) --- */\n"
        else:
            section1 += f"/* --- {name} --- */\n"
        section1 += block + "\n\nGO\n\n"

    section2 = "/* ========== SECCIÓN 2 — Procedimientos he_sp_* ========== */\n\n"
    for name in SP_FILES:
        block = inject_audit(read_file(name).strip())
        section2 += f"/* --- {name} --- */\n"
        section2 += block + "\n\n"

    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(header + section1 + section2, encoding="utf-8")
    print(f"Escrito {OUT} ({OUT.stat().st_size} bytes)")


if __name__ == "__main__":
    import sys

    if "--write-sources" in sys.argv:
        write_sources()
    main()