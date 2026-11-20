#!/usr/bin/env python3
"""Copia el SQL único de navegación al paquete SSMS."""
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "SGH.Infrastructure" / "Navegacion" / "Persistence" / "001_navegacion_hotldb.sql"
OUT = ROOT / "Docs" / "datos" / "paquetes-ssms" / "Navegacion_ejecutar_HOTELDB.sql"

if __name__ == "__main__":
    if not SRC.is_file():
        raise SystemExit(f"No existe fuente: {SRC}")
    OUT.parent.mkdir(parents=True, exist_ok=True)
    texto = SRC.read_text(encoding="utf-8")
    OUT.write_text(texto, encoding="utf-8")
    print(f"Copiado {SRC.name} -> {OUT} ({len(texto)} bytes)")