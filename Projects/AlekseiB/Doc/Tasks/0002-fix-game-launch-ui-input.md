# Task 0002: Fix level launch, HUD overlay order and pointer aiming

## Goal

Make the generated level scenes actually playable again and correct the pointer input:

- The game must start: `GameBootstrap` must resolve a `LevelDefinition` from `LevelDirector`.
- The result overlay must render on top of the HUD, not under it.
- Mouse/touch aiming must convert the screen pointer into world space instead of using pixels.
- The content builder must be able to regenerate correct scenes from a clean clone.

Feature document: `Doc/Features/core-gameplay.md`.
Follows: `Doc/Tasks/0001-angry-birds-vertical-slice.md`.

## Findings (root causes)

1. **Launch failure.** In `Level_01/02/03.unity` every `LevelDirector` reference was
   `{fileID: 0}`: `level` and the five prefab references (`birdPrefab`, `pigPrefab`,
   `woodBlockPrefab`, `glassBlockPrefab`, `stoneBlockPrefab`). `GameBootstrap.Awake`
   therefore logged `[GameBootstrap] The level director has no level definition.` and
   disabled itself, so no level was built.
   `GameContentBuilder` assigned those references but the scene save dropped references
   to custom-type assets (ScriptableObject `LevelDefinition` and MonoBehaviour prefab
   components), while references to built-in types (sprites, `SpriteRenderer`,
   materials) survived.
2. **Pointer aiming.** `GameBootstrap.ResolveAimPoint()` returned `input.PointerPosition`
   (screen pixels) directly, so the slingshot pulled toward a pixel value instead of the
   world point under the cursor. No `ScreenToWorldPoint` call existed in the project.
3. **HUD overlay order.** The Canvas children were `[ResultPanel, HUD]`, so the HUD
   (stars, restart button) rendered on top of the result overlay. The HUD GameObject was
   a plain `Transform`, not a stretched `RectTransform`.
4. **Builder hygiene.** Two `Debug.Log` statements were left in `CreateScenes`.

## Affected areas

- `Assets/AngryBirds/Scenes/Level_01.unity`, `Level_02.unity`, `Level_03.unity`
- `Assets/AngryBirds/Scripts/Infrastracture/Bootstrap/GameBootstrap.cs`
- `Assets/AngryBirds/Scripts/Editor/Builder/GameContentBuilder.cs`

## Plan

1. Patch the three level scenes: restore `LevelDirector.level` and the five prefab refs,
   put HUD before ResultPanel in the Canvas, make the HUD a stretched `RectTransform`.
2. `GameBootstrap`: cache the camera, convert pointer position with `ScreenToWorldPoint`
   and only start pointer aiming when the press is near the loaded bird.
3. `GameContentBuilder`: remove debug logs, create the HUD as a stretched `RectTransform`,
   create the result overlay after the HUD, mark wired components dirty, and re-open each
   saved scene to re-assign level/prefab references so regeneration is deterministic.
4. Update documentation.

## Acceptance criteria

- [ ] Opening any `Level_0X` scene and pressing Play builds the level and loads a bird.
- [ ] Pulling with the mouse moves the bird under the cursor and launches it.
- [ ] Clicking the restart button does not also grab/launch the slingshot.
- [ ] The result overlay covers the HUD when the level finishes.
- [ ] Re-running `Tools/AngryBirds/2. Build Game Content` keeps valid references.
- [ ] No compile errors and no new Console warnings.

## Validation performed

- Static review of the three scene files and the runtime/editor scripts.
- YAML references cross-checked against the prefab `.meta` GUIDs and component fileIDs.
- Unity play-mode validation still pending because the editor was open while the scenes
  were patched; it must be re-run after reopening the project.

## Outcome

- Scene references for `LevelDirector` restored in all three scenes.
- `GameBootstrap` now converts screen pointer positions to world positions and guards
  pointer grabs by distance to the slingshot anchor (`pointerGrabRadius`).
- HUD reordered below the result overlay and converted to a stretched `RectTransform`.
- Builder hardened; regeneration now re-assigns asset references after the scene save.
- Assigned: `Doc/Reviews/0001-launch-ui-input-review.md`.
