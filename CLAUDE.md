# DeathRecap

Shows a player a recap of their death once they enter spectator. The wording depends on the kind of death (player, SCP, zombie, teamkill, environment, custom reason, unknown), with optional detail lines (distance, hit location, killer health, damage dealt, damage breakdown by source, survival time, location, kills that life). Current version: **v2.0.0**.

See `../CLAUDE.md` for shared plugin conventions and the `AutoUpdate` module design.

## Structure

- `Plugin.cs`, `Config.cs`, `Translation.cs`
- `Modules/Recap.cs` — tracking (damage per attacker and per cause, distances, life timers, kills) and showing the recap
- `Modules/DeathClassifier.cs` — sorts a death into a `DeathCategory`
- `Modules/DeathContext.cs` — the data one recap is built from
- `Modules/RecapFormatter.cs` — builds the text from a `DeathContext` using `Translation` templates and `Config` toggles
- `Modules/RoleDisplay.cs` — role names, including custom roles
- `Modules/CauseNames.cs` — readable names for `DamageType`
- `Modules/DeathDetails.cs` — hit location and death location
- `Modules/AutoUpdate.cs` — shared module, see `../CLAUDE.md`

## Design rules

- **Must work on a vanilla server.** Nothing may assume a specific custom-role plugin; extras degrade to the base role name silently.
- Everything shown is a `Translation` entry with a default, and every optional line has a `Config` toggle. New entries get defaults automatically; existing translation entries are never overwritten by EXILED, so don't change the meaning of an existing placeholder.
- The 2.0.0 rewrite dropped the old single `RecapText` entry; servers that customized it lose that wording, nothing breaks.

## Known history / gotchas

- The recap display is a MEC coroutine (`Timing.RunCoroutine` in `RecapLoop`), not `async void`/`Task.Delay`. `StopRecap`/`OnLeft`/`ResetState` kill it cleanly, and `UnregisterEvents` calls `ResetState`.
- Damage is the drop in health + Hume Shield + artificial health (`TotalHealth`), not `Health` alone. `HealthBeforeHit` is cleared on every `Hurting` and consumed on `Hurt`, so a stale snapshot can't be reused. Hits with no attacker are recorded per `DamageType` in `DamageTakenByCause`.
- Event order: `Hurt` fires right after `ApplyDamage` and before death, so the killing blow is counted. `KillPlayer` sets the role to Spectator (firing `Spawned`) BEFORE `Died` fires, so anything about the victim's position must be captured on `Dying` (`OnPlayerDying` does this for the death location).
- `ev.DamageHandler` on `Died`/`Hurt` is always EXILED's `CustomDamageHandler` wrapper. Use `ev.DamageHandler.Type` (a `DamageType`) and `ev.DamageHandler.Is<T>(out T)` for the game's own handler types (`FirearmDamageHandler`, `CustomReasonDamageHandler`); `BaseIs<T>` is only for EXILED's wrapper types. The `Hitbox` field lives on the game's `StandardDamageHandler`.
- Custom role names: `Player.UniqueRole` first, then UCR via reflection (`SummonedCustomRole.TryGet(ReferenceHub, out ...)` - same shape in the LabAPI builds and the last EXILED build v9.3.0), then the custom-info heuristic (custom info non-empty AND the Role flag missing from `InfoArea`, last line - exact for SER's `CRole`, off by default), then the base role. Every attempt is logged under `Debug` as `Killer role:`.
- Life timers only start if none exists (so escaping doesn't reset survival time) and are removed on death or leave. Kills that life only count non-teamkills.
- Config values are clamped when the recap starts (padding to >= 0, text size to >= 1) so bad values can't throw.
- Damage is clamped so a lethal hit can't count more than the health the victim had left (`Math.Max(0f, TotalHealth)`). Without it, overkill inflated "damage taken" past the victim's max health.
- Some scripted kills (e.g. the admin `explode` command on a dummy) call `KillPlayer` on someone who is already dead, firing `Died` a second time. `OnPlayerDied` ignores deaths whose `TargetOldRole` is already Spectator/Overwatch/None, otherwise the second event replaces the real recap.
- The recap is a header line plus rows from `Config.RecapLayout` (each entry lists segment keys like `hit, distance, killer_health`); `RecapFormatter.BuildSegment` returns null for a segment that's switched off or has no data, and empty rows are dropped. Nine separate lines ran off the bottom of the screen, so the default groups details several to a line, joined by `Translation.DetailSeparator`.