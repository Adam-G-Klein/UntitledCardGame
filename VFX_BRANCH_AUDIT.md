# VFXJuly branch audit

As of 2026-10-26 · compared against the branch's base on `main` (`336caf88`), including committed, uncommitted and untracked work.

**Verdict:** the branch now touches programmer-owned code and data only through VFX slots, VFX effect steps and null-safe hooks. Every changed card, enemy, companion and power file has been cleaned down to VFX-only changes (verified: 0 gameplay steps changed or removed in 488 files). Five behaviour changes need a programmer's sign-off, and four decisions remain before merge.

## Verdict by area

| Area | Follows the programmers' workflow? | Risk to programmers | Action |
| --- | --- | --- | --- |
| Card / enemy / companion / power data (488 files) | Yes: `vfxPrefab`, `InstantiatePrefab`, `FXEffectStep`, trigger-VFX steps | Was high (Unity re-save noise, stale fields dropped); now low | Cleaned in this audit |
| New VFX prefabs, textures, materials (~370 files) | Yes: new files in their own folders; originals copied, never edited (`CM_` / `CTX_`) | None | None |
| VFX runtime scripts (21 new, `Assets/Scripts/VfxControl`) | Yes: components on VFX prefabs, or watchers that read state | Low: they reference game types, so renames break them like any code | None |
| Hooks in programmer scripts (18 files) | Mostly: one-line, null-safe calls into VFX slots | Low, except the five below | Sign-off on 5 |
| Gameplay prefabs (WorldspaceCard, ShopManager, BossVisuals, Act 1–3 environments, VFXBackground, EnemyAttackVFX, CardExhaustVFX) | Yes: VFX fields and VFX components only | Low; merge conflicts if edited on `main` too | Merge with care |
| Scenes (CombatScene, MainMenu, EndOfRunProgressScene) | Yes: VFX slots, a "Precast Trails" object, wipe settings | Medium: scene files merge badly | Decide (below) |
| Timelines (card cast / consume / create / exhaust, enemy attack, FX experiences) | Yes: the FX experience workflow | Low | None |
| Editor tools (`Assets/Editor/VFX*`) | n/a, artist tooling | Medium: compile coupling | Decide (below) |
| Project settings (`TagManager`: WorldCards layer) | n/a | Low: clash if `main` uses that layer slot | Check before merge |
| `Packages/manifest.json`, `packages-lock.json` | n/a | Must never be committed (local MCP package) | Exclude from commit |

## Cleaned up in this audit

- **Card data rebuilt to VFX-only diffs.** Every changed data file was rebuilt from its base version, re-applying only the VFX changes:
  - 304 effect slots and 181 spawn scales;
  - the effect steps the VFX wiring added (102 trigger badges, 69 effect spawns, 40 card cues, 22 FX sequences, 17 card-changed cues), plus the 28 wait and 19 targeting steps that time them;
  - 11 doubled exhaust effects removed.

  This dropped ~2,200 lines of Unity re-save noise: default values for newer fields, stale fields the code no longer has, and recreated tooltip entries. Examples that would have looked alarming in review:
  - Hold'em "losing" its card pools;
  - the demo Hold'em "losing" its starting deck;
  - 9 DialogueStep entries deleted. These are restored, so if DialogueStep comes back, the data is intact.
- **Verified:** 0 gameplay steps changed or removed. Gameplay step order is identical to the base. Every file loads in Unity; the only empty steps are the 62 whose classes were already removed on `main` (same count as the base). Backup of the pre-cleanup files: the session scratchpad `data_backup` folder.
- **Runtime save data reverted:** `ActivePlayerData_GameState.asset` changed only from playing.
- **Deleted assets archived instead:** `M_CardFlicker.mat` and `CardFlicker.shadergraph` had been deleted outright. They're restored from git into the archive, per the never-delete rule.
- **One-off migrations can't be rerun by accident:** removed the menu entries for the 13 card-wiring migrations and the 4 blueprint unpack/prune/palette steps. They were all applied already; rerunning one later could overwrite a programmer's own effect or scale choices. The code stays, callable from code only.

## Needs programmer sign-off

