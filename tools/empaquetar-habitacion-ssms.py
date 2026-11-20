#!/usr/bin/env python3
"""Copia el SQL único de habitación al paquete SSMS (mismo contenido)."""
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "SGH.Infrastructure" / "Habitacion" / "Persistence" / "001_habitacion_hotldb.sql"
OUT = ROOT / "Docs" / "datos" / "paquetes-ssms" / "Habitacion_ejecutar_HOTELDB.sql"

if __name__ == "__main__":
    if not SRC.is_file():
        raise SystemExit(f"No existe fuente: {SRC}")
    OUT.parent.mkdir(parents=True, exist_ok=True)
    texto = SRC.read_text(encoding="utf-8")
    OUT.write_text(texto, encoding="utf-8")
    print(f"Copiado {SRC.name} -> {OUT} ({len(texto)} bytes)")