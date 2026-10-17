# Review 0001: Level launch, pointer aiming and UI overlay

Reviewed scope: task `0002-fix-game-launch-ui-input.md` — the level launch fix, pointer
aiming, HUD/result overlay order and the content builder reference rewiring.

Status: review written after task `0003` was implemented, so the findings below include the
follow-ups that task `0003` already addressed.

## Findings (by severity)

### High

1. **Scene asset references were dropped on build.** `LevelDirector.level` and the five
   prefab references were `{fileID: 0}` in all three level scenes, so bootstrap disabled
   itself and no level was built. References to custom-type assets were dropped by the
   scene save while built-in-type references survived.
   → Fixed in task 0002 (hand patch + builder rewire pass).
2. **Pointer aiming used screen pixels.** `ResolveAimPoint()` returned the raw pointer
   position instead of `ScreenToWorldPoint`, so the slingshot pulled toward a pixel value.
   → Fixed in task 0002 (`PointerToWorld()` + grab-proximity guard).
3. **`SpriteRenderer.size` was ignored everywhere.** Blocks, ground segments, burst pieces
   and the sky/clouds set `size` in the default `Simple` draw mode, so they rendered at
   native sprite size. The sky was an invisible 0.04-unit sliver and blocks did not match
   their colliders.
   → Fixed in task 0003 (`SpriteDrawMode.Sliced`).
4. **HUD widgets overlapped the play area.** The live stars and the restart button were
   center-anchored in the stretched HUD, so the restart button floated over the gameplay.
   → Fixed in task 0003 (stars top-left, restart button top-right).

### Medium

5. **Sprites shimmered when zoomed out.** Placeholder sprites had no mipmaps and were
   displayed well below their source resolution.
   → Fixed in task 0003 (mipmaps enabled via importer and `.png.meta` files).

### Low

6. **HUD was a plain `Transform` and Canvas order was inverted** (`[ResultPanel, HUD]`),
   in addition to the overlay ordering fix.
   → Fixed in task 0002.

## Accepted risks

- Placeholder art is intentionally simple and flat; replacing it file by file is still a
  separate effort.
- `dotnet build` was used for compile validation because the editor stays open; the two
  stale `TutorialInfo` entries in the generated csproj files were bypassed locally and the
  files restored afterwards.

## Follow-up actions

- Unity play-test: open `Level_01`, press Play, verify aim/launch, restart behavior,
  result overlay, block/ground sizing and the top-right restart button.
- Re-run `Tools/AngryBirds/2. Build Game Content` once and confirm references stay valid
  (task 0002 acceptance criterion) and the HUD/backdrop layout reproduces.
- Clean up: Unity should regenerate the csproj files without the deleted `TutorialInfo`
  entries; no manual csproj edits are kept.

Implemented by: `Doc/Tasks/0002-fix-game-launch-ui-input.md`,
`Doc/Tasks/0003-fix-hud-layout-and-sprite-scaling.md`.