#!/usr/bin/env bash
# Copia los .sql de SGH.Infrastructure a /scripts-ejecutar/ en la raíz (carpeta gitignored).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DEST="$ROOT/scripts-ejecutar"
INFRA="$ROOT/SGH.Infrastructure"
mkdir -p "$DEST"

rm -f "$DEST"/*.sql 2>/dev/null || true

while IFS= read -r -d '' archivo; do
  rel="${archivo#$INFRA/}"
  modulo="${rel%%/*}"
  base="$(basename "$archivo")"
  case "$base" in
    *borrador*) continue ;;
  esac
  destino="$DEST/${modulo}_${base}"
  cp -a "$archivo" "$destino"
done < <(find "$INFRA" -path '*/Persistence/*.sql' -type f -print0)

MIGR="$ROOT/Docs/datos/migracion"
if [[ -d "$MIGR" ]]; then
  while IFS= read -r -d '' archivo; do
    base="$(basename "$archivo")"
    cp -a "$archivo" "$DEST/Migracion_${base}"
  done < <(find "$MIGR" -maxdepth 1 -name '*.sql' -type f -print0)
fi

cat >"$DEST/LEEME.md" <<'EOF'
# Scripts HOTELDB — copia local (no versionada)

Carpeta en la **raíz del repo** (`/scripts-ejecutar/`). Está en `.gitignore`; solo vive en tu disco.

Orden y dependencias: `Docs/datos/hoteldb-scripts-orden-ejecucion.md`.

Regenerar copias desde el repo:

```bash
./tools/sincronizar-scripts-ejecutar.sh
```

Los archivos se nombran `{Modulo}_{nombre-original}.sql` para evitar colisiones entre varios `001_*.sql`.

Parche listado por fila (escritorio): re-ejecutar en HOTELDB:

- `Habitacion_001_habitacion_hotldb.sql`
EOF

echo "Copiados $(find "$DEST" -maxdepth 1 -name '*.sql' | wc -l) scripts en $DEST"