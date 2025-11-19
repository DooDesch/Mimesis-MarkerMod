# MIMESIS - MarkerMod

Use paintballs as markers in dungeons.

Paintballs create puddles and leave footprints when you walk through them. Without this mod, these effects are useless as they disappear too quickly. MarkerMod makes them into practical markers you can use in dungeons.

![Version](https://img.shields.io/badge/version-1.3.1-blue)
![Game](https://img.shields.io/badge/game-MIMESIS-purple)
![MelonLoader](https://img.shields.io/badge/MelonLoader-0.7.1+-green)
![Status](https://img.shields.io/badge/status-working-brightgreen)

## Features

- Puddles stay permanently (optional)
- Footprints remain visible (optional)
- Configurable lifetime for both effects
- Perfect for navigation in complex dungeons

## Installation

1. Install the mod via Thunderstore Mod Manager or manually
2. Launch the game once to generate the configuration file
3. Adjust settings in `UserData/MelonPreferences.cfg` as needed

## Configuration

The mod adds a `MarkerMod` section to your configuration file:

- `keepFootprints`: Enable permanent footprints (default: `true`)
- `keepPuddles`: Enable permanent puddles (default: `false`)
- `permanentLifetimeSeconds`: Lifetime of effects in seconds (default: `86400`)
- `infinitePaintballs`: Paintballs are not consumed when thrown (default: `false`)
