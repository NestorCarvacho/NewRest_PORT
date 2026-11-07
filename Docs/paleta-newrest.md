# Paleta Newrest

Colores de marca de [newrest.eu/es](https://www.newrest.eu/es/), tomados de las variables CSS del tema (`--color-*`).

## Colores de marca

| Token | Hex | RGB | Uso en la home |
|---|---|---|---|
| peach | `#FF9853` | 255, 152, 83 | Color principal. Hero, cabecera y texto de acento. |
| caramel | `#DB6F16` | 219, 111, 22 | Naranja más oscuro. El pie va en degradado de peach a caramel. |
| burgundy | `#651C32` | 101, 28, 50 | Granate. Títulos y bordes. |
| eggshell | `#FAE0C9` | 250, 224, 201 | Fondo cálido claro. |
| blizzard | `#E0DFDA` | 224, 223, 218 | Gris cálido de fondos suaves (a veces al 40 %). |
| gray | `#757578` | 117, 117, 120 | Texto secundario. |
| deep | `#050503` | 5, 5, 3 | Casi negro. Texto y velos (al 30 %). |
| white | `#FFFFFF` | 255, 255, 255 | Fondos y texto sobre naranja. |
| black | `#000000` | 0, 0, 0 | Texto y sombras. |

## Tokens definidos con poco uso en la portada

| Token | Hex | RGB |
|---|---|---|
| rse-green | `#2F3B23` | 47, 59, 35 |
| rse-green-2 | `#96AB6A` | 150, 171, 106 |
| services | `#2CA19C` | 44, 161, 156 |
| jaune | `#FDF298` | 253, 242, 152 |
| yellow | `#FFFF00` | 255, 255, 0 |

## Degradado del pie

El pie usa un degradado vertical de peach a caramel:

```css
background: linear-gradient(to bottom, #FF9853 50%, #DB6F16);
```

## Variables CSS

```css
--color-peach: #ff9853;
--color-caramel: #db6f16;
--color-burgundy: #651c32;
--color-eggshell: #fae0c9;
--color-blizzard: #e0dfda;
--color-gray: #757578;
--color-deep: #050503;
--color-white: #fff;
--color-black: #000;
--color-rse-green: #2f3b23;
--color-rse-green-2: #96ab6a;
--color-services: #2ca19c;
--color-jaune: #fdf298;
--color-yellow: #ff0;
```
