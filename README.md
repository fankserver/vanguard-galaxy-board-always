# Board Always

A BepInEx 5 plugin using Mod API's public boarding rules. Requires Mod API **0.2.7 or newer** and an inspected game build; boarding initializes automatically, with no enable switch. Uses `ModApi.Services.BoardingRules` / `IBoardingRuleService`. Policies register independently of current health; the API gates their evaluation while unavailable. Unavailable integration logs a warning; there is no native patch fallback. Compiled consumers must match the API they were built against.

## Policy and configuration

Plugin identity `vg.boardalways` and `[General]` keys in `BepInEx/config/vg.boardalways.cfg` are retained. No consumer save data or migration serializer is needed.

| Key | Default | Behavior |
|---|---|---|
| `Enabled` | true | Gates every policy, including difficulty. Disabling removes this consumer's adjustments, not another mod's rules or already-saved creation tuning. |
| `DifficultyModifier` | 1 | Ship defender power and initial health, from 0 to 10. Values above 1 increase difficulty. Applied once at creation and in estimates; saved encounters retain their values. |
| `IntegrityDamageMultiplier` | 1 | Ship boarding integrity damage, from 0 to 10. Scuttle damage passes through the API accounting boundary once. Authoritative host destruction cannot be prevented. |

While enabled, structurally eligible ships strictly below 40% hull receive an Allow policy at the native damage boundary. This bypasses the random/accumulated-damage gate, not structural exclusions. Other providers can conflict or deny; this is not exclusive control or a guarantee against other mods. EMP does not increase an already guaranteed policy result.

Difficulty and integrity policy are **ships only**, not shared installation simulations. Enabled applies consistently to all policies. Scuttle damage is not rescaled by a second postfix. These supported semantics intentionally correct the patch-based configuration omissions and shared-installation/double-scaling behavior rather than preserving those bugs. Out-of-range finite values clamp to [0,10]; nonfinite values use 1.

## Build and install

Build Mod API Release in the adjacent `vanguard-galaxy-api` directory first, then:

```sh
make build CONFIG=Release
make test CONFIG=Release
```

`make package CONFIG=Release` writes `dist/VGBBoardAlways-v<version>.zip` and the matching `dist/update.json` feed. Packaging validates that the project version, `PluginVersion`, the version compiled into the packaged assembly and the release tag all agree, and that the metadata sidecar stays within the Mod API's documented field limits. `python3 -m unittest test_package` in `tools/` covers that gate, including rejection of a stale or foreign DLL dropped into the build output. Publication uploads those exact assets to the tagged release.

Override `API_DIR` or `API_ABSTRACTIONS` for a different API checkout. Compile references are redistributable: BepInEx and `UnityEngine.Modules` come from NuGet, Unity solely to compile BepInEx's `BaseUnityPlugin`, and the contract from `VGModAPI.Abstractions`. No Assembly-CSharp reference, Harmony patches or reflection wrappers remain.

Install the separately supplied Mod API first, then copy the `VGBBoardAlways` folder from the release archive into `BepInEx/plugins/`, or run `make deploy CONFIG=Release`. The folder holds the plugin DLL beside `vg.boardalways.vgmod.json`, so the Mod API's Mods menu shows this mod's author, description, project link and update status. Remove any older standalone `BepInEx/plugins/VGBBoardAlways.dll` first; deployment refuses to run while it exists. Do not copy local reference DLLs, test output or another copy of the abstractions assembly. Uninstall by removing the folder; retaining the config file is safe.

## Mod metadata

`vg.boardalways.vgmod.json` is optional author metadata read by the Mod API from beside the loaded DLL. It carries author, description, project URL and the stable update feed. It deliberately declares no version: installed version is authoritative loader data, and the published feed version comes from the release, not this file.

Host policy tests verify Enabled, scope, multiplier bounds, registration cleanup, single consumer integrity contribution, and that the metadata sidecar matches the compiled loader GUID within documented limits. They are not Unity qualification of native eligibility, both scuttle paths, simultaneous consumers or save/load. Those gates require controlled testing of the exact consumer/API build. No release publication or game deployment is implied by a source build.

## License

MIT
