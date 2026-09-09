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

Public release packaging is gated until approved redistributable API/Unity compile-reference sourcing is configured. The manual packaging workflow fails explicitly rather than using local game references or publishing an incomplete package.

Override `API_DIR` for a different API checkout. `make` links ignored compile references. Boarding policy references only `VGModAPI.Abstractions`; minimal Unity references exist solely to compile BepInEx's `BaseUnityPlugin`. No Assembly-CSharp reference, Harmony patches or reflection wrappers remain.

Install the separately supplied Mod API first, then copy **only** `VGBBoardAlways/bin/Release/netstandard2.1/VGBBoardAlways.dll` into `BepInEx/plugins/`. Do not copy local reference DLLs, test output or another copy of the abstractions assembly. Restart after changing Mod API integration configuration. Uninstall by removing `VGBBoardAlways.dll`; retaining the config file is safe.

Host policy tests verify Enabled, scope, multiplier bounds, registration cleanup and single consumer integrity contribution. They are not Unity qualification of native eligibility, both scuttle paths, simultaneous consumers or save/load. Those gates require controlled testing of the exact consumer/API build. No release publication or game deployment is implied by a source build.

## License

MIT
