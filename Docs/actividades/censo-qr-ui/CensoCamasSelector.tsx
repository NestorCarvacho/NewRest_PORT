/**
 * Referencia de migración: React 18 + TypeScript + MUI v5.
 * Misma lógica que ReglasCenso / censo-camas-logica.js (HostEat NewRest).
 *
 * Uso:
 *   <CensoCamasSelector capacidadTotal={2} turno="A" onChange={...} />
 */
import * as React from "react";
import {
    FormControl,
    FormLabel,
    RadioGroup,
    FormControlLabel,
    Radio,
    Stack,
    Typography,
} from "@mui/material";

const MAX_CAMAS = 4;
export type TurnoCenso = "D" | "N" | "A";

function capacidadEfectiva(capacidadTotal: number): number {
    return Math.max(0, Math.min(MAX_CAMAS, capacidadTotal));
}

function radioDiaDeshabilitado(valor: number, capacidadTotal: number): boolean {
    return valor > capacidadEfectiva(capacidadTotal);
}

function radioNocheDeshabilitado(valor: number, capacidadTotal: number, camasDia: number): boolean {
    return valor > Math.max(0, capacidadEfectiva(capacidadTotal) - camasDia);
}

function ajustarCamasNoche(camasNoche: number, capacidadTotal: number, camasDia: number): number {
    const maxNoche = Math.max(0, capacidadEfectiva(capacidadTotal) - camasDia);
    return Math.min(camasNoche, maxNoche);
}

export function validarOcupacionCamas(
    capacidadTotal: number,
    turno: TurnoCenso,
    camasDia: number,
    camasNoche: number
): string | null {
    const cap = capacidadEfectiva(capacidadTotal);
    if (turno === "D") {
        return camasDia >= 0 && camasDia <= cap ? null : `Las camas de día deben estar entre 0 y ${cap}.`;
    }
    if (turno === "N") {
        return camasNoche >= 0 && camasNoche <= cap ? null : `Las camas de noche deben estar entre 0 y ${cap}.`;
    }
    if (turno === "A") {
        if (radioDiaDeshabilitado(camasDia, capacidadTotal)) {
            return `Las camas de día deben estar entre 0 y ${cap}.`;
        }
        if (radioNocheDeshabilitado(camasNoche, capacidadTotal, camasDia) || camasDia + camasNoche > cap) {
            return "La suma de camas día y noche no puede superar la capacidad de la habitación.";
        }
        return null;
    }
    return "Seleccione un turno válido.";
}

const OPCIONES = [0, 1, 2, 3, 4] as const;

export type CensoCamasSelectorProps = {
    capacidadTotal: number;
    turno: TurnoCenso;
    camasDia: number;
    camasNoche: number;
    onChange: (estado: { camasDia: number; camasNoche: number }) => void;
    /** Metadatos opcionales del paso Censo (wizard legacy) */
    detalleHabitacion?: { fecha?: string; modulo?: string; piso?: string; habitacion?: string };
};

function GrupoCamas(props: {
    etiqueta: string;
    nombre: string;
    valor: number;
    deshabilitar: (i: number) => boolean;
    onSeleccion: (valor: number) => void;
}) {
    return (
        <FormControl component="fieldset" sx={{ mb: 2 }}>
            <FormLabel component="legend">{props.etiqueta}</FormLabel>
            <RadioGroup
                row
                name={props.nombre}
                value={String(props.valor)}
                onChange={(_, v) => props.onSeleccion(Number(v))}
            >
                {OPCIONES.map((i) => (
                    <FormControlLabel
                        key={i}
                        value={String(i)}
                        control={<Radio />}
                        label={String(i)}
                        disabled={props.deshabilitar(i)}
                    />
                ))}
            </RadioGroup>
        </FormControl>
    );
}

export function CensoCamasSelector({
    capacidadTotal,
    turno,
    camasDia,
    camasNoche,
    onChange,
    detalleHabitacion,
}: CensoCamasSelectorProps) {
    const cap = capacidadEfectiva(capacidadTotal);

    const setDia = (valor: number) => {
        if (turno === "A") {
            const nocheAjustada = ajustarCamasNoche(camasNoche, capacidadTotal, valor);
            onChange({ camasDia: valor, camasNoche: nocheAjustada });
        } else {
            onChange({ camasDia: valor, camasNoche });
        }
    };

    const setNoche = (valor: number) => onChange({ camasDia, camasNoche: valor });

    const error = validarOcupacionCamas(capacidadTotal, turno, camasDia, camasNoche);

    return (
        <Stack spacing={1}>
            {detalleHabitacion ? (
                <Typography variant="body2" color="text.secondary">
                    {[detalleHabitacion.fecha, detalleHabitacion.modulo, detalleHabitacion.piso, detalleHabitacion.habitacion]
                        .filter(Boolean)
                        .join(" · ")}
                </Typography>
            ) : null}

            {turno === "D" ? (
                <GrupoCamas
                    etiqueta="Camas ocupadas (día)"
                    nombre="camas-dia"
                    valor={camasDia}
                    deshabilitar={(i) => radioDiaDeshabilitado(i, capacidadTotal)}
                    onSeleccion={setDia}
                />
            ) : null}

            {turno === "N" ? (
                <GrupoCamas
                    etiqueta="Camas ocupadas (noche)"
                    nombre="camas-noche"
                    valor={camasNoche}
                    deshabilitar={(i) => radioDiaDeshabilitado(i, capacidadTotal)}
                    onSeleccion={setNoche}
                />
            ) : null}

            {turno === "A" ? (
                <>
                    <GrupoCamas
                        etiqueta="Camas ocupadas (día)"
                        nombre="camas-dia"
                        valor={camasDia}
                        deshabilitar={(i) => radioDiaDeshabilitado(i, capacidadTotal)}
                        onSeleccion={setDia}
                    />
                    <GrupoCamas
                        etiqueta="Camas ocupadas (noche)"
                        nombre="camas-noche"
                        valor={camasNoche}
                        deshabilitar={(i) => radioNocheDeshabilitado(i, capacidadTotal, camasDia)}
                        onSeleccion={setNoche}
                    />
                    <Typography variant="caption" color="text.secondary">
                        Capacidad: {cap}. Restante noche: {Math.max(0, cap - camasDia)}.
                    </Typography>
                </>
            ) : null}

            {error ? (
                <Typography variant="body2" color="error">
                    {error}
                </Typography>
            ) : null}
        </Stack>
    );
}

/** Ejemplo de botón guardar del wizard */
export function puedeGuardarCenso(
    capacidadTotal: number,
    turno: TurnoCenso,
    camasDia: number,
    camasNoche: number
): boolean {
    return validarOcupacionCamas(capacidadTotal, turno, camasDia, camasNoche) === null;
}