These change timing or flow, not just visuals. Each can be switched off or reverted on its own.

| Change | Where | What it does | Off switch |
| --- | --- | --- | --- |
| Hit-stop and last-kill slow-mo | `CombatEntityManager.PlayHitWeight` / `PlayLastKillIfFinalEnemy`, `HitStop` | Briefly scales `Time.timeScale` on heavy hits (≥8 damage) and on the fight's last kill | `useHitStop`, `useLastKillSlowMo` in Encounter Constants |
| In-run scene transitions | `SceneTransitionManager.LoadSceneWithReveal`, `ScreenFader`, `GameStateVariableSO` | In-run location changes use an ink wipe and hold on black until the new scene settles (up to 1.2 s) | Clear `revealMaterial` on the transition manager → old fade |
| Precast trails | `InstantiatePrefab`, `CombatEffectStep` | Ally-to-ally effects play a trail first; the effect's VFX lands when the trail arrives. Gameplay still applies immediately | Disable the "Precast Trails" object in CombatScene |
| Enemy attack waits for its effect | `GenericEnemyAttackEffectStep.asset`, `waitForEffect` 0 → 1 | The enemy turn waits for the attack sequence to finish | Set it back to 0 |
| 28 short waits in cards | `WaitForSecondsEffect` steps added beside the FX sequences (gun shots, ability triggers, Scry) | Adds pauses of 0.16–0.5 s (11 × 0.16, 8 × 0.5) so the effect reads before the next step | Delete the step |

## Open decisions before merge

1. **Editor-tool compile coupling. Decided: archive the wiring tool.** `VFXRetoolBuilder` (32 references) and `VFXRetoolWiring` (29) name game types directly: `InstantiatePrefab`, `FXExperience`, `PositionClip`, `CardType` and others. If a programmer renames one, these editor scripts stop compiling, and Unity won't enter play mode until fixed.
   - `VFXRetoolWiring.cs` moves to `VFX Archive (Unused)/Assets/Editor/`, since all its migrations are applied. That removes about half the coupling.
   - The builder stays; if a rename ever breaks it, it's a quick fix.
2. **Scenes.** Scene files merge badly. Either merge them before programmers next edit those scenes, or move the scene-side VFX wiring onto prefabs. CombatScene's "Precast Trails" object is the main one.
   - The MainMenu also holds a disabled preview tool (the wipe transition preview). It should come out.
3. **Churn to revert before committing.** The UI Toolkit render textures and panel settings, and `Assets/UI/testRenderText/PostCombatMaterial.mat`, change whenever the game runs. My revert was blocked by permissions, so run:
   `git checkout -- "Assets/UI Toolkit" "Assets/UI/testRenderText/PostCombatMaterial.mat"`
4. **Never commit `Packages/manifest.json` or `Packages/packages-lock.json`** (the Unity MCP package is local-only).

## Keeping it this way

- **Card data:** VFX changes go only into `vfxPrefab`, `InstantiatePrefab`, `FXEffectStep` and the trigger-VFX steps. Never into gameplay steps or values.
- **Programmer-owned prefabs and scripts:** only VFX slots, VFX components, and hooks that do nothing when the slot is empty.
- **Generated effects:** the effects in `Assets/Prefabs/VFX/Card Effects V2` come from `VFXRetoolBuilder`. Anyone can hand-edit them; a rebuild would overwrite those edits, so rebuild only from the recipe.
- **Unity re-save noise:** Unity re-saves data files when tools touch them. Before committing, diff data files against `main`, and keep only the VFX lines.

## Inventory

| Kind | Count |
| --- | --- |
| Data files with VFX changes | 488 |
| New VFX prefabs | 154 (Card Effects V2, FX Experience variants, combat feel, status, mana, board lighting, blueprints) |
| New textures / materials | 217 |
| New VFX runtime scripts | 21 (+2 card-cue effect steps) |
| Programmer scripts with VFX hooks | 18 |
| Gameplay prefabs with VFX slots / components | 9 |
| Scenes with VFX wiring | 3 |
| Archived unused assets | `VFX Archive (Unused)/`, each listed in its README |
