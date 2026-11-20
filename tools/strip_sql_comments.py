#!/usr/bin/env python3
"""Quita comentarios SQL salvo bloques de auditoría (author / date / description)."""
from __future__ import annotations

import re
import sys
from pathlib import Path

AUDIT_MARKER = re.compile(r"^\s*--=+\s*$")
AUDIT_FIELD = re.compile(r"^\s*--\s+(author|date|description)\s*:", re.IGNORECASE)


def remove_block_comments(text: str) -> str:
    out: list[str] = []
    i = 0
    n = len(text)
    while i < n:
        if text[i : i + 2] == "/*":
            j = text.find("*/", i + 2)
            if j == -1:
                break
            i = j + 2
            continue
        out.append(text[i])
        i += 1
    return "".join(out)


def strip_trailing_line_comment(line: str) -> str:
    in_str = False
    quote = ""
    for i, ch in enumerate(line):
        if ch in ("'", '"') and (i == 0 or line[i - 1] != "\\"):
            if not in_str:
                in_str = True
                quote = ch
            elif ch == quote:
                in_str = False
                quote = ""
            continue
        if not in_str and ch == "-" and i + 1 < len(line) and line[i + 1] == "-":
            return line[:i].rstrip()
    return line


def strip_sql(text: str) -> str:
    text = remove_block_comments(text)
    lines_out: list[str] = []
    in_audit = False

    for raw in text.splitlines():
        line = raw.rstrip()
        if not line.strip():
            if in_audit:
                lines_out.append(line)
                continue
            if lines_out and lines_out[-1] == "":
                continue
            lines_out.append("")
            continue

        if AUDIT_MARKER.match(line):
            lines_out.append(line)
            in_audit = not in_audit
            continue

        if in_audit:
            lines_out.append(line)
            continue

        cleaned = strip_trailing_line_comment(line)
        stripped = cleaned.strip()
        if not stripped:
            continue
        if stripped.startswith("--"):
            continue
        lines_out.append(cleaned.rstrip())

    while lines_out and lines_out[-1] == "":
        lines_out.pop()
    return "\n".join(lines_out) + ("\n" if lines_out else "")


def process_file(path: Path) -> bool:
    original = path.read_text(encoding="utf-8")
    updated = strip_sql(original)
    if updated != original:
        path.write_text(updated, encoding="utf-8")
        return True
    return False


def main(paths: list[str]) -> None:
    for p in paths:
        path = Path(p)
        if not path.is_file():
            print(f"omitido (no existe): {path}", file=sys.stderr)
            continue
        if process_file(path):
            print(f"limpiado {path}")
        else:
            print(f"sin cambios {path}")


if __name__ == "__main__":
    main(sys.argv[1:])