#!/usr/bin/env python3
"""
Genera 001_solicitud_hotldb.sql y el paquete SSMS desde los fragmentos legacy (si existen)
o copia Persistence -> paquete cuando solo hay el archivo único.
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PERSIST = ROOT / "SGH.Infrastructure" / "Solicitud" / "Persistence"
SRC = PERSIST / "001_solicitud_hotldb.sql"
OUT = ROOT / "Docs" / "datos" / "paquetes-ssms" / "Solicitud_ejecutar_HOTELDB.sql"

LEGACY_ORDER = (
    "001_sp_solicitud.sql",
    "002_sp_pasajeros_solicitud.sql",
    "003_sp_guardar_pasajero.sql",
    "005_sp_habitaciones_solicitud.sql",
    "004_sp_generar_reservas.sql",
    "007_sp_solicitud_asegurar_periodo.sql",
    "008_sp_reserva_finalizar_checkout.sql",
    "006_sp_cargar_pasajero.sql",
)

AUTHOR = "Nestor Carvacho"
DATE = "2026-11-20"

DESCRIPTIONS: dict[str, str] = {
    "he_sp_solicitud_buscar": "Listado de solicitudes con rango 30 días y TOP @MaxFilas por defecto.",
    "he_sp_solicitud_obtener_primera_reserva": "Id de la primera reserva generada para una solicitud.",
    "he_sp_solicitud_listar_catalogo": "Catálogos para filtros y formulario de solicitud.",
    "he_sp_solicitud_listar_organizacion": "Áreas y solicitantes para el campamento.",
    "he_sp_solicitud_guardar": "Alta y edición de solicitud de alojamiento.",
    "he_sp_solicitud_anular": "Anula solicitud en estado permitido.",
    "he_sp_solicitud_listar_pasajeros": "Pasajeros vinculados a la solicitud.",
    "he_sp_solicitud_agregar_cupos": "Aumenta cupos de pasajeros en la solicitud.",
    "he_sp_solicitud_eliminar_pasajero": "Quita pasajero de la solicitud.",
    "he_sp_solicitud_guardar_pasajero": "Alta o edición de pasajero en detalle.",
    "he_sp_solicitud_habitaciones_disponibles": "Habitaciones disponibles para asignar a solicitud.",
    "he_sp_solicitud_asignar_habitacion": "Asigna habitación a solicitud/reserva en preparación.",
    "he_sp_solicitud_desasignar_habitacion": "Quita asignación de habitación.",
    "he_sp_solicitud_generar_reservas": "Genera reservas desde solicitud (legado SOL/RES).",
    "he_sp_solicitud_asegurar_periodo": "Alinea fechas de solicitud para importación masiva.",
    "he_sp_reserva_finalizar_checkout": "Checkout y cierre de reserva tras importación u operación O.",
    "he_sp_solicitud_cargar_pasajero": "Carga masiva de pasajeros (SOL_GuardaPasajero masivo).",
}

PROC_BEGIN = re.compile(
    r"(CREATE OR ALTER PROCEDURE dbo\.(?P<name>he_sp_\w+)\s.*?AS\s*BEGIN)\s*"
    r"(?P<body>(?:SET NOCOUNT ON;\s*)?)",
    re.DOTALL | re.IGNORECASE,
)


def audit_block(proc_name: str) -> str:
    desc = DESCRIPTIONS.get(proc_name, f"Procedimiento {proc_name} del módulo solicitud.")
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
        if "--  author:" in body[:500]:
            return match.group(0)
        proc = match.group("name")
        prefix = match.group(1)
        set_nc = ""
        if "SET NOCOUNT ON" not in body[:120]:
            set_nc = "    SET NOCOUNT ON;\n\n"
        elif not body.lstrip().startswith("    SET NOCOUNT"):
            set_nc = "    SET NOCOUNT ON;\n\n"
            body = re.sub(r"^\s*SET NOCOUNT ON;\s*", "", body, count=1, flags=re.IGNORECASE)
        return f"{prefix}\n{audit_block(proc)}{set_nc}{body.lstrip()}"

    return PROC_BEGIN.sub(repl, sql)


def strip_sql(text: str) -> str:
    sys.path.insert(0, str(Path(__file__).resolve().parent))
    from strip_sql_comments import strip_sql as _strip

    return _strip(text)


def append_batch(sql: str) -> str:
    sql = strip_sql(sql.strip())
    if not sql.upper().endswith("GO"):
        sql += "\nGO"
    return sql + "\n\n"


def build_from_legacy() -> str:
    header = "SET ANSI_NULLS ON;\nSET QUOTED_IDENTIFIER ON;\nGO\n\n"
    body = ""
    for name in LEGACY_ORDER:
        path = PERSIST / name
        if not path.is_file():
            raise FileNotFoundError(path)
        body += append_batch(path.read_text(encoding="utf-8"))
    merged = inject_audit(body)
    merged = re.sub(r"\nGO\s*\n(?:\s*\n)*GO\s*\n", "\nGO\n", merged, flags=re.IGNORECASE)
    return strip_sql(header + merged)


def publish(texto: str) -> None:
    PERSIST.mkdir(parents=True, exist_ok=True)
    SRC.write_text(texto, encoding="utf-8")
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(texto, encoding="utf-8")


if __name__ == "__main__":
    legacy_any = any((PERSIST / n).is_file() for n in LEGACY_ORDER)
    if legacy_any:
        publish(build_from_legacy())
        print(f"Consolidado -> {SRC} ({SRC.stat().st_size} bytes)")
    elif SRC.is_file():
        OUT.write_text(SRC.read_text(encoding="utf-8"), encoding="utf-8")
        print(f"Copiado {SRC.name} -> {OUT}")
    else:
        raise SystemExit("Sin 001_solicitud_hotldb.sql ni fragmentos legacy.")