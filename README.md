# Lost Adventurer Mod

Fixes a vanilla bug where the **Lost Adventurer never appears after four floor helped**, making it impossible to complete his questline.

## The Bug

In the base game, the Lost Adventurer's `chanceToSpawn` is set to `0` for the last floor that need help, meaning he can never spawn there even when all conditions are met (helped on 4 previous floors). This mod patches that value at runtime so the encounter can actually happen.

## Features

- **Bug fix**: Forces the Lost Adventurer to spawn on the last floor when you've helped him on the others 4 floors
- **In-game config** via the MOD CONFIG menu (requires Gunfig):
  - Toggle the HUD overlay on/off
  - See which floors you've already helped him on (✓/✗)
  - See your current floor and progress
  - Force the Lost Adventurer to appear on any specific floor
  - View real-time patch and blueprint debug logs

## How to Use

1. Help the Lost Adventurer on floors as normal
2. In the **MOD CONFIG** menu, set **Forzar en piso** to **Forge**
3. Enter the Forge — the Lost Adventurer will appear

## Requirements

- [Mod the Gungeon API](https://thunderstore.io/c/enter-the-gungeon/p/MtG_API/Mod_the_Gungeon_API/)
- [Gunfig](https://thunderstore.io/c/enter-the-gungeon/p/CaptainPretzel/Gunfig/)

## Installation

Use the Thunderstore Mod Manager or extract the `BepInEx` folder into your Enter the Gungeon directory.

## More info

Hablo español, y no he probado como cambiar los idiomas y cosas por el estilo, cualquier aporte se agradece.
I speak Spanish, and I haven't tried changing languages and things like that, so any input is appreciated.
