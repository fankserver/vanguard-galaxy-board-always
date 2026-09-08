# Board Always (VGBoardAlways)

A BepInEx plugin for [Vanguard Galaxy](https://store.steampowered.com/app/3471800/) that removes the RNG gate from ship boarding and adds configurable difficulty controls for dungeon encounters.

- **Always boardable** — enemy ships below 40% HP become boardable on every damage tick instead of relying on the vanilla RNG roll. The 15% damage accumulation gate is bypassed entirely.
- **EMP synergy** — boarding chance scales with EMP charge on the target, making EMP weapons a strategic choice for capture-focused builds.
- **Difficulty modifier** — scales defender combat power and HP globally. Set below 1.0 to make high-level dungeons easier, or above 1.0 for a tougher challenge.
- **Integrity damage control** — configurable multiplier for hull integrity loss during boarding. Set to 0.0 to prevent integrity loss entirely, or a small value (e.g. 0.05) for very gradual degradation.
- **Scuttling protection** — integrity damage from emergency scuttling is also scaled by the multiplier so your ship doesn't take unintended hull damage when boarding goes wrong.

## Install

1. **Install BepInEx 5.x** — grab `BepInEx_win_x64_5.4.x.zip` from the [BepInEx releases](https://github.com/BepInEx/BepInEx/releases) and unzip it into your Vanguard Galaxy install folder (next to `VanguardGalaxy.exe`).
2. **Launch the game once** so BepInEx creates its `BepInEx/plugins/` and `BepInEx/config/` subfolders, then close the game.
3. **Download the VGBoardAlways release** zip from [Releases](https://github.com/fank/vanguard-galaxy-board-always/releases).
4. **Unzip** into `BepInEx/plugins/`:
   ```
   VanguardGalaxy/BepInEx/plugins/
     VGBoardAlways.dll
   ```
5. **Launch the game.** Open the BepInEx console — you should see a load line ending with:
   ```
   [Info :Board Always] Board Always v0.2.0 loaded (N patches)
   ```

## Configuration

Edit `BepInEx/config/vg.boardalways.cfg` after first launch:

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `true` | When enabled, ships below 40% HP become boardable without RNG. |
| `DifficultyModifier` | `1.0` | Global multiplier for dungeon enemy difficulty. Scales defender combat power and HP. `0.5` = half difficulty, `2.0` = double. |
| `IntegrityDamageMultiplier` | `1.0` | Multiplier for hull integrity damage during boarding. `0.0` = no integrity loss, `0.05` = very slow degradation. |

## Uninstall

Delete `BepInEx/plugins/VGBoardAlways.dll` and `BepInEx/config/vg.boardalways.cfg` (optional). No per-save state.

## Build

The game's `Assembly-CSharp.dll` is referenced from the game install at build time via a symlink (`make link-asm`). BepInEx, HarmonyX, and Unity engine modules come from NuGet.

```bash
# Build the DLL
make build

# Build + copy into the game's BepInEx/plugins/ folder (WSL/Steam path; edit Makefile if yours differs)
make deploy

# Clean build artifacts
make clean
```

## License

MIT